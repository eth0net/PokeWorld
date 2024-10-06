using Verse;

namespace PokeWorld;

public static class PutInBallUtility
{
    public static void UpdatePutInBallDesignation(ThingWithComps t)
    {
        var comp = t.TryGetComp<CompPokemon>();
        var designation = t.Map.designationManager.DesignationOn(
            t, DefDatabase<DesignationDef>.GetNamed("PW_PutInBall")
        );
        if (comp != null && designation == null)
        {
            comp.wantPutInBall = true;
            t.Map.designationManager.AddDesignation(
                new Designation(t, DefDatabase<DesignationDef>.GetNamed("PW_PutInBall"))
            );
        }
        else if (comp != null)
        {
            comp.wantPutInBall = false;
            designation?.Delete();
        }
    }

    public static void PutPokemonInBall(Pawn pokemon)
    {
        var comp = pokemon.TryGetComp<CompPokemon>();
        if (comp != null)
        {
            comp.wantPutInBall = false;
            if (comp.levelTracker.flagIsEvolving) comp.levelTracker.CancelEvolution();
            if (pokemon.carryTracker != null && pokemon.carryTracker.CarriedThing != null)
                pokemon.carryTracker.TryDropCarriedThing(pokemon.Position, ThingPlaceMode.Near, out var droppedThing);
            if (pokemon.inventory != null) pokemon.inventory.DropAllNearPawn(pokemon.Position);
            var pos = pokemon.Position;
            var map = pokemon.Map;
            pokemon.DeSpawn();
            comp.inBall = true;
            var thing = ThingMaker.MakeThing(comp.ballDef);
            var ball = thing as CryptosleepBall;
            ball.stackCount = 1;
            ball.TryAcceptThing(pokemon);
            GenPlace.TryPlaceThing(ball, pos, map, ThingPlaceMode.Near);
        }
    }

    public static void PutCorpseInBall(Corpse corpse, ThingDef ballDef)
    {
        var pos = corpse.Position;
        var map = corpse.Map;
        corpse.DeSpawn();
        var thing = ThingMaker.MakeThing(ballDef);
        var ball = thing as CryptosleepBall;
        ball.stackCount = 1;
        ball.TryAcceptThing(corpse);
        GenPlace.TryPlaceThing(ball, pos, map, ThingPlaceMode.Near);
    }
}
