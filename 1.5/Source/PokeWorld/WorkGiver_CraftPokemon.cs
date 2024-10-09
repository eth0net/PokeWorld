using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace PokeWorld;

internal class WorkGiver_CraftPokemon : WorkGiver_Scanner
{
    private static readonly IntRange ReCheckFailedBillTicksRange = new(500, 600);

    private static string MissingMaterialsTranslated;

    private static readonly List<Thing> relevantThings = new();

    private static readonly HashSet<Thing> processedThings = new();

    private static readonly List<Thing> newRelevantThings = new();

    private static readonly List<Thing> tmpMedicine = new();

    private static readonly DefCountList availableCounts = new();

    private readonly List<ThingCount> chosenIngThings = new();

    public override PathEndMode PathEndMode => PathEndMode.InteractionCell;

    public override ThingRequest PotentialWorkThingRequest
    {
        get
        {
            if (def.fixedBillGiverDefs is { Count: 1 })
                return ThingRequest.ForDef(def.fixedBillGiverDefs[0]);
            return ThingRequest.ForGroup(ThingRequestGroup.PotentialBillGiver);
        }
    }

    public override Danger MaxPathDanger(Pawn pawn)
    {
        return Danger.Some;
    }

    public static void ResetStaticData()
    {
        MissingMaterialsTranslated = "MissingMaterials".Translate();
    }

    public override bool ShouldSkip(Pawn pawn, bool forced = false)
    {
        var list = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.PotentialBillGiver);
        foreach (var t in list)
            if (t is IBillGiver billGiver && ThingIsUsableBillGiver(t) &&
                billGiver.BillStack.AnyShouldDoNow)
                return false;

