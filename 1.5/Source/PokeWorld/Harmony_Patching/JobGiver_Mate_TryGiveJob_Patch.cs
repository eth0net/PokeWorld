using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace PokeWorld;

[HarmonyPatch(typeof(JobGiver_Mate))]
[HarmonyPatch("TryGiveJob")]
internal class JobGiver_Mate_TryGiveJob_Patch
{
    public static bool Prefix(Pawn __0, ref Job __result)
    {
        var comp = __0.TryGetComp<CompPokemon>();
        if (comp == null) return true;

        if ((__0.gender != Gender.Male && __0.TryGetComp<CompDittoEggLayer>() == null) ||
            !comp.friendshipTracker.CanMate())
        {
            __result = null;
            return false;
        }

        bool validator(Thing t)
        {
            var pawn3 = t as Pawn;
            if (pawn3.Downed) return false;
            if (!pawn3.CanCasuallyInteractNow() || pawn3.IsForbidden(__0)) return false;
            if (pawn3.Faction != __0.Faction) return false;
            var comp2 = pawn3.TryGetComp<CompPokemon>();
            if (comp2 == null) return false;
            return PokemonUtility.FertileMateTarget(__0, pawn3);
        }

        var pawn2 = (Pawn)GenClosest.ClosestThingReachable(
            __0.Position, __0.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.Touch,
            TraverseParms.For(__0), 30f, validator
        );
        if (pawn2 == null)
        {
            __result = null;
            return false;
        }

        __result = JobMaker.MakeJob(JobDefOf.Mate, pawn2);
        return false;
    }
}
