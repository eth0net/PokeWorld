using HarmonyLib;
using RimWorld;
using Verse;

namespace PokeWorld;

[HarmonyPatch(typeof(StockGenerator_Animals))]
[HarmonyPatch("PawnKindAllowed")]
internal class StockGenerator_Animals_PawnKindAllowed_Patch
{
    public static bool Prefix(PawnKindDef __0, ref bool __result)
    {
        if (PokeWorldSettings.MinSelected() && __0.race.HasComp(typeof(CompPokemon)))
        {
            __result = false;
            return false;
        }

        if (PokeWorldSettings.MaxSelected() && !__0.race.HasComp(typeof(CompPokemon)))
        {
            __result = false;
            return false;
        }

        if (__0.race.HasComp(typeof(CompPokemon)) &&
            !PokeWorldSettings.GenerationAllowed(__0.race.GetCompProperties<CompProperties_Pokemon>().generation))
        {
            __result = false;
            return false;
        }

        return true;
    }
}
