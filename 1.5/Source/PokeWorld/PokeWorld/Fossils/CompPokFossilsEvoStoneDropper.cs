using Verse;

namespace PokeWorld
{
    public class CompPokFossilsEvoStoneDropper : ThingComp
    {
        public CompProperties_PokFossilsEvoStoneDropper Props => (CompProperties_PokFossilsEvoStoneDropper)this.props;

        public float StoneDropRate => Props.stoneDropRate;

        public float FossilDropRate => Props.fossilDropRate;
    }

    public class CompProperties_PokFossilsEvoStoneDropper : CompProperties
    {
        public float stoneDropRate;
        
        public float fossilDropRate;
        
        public CompProperties_PokFossilsEvoStoneDropper()
        {
            this.compClass = typeof(CompPokFossilsEvoStoneDropper);
        }
    }
}