using RimWorld;
using Verse;

namespace PokeWorld
{
    public static class PokemonUtility
    {
        public static bool FertileMateTarget(Pawn male, Pawn female)
        {
            CompPokemon compPokemonMale = male.TryGetComp<CompPokemon>();
            CompPokemon compPokemonFemale = female.TryGetComp<CompPokemon>();
            if (compPokemonMale == null || !compPokemonMale.friendshipTracker.CanMate() || compPokemonFemale == null || !compPokemonFemale.friendshipTracker.CanMate() || female.health.hediffSet.HasHediff(HediffDefOf.Sterilized))
            {
                return false;
            }
            EggGroupDef undiscovered = DefDatabase<EggGroupDef>.GetNamed("Undiscovered");
            EggGroupDef ditto = DefDatabase<EggGroupDef>.GetNamed("Ditto");
            if (compPokemonMale.EggGroups.Contains(undiscovered) || compPokemonFemale.EggGroups.Contains(undiscovered))
            {
                return false;
            }
            if (compPokemonMale.EggGroups.Contains(ditto) && !compPokemonFemale.EggGroups.Contains(ditto) && female.gender != Gender.Male)
            {
                CompEggLayer compEggLayer = female.TryGetComp<CompEggLayer>();
                if (compEggLayer != null)
                {
                    return !compEggLayer.FullyFertilized;
                }
            }
            else if (compPokemonFemale.EggGroups.Contains(ditto) && !compPokemonMale.EggGroups.Contains(ditto))
            {
                CompDittoEggLayer compDittoEggLayer = female.TryGetComp<CompDittoEggLayer>();
                if (compDittoEggLayer != null)
                {
                    return !compDittoEggLayer.FullyFertilized;
                }
            }
            if (female.gender != Gender.Female)
            {
                return false;
            }
            foreach (EggGroupDef def in compPokemonMale.EggGroups)
            {
                if (compPokemonFemale.EggGroups.Contains(def))
                {
                    CompEggLayer compEggLayer = female.TryGetComp<CompEggLayer>();
                    if (compEggLayer != null)
                    {
                        return !compEggLayer.FullyFertilized;
                    }
                }
            }
            return false;
        }
    }
}
