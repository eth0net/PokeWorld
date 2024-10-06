using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace PokeWorld;

[HarmonyPatch(typeof(JobGiver_Manhunter))]
[HarmonyPatch("TryGiveJob")]
internal class JobGiver_Manhunter_TryGiveJob_Patch
{
    public static readonly IntRange ExpiryInterval_ShooterSucceeded = new(450, 550);

    private static readonly IntRange ExpiryInterval_Melee = new(360, 480);

    private static readonly float targetKeepRadius = 65f;

    public static bool Prefix(Pawn __0, ref Job __result)
    {
        var comp = __0.TryGetComp<CompPokemon>();
        if (comp == null) return true;
        UpdateEnemyTarget(__0);
        var enemyTarget = __0.mindState.enemyTarget;
        if (enemyTarget == null)
        {
            __result = null;
            return false;
        }

        if (enemyTarget is Pawn pawn2 && pawn2.IsPsychologicallyInvisible())
        {
            __result = null;
            return false;
        }

        var verb = __0.TryGetAttackVerb(enemyTarget);
        if (verb == null)
        {
            __result = null;
            return false;
        }

        if (verb.tool != null)
        {
            __result = MeleeAttackJob(enemyTarget);
            return false;
        }

        var num = CoverUtility.CalculateOverallBlockChance(__0, enemyTarget.Position, __0.Map) > 0.01f;
        var flag = __0.Position.Standable(__0.Map) &&
                   __0.Map.pawnDestinationReservationManager.CanReserve(__0.Position, __0);
        var flag2 = verb.CanHitTarget(enemyTarget);
        var flag3 = (__0.Position - enemyTarget.Position).LengthHorizontalSquared < 25;
        if ((num && flag && flag2) || (flag3 && flag2))
        {
            __result = JobMaker.MakeJob(
                DefDatabase<JobDef>.GetNamed("PW_PokemonWaitCombat"), ExpiryInterval_ShooterSucceeded.RandomInRange,
                true
            );
            return false;
        }

        if (!TryFindShootingPosition(__0, out var dest))
        {
            __result = null;
            return false;
        }

        if (dest == __0.Position)
        {
            __result = JobMaker.MakeJob(
                DefDatabase<JobDef>.GetNamed("PW_PokemonWaitCombat"), ExpiryInterval_ShooterSucceeded.RandomInRange,
                true
            );
            return false;
        }

        var job = JobMaker.MakeJob(JobDefOf.Goto, dest);
        job.expiryInterval = ExpiryInterval_ShooterSucceeded.RandomInRange;
        job.checkOverrideOnExpire = true;
        __result = job;
        return false;
    }

    private static bool TryFindShootingPosition(Pawn pawn, out IntVec3 dest)
    {
        var enemyTarget = pawn.mindState.enemyTarget;
        var verb = pawn.TryGetAttackVerb(enemyTarget);
        if (verb == null)
        {
            dest = IntVec3.Invalid;
            return false;
        }

        CastPositionRequest newReq = default;
        newReq.caster = pawn;
        newReq.target = enemyTarget;
        newReq.verb = verb;
        newReq.maxRangeFromTarget = verb.verbProps.range;
        newReq.wantCoverFromTarget = false;
        return CastPositionFinder.TryFindCastPosition(newReq, out dest);
    }

    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Need to match the target method")]
    private static bool ExtraTargetValidator(Pawn pawn, Thing target)
    {
        return true;
    }

    private static Job MeleeAttackJob(Thing enemyTarget)
    {
        var job = JobMaker.MakeJob(JobDefOf.AttackMelee, enemyTarget);
        job.expiryInterval = ExpiryInterval_Melee.RandomInRange;
        job.checkOverrideOnExpire = true;
        job.expireRequiresEnemiesNearby = true;
        return job;
    }

    private static void UpdateEnemyTarget(Pawn pawn)
    {
        var thing = pawn.mindState.enemyTarget;
        if (thing != null && (thing.Destroyed ||
                              Find.TickManager.TicksGame - pawn.mindState.lastEngageTargetTick > 400 ||
                              !pawn.CanReach(thing, PathEndMode.Touch, Danger.Deadly) ||
                              (pawn.Position - thing.Position).LengthHorizontalSquared >
                              targetKeepRadius * targetKeepRadius ||
                              ((IAttackTarget)thing).ThreatDisabled(pawn))) thing = null;
        if (thing == null)
        {
            thing = FindAttackTargetIfPossible(pawn);
            if (thing != null)
            {
                var methodNotify_EngagedTarget = pawn.mindState.GetType().GetMethod(
                    "Notify_EngagedTarget", BindingFlags.NonPublic | BindingFlags.Instance
                );
                methodNotify_EngagedTarget.Invoke(pawn.mindState, new object[] { });
                pawn.GetLord()?.Notify_PawnAcquiredTarget(pawn, thing);
            }
        }
        else
        {
            var thing2 = FindAttackTargetIfPossible(pawn);
            if (thing2 == null)
            {
                thing = null;
            }
            else if (thing2 != null && thing2 != thing)
            {
                var methodNotify_EngagedTarget = pawn.mindState.GetType().GetMethod(
                    "Notify_EngagedTarget", BindingFlags.NonPublic | BindingFlags.Instance
                );
                methodNotify_EngagedTarget.Invoke(pawn.mindState, new object[] { });
                thing = thing2;
            }
        }

        pawn.mindState.enemyTarget = thing;
        if (thing is Pawn && thing.Faction == Faction.OfPlayer && pawn.Position.InHorDistOf(thing.Position, 40f))
            Find.TickManager.slower.SignalForceNormalSpeed();
    }

    private static Thing FindAttackTargetIfPossible(Pawn pawn)
    {
        if (pawn.TryGetAttackVerb(null) == null) return null;
        return FindAttackTarget(pawn);
    }

    private static Thing FindAttackTarget(Pawn pawn)
    {
        var targetScanFlags = TargetScanFlags.NeedReachableIfCantHitFromMyPos | TargetScanFlags.NeedThreat |
                              TargetScanFlags.NeedAutoTargetable;
        return (Thing)AttackTargetFinder.BestAttackTarget(pawn, targetScanFlags, x => ExtraTargetValidator(pawn, x));
    }
}
