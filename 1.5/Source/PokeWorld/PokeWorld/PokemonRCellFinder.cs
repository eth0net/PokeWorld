using System;
using Verse;
using Verse.AI;

namespace PokeWorld;

public static class PokemonRCellFinder
{
    public static IntVec3 BestOrderedGotoDestNear(IntVec3 root, Pawn searcher, Predicate<IntVec3> cellValidator = null)
    {
        var map = searcher.Map;

        bool predicate(IntVec3 c)
        {
            if (cellValidator != null && !cellValidator(c)) return false;
            if (!map.pawnDestinationReservationManager.CanReserve(c, searcher) || !c.Standable(map) ||
                !searcher.CanReach(c, PathEndMode.OnCell, Danger.Deadly)) return false;
            var thingList = c.GetThingList(map);
            for (var i = 0; i < thingList.Count; i++)
                if (thingList[i] is Pawn pawn && pawn != searcher && pawn.RaceProps.Humanlike &&
                    ((searcher.Faction.IsPlayer && pawn.Faction == searcher.Faction) ||
                     (!searcher.Faction.IsPlayer && !pawn.Faction.IsPlayer)))
                    return false;
            return true;
        }

        if (predicate(root)) return root;
        var num = 1;
        IntVec3 result = default;
        var num2 = -1000f;
        var flag = false;
        var num3 = GenRadial.NumCellsInRadius(30f);
        while (true)
        {
            var intVec = root + GenRadial.RadialPattern[num];
            if (predicate(intVec))
            {
                var num4 = CoverUtility.TotalSurroundingCoverScore(intVec, map);
                if (num4 > num2)
                {
                    num2 = num4;
                    result = intVec;
                    flag = true;
                }
            }

            if (num >= 8 && flag) break;
            num++;
            if (num >= num3) return searcher.Position;
        }

        return result;
    }
}
