using System;
using Verse;

namespace PokeWorld;

internal class CompPokeball : ThingComp
{
    public CompProperties_Pokeball Props => (CompProperties_Pokeball)props;
    public ThingDef ballDef => Props.ballDef;
}

public class CompProperties_Pokeball : CompProperties
{
    public ThingDef ballDef;

    public CompProperties_Pokeball()
    {
        compClass = typeof(CompPokeball);
    }

    public CompProperties_Pokeball(Type compClass) : base(compClass)
    {
        this.compClass = compClass;
    }
}
