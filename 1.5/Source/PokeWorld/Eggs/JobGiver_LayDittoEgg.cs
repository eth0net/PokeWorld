using RimWorld;
using Verse;
using Verse.AI;

namespace PokeWorld;

public class JobGiver_LayDittoEgg : ThinkNode_JobGiver
{
    private const float LayRadius = 5f;

    protected override Job TryGiveJob(Pawn pawn)
    {
        var compDittoEggLayer = pawn.TryGetComp<CompDittoEggLayer>();
        if (compDittoEggLayer == null || !compDittoEggLayer.CanLayNow) return null;
        var intVec = RCellFinder.RandomWanderDestFor(pawn, pawn.Position, LayRadius, null, Danger.Some);
        return JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("PW_LayDittoEgg"), intVec);
    }
}
