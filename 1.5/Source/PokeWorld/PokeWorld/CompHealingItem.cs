using System;
using Verse;

namespace PokeWorld;

internal class CompHealingItem : ThingComp
{
    public CompProperties_HealingItem Props => (CompProperties_HealingItem)props;
    public float HealingAmount => Props.healingAmount;
}

internal class CompProperties_HealingItem : CompProperties
{
    public float healingAmount = 20;

    public CompProperties_HealingItem()
    {
        compClass = typeof(CompHealingItem);
    }

    public CompProperties_HealingItem(Type compClass) : base(compClass)
    {
        this.compClass = compClass;
    }
}