        return true;
    }

    public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
    {
        if (thing is not IBillGiver billGiver || !ThingIsUsableBillGiver(thing) || !billGiver.BillStack.AnyShouldDoNow ||
            !billGiver.UsableForBillsAfterFueling() || !pawn.CanReserve(thing, 1, -1, null, forced) ||
            thing.IsBurning() || thing.IsForbidden(pawn) ||
            (thing.def.hasInteractionCell && !pawn.CanReserveSittableOrSpot(thing.InteractionCell, forced)))
            return null;
        var compRefuelable = thing.TryGetComp<CompRefuelable>();
        if (compRefuelable is { HasFuel: false })
        {
            if (!RefuelWorkGiverUtility.CanRefuel(pawn, thing, forced)) return null;
            return RefuelWorkGiverUtility.RefuelJob(pawn, thing, forced);
        }

        billGiver.BillStack.RemoveIncompletableBills();
        return StartOrResumeBillJob(pawn, billGiver);
    }

    private static UnfinishedThing ClosestUnfinishedThingForBill(Pawn pawn, Bill_ProductionWithUft bill)
    {
        bool validator(Thing t)
        {
            return !t.IsForbidden(pawn) && ((UnfinishedThing)t).Recipe == bill.recipe &&
                   ((UnfinishedThing)t).Creator == pawn &&
                   ((UnfinishedThing)t).ingredients.TrueForAll(x => bill.IsFixedOrAllowedIngredient(x.def)) &&
                   pawn.CanReserve(t);
        }

        return (UnfinishedThing)GenClosest.ClosestThingReachable(
            pawn.Position, pawn.Map, ThingRequest.ForDef(bill.recipe.unfinishedThingDef), PathEndMode.InteractionCell,
            TraverseParms.For(pawn, pawn.NormalMaxDanger()), 9999f, validator
        );
    }

    private static Job FinishUftJob(Pawn pawn, UnfinishedThing uft, Bill_ProductionWithUft bill)
    {
        if (uft.Creator != pawn)
        {
            Log.Error(
                string.Concat(
                    "Tried to get FinishUftJob for ", pawn, " finishing ", uft, " but its creator is ", uft.Creator
                )
            );
            return null;
        }

        var job = WorkGiverUtility.HaulStuffOffBillGiverJob(pawn, bill.billStack.billGiver, uft);
        if (job != null && job.targetA.Thing != uft) return job;
        var job2 = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("PW_CraftPokemon"), (Thing)bill.billStack.billGiver);
        job2.bill = bill;
        job2.targetQueueB = [uft];
        job2.countQueue = [1];
        job2.haulMode = HaulMode.ToCellNonStorage;
        return job2;
    }

    private Job StartOrResumeBillJob(Pawn pawn, IBillGiver giver)
    {
        foreach (var bill in giver.BillStack)
        {
            if ((bill.recipe.requiredGiverWorkType != null && bill.recipe.requiredGiverWorkType != def.workType) ||
                (Find.TickManager.TicksGame <= bill.nextTickToSearchForIngredients &&
                 FloatMenuMakerMap.makingFor != pawn) || !bill.ShouldDoNow() ||
                !bill.PawnAllowedToStartAnew(pawn)) continue;
            if (!bill.ShouldDoNow() || !bill.PawnAllowedToStartAnew(pawn)) continue;
            var skillRequirement = bill.recipe.FirstSkillRequirementPawnDoesntSatisfy(pawn);
            if (skillRequirement != null)
            {
                JobFailReason.Is("UnderRequiredSkill".Translate(skillRequirement.minLevel), bill.Label);
                continue;
            }

            if (bill is Bill_Medical medical && medical.IsSurgeryViolationOnExtraFactionMember(pawn))
            {
                JobFailReason.Is("SurgeryViolationFellowFactionMember".Translate());
                continue;
            }

            if (bill is Bill_ProductionWithUft bill_ProductionWithUft)
            {
                if (bill_ProductionWithUft.BoundUft != null)
                {
                    if (bill_ProductionWithUft.BoundWorker == pawn &&
                        pawn.CanReserveAndReach(bill_ProductionWithUft.BoundUft, PathEndMode.Touch, Danger.Deadly) &&
                        !bill_ProductionWithUft.BoundUft.IsForbidden(pawn))
                        return FinishUftJob(pawn, bill_ProductionWithUft.BoundUft, bill_ProductionWithUft);
                    continue;
                }

                var unfinishedThing = ClosestUnfinishedThingForBill(pawn, bill_ProductionWithUft);
                if (unfinishedThing != null) return FinishUftJob(pawn, unfinishedThing, bill_ProductionWithUft);
            }

            if (!TryFindBestBillIngredients(bill, pawn, (Thing)giver, chosenIngThings))
            {
                if (FloatMenuMakerMap.makingFor != pawn)
                    bill.nextTickToSearchForIngredients =
                        Find.TickManager.TicksGame + ReCheckFailedBillTicksRange.RandomInRange;
                else
                    JobFailReason.Is(MissingMaterialsTranslated, bill.Label);
                chosenIngThings.Clear();
                continue;
            }

            var result = TryStartNewCraftPokemonJob(pawn, bill, giver, chosenIngThings, out _);
            chosenIngThings.Clear();
            return result;
        }

        chosenIngThings.Clear();
        return null;
    }

    public static Job TryStartNewCraftPokemonJob(
        Pawn pawn, Bill bill, IBillGiver giver, List<ThingCount> chosenIngThings, out Job haulOffJob,
        bool dontCreateJobIfHaulOffRequired = true
    )
    {
        haulOffJob = WorkGiverUtility.HaulStuffOffBillGiverJob(pawn, giver, null);
        if (haulOffJob != null && dontCreateJobIfHaulOffRequired) return haulOffJob;
        var job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("PW_CraftPokemon"), (Thing)giver);
        job.targetQueueB = new List<LocalTargetInfo>(chosenIngThings.Count);
        job.countQueue = new List<int>(chosenIngThings.Count);
        foreach (var t in chosenIngThings)
        {
            job.targetQueueB.Add(t.Thing);
            job.countQueue.Add(t.Count);
        }

        job.haulMode = HaulMode.ToCellNonStorage;
        job.bill = bill;
        return job;
    }

    public bool ThingIsUsableBillGiver(Thing thing)
    {
        var corpse = thing as Corpse;
        Pawn pawn2 = null;
        if (corpse != null) pawn2 = corpse.InnerPawn;
        if (def.fixedBillGiverDefs != null && def.fixedBillGiverDefs.Contains(thing.def)) return true;
        if (thing is Pawn pawn)
        {
            if (def.billGiversAllHumanlikes && pawn.RaceProps.Humanlike) return true;
            if (def.billGiversAllMechanoids && pawn.RaceProps.IsMechanoid) return true;
            if (def.billGiversAllAnimals && pawn.RaceProps.Animal) return true;
        }

        if (corpse == null || pawn2 == null) return false;
        if (def.billGiversAllHumanlikesCorpses && pawn2.RaceProps.Humanlike) return true;
        if (def.billGiversAllMechanoidsCorpses && pawn2.RaceProps.IsMechanoid) return true;
        if (def.billGiversAllAnimalsCorpses && pawn2.RaceProps.Animal) return true;

        return false;
    }

    public static bool TryFindBestFixedIngredients(
        List<IngredientCount> ingredients, Pawn pawn, Thing ingredientDestination, List<ThingCount> chosen,
        float searchRadius = 999f
    )
    {
        return TryFindBestIngredientsHelper(
            t => ingredients.Any(ingNeed => ingNeed.filter.Allows(t)),
            foundThings => TryFindBestIngredientsInSet_NoMixHelper(
                foundThings, ingredients, chosen, GetBillGiverRootCell(ingredientDestination, pawn), false
            ), ingredients, pawn, ingredientDestination, chosen, searchRadius
        );
    }

    private static bool TryFindBestBillIngredients(Bill bill, Pawn pawn, Thing billGiver, List<ThingCount> chosen)
    {
        return TryFindBestIngredientsHelper(
            t => bill.IsFixedOrAllowedIngredient(t) && bill.recipe.ingredients.Any(ingNeed => ingNeed.filter.Allows(t)),
            foundThings => TryFindBestBillIngredientsInSet(
                foundThings, bill, chosen, GetBillGiverRootCell(billGiver, pawn), billGiver is Pawn
            ), bill.recipe.ingredients, pawn, billGiver, chosen, bill.ingredientSearchRadius
        );
    }

    private static bool TryFindBestIngredientsHelper(
        Predicate<Thing> thingValidator, Predicate<List<Thing>> foundAllIngredientsAndChoose,
        List<IngredientCount> ingredients, Pawn pawn, Thing billGiver, List<ThingCount> chosen, float searchRadius
    )
    {
        chosen.Clear();
        newRelevantThings.Clear();
        if (ingredients.Count == 0) return true;
        var billGiverRootCell = GetBillGiverRootCell(billGiver, pawn);
        var rootReg = billGiverRootCell.GetRegion(pawn.Map);
        if (rootReg == null) return false;
        relevantThings.Clear();
        processedThings.Clear();
        var foundAll = false;
        var radiusSq = searchRadius * searchRadius;

        bool baseValidator(Thing t)
        {
            return t.Spawned && !t.IsForbidden(pawn) &&
                   (t.Position - billGiver.Position).LengthHorizontalSquared < radiusSq && thingValidator(t) &&
                   pawn.CanReserve(t);
        }

        var billGiverIsPawn = billGiver is Pawn;
        if (billGiverIsPawn)
        {
            AddEveryMedicineToRelevantThings(pawn, billGiver, relevantThings, baseValidator, pawn.Map);
            if (foundAllIngredientsAndChoose(relevantThings))
            {
                relevantThings.Clear();
                return true;
            }
        }

        var traverseParams = TraverseParms.For(pawn);
        RegionEntryPredicate entryCondition = null;
        if (Math.Abs(999f - searchRadius) >= 1f)
            entryCondition = delegate(Region from, Region r)
            {
                if (!r.Allows(traverseParams, false)) return false;
                var extentsClose = r.extentsClose;
                var num = Math.Abs(
                    billGiver.Position.x - Math.Max(
                        extentsClose.minX, Math.Min(billGiver.Position.x, extentsClose.maxX)
                    )
                );
                if (num > searchRadius) return false;
                var num2 = Math.Abs(
                    billGiver.Position.z - Math.Max(
                        extentsClose.minZ, Math.Min(billGiver.Position.z, extentsClose.maxZ)
                    )
                );
                return !(num2 > searchRadius) && num * num + num2 * num2 <= radiusSq;
            };
        else
            entryCondition = (from, r) => r.Allows(traverseParams, false);
        var adjacentRegionsAvailable = rootReg.Neighbors.Count(region => entryCondition(rootReg, region));
        var regionsProcessed = 0;
        processedThings.AddRange(relevantThings);

        bool regionProcessor(Region r)
        {
            var list = r.ListerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.HaulableEver));
            foreach (var thing in list)
            {
                if (processedThings.Contains(thing) ||
                    !ReachabilityWithinRegion.ThingFromRegionListerReachable(
                        thing, r, PathEndMode.ClosestTouch, pawn
                    ) ||
                    !baseValidator(thing) || thing.def.IsMedicine && billGiverIsPawn) continue;
                newRelevantThings.Add(thing);
                processedThings.Add(thing);
            }

            regionsProcessed++;
            if (newRelevantThings.Count <= 0 || regionsProcessed <= adjacentRegionsAvailable) return false;
            relevantThings.AddRange(newRelevantThings);
            newRelevantThings.Clear();
            if (!foundAllIngredientsAndChoose(relevantThings)) return false;
            foundAll = true;
            return true;

        }

        RegionTraverser.BreadthFirstTraverse(rootReg, entryCondition, regionProcessor, 99999);
        relevantThings.Clear();
        newRelevantThings.Clear();
        processedThings.Clear();
        return foundAll;
    }

    private static IntVec3 GetBillGiverRootCell(Thing billGiver, Pawn forPawn)
    {
        if (billGiver is not Building building) return billGiver.Position;
        if (building.def.hasInteractionCell) return building.InteractionCell;
        Log.Error(
            string.Concat("Tried to find bill ingredients for ", billGiver, " which has no interaction cell.")
        );
        return forPawn.Position;
    }

    private static void AddEveryMedicineToRelevantThings(
        Pawn pawn, Thing billGiver, List<Thing> relevantThings, Predicate<Thing> baseValidator, Map map
    )
    {
        var medicalCareCategory = GetMedicalCareCategory(billGiver);
        var list = map.listerThings.ThingsInGroup(ThingRequestGroup.Medicine);
        tmpMedicine.Clear();
        foreach (var thing in list)
        {
            if (medicalCareCategory.AllowsMedicine(thing.def) && baseValidator(thing) &&
                pawn.CanReach(thing, PathEndMode.OnCell, Danger.Deadly)) tmpMedicine.Add(thing);
        }

        tmpMedicine.SortBy(
            x => 0f - x.GetStatValue(StatDefOf.MedicalPotency), x => x.Position.DistanceToSquared(billGiver.Position)
        );
        relevantThings.AddRange(tmpMedicine);
        tmpMedicine.Clear();
    }

    private static MedicalCareCategory GetMedicalCareCategory(Thing billGiver)
    {
        if (billGiver is Pawn { playerSettings: not null } pawn)
            return pawn.playerSettings.medCare;
        return MedicalCareCategory.Best;
    }

    private static bool TryFindBestBillIngredientsInSet(
        List<Thing> availableThings, Bill bill, List<ThingCount> chosen, IntVec3 rootCell, bool alreadySorted
    )
    {
        if (bill.recipe.allowMixingIngredients)
            return TryFindBestBillIngredientsInSet_AllowMix(availableThings, bill, chosen, rootCell);
        return TryFindBestBillIngredientsInSet_NoMix(availableThings, bill, chosen, rootCell, alreadySorted);
    }

    private static bool TryFindBestBillIngredientsInSet_NoMix(
        List<Thing> availableThings, Bill bill, List<ThingCount> chosen, IntVec3 rootCell, bool alreadySorted
    )
    {
        return TryFindBestIngredientsInSet_NoMixHelper(
            availableThings, bill.recipe.ingredients, chosen, rootCell, alreadySorted, bill
        );
    }

    private static bool TryFindBestIngredientsInSet_NoMixHelper(
        List<Thing> availableThings, List<IngredientCount> ingredients, List<ThingCount> chosen, IntVec3 rootCell,
        bool alreadySorted, Bill bill = null
    )
    {
        if (!alreadySorted)
        {
            int comparison(Thing t1, Thing t2)
            {
                float num4 = (t1.Position - rootCell).LengthHorizontalSquared;
                float value = (t2.Position - rootCell).LengthHorizontalSquared;
                return num4.CompareTo(value);
            }

            availableThings.Sort(comparison);
        }

        chosen.Clear();
        availableCounts.Clear();
        availableCounts.GenerateFrom(availableThings);
        foreach (var ingredientCount in ingredients)
        {
            var flag = false;
            for (var j = 0; j < availableCounts.Count; j++)
            {
                var num = bill != null
                    ? ingredientCount.CountRequiredOfFor(availableCounts.GetDef(j), bill.recipe)
                    : ingredientCount.GetBaseCount();
                if ((bill != null && !bill.recipe.ignoreIngredientCountTakeEntireStacks &&
                     num > availableCounts.GetCount(j)) || !ingredientCount.filter.Allows(availableCounts.GetDef(j)) ||
                    (bill != null && !ingredientCount.IsFixedIngredient &&
                     !bill.ingredientFilter.Allows(availableCounts.GetDef(j)))) continue;
                foreach (var t in availableThings)
                {
                    if (t.def != availableCounts.GetDef(j)) continue;
                    var num2 = t.stackCount - ThingCountUtility.CountOf(chosen, t);
                    if (num2 <= 0) continue;
                    if (bill != null && bill.recipe.ignoreIngredientCountTakeEntireStacks)
                    {
                        ThingCountUtility.AddToList(chosen, t, num2);
                        return true;
                    }

                    var num3 = Mathf.Min(Mathf.FloorToInt(num), num2);
                    ThingCountUtility.AddToList(chosen, t, num3);
                    num -= num3;
                    if (!(num < 0.001f)) continue;
                    flag = true;
                    var count = availableCounts.GetCount(j);
                    count -= num;
                    availableCounts.SetCount(j, count);
                    break;
                }

                if (flag) break;
            }

            if (!flag) return false;
        }

        return true;
    }

    private static bool TryFindBestBillIngredientsInSet_AllowMix(
        List<Thing> availableThings, Bill bill, List<ThingCount> chosen, IntVec3 rootCell
    )
    {
        chosen.Clear();
        availableThings.SortBy(
            t => bill.recipe.IngredientValueGetter.ValuePerUnitOf(t.def),
            t => (t.Position - rootCell).LengthHorizontalSquared
        );
        foreach (var ingredientCount in bill.recipe.ingredients)
        {
            var num = ingredientCount.GetBaseCount();
            foreach (var thing in availableThings)
            {
                if (!ingredientCount.filter.Allows(thing) ||
                    (!ingredientCount.IsFixedIngredient && !bill.ingredientFilter.Allows(thing))) continue;
                var num2 = bill.recipe.IngredientValueGetter.ValuePerUnitOf(thing.def);
                var num3 = Mathf.Min(Mathf.CeilToInt(num / num2), thing.stackCount);
                ThingCountUtility.AddToList(chosen, thing, num3);
                num -= num3 * num2;
                if (num <= 0.0001f) break;
            }

            if (num > 0.0001f) return false;
        }

        return true;
    }

    private class DefCountList
    {
        private readonly List<float> counts = new();
        private readonly List<ThingDef> defs = new();

        public int Count => defs.Count;

        public float this[ThingDef def]
        {
            get
            {
                var num = defs.IndexOf(def);
                return num < 0 ? 0f : counts[num];
            }
            set
            {
                var num = defs.IndexOf(def);
                if (num < 0)
                {
                    defs.Add(def);
                    counts.Add(value);
                    num = defs.Count - 1;
                }
                else
                {
                    counts[num] = value;
                }

                CheckRemove(num);
            }
        }

        public float GetCount(int index)
        {
            return counts[index];
        }

        public void SetCount(int index, float val)
        {
            counts[index] = val;
            CheckRemove(index);
        }

        public ThingDef GetDef(int index)
        {
            return defs[index];
        }

        private void CheckRemove(int index)
        {
            if (counts[index] != 0f) return;
            counts.RemoveAt(index);
            defs.RemoveAt(index);
        }

        public void Clear()
        {
            defs.Clear();
            counts.Clear();
        }

        public void GenerateFrom(List<Thing> things)
        {
            Clear();
            foreach (var t in things)
                this[t.def] += t.stackCount;
        }
    }
}
