using Verse;

namespace PokeWorld
{
    public class CompPokFossilsEvoStoneDropper : ThingComp
    {
        public CompProperties_PokFossilsEvoStoneDropper Props => (CompProperties_PokFossilsEvoStoneDropper)props;
        public float stoneDropRate => Props.stoneDropRate;
        public float fossilDropRate => Props.fossilDropRate;
    }

    public class CompProperties_PokFossilsEvoStoneDropper : CompProperties
    {
        public float fossilDropRate;
        public float stoneDropRate;

        public CompProperties_PokFossilsEvoStoneDropper()
        {
            compClass = typeof(CompPokFossilsEvoStoneDropper);
        }
    }
}
