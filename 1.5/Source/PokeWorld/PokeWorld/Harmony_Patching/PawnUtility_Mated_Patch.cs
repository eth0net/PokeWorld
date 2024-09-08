using HarmonyLib;
using RimWorld;
using Verse;

namespace PokeWorld;

[HarmonyPatch(typeof(PawnUtility))]
[HarmonyPatch("Mated")]
internal class PawnUtility_Mated_Patch
{
    public static bool Prefix(Pawn __0, Pawn __1)
    {
        var comp = __1.TryGetComp<CompPokemon>();
        if (comp == null || !comp.EggGroups.Contains(DefDatabase<EggGroupDef>.GetNamed("Ditto"))) return true;
        var compDittoEggLayer = __1.TryGetComp<CompDittoEggLayer>();
        if (compDittoEggLayer != null && !__0.health.hediffSet.HasHediff(HediffDefOf.Sterilized) &&
            !__1.health.hediffSet.HasHediff(HediffDefOf.Sterilized)) compDittoEggLayer.Fertilize(__0);
        return false;
    }
}
