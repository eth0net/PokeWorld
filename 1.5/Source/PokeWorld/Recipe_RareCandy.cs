using System.Collections.Generic;
using RimWorld;
using Verse;

namespace PokeWorld;

public class Recipe_RareCandy : Recipe_Surgery
{
    //Surgery recipe, not crafting recipe
    public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
    {
        if (billDoer != null)
        {
            var comp = pawn.TryGetComp<CompPokemon>();
            if (comp != null && comp.levelTracker != null)
                for (var i = 0; i < bill.recipe.ingredients[0].GetBaseCount(); i++)
                    comp.levelTracker.IncreaseExperience(
                        comp.levelTracker.totalExpForNextLevel - comp.levelTracker.experience
                    );
        }
    }
}
