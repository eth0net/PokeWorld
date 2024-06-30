using HarmonyLib;
using Verse;

namespace PokeWorld.Harmony_Patching;

[HarmonyPatch(typeof(Pawn))]
[HarmonyPatch(nameof(Pawn.HealthScale))]
[HarmonyPatch(MethodType.Getter)]
public class Pawn_get_HealthScale_Patch
{
    public static void Postfix(Pawn __instance, ref float __result)
    {
        var comp = __instance?.TryGetComp<CompPokemon>();
        if (comp is { statTracker: not null }) __result *= comp.statTracker.HealthScaleMult;
    }
}
