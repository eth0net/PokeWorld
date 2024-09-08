using System.Collections.Generic;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace PokeWorld;

internal class JobDriver_PokemonGotoForced : JobDriver
{
    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        pawn.Map.pawnDestinationReservationManager.Reserve(pawn, job, job.targetA.Cell);
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        var toil = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
        toil.AddPreTickAction(
            delegate
            {
                if (job.exitMapOnArrival && pawn.Map.exitMapGrid.IsExitCell(pawn.Position)) TryExitMap();
            }
        );
        toil.FailOn(
            () => job.failIfCantJoinOrCreateCaravan && !CaravanExitMapUtility.CanExitMapAndJoinOrCreateCaravanNow(pawn)
        );
        yield return toil;
        var toil2 = new Toil
        {
            initAction = delegate
            {
                if (pawn.mindState != null && pawn.mindState.forcedGotoPosition == TargetA.Cell)
                    pawn.mindState.forcedGotoPosition = IntVec3.Invalid;
                if (job.exitMapOnArrival &&
                    (pawn.Position.OnEdge(pawn.Map) || pawn.Map.exitMapGrid.IsExitCell(pawn.Position))) TryExitMap();
            },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
        yield return toil2;
        var toil3 = new Toil
        {
            initAction = delegate
            {
                if (pawn.jobs.jobQueue.Count == 0)
                {
                    var job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("PW_PokemonWaitCombat"));
                    job.expiryInterval = 0;
                    pawn.jobs.TryTakeOrderedJob(job);
                }

                EndJobWith(JobCondition.Succeeded);
            }
        };
        yield return toil3;
    }

    private void TryExitMap()
    {
        if (!job.failIfCantJoinOrCreateCaravan || CaravanExitMapUtility.CanExitMapAndJoinOrCreateCaravanNow(pawn))
            pawn.ExitMap(true, CellRect.WholeMap(Map).GetClosestEdge(pawn.Position));
    }
}
