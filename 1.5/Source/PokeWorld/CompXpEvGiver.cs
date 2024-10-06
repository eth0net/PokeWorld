using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace PokeWorld;

internal class CompXpEvGiver : ThingComp
{
    private readonly int maxCount = 8;
    private int expToGive;
    private List<Pawn> giveTo;

    private int lastHitTime = -1;

    public override void Initialize(CompProperties props)
    {
        base.Initialize(props);
        giveTo = new List<Pawn>();
    }

    public override void CompTickRare()
    {
        if (giveTo.Count > 0 && GenTicks.TicksAbs - lastHitTime > 60000) giveTo.Clear();
    }

    public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
    {
        base.PostPreApplyDamage(ref dinfo, out absorbed);
        if (lastHitTime == -1) expToGive = GetExperienceYield();
    }

    public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
    {
        base.PostPostApplyDamage(dinfo, totalDamageDealt);
        lastHitTime = GenTicks.TicksAbs;
        var pawn = parent as Pawn;
        if (dinfo.Instigator is Pawn instigator)
        {
            var instigatorComp = instigator.TryGetComp<CompPokemon>();
            if (instigatorComp != null && instigator.Faction == Faction.OfPlayer && parent.Faction != Faction.OfPlayer)
                if (pawn == null || !pawn.Downed || pawn.Dead || parent.Destroyed)
                    if (!giveTo.Contains(instigator) && giveTo.Count < maxCount && instigator != parent)
                        giveTo.Add(instigator);
        }

        if ((pawn != null && pawn.Dead) || parent.Destroyed) DistributeXPandEV();
    }

    private int GetExperienceYield()
    {
        return (int)parent.GetStatValue(DefDatabase<StatDef>.GetNamed("PW_BaseXPYield"));
    }

    private void FilterDeadAndDownedFromGiveTo()
    {
        giveTo = giveTo.Where(pawn => pawn != null && !pawn.Dead && !pawn.Downed).ToList();
    }

    private void DistributeXPandEV()
    {
        FilterDeadAndDownedFromGiveTo();
        var ownComp = parent.TryGetComp<CompPokemon>();
        foreach (var pawn in giveTo)
        {
            var ennemyComp = pawn.TryGetComp<CompPokemon>();
            if (ennemyComp != null)
            {
                ennemyComp.levelTracker.IncreaseExperience(expToGive / giveTo.Count);
                if (ownComp != null)
                    foreach (var EV in ownComp.EVYields)
                        ennemyComp.statTracker.IncreaseEV(EV.stat, EV.value);
            }
        }
    }

    public void DistributeAfterCatch()
    {
        DistributeXPandEV();
    }

    public override void PostExposeData()
    {
        Scribe_Values.Look(ref lastHitTime, "lastHitTime", -1);
        Scribe_Values.Look(ref expToGive, "expToGive");
        Scribe_Collections.Look(ref giveTo, "giveTo", LookMode.Reference);
    }
}
