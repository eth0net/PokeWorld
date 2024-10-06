using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace PokeWorld;

public class StockGenerator_PokemonEgg : StockGenerator
{
    private readonly ThingCategoryDef categoryDef = null;

    private readonly List<ThingCategoryDef> excludedCategories = new();

    private readonly List<ThingDef> excludedThingDefs = new();

    private IntRange thingDefCountRange = IntRange.one;

    public override IEnumerable<Thing> GenerateThings(int forTile, Faction faction = null)
    {
        var generatedDefs = new List<ThingDef>();
        var numThingDefsToUse = thingDefCountRange.RandomInRange;
        for (var i = 0; i < numThingDefsToUse; i++)
        {
            if (!categoryDef.DescendantThingDefs.Where(
                    t => t.tradeability.TraderCanSell() &&
                         PokeWorldSettings.GenerationAllowed(
                             t.GetCompProperties<CompProperties_PokemonEggHatcher>().hatcherPawn.race
                                 .GetCompProperties<CompProperties_Pokemon>().generation
                         ) && (int)t.techLevel <= (int)maxTechLevelGenerate && !generatedDefs.Contains(t) &&
                         (excludedThingDefs == null || !excludedThingDefs.Contains(t)) &&
                         (excludedCategories == null || !excludedCategories.Any(c => c.DescendantThingDefs.Contains(t)))
                ).TryRandomElement(out var chosenThingDef)) break;
            foreach (var item in StockGeneratorUtility.TryMakeForStock(
                         chosenThingDef, RandomCountOf(chosenThingDef), faction
                     )) yield return item;
            generatedDefs.Add(chosenThingDef);
            chosenThingDef = null;
        }
    }

    public override bool HandlesThingDef(ThingDef t)
    {
        if (categoryDef.DescendantThingDefs.Contains(t) && t.HasComp(typeof(CompPokemonEggHatcher)) &&
            PokeWorldSettings.GenerationAllowed(
                t.GetCompProperties<CompProperties_PokemonEggHatcher>().hatcherPawn.race
                    .GetCompProperties<CompProperties_Pokemon>().generation
            ) && t.tradeability != 0 && (int)t.techLevel <= (int)maxTechLevelBuy &&
            (excludedThingDefs == null || !excludedThingDefs.Contains(t)))
        {
            if (excludedCategories != null) return !excludedCategories.Any(c => c.DescendantThingDefs.Contains(t));
            return true;
        }

        return false;
    }
}
