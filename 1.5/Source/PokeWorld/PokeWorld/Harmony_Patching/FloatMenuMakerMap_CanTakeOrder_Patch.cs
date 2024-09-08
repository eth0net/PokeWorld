using HarmonyLib;
using RimWorld;
using Verse;

namespace PokeWorld;

[HarmonyPatch(typeof(FloatMenuMakerMap))]
[HarmonyPatch("CanTakeOrder")]
internal class FloatMenuMakerMap_CanTakeOrder_Patch
{
    public static void Postfix(Pawn __0, ref bool __result)
    {
        if (__result == false && __0.Spawned && __0.Faction == Faction.OfPlayer &&
            __0.TryGetComp<CompPokemon>() != null && __0.MentalStateDef == null && __0.playerSettings != null &&
            __0.playerSettings.Master != null && __0.playerSettings.Master.Drafted) __result = true;
    }
}
