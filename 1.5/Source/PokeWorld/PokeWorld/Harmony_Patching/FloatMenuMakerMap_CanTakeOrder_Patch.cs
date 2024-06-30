using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using RimWorld;
using Verse;

namespace PokeWorld.Harmony_Patching;

[HarmonyPatch(typeof(FloatMenuMakerMap))]
[HarmonyPatch(nameof(FloatMenuMakerMap.CanTakeOrder))]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
internal class FloatMenuMakerMap_CanTakeOrder_Patch
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static void Postfix(Pawn __0, ref bool __result)
    {
        if (__result == false && __0.Spawned && __0.Faction == Faction.OfPlayer &&
            __0.TryGetComp<CompPokemon>() != null && __0.MentalStateDef == null && __0.playerSettings != null &&
            __0.playerSettings.Master != null && __0.playerSettings.Master.Drafted) __result = true;
    }
}
