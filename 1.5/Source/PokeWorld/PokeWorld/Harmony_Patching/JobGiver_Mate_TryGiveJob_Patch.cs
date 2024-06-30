using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using PokeWorld.Eggs;
using RimWorld;
using Verse;
using Verse.AI;

namespace PokeWorld.Harmony_Patching;

[HarmonyPatch(typeof(JobGiver_Mate))]
[HarmonyPatch(nameof(JobGiver_Mate.TryGiveJob))]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
internal class JobGiver_Mate_TryGiveJob_Patch
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
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

        var pawn2 = (Pawn)GenClosest.ClosestThingReachable(__0.Position, __0.Map,
            ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.Touch, TraverseParms.For(__0), 30f,
            Validator);
        if (pawn2 == null)
        {
            __result = null;
            return false;
        }

        __result = JobMaker.MakeJob(JobDefOf.Mate, pawn2);
        return false;

        bool Validator(Thing t)
        {
            if (t is not Pawn pawn3) return false;
            if (pawn3.Downed) return false;
            if (!pawn3.CanCasuallyInteractNow() || pawn3.IsForbidden(__0)) return false;
            if (pawn3.Faction != __0.Faction) return false;
            var comp2 = pawn3.TryGetComp<CompPokemon>();
            return comp2 != null && PokemonUtility.FertileMateTarget(__0, pawn3);
        }
    }
}
