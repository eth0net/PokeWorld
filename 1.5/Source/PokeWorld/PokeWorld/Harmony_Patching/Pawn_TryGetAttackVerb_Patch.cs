using HarmonyLib;
using PokeWorld.Pokemon_Moves;
using Verse;

namespace PokeWorld.Harmony_Patching;

[HarmonyPatch(typeof(Pawn))]
[HarmonyPatch("TryGetAttackVerb")]
internal class Pawn_TryGetAttackVerb_Patch
{
    public static bool Prefix(Pawn __instance, ref Verb __result)
    {
        var comp = __instance.TryGetComp<CompPokemon>();
        if (comp == null || comp.moveTracker == null) return true;
        if (PokemonAttackGizmoUtility.CanUseAnyRangedVerb(__instance))
        {
            __result = PokemonAttackGizmoUtility.GetAnyRangedVerb(__instance);
            return false;
        }

        return true;
    }
}
