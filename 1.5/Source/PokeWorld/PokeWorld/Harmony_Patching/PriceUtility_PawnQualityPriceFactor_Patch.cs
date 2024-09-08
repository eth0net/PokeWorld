using System.Text;
using HarmonyLib;
using RimWorld;
using Verse;

namespace PokeWorld;

[HarmonyPatch(typeof(PriceUtility))]
[HarmonyPatch("PawnQualityPriceFactor")]
internal class PriceUtility_PawnQualityPriceFactor_Patch
{
    public static void Postfix(Pawn __0, StringBuilder __1, ref float __result)
    {
        var comp = __0.TryGetComp<CompPokemon>();
        if (comp != null)
        {
            __1?.AppendLine("PW_Pokemon".Translate());
            var levelFactor = comp.levelTracker.level / 20f;
            __1?.AppendLine(
                "   " + "PW_PriceFactorLevel".Translate(comp.levelTracker.level, levelFactor.ToStringPercent())
                    .ToLower().CapitalizeFirst()
            );
            __result *= levelFactor;
            if (comp.shinyTracker.isShiny)
            {
                float shinyFactor = 4;
                __1?.AppendLine(
                    "   " + "PW_PriceFactorShiny".Translate(shinyFactor.ToStringPercent()).ToLower().CapitalizeFirst()
                );
                __result *= shinyFactor;
            }
        }
    }
}
