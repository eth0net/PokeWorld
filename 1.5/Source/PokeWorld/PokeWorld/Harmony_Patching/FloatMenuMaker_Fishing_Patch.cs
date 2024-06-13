using HarmonyLib;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace PokeWorld
{
    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch(nameof(Pawn.GetExtraFloatMenuOptionsFor))]
    class Pawn_GetExtraFloatMenuOptionsFor_Patch
    {
        public static void Postfix(Pawn __instance, IntVec3 __0, ref IEnumerable<FloatMenuOption> __result)
        {
            if (__instance.Drafted)
            {
                return;
            }
            Thing fishingRod = null;
            foreach (Thing thing in __instance.EquippedWornOrInventoryThings)
            {
                if (thing.TryGetComp<CompFishingRod>() != null)
                {
                    fishingRod = thing;
                    break;
                }
            }
            /*if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            {

            }*/

            if (fishingRod == null)
            {
                return;
            }
            TerrainDef targetTerrain = __0.GetTerrain(__instance.Map);
            if (!FishingUtility.IsFishingTerrain(targetTerrain))
            {
                return;
            }
            if (!ReachabilityUtility.CanReach(__instance, __0, PathEndMode.Touch, Danger.Unspecified))
            {
                __result = __result.AddItem(new FloatMenuOption("Cannot reach fishing spot", null));
                return;
            }
            else
            {
                Action action = GetFishingAction(__instance, __0, fishingRod);
                __result = __result.AddItem(new FloatMenuOption($"Fish here ({targetTerrain.label})", action));
            }

        }

        private static Action GetFishingAction(Pawn pawn, IntVec3 targetTerrain, Thing fishingRod)
        {
            void action()
            {
                Job job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("PW_Fish"), targetTerrain);
                job.targetB = fishingRod;
                pawn.jobs.TryTakeOrderedJob(job, JobTag.MiscWork);
            }
            return action;
        }
    }
}