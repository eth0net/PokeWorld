using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace PokeWorld;

public class JobDriver_PokemonAttackMelee : JobDriver
{
    private int numMeleeAttacksMade;
    private bool startedIncapacitated;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref startedIncapacitated, "startedIncapacitated");
        Scribe_Values.Look(ref numMeleeAttacksMade, "numMeleeAttacksMade");
    }

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        if (job.targetA.Thing is IAttackTarget attackTarget)
            pawn.Map.attackTargetReservationManager.Reserve(pawn, job, attackTarget);
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        yield return Toils_General.DoAtomic(
            delegate
            {
                if (job.targetA.Thing is Pawn pawn)
                {
                    if (pawn.Downed && this.pawn.mindState.duty != null &&
                        this.pawn.mindState.duty.attackDownedIfStarving &&
                        this.pawn.Starving()) job.killIncappedTarget = true;
                    startedIncapacitated = pawn.Downed;
                }
            }
        );
        yield return Toils_Misc.ThrowColonistAttackingMote(TargetIndex.A);
        yield return Toils_Combat.FollowAndMeleeAttack(
            TargetIndex.A, delegate
            {
                var thing = job.GetTarget(TargetIndex.A).Thing;
                if (job.reactingToMeleeThreat && thing is Pawn p && !p.Awake())
                    EndJobWith(JobCondition.InterruptForced);
                if (pawn.Faction != null && pawn.Faction.IsPlayer && !PokemonMasterUtility.IsPokemonInMasterRange(pawn))
                {
                    EndJobWith(JobCondition.Succeeded);
                }
                else if (pawn.meleeVerbs.TryMeleeAttack(thing, job.verbToUse) && pawn.CurJob != null &&
                         pawn.jobs.curDriver == this)
                {
                    numMeleeAttacksMade++;
                    if (TargetA.Thing.Destroyed || ((p = thing as Pawn) != null && !startedIncapacitated && p.Downed) ||
                        (p != null && p.IsPsychologicallyInvisible()))
                    {
                        EndJobWith(JobCondition.Succeeded);
                        if (pawn.Faction != null && pawn.Faction.IsPlayer &&
                            PokemonMasterUtility.IsPokemonInMasterRange(pawn))
                            if (pawn.jobs.jobQueue.Count == 0)
                            {
                                var job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("PW_PokemonWaitCombat"));
                                job.expiryInterval = 0;
                                pawn.jobs.TryTakeOrderedJob(job);
                            }

                        return;
                    }

                    if (numMeleeAttacksMade >= job.maxNumMeleeAttacks) EndJobWith(JobCondition.Succeeded);
                }
            }
        ).FailOnDespawnedOrNull(TargetIndex.A);
    }

    public override void Notify_PatherFailed()
    {
        if (job.attackDoorIfTargetLost)
        {
            Thing thing;
            using (var pawnPath = Map.pathFinder.FindPath(
                       pawn.Position, TargetA.Cell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassDoors)
                   ))
            {
                if (!pawnPath.Found) return;
                thing = pawnPath.FirstBlockingBuilding(out var _, pawn);
            }

            if (thing != null && thing.Position.InHorDistOf(pawn.Position, 6f))
            {
                job.targetA = thing;
                job.maxNumMeleeAttacks = Rand.RangeInclusive(2, 5);
                job.expiryInterval = Rand.Range(2000, 4000);
                return;
            }
        }

        base.Notify_PatherFailed();
    }

    public override bool IsContinuation(Job j)
    {
        return job.GetTarget(TargetIndex.A) == j.GetTarget(TargetIndex.A);
    }
}
