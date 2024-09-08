using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace PokeWorld;

public static class PokemonMeleeUtility
{
    public static Action GetPokemonMeleeAttackAction(Pawn pawn, LocalTargetInfo target, out string failStr)
    {
        failStr = "";
        if (target.IsValid && !pawn.CanReach(target, PathEndMode.Touch, Danger.Deadly))
        {
            failStr = "NoPath".Translate();
        }
        else if (pawn.meleeVerbs.TryGetMeleeVerb(target.Thing) == null)
        {
            failStr = "Incapable".Translate();
        }
        else if (pawn == target.Thing)
        {
            failStr = "CannotAttackSelf".Translate();
        }
        else if (pawn.playerSettings.Master.Position.DistanceTo(target.Cell) > 20)
        {
            failStr = "PW_WarningTargetTooFarFromMaster".Translate();
        }
        else
        {
            Pawn target2;
            if ((target2 = target.Thing as Pawn) == null ||
                (!pawn.InSameExtraFaction(target2, ExtraFactionType.HomeFaction) &&
                 !pawn.InSameExtraFaction(target2, ExtraFactionType.MiniFaction)))
                return delegate
                {
                    var job = JobMaker.MakeJob(JobDefOf.AttackMelee, target);
                    var pawn2 = target.Thing as Pawn;
                    if (pawn2 != null) job.killIncappedTarget = pawn2.Downed;
                    pawn.jobs.TryTakeOrderedJob(job);
                };
            failStr = "CannotAttackSameFactionMember".Translate();
        }

        failStr = failStr.CapitalizeFirst();
        return null;
    }
}
