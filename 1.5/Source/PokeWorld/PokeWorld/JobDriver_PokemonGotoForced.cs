﻿using RimWorld.Planet;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace PokeWorld
{
    class JobDriver_PokemonGotoForced : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            pawn.Map.pawnDestinationReservationManager.Reserve(pawn, job, job.targetA.Cell);
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil toil = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
            toil.AddPreTickAction(delegate
            {
                if (job.exitMapOnArrival && pawn.Map.exitMapGrid.IsExitCell(pawn.Position))
                {
                    TryExitMap();
                }
            });
            toil.FailOn(() => job.failIfCantJoinOrCreateCaravan && !CaravanExitMapUtility.CanExitMapAndJoinOrCreateCaravanNow(pawn));
            yield return toil;
            Toil toil2 = new Toil
            {
                initAction = delegate
                {
                    if (pawn.mindState != null && pawn.mindState.forcedGotoPosition == base.TargetA.Cell)
                    {
                        pawn.mindState.forcedGotoPosition = IntVec3.Invalid;
                    }
                    if (job.exitMapOnArrival && (pawn.Position.OnEdge(pawn.Map) || pawn.Map.exitMapGrid.IsExitCell(pawn.Position)))
                    {
                        TryExitMap();
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield return toil2;
            Toil toil3 = new Toil
            {
                initAction = delegate
                {
                    if (pawn.jobs.jobQueue.Count == 0)
                    {
                        Job job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("PW_PokemonWaitCombat"));
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
            {
                pawn.ExitMap(allowedToJoinOrCreateCaravan: true, CellRect.WholeMap(base.Map).GetClosestEdge(pawn.Position));
            }
        }
    }
}
