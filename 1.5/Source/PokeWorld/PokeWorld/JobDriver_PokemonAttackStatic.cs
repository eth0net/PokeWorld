using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace PokeWorld;

public class JobDriver_PokemonAttackStatic : JobDriver
{
    private int numAttacksMade;
    private bool startedIncapacitated;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref startedIncapacitated, "startedIncapacitated");
        Scribe_Values.Look(ref numAttacksMade, "numAttacksMade");
    }

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        yield return Toils_Misc.ThrowColonistAttackingMote(TargetIndex.A);
        var init = new Toil
        {
            initAction = delegate
            {
                if (TargetThingA is Pawn pawn2) startedIncapacitated = pawn2.Downed;
                pawn.pather.StopDead();
            },
            tickAction = delegate
            {
                if (!TargetA.IsValid) EndJobWith(JobCondition.Succeeded);
                if (pawn.Faction != null && pawn.Faction.IsPlayer && !PokemonMasterUtility.IsPokemonInMasterRange(pawn))
                {
                    EndJobWith(JobCondition.Succeeded);
                }
                else
                {
                    if (TargetA.HasThing)
                    {
                        var pawn3 = TargetA.Thing as Pawn;
                        if (TargetA.Thing.Destroyed || (pawn3 != null && !startedIncapacitated && pawn3.Downed) ||
                            (pawn3 != null && pawn3.IsPsychologicallyInvisible()))
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
                    }

                    if (numAttacksMade >= job.maxNumStaticAttacks && !pawn.stances.FullBodyBusy)
                    {
                        EndJobWith(JobCondition.Succeeded);
                    }
                    else if (pawn.TryStartAttack(TargetA))
                    {
                        numAttacksMade++;
                    }
                    else if (!pawn.stances.FullBodyBusy)
                    {
                        var verb = pawn.TryGetAttackVerb(TargetA.Thing, !pawn.IsColonist);
                        if (job.endIfCantShootTargetFromCurPos &&
                            (verb == null || !verb.CanHitTargetFrom(pawn.Position, TargetA)))
                        {
                            EndJobWith(JobCondition.Incompletable);
                        }
                        else if (job.endIfCantShootInMelee)
                        {
                            if (verb == null)
                            {
                                EndJobWith(JobCondition.Incompletable);
                            }
                            else
                            {
                                var num = verb.verbProps.EffectiveMinRange(TargetA, pawn);
                                if (pawn.Position.DistanceToSquared(TargetA.Cell) < num * num &&
                                    pawn.Position.AdjacentTo8WayOrInside(TargetA.Cell))
                                    EndJobWith(JobCondition.Incompletable);
                            }
                        }
                    }
                }
            },
            defaultCompleteMode = ToilCompleteMode.Never
        };
        init.activeSkill = () => Toils_Combat.GetActiveSkillForToil(init);
        yield return init;
    }
}
