using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace PokeWorld;

internal class JobDriver_Fish : JobDriver
{
    protected IntVec3 TargetCell => job.GetTarget(TargetIndex.A).Cell;

    protected Thing FishingRod => job.GetTarget(TargetIndex.B).Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(TargetCell, job, 1, 1, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        //this.FailOnDestroyedOrNull(TargetIndex.A);
        this.FailOn(() => !FishingUtility.IsFishingTerrain(TargetCell.GetTerrain(pawn.Map)));
        this.FailOn(() => !pawn.EquippedWornOrInventoryThings.Contains(FishingRod));
        var gotoToil = Toils_Goto.GotoCell(TargetCell, PathEndMode.Touch)
            .FailOn(() => !pawn.CanReach(TargetCell, PathEndMode.Touch, Danger.Deadly));
        yield return gotoToil;
        var ticks = FishingUtility.ComputeFishingTicks(pawn, FishingRod);
        var toil = Toils_General.Wait(ticks, TargetIndex.A);
        toil.activeSkill = () => SkillDefOf.Animals;
        toil.WithProgressBarToilDelay(TargetIndex.A);
        yield return toil;
        var toil2 = new Toil
        {
            initAction = delegate
            {
                FishingUtility.TryFish(pawn, FishingRod, TargetCell);
                //fishingSpot.Fish(pawn,rod,targetTerrain);
            },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
        yield return toil2;
        yield return Toils_Jump.Jump(gotoToil);
        /*Toil work = ToilMaker.MakeToil("MakeNewToils");
        work.tickAction = delegate
        {
            Pawn actor = work.actor;
            actor.skills.Learn(SkillDefOf.Animals, 0.035f);
        };
        work.defaultCompleteMode = ToilCompleteMode.Never;
        work.WithEffect(EffecterDefOf.Drill, TargetIndex.A);
        work.activeSkill = () => SkillDefOf.Animals;
        yield return work;*/
    }
}
