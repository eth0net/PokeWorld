using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace PokeWorld;

public static class Toils_RecipeCraftPokemon
{
    public static Toil MakeUnfinishedThingIfNeeded()
    {
        var toil = new Toil();
        toil.initAction = delegate
        {
            var actor = toil.actor;
            var curJob = actor.jobs.curJob;
            if (!curJob.RecipeDef.UsesUnfinishedThing ||
                curJob.GetTarget(TargetIndex.B).Thing is UnfinishedThing) return;
            var list = CalculateIngredients(curJob, actor);
            var thing = CalculateDominantIngredient(curJob, list);
            foreach (var thing2 in list)
            {
                actor.Map.designationManager.RemoveAllDesignationsOn(thing2);
                if (thing2.Spawned) thing2.DeSpawn();
            }

            var stuff = curJob.RecipeDef.unfinishedThingDef.MadeFromStuff ? thing.def : null;
            var unfinishedThing = (UnfinishedThing)ThingMaker.MakeThing(curJob.RecipeDef.unfinishedThingDef, stuff);
            unfinishedThing.Creator = actor;
            unfinishedThing.BoundBill = (Bill_ProductionWithUft)curJob.bill;
            unfinishedThing.ingredients = list;
            unfinishedThing.TryGetComp<CompColorable>()?.SetColor(thing.DrawColor);
            GenSpawn.Spawn(unfinishedThing, curJob.GetTarget(TargetIndex.A).Cell, actor.Map);
            curJob.SetTarget(TargetIndex.B, unfinishedThing);
            actor.Reserve(unfinishedThing, curJob);
        };
        return toil;
    }

