using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using RimWorld;
using Verse;

namespace PokeWorld.Harmony_Patching;

[HarmonyPatch(typeof(PawnUtility))]
[HarmonyPatch(nameof(PawnUtility.GetManhunterOnDamageChance))]
[HarmonyPatch([typeof(Pawn), typeof(float), typeof(Thing)])]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
internal class PawnUtility_GetManhunterOnDamageChance_Patch
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static void Postfix(Pawn __0, float __1, Thing __2, ref float __result)
    {
        if (__2 == null) return;
        var instigator = __2 as Pawn;
        var instigatorComp = instigator.TryGetComp<CompPokemon>();
        if (instigatorComp == null) return;
        var targetComp = __0.TryGetComp<CompPokemon>();
        if (targetComp != null)
            __result *= GenMath.LerpDoubleClamped(-10f, 10f, 1f, 3f,
                targetComp.levelTracker.level - instigatorComp.levelTracker.level);
    }
}
