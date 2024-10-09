using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace PokeWorld;

internal class JobDriver_CraftPokemon : JobDriver
{
    public const PathEndMode GotoIngredientPathEndMode = PathEndMode.ClosestTouch;

    public const TargetIndex BillGiverInd = TargetIndex.A;

    public const TargetIndex IngredientInd = TargetIndex.B;

    public const TargetIndex IngredientPlaceCellInd = TargetIndex.C;

    public int billStartTick;

    public int ticksSpentDoingRecipeWork;
    public float workLeft;

    public IBillGiver BillGiver => job.GetTarget(TargetIndex.A).Thing as IBillGiver ??
                                   throw new InvalidOperationException("DoBill on non-Billgiver.");

    public override string GetReport()
    {
        return job.RecipeDef != null ? ReportStringProcessed(job.RecipeDef.jobString) : base.GetReport();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref workLeft, "workLeft");
        Scribe_Values.Look(ref billStartTick, "billStartTick");
        Scribe_Values.Look(ref ticksSpentDoingRecipeWork, "ticksSpentDoingRecipeWork");
    }

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        var thing = job.GetTarget(TargetIndex.A).Thing;
        if (!pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed)) return false;
        if (thing != null && thing.def.hasInteractionCell &&
            !pawn.ReserveSittableOrSpot(thing.InteractionCell, job, errorOnFailed)) return false;
        pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.B), job);
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        AddEndCondition(
            delegate
            {
                var thing = GetActor().jobs.curJob.GetTarget(TargetIndex.A).Thing;
                return thing is not Building || thing.Spawned ? JobCondition.Ongoing : JobCondition.Incompletable;
            }
        );
        this.FailOnBurningImmobile(TargetIndex.A);
        this.FailOn(
            delegate
            {
                if (job.GetTarget(TargetIndex.A).Thing is not IBillGiver billGiver) return false;
                if (job.bill.DeletedOrDereferenced) return true;
                return !billGiver.CurrentlyUsableForBills();
            }
        );
        var gotoBillGiver = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
        var toil = new Toil
        {
            initAction = delegate
            {
                if (job.targetQueueB is not { Count: 1 }) return;
                if (job.targetQueueB[0].Thing is UnfinishedThing unfinishedThing)
                    unfinishedThing.BoundBill = (Bill_ProductionWithUft)job.bill;
            }
        };
        yield return toil;
        yield return Toils_Jump.JumpIf(gotoBillGiver, () => job.GetTargetQueue(TargetIndex.B).NullOrEmpty());
        foreach (var item in CollectIngredientsToils(TargetIndex.B, TargetIndex.A, TargetIndex.C)) yield return item;
        yield return gotoBillGiver;
        yield return Toils_RecipeCraftPokemon.MakeUnfinishedThingIfNeeded();
        yield return Toils_RecipeCraftPokemon.DoRecipeWork().FailOnDespawnedNullOrForbiddenPlacedThings()
            .FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
        yield return Toils_RecipeCraftPokemon.FinishRecipeAndSpawnPokemon();
        if (job.RecipeDef.products.NullOrEmpty() && job.RecipeDef.specialProducts.NullOrEmpty()) yield break;
        var recount = new Toil();
        recount.initAction = delegate
        {
            if (recount.actor.jobs.curJob.bill is Bill_Production bill_Production &&
                bill_Production.repeatMode == BillRepeatModeDefOf.TargetCount)
                Map.resourceCounter.UpdateResourceCounts();
        };
        yield return recount;
    }

    public static IEnumerable<Toil> CollectIngredientsToils(
        TargetIndex ingredientInd, TargetIndex billGiverInd, TargetIndex ingredientPlaceCellInd,
        bool subtractNumTakenFromJobCount = false, bool failIfStackCountLessThanJobCount = true
    )
    {
        var extract = Toils_JobTransforms.ExtractNextTargetFromQueue(ingredientInd);
        yield return extract;
        var getToHaulTarget = Toils_Goto.GotoThing(ingredientInd, PathEndMode.ClosestTouch)
            .FailOnDespawnedNullOrForbidden(ingredientInd).FailOnSomeonePhysicallyInteracting(ingredientInd);
        yield return getToHaulTarget;
        yield return Toils_Haul.StartCarryThing(
            ingredientInd, true, subtractNumTakenFromJobCount, failIfStackCountLessThanJobCount
        );
        yield return JumpToCollectNextIntoHandsForBill(getToHaulTarget, TargetIndex.B);
        yield return Toils_Goto.GotoThing(billGiverInd, PathEndMode.InteractionCell)
            .FailOnDestroyedOrNull(ingredientInd);
        var findPlaceTarget = Toils_JobTransforms.SetTargetToIngredientPlaceCell(
            billGiverInd, ingredientInd, ingredientPlaceCellInd
        );
        yield return findPlaceTarget;
        yield return Toils_RecipeCraftPokemon.PlaceHauledThingInCell(ingredientPlaceCellInd, findPlaceTarget, false);
        yield return Toils_Jump.JumpIfHaveTargetInQueue(ingredientInd, extract);
    }

    public static Toil JumpToCollectNextIntoHandsForBill(Toil gotoGetTargetToil, TargetIndex ind)
    {
        var toil = new Toil();
        toil.initAction = delegate
        {
            var actor = toil.actor;
            if (actor.carryTracker.CarriedThing == null)
            {
                Log.Error(
                    string.Concat("JumpToAlsoCollectTargetInQueue run on ", actor, " who is not carrying something.")
                );
            }
            else if (!actor.carryTracker.Full)
            {
                var curJob = actor.jobs.curJob;
                var targetQueue = curJob.GetTargetQueue(ind);
                if (targetQueue.NullOrEmpty()) return;
                for (var i = 0; i < targetQueue.Count; i++)
                    if (GenAI.CanUseItemForWork(actor, targetQueue[i].Thing) &&
                        targetQueue[i].Thing.CanStackWith(actor.carryTracker.CarriedThing) &&
                        !((actor.Position - targetQueue[i].Thing.Position).LengthHorizontalSquared > 64f))
                    {
                        var num = actor.carryTracker.CarriedThing?.stackCount ?? 0;
                        var a = curJob.countQueue[i];
                        a = Mathf.Min(a, targetQueue[i].Thing.def.stackLimit - num);
                        a = Mathf.Min(a, actor.carryTracker.AvailableStackSpace(targetQueue[i].Thing.def));
                        if (a <= 0) continue;
                        curJob.count = a;
                        curJob.SetTarget(ind, targetQueue[i].Thing);
                        curJob.countQueue[i] -= a;
                        if (curJob.countQueue[i] <= 0)
                        {
                            curJob.countQueue.RemoveAt(i);
                            targetQueue.RemoveAt(i);
                        }

                        actor.jobs.curDriver.JumpToToil(gotoGetTargetToil);
                        break;
                    }
            }
        };
        return toil;
    }
}