    public static Toil DoRecipeWork()
    {
        var toil = new Toil();
        toil.initAction = delegate
        {
            var actor3 = toil.actor;
            var curJob3 = actor3.jobs.curJob;
            var jobDriver_CraftPokemon2 = (JobDriver_CraftPokemon)actor3.jobs.curDriver;
            var unfinishedThing3 = curJob3.GetTarget(TargetIndex.B).Thing as UnfinishedThing;
            if (unfinishedThing3 is { Initialized: true })
            {
                jobDriver_CraftPokemon2.workLeft = unfinishedThing3.workLeft;
            }
            else
            {
                jobDriver_CraftPokemon2.workLeft = curJob3.bill.recipe.WorkAmountTotal(unfinishedThing3);
                if (unfinishedThing3 != null) unfinishedThing3.workLeft = jobDriver_CraftPokemon2.workLeft;
            }

            jobDriver_CraftPokemon2.billStartTick = Find.TickManager.TicksGame;
            jobDriver_CraftPokemon2.ticksSpentDoingRecipeWork = 0;
            curJob3.bill.Notify_DoBillStarted(actor3);
        };
        toil.tickAction = delegate
        {
            var actor2 = toil.actor;
            var curJob2 = actor2.jobs.curJob;
            var jobDriver_CraftPokemon = (JobDriver_CraftPokemon)actor2.jobs.curDriver;
            var unfinishedThing2 = curJob2.GetTarget(TargetIndex.B).Thing as UnfinishedThing;
            if (unfinishedThing2 is { Destroyed: true })
            {
                actor2.jobs.EndCurrentJob(JobCondition.Incompletable);
            }
            else
            {
                jobDriver_CraftPokemon.ticksSpentDoingRecipeWork++;
                curJob2.bill.Notify_PawnDidWork(actor2);
                (toil.actor.CurJob.GetTarget(TargetIndex.A).Thing as IBillGiverWithTickAction)?.UsedThisTick();
                if (curJob2.RecipeDef.workSkill != null && curJob2.RecipeDef.UsesUnfinishedThing)
                    actor2.skills.Learn(curJob2.RecipeDef.workSkill, 0.1f * curJob2.RecipeDef.workSkillLearnFactor);
                var num = curJob2.RecipeDef.workSpeedStat == null
                    ? 1f
                    : actor2.GetStatValue(curJob2.RecipeDef.workSpeedStat);
                if (curJob2.RecipeDef.workTableSpeedStat != null)
                    if (jobDriver_CraftPokemon.BillGiver is Building_WorkTable building_WorkTable)
                        num *= building_WorkTable.GetStatValue(curJob2.RecipeDef.workTableSpeedStat);
                if (DebugSettings.fastCrafting) num *= 30f;
                jobDriver_CraftPokemon.workLeft -= num;
                if (unfinishedThing2 != null) unfinishedThing2.workLeft = jobDriver_CraftPokemon.workLeft;
                actor2.GainComfortFromCellIfPossible(true);
                if (jobDriver_CraftPokemon.workLeft <= 0f)
                {
                    jobDriver_CraftPokemon.ReadyForNextToil();
                }
                else if (curJob2.bill.recipe.UsesUnfinishedThing)
                {
                    var num2 = Find.TickManager.TicksGame - jobDriver_CraftPokemon.billStartTick;
                    if (num2 >= 3000 && num2 % 1000 == 0) actor2.jobs.CheckForJobOverride();
                }
            }
        };
        toil.defaultCompleteMode = ToilCompleteMode.Never;
        toil.WithEffect(() => toil.actor.CurJob.bill.recipe.effectWorking, TargetIndex.A);
        toil.PlaySustainerOrSound(() => toil.actor.CurJob.bill.recipe.soundWorking);
        toil.WithProgressBar(
            TargetIndex.A, delegate
            {
                var actor = toil.actor;
                var curJob = actor.CurJob;
                var unfinishedThing = curJob.GetTarget(TargetIndex.B).Thing as UnfinishedThing;
                return 1f - ((JobDriver_CraftPokemon)actor.jobs.curDriver).workLeft /
                    curJob.bill.recipe.WorkAmountTotal(unfinishedThing);
            }
        );
        toil.FailOn(
            (Func<bool>)delegate
            {
                var recipeDef = toil.actor.CurJob.RecipeDef;
                if (recipeDef is not { interruptIfIngredientIsRotting: true }) return toil.actor.CurJob.bill.suspended;
                var target = toil.actor.CurJob.GetTarget(TargetIndex.B);
                if (target.HasThing && (int)target.Thing.GetRotStage() > 0) return true;

                return toil.actor.CurJob.bill.suspended;
            }
        );
        toil.activeSkill = () => toil.actor.CurJob.bill.recipe.workSkill;
        return toil;
    }

    public static Toil FinishRecipeAndSpawnPokemon()
    {
        var toil = new Toil();
        toil.initAction = delegate { FinishRecipeAndSpawnPokemonAction(toil); };
        return toil;
    }

