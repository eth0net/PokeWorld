using System.Linq;
using RimWorld;
using Verse;

namespace PokeWorld.Quests;

public class GenStep_LegendaryPokemon : GenStep_Scatterer
{
    public override int SeedPart => 860042045;

    public override bool CanScatterAt(IntVec3 c, Map map)
    {
        if (!base.CanScatterAt(c, map)) return false;
        if (!c.Standable(map)) return false;
        return !c.Roofed(map) && map.reachability.CanReachMapEdge(c, TraverseParms.For(TraverseMode.PassDoors));
    }

    public override void ScatterAt(IntVec3 loc, Map map, GenStepParams parms, int count = 1)
    {
        var pawn = (Pawn)parms.sitePart.things.Take(parms.sitePart.things.First());
        GenSpawn.Spawn(pawn, loc, map);
        pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);
        MapGenerator.rootsToUnfog.Add(loc);
        MapGenerator.SetVar("RectOfInterest", CellRect.CenteredOn(loc, 1, 1));
    }
}
