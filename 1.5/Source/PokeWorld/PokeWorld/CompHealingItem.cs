using System;
using Verse;

namespace PokeWorld
{
    class CompHealingItem : ThingComp
    {

        public CompProperties_HealingItem Props => (CompProperties_HealingItem)this.props;
        public float HealingAmount => Props.healingAmount;
    }

    class CompProperties_HealingItem : CompProperties
    {
        public float healingAmount = 20;
        public CompProperties_HealingItem()
        {
            this.compClass = typeof(CompHealingItem);
        }

        public CompProperties_HealingItem(Type compClass) : base(compClass)
        {
            this.compClass = compClass;
        }
    }
}
