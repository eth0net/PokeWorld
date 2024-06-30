using HarmonyLib;
using Verse;

namespace PokeWorld.Harmony_Patching;

[HarmonyPatch(typeof(Pawn))]
[HarmonyPatch("get_HealthScale")]
public class Pawn_get_HealthScale_Patch
{
    public static void Postfix(Pawn __instance, ref float __result)
    {
        if (__instance != null)
        {
            var comp = __instance.TryGetComp<CompPokemon>();
            if (comp != null && comp.statTracker != null) __result *= comp.statTracker.HealthScaleMult;
        }
    }
}