    public static void FinishRecipeAndSpawnPokemonAction(Toil toil)
    {
        var actor = toil.actor;
        var curJob = actor.jobs.curJob;
        var jobDriver_CraftPokemon = (JobDriver_CraftPokemon)actor.jobs.curDriver;
        if (curJob.RecipeDef.workSkill != null && !curJob.RecipeDef.UsesUnfinishedThing)
        {
            var xp = jobDriver_CraftPokemon.ticksSpentDoingRecipeWork * 0.1f * curJob.RecipeDef.workSkillLearnFactor;
            actor.skills.GetSkill(curJob.RecipeDef.workSkill).Learn(xp);
        }

        var ingredients = CalculateIngredients(curJob, actor);
        var dominantIngredient = CalculateDominantIngredient(curJob, ingredients);
        var list = GenRecipe.MakeRecipeProducts(
            curJob.RecipeDef, actor, ingredients, dominantIngredient, jobDriver_CraftPokemon.BillGiver
        ).ToList();
        ConsumeIngredients(ingredients, curJob.RecipeDef, actor.Map);
        curJob.bill.Notify_IterationCompleted(actor, ingredients);
        RecordsUtility.Notify_BillDone(actor, list);
        var unfinishedThing = curJob.GetTarget(TargetIndex.B).Thing as UnfinishedThing;
        if (curJob.bill.recipe.WorkAmountTotal(unfinishedThing) >= 10000f && list.Count > 0)
            TaleRecorder.RecordTale(TaleDefOf.CompletedLongCraftingProject, actor, list[0].GetInnerIfMinified().def);
        if (list.Any()) Find.QuestManager.Notify_ThingsProduced(actor, list);
        if (list.Count == 0) actor.jobs.EndCurrentJob(JobCondition.Succeeded);
        var isFossil = false;
        var message = "PW_CraftedPokemon";
        if (curJob.RecipeDef.products[0].thingDef.HasComp(typeof(CompPokemon)) && curJob.RecipeDef.products[0].thingDef
                .GetCompProperties<CompProperties_Pokemon>().attributes.Contains(PokemonAttribute.Fossil))
        {
            message = "PW_ResurrectedFossil";
            isFossil = true;
        }
        else if (curJob.RecipeDef.products[0].thingDef == DefDatabase<ThingDef>.GetNamed("PW_Spiritomb"))
        {
            message = "PW_UnleashedPokemon";
            isFossil = false;
        }

        var revivedPokemon = PokemonGeneratorUtility.GenerateAndSpawnNewPokemon(
            DefDatabase<PawnKindDef>.GetNamed(curJob.RecipeDef.products[0].thingDef.defName), Faction.OfPlayer,
            actor.Position, actor.Map, actor, true, isFossil
        );
        Find.World.GetComponent<PokedexManager>().AddPokemonKindCaught(
            DefDatabase<PawnKindDef>.GetNamed(curJob.RecipeDef.products[0].thingDef.defName)
        );
        Messages.Message(
            message.Translate(actor.LabelShortCap, revivedPokemon.KindLabel), revivedPokemon,
            MessageTypeDefOf.PositiveEvent
        );
    }

    private static List<Thing> CalculateIngredients(Job job, Pawn actor)
    {
        if (job.GetTarget(TargetIndex.B).Thing is UnfinishedThing unfinishedThing)
        {
            var ingredients = unfinishedThing.ingredients;
            job.RecipeDef.Worker.ConsumeIngredient(unfinishedThing, job.RecipeDef, actor.Map);
            job.placedThings = null;
            return ingredients;
        }

        var list = new List<Thing>();
        if (job.placedThings != null)
            foreach (var t in job.placedThings)
            {
                if (t.Count <= 0)
                {
                    Log.Error(
                        string.Concat(
                            "PlacedThing ", t, " with count ", t.Count, " for job ",
                            job
                        )
                    );
                    continue;
                }

                var thing = t.Count >= t.thing.stackCount
                    ? t.thing
                    : t.thing.SplitOff(t.Count);
                t.Count = 0;
                if (list.Contains(thing))
                {
                    Log.Error("Tried to add ingredient from job placed targets twice: " + thing);
                    continue;
                }

                list.Add(thing);
                if (job.RecipeDef.autoStripCorpses) (thing as IStrippable)?.Strip();
            }

        job.placedThings = null;
        return list;
    }

    private static Thing CalculateDominantIngredient(Job job, List<Thing> ingredients)
    {
        if (job.GetTarget(TargetIndex.B).Thing is UnfinishedThing uft && uft.def.MadeFromStuff)
            return uft.ingredients.First(ing => ing.def == uft.Stuff);
        if (ingredients.NullOrEmpty()) return null;
        if (job.RecipeDef.productHasIngredientStuff) return ingredients[0];
        if (job.RecipeDef.products.Any(x => x.thingDef.MadeFromStuff))
            return ingredients.Where(x => x.def.IsStuff).RandomElementByWeight(x => x.stackCount);
        return ingredients.RandomElementByWeight(x => x.stackCount);

    }

    private static void ConsumeIngredients(List<Thing> ingredients, RecipeDef recipe, Map map)
    {
        foreach (var t in ingredients)
            recipe.Worker.ConsumeIngredient(t, recipe, map);
    }

