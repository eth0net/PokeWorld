using System.Collections.Generic;
using System.Linq;
using PokeWorld.ModSetting;
using RimWorld;
using Verse;

namespace PokeWorld;

//Surgery recipe, not crafting recipe
public class Recipe_EvolutionItem : Recipe_Surgery
{
    public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients,
        Bill bill)
    {
        if (billDoer != null) pawn.GetComp<CompPokemon>().levelTracker.TryEvolveWithItem(ingredients[0]);
    }

    public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
    {
        if (thing is not Pawn pawn) return false;
        var comp = pawn.TryGetComp<CompPokemon>();
        if (comp?.evolutions == null) return false;
        return Enumerable.Any(comp.evolutions,
            evo => PokeWorldSettings.GenerationAllowed(evo.pawnKind.race.GetCompProperties<CompProperties_Pokemon>()
                .generation));
    }
}
