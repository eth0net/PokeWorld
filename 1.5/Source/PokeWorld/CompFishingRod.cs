using System;
using Verse;

namespace PokeWorld;

internal class CompFishingRod : ThingComp
{
}

public class CompProperties_FishingRod : CompProperties
{
    public CompProperties_FishingRod()
    {
        compClass = typeof(CompFishingRod);
    }

    public CompProperties_FishingRod(Type compClass) : base(compClass)
    {
        this.compClass = compClass;
    }
}
