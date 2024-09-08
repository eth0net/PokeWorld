using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace PokeWorld;

public static class PokemonFloatMenuUtility
{
    public static Action GetRangedAttackAction(Pawn pawn, LocalTargetInfo target, out string failStr)
    {
        var comp = pawn.TryGetComp<CompPokemon>();
        failStr = "";
        if (comp == null || comp.moveTracker == null) return null;
        if (!PokemonAttackGizmoUtility.CanUseAnyRangedVerb(pawn)) return null;
        var longestRangeVerb = PokemonAttackGizmoUtility.GetLongestRangeVerb(pawn);
        if (longestRangeVerb == null) return null;

        if (target.IsValid && !longestRangeVerb.CanHitTarget(target))
        {
            if (!pawn.Position.InHorDistOf(target.Cell, longestRangeVerb.verbProps.range))
                failStr = "OutOfRange".Translate();
            var num = longestRangeVerb.verbProps.EffectiveMinRange(target, pawn);
            if (pawn.Position.DistanceToSquared(target.Cell) < num * num)
                failStr = "TooClose".Translate();
            else
                failStr = "CannotHitTarget".Translate();
        }
        else if (pawn == target.Thing)
        {
            failStr = "CannotAttackSelf".Translate();
        }
        else if (pawn.playerSettings.Master == null || pawn.playerSettings.Master.Map != pawn.Map)
        {
            failStr = "PW_WarningNoMaster".Translate();
        }
        else if (pawn.playerSettings.Master.Drafted == false)
        {
            failStr = "PW_WarningMasterNotDrafted".Translate();
        }
        else if (pawn.Position.DistanceTo(pawn.playerSettings.Master.Position) >
                 PokemonMasterUtility.GetMasterObedienceRadius(pawn))
        {
            failStr = "PW_WarningMasterTooFar".Translate();
        }
        else
        {
            if (!(target.Thing is Pawn target2) || (!pawn.InSameExtraFaction(target2, ExtraFactionType.HomeFaction) &&
                                                    !pawn.InSameExtraFaction(target2, ExtraFactionType.MiniFaction)))
                return delegate
                {
                    var job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("PW_PokemonAttackStatic"), target);
                    pawn.jobs.TryTakeOrderedJob(job);
                };
            failStr = "CannotAttackSameFactionMember".Translate();
        }

        failStr = failStr.CapitalizeFirst();
        return null;
    }

    public static Action GetMeleeAttackAction(Pawn pawn, LocalTargetInfo target, out string failStr)
    {
        var comp = pawn.TryGetComp<CompPokemon>();
        failStr = "";
        if (comp == null || comp.moveTracker == null) return null;

        if (target.IsValid && !pawn.CanReach(target, PathEndMode.Touch, Danger.Deadly))
        {
            failStr = "NoPath".Translate();
        }
        else if (!PokemonAttackGizmoUtility.CanUseAnyMeleeVerb(pawn))
        {
            failStr = "Incapable".Translate();
        }
        else if (pawn == target.Thing)
        {
            failStr = "CannotAttackSelf".Translate();
        }
        else if (pawn.playerSettings.Master == null || pawn.playerSettings.Master.Map != pawn.Map)
        {
            failStr = "PW_WarningNoMaster".Translate();
        }
        else if (pawn.playerSettings.Master.Drafted == false)
        {
            failStr = "PW_WarningMasterNotDrafted".Translate();
        }
        else if (pawn.Position.DistanceTo(pawn.playerSettings.Master.Position) >
                 PokemonMasterUtility.GetMasterObedienceRadius(pawn))
        {
            failStr = "PW_WarningMasterTooFar".Translate();
        }
        else if (target != null && target.Cell.DistanceTo(pawn.playerSettings.Master.Position) >
                 PokemonMasterUtility.GetMasterObedienceRadius(pawn))
        {
            failStr = "PW_WarningTargetTooFarFromMaster".Translate();
        }
        else
        {
            if (!(target.Thing is Pawn target2) || (!pawn.InSameExtraFaction(target2, ExtraFactionType.HomeFaction) &&
                                                    !pawn.InSameExtraFaction(target2, ExtraFactionType.MiniFaction)))
                return delegate
                {
                    var job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("PW_PokemonAttackMelee"), target);
                    if (target.Thing is Pawn pawn2) job.killIncappedTarget = pawn2.Downed;
                    pawn.jobs.TryTakeOrderedJob(job);
                };
            failStr = "CannotAttackSameFactionMember".Translate();
        }

        failStr = failStr.CapitalizeFirst();
        return null;
    }

    public static FloatMenuOption DecoratePrioritizedTask(
        FloatMenuOption option, Pawn pawn, LocalTargetInfo target, string reservedText = "ReservedBy"
    )
    {
        if (option.action == null) return option;
        if (pawn != null && !pawn.CanReserve(target) && pawn.CanReserve(target, ignoreOtherReservations:true))
        {
            var pawn2 = pawn.Map.reservationManager.FirstRespectedReserver(target, pawn) ??
                        pawn.Map.physicalInteractionReservationManager.FirstReserverOf(target);
            if (pawn2 != null) option.Label = option.Label + ": " + reservedText.Translate(pawn2.LabelShort, pawn2);
        }

        if (option.revalidateClickTarget != null && option.revalidateClickTarget != target.Thing)
            Log.ErrorOnce(
                $"Click target mismatch; {option.revalidateClickTarget} vs {target.Thing} in {option.Label}", 52753118
            );
        option.revalidateClickTarget = target.Thing;
        return option;
    }
}