    public static Toil PlaceHauledThingInCell(
        TargetIndex cellInd, Toil nextToilOnPlaceFailOrIncomplete, bool storageMode,
        bool tryStoreInSameStorageIfSpotCantHoldWholeStack = false
    )
    {
        var toil = new Toil();
        toil.initAction = delegate
        {
            var actor = toil.actor;
            var curJob = actor.jobs.curJob;
            var cell = curJob.GetTarget(cellInd).Cell;
            if (actor.carryTracker.CarriedThing == null)
            {
                Log.Error(string.Concat(actor, " tried to place hauled thing in cell but is not hauling anything."));
            }
            else
            {
                var slotGroup = actor.Map.haulDestinationManager.SlotGroupAt(cell);
                if (slotGroup != null && slotGroup.Settings.AllowedToAccept(actor.carryTracker.CarriedThing))
                    actor.Map.designationManager.TryRemoveDesignationOn(
                        actor.carryTracker.CarriedThing, DesignationDefOf.Haul
                    );
                Action<Thing, int> placedAction = null;
                if (curJob.def == JobDefOf.DoBill || curJob.def == DefDatabase<JobDef>.GetNamed("PW_CraftPokemon") ||
                    curJob.def == JobDefOf.RecolorApparel || curJob.def == JobDefOf.RefuelAtomic ||
                    curJob.def == JobDefOf.RearmTurretAtomic)
                    placedAction = delegate(Thing th, int added)
                    {
                        curJob.placedThings ??= [];
                        var thingCountClass = curJob.placedThings.Find(x => x.thing == th);
                        if (thingCountClass != null)
                            thingCountClass.Count += added;
                        else
                            curJob.placedThings.Add(new ThingCountClass(th, added));
                    };
                if (actor.carryTracker.TryDropCarriedThing(cell, ThingPlaceMode.Direct, out var _, placedAction)) 
                    return;
                if (storageMode)
                {
                    if (nextToilOnPlaceFailOrIncomplete != null &&
                        ((tryStoreInSameStorageIfSpotCantHoldWholeStack &&
                          StoreUtility.TryFindBestBetterStoreCellForIn(
                              actor.carryTracker.CarriedThing, actor, actor.Map, StoragePriority.Unstored,
                              actor.Faction, cell.GetSlotGroup(actor.Map), out var foundCell
                          )) || StoreUtility.TryFindBestBetterStoreCellFor(
                            actor.carryTracker.CarriedThing, actor, actor.Map, StoragePriority.Unstored,
                            actor.Faction, out foundCell
                        )))
                    {
                        if (actor.CanReserve(foundCell)) actor.Reserve(foundCell, actor.CurJob);
                        actor.CurJob.SetTarget(cellInd, foundCell);
                        actor.jobs.curDriver.JumpToToil(nextToilOnPlaceFailOrIncomplete);
                    }
                    else
                    {
                        var job = HaulAIUtility.HaulAsideJobFor(actor, actor.carryTracker.CarriedThing);
                        if (job != null)
                        {
                            curJob.targetA = job.targetA;
                            curJob.targetB = job.targetB;
                            curJob.targetC = job.targetC;
                            curJob.count = job.count;
                            curJob.haulOpportunisticDuplicates = job.haulOpportunisticDuplicates;
                            curJob.haulMode = job.haulMode;
                            actor.jobs.curDriver.JumpToToil(nextToilOnPlaceFailOrIncomplete);
                        }
                        else
                        {
                            Log.Error(
                                string.Concat(
                                    "Incomplete haul for ", actor, ": Could not find anywhere to put ",
                                    actor.carryTracker.CarriedThing, " near ", actor.Position,
                                    ". Destroying. This should never happen!"
                                )
                            );
                            actor.carryTracker.CarriedThing.Destroy();
                        }
                    }
                }
                else if (nextToilOnPlaceFailOrIncomplete != null)
                {
                    actor.jobs.curDriver.JumpToToil(nextToilOnPlaceFailOrIncomplete);
                }
            }
        };
        return toil;
    }
}
