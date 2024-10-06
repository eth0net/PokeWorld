using RimWorld;
using Verse;

namespace PokeWorld;

public static class PokemonUtility
{
    public static bool FertileMateTarget(Pawn male, Pawn female)
    {
        var compPokemonMale = male.TryGetComp<CompPokemon>();
        var compPokemonFemale = female.TryGetComp<CompPokemon>();
        if (compPokemonMale == null || !compPokemonMale.friendshipTracker.CanMate() || compPokemonFemale == null ||
            !compPokemonFemale.friendshipTracker.CanMate() ||
            female.health.hediffSet.HasHediff(HediffDefOf.Sterilized)) return false;
        var undiscovered = DefDatabase<EggGroupDef>.GetNamed("Undiscovered");
        var ditto = DefDatabase<EggGroupDef>.GetNamed("Ditto");
        if (compPokemonMale.EggGroups.Contains(undiscovered) ||
            compPokemonFemale.EggGroups.Contains(undiscovered)) return false;
        if (compPokemonMale.EggGroups.Contains(ditto) && !compPokemonFemale.EggGroups.Contains(ditto) &&
            female.gender != Gender.Male)
        {
            var compEggLayer = female.TryGetComp<CompEggLayer>();
            if (compEggLayer != null) return !compEggLayer.FullyFertilized;
        }
        else if (compPokemonFemale.EggGroups.Contains(ditto) && !compPokemonMale.EggGroups.Contains(ditto))
        {
            var compDittoEggLayer = female.TryGetComp<CompDittoEggLayer>();
            if (compDittoEggLayer != null) return !compDittoEggLayer.FullyFertilized;
        }

        if (female.gender != Gender.Female) return false;
        foreach (var def in compPokemonMale.EggGroups)
            if (compPokemonFemale.EggGroups.Contains(def))
            {
                var compEggLayer = female.TryGetComp<CompEggLayer>();
                if (compEggLayer != null) return !compEggLayer.FullyFertilized;
            }

        return false;
    }
}
