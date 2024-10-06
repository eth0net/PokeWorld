using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace PokeWorld;

public class JobDriver_PokemonWaitCombat : JobDriver
{
    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        var toil = new Toil
        {
            initAction = delegate
            {
                Map.pawnDestinationReservationManager.Reserve(pawn, job, pawn.Position);
                pawn.pather.StopDead();
                CheckForAutoAttack();
            },
            tickAction = delegate
            {
                if (job.expiryInterval == -1 && job.def == DefDatabase<JobDef>.GetNamed("PW_PokemonWaitCombat"))
                {
                    Log.Error(string.Concat(pawn, " in eternal WaitCombat"));
                    ReadyForNextToil();
                }

                if (pawn.Faction != null && pawn.Faction.IsPlayer && !PokemonMasterUtility.IsPokemonInMasterRange(pawn))
                    ReadyForNextToil();
                else if ((Find.TickManager.TicksGame + pawn.thingIDNumber) % 4 == 0) CheckForAutoAttack();
            }
        };
        DecorateWaitToil(toil);
        toil.defaultCompleteMode = ToilCompleteMode.Never;
        if (pawn.mindState != null && pawn.mindState.duty != null && pawn.mindState.duty.focus != null)
        {
            var focusLocal = pawn.mindState.duty.focus;
            toil.handlingFacing = false;
            toil.tickAction = (Action)Delegate.Combine(
                toil.tickAction, (Action)delegate { pawn.rotationTracker.FaceTarget(focusLocal); }
            );
        }
        else if (pawn.Faction != null && pawn.Faction.IsPlayer && PokemonMasterUtility.IsPokemonInMasterRange(pawn))
        {
            toil.handlingFacing = false;
            toil.tickAction = (Action)Delegate.Combine(
                toil.tickAction, (Action)delegate { pawn.Rotation = Rot4.South; }
            );
        }

        yield return toil;
    }

    public virtual void DecorateWaitToil(Toil wait)
    {
    }

    public override void Notify_StanceChanged()
    {
        if (pawn.stances.curStance is Stance_Mobile) CheckForAutoAttack();
    }

    private void CheckForAutoAttack()
    {
        if (this.pawn.Downed || this.pawn.stances.FullBodyBusy) return;
        collideWithPawns = false;
        Fire fire = null;
        for (var i = 0; i < 9; i++)
        {
            var c = this.pawn.Position + GenAdj.AdjacentCellsAndInside[i];
            if (!c.InBounds(this.pawn.Map)) continue;
            var thingList = c.GetThingList(Map);
            for (var j = 0; j < thingList.Count; j++)
            {
                if (thingList[j] is Pawn pawn && !pawn.Downed && this.pawn.HostileTo(pawn) &&
                    GenHostility.IsActiveThreatTo(pawn, this.pawn.Faction))
                {
                    this.pawn.meleeVerbs.TryMeleeAttack(pawn);
                    collideWithPawns = true;
                    return;
                }

                if (thingList[j] is Fire fire2 && (fire == null || fire2.fireSize < fire.fireSize || i == 8) &&
                    (fire2.parent == null || fire2.parent != this.pawn)) fire = fire2;
            }
        }

        if (fire != null && (!this.pawn.InMentalState || this.pawn.MentalState.def.allowBeatfire))
        {
            pawn.natives.TryBeatFire(fire);
        }
        else
        {
            var currentEffectiveVerb = pawn.CurrentEffectiveVerb;
            if (currentEffectiveVerb != null && !(currentEffectiveVerb.tool != null))
            {
                var targetScanFlags = TargetScanFlags.NeedLOSToAll | TargetScanFlags.NeedThreat |
                                      TargetScanFlags.NeedAutoTargetable;
                if (currentEffectiveVerb.IsIncendiary_Ranged()) targetScanFlags |= TargetScanFlags.NeedNonBurning;
                var thing = (Thing)AttackTargetFinder.BestShootTargetFromCurrentPosition(pawn, targetScanFlags);
                if (thing != null)
                {
                    pawn.TryStartAttack(thing);
                    collideWithPawns = true;
                }
            }
        }
    }
}
