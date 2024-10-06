using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace PokeWorld;

internal class DamageWorker_PokemonSmash : DamageWorker_PokemonMeleeMove
{
    protected override BodyPartRecord ChooseHitPart(DamageInfo dinfo, Pawn pawn)
    {
        return pawn.health.hediffSet.GetRandomNotMissingPart(dinfo.Def, dinfo.Height, BodyPartDepth.Outside);
    }

    protected override void ApplySpecialEffectsToPart(
        Pawn pawn, float totalDamage, DamageInfo dinfo, DamageResult result
    )
    {
        var flag = Rand.Chance(def.bluntInnerHitChance);
        var num = flag ? def.bluntInnerHitDamageFractionToConvert.RandomInRange : 0f;
        var num2 = totalDamage * (1f - num);
        var lastInfo = dinfo;
        while (true)
        {
            num2 -= FinalizeAndAddInjury(pawn, num2, lastInfo, result);
            if (!pawn.health.hediffSet.PartIsMissing(lastInfo.HitPart) || num2 <= 1f) break;
            var parent = lastInfo.HitPart.parent;
            if (parent == null) break;
            lastInfo.SetHitPart(parent);
        }

        if (flag && !lastInfo.HitPart.def.IsSolid(lastInfo.HitPart, pawn.health.hediffSet.hediffs) &&
            lastInfo.HitPart.depth == BodyPartDepth.Outside && (from x in pawn.health.hediffSet.GetNotMissingParts()
                where x.parent == lastInfo.HitPart && x.def.IsSolid(x, pawn.health.hediffSet.hediffs) &&
                      x.depth == BodyPartDepth.Inside
                select x).TryRandomElementByWeight(x => x.coverageAbs, out var result2))
        {
            var dinfo2 = lastInfo;
            dinfo2.SetHitPart(result2);
            var totalDamage2 = totalDamage * num + totalDamage * def.bluntInnerHitDamageFractionToAdd.RandomInRange;
            FinalizeAndAddInjury(pawn, totalDamage2, dinfo2, result);
        }

        if (pawn.Dead) return;
        SimpleCurve simpleCurve = null;
        if (lastInfo.HitPart.parent == null)
            simpleCurve = def.bluntStunChancePerDamagePctOfCorePartToBodyCurve;
        else
            foreach (var item in pawn.RaceProps.body.GetPartsWithTag(BodyPartTagDefOf.ConsciousnessSource))
                if (InSameBranch(item, lastInfo.HitPart))
                {
                    simpleCurve = def.bluntStunChancePerDamagePctOfCorePartToHeadCurve;
                    break;
                }

        if (simpleCurve != null)
        {
            var x2 = totalDamage / pawn.def.race.body.corePart.def.GetMaxHealth(pawn);
            if (Rand.Chance(simpleCurve.Evaluate(x2)))
            {
                var dinfo3 = dinfo;
                dinfo3.Def = DamageDefOf.Stun;
                dinfo3.SetAmount(def.bluntStunDuration.SecondsToTicks() / 30f);
                pawn.TakeDamage(dinfo3);
            }
        }
    }

    [DebugOutput]
    public static void StunChances()
    {
        var bluntBodyStunChance = delegate(ThingDef d, float dam, bool onHead)
        {
            var obj = onHead
                ? DamageDefOf.Blunt.bluntStunChancePerDamagePctOfCorePartToHeadCurve
                : DamageDefOf.Blunt.bluntStunChancePerDamagePctOfCorePartToBodyCurve;
            var pawn2 = PawnGenerator.GeneratePawn(
                new PawnGenerationRequest(
                    d.race.AnyPawnKind, Find.FactionManager.FirstFactionOfDef(d.race.AnyPawnKind.defaultFactionType),
                    PawnGenerationContext.NonPlayer, -1, true
                )
            );
            var x = dam / d.race.body.corePart.def.GetMaxHealth(pawn2);
            Find.WorldPawns.PassToWorld(pawn2, PawnDiscardDecideMode.Discard);
            return Mathf.Clamp01(obj.Evaluate(x)).ToStringPercent();
        };
        var list = new List<TableDataGetter<ThingDef>>();
        list.Add(new TableDataGetter<ThingDef>("defName", d => d.defName));
        list.Add(new TableDataGetter<ThingDef>("body size", d => d.race.baseBodySize.ToString("F2")));
        list.Add(new TableDataGetter<ThingDef>("health scale", d => d.race.baseHealthScale.ToString("F2")));
        list.Add(
            new TableDataGetter<ThingDef>(
                "body size\n* health scale", d => (d.race.baseHealthScale * d.race.baseBodySize).ToString("F2")
            )
        );
        list.Add(
            new TableDataGetter<ThingDef>(
                "core part\nhealth", delegate(ThingDef d)
                {
                    var pawn = PawnGenerator.GeneratePawn(
                        new PawnGenerationRequest(
                            d.race.AnyPawnKind,
                            Find.FactionManager.FirstFactionOfDef(d.race.AnyPawnKind.defaultFactionType),
                            PawnGenerationContext.NonPlayer, -1, true
                        )
                    );
                    var maxHealth = d.race.body.corePart.def.GetMaxHealth(pawn);
                    Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
                    return maxHealth;
                }
            )
        );
        list.Add(new TableDataGetter<ThingDef>("stun\nchance\nbody\n5", d => bluntBodyStunChance(d, 5f, false)));
        list.Add(new TableDataGetter<ThingDef>("stun\nchance\nbody\n10", d => bluntBodyStunChance(d, 10f, false)));
        list.Add(new TableDataGetter<ThingDef>("stun\nchance\nbody\n15", d => bluntBodyStunChance(d, 15f, false)));
        list.Add(new TableDataGetter<ThingDef>("stun\nchance\nbody\n20", d => bluntBodyStunChance(d, 20f, false)));
        list.Add(new TableDataGetter<ThingDef>("stun\nchance\nhead\n5", d => bluntBodyStunChance(d, 5f, true)));
        list.Add(new TableDataGetter<ThingDef>("stun\nchance\nhead\n10", d => bluntBodyStunChance(d, 10f, true)));
        list.Add(new TableDataGetter<ThingDef>("stun\nchance\nhead\n15", d => bluntBodyStunChance(d, 15f, true)));
        list.Add(new TableDataGetter<ThingDef>("stun\nchance\nhead\n20", d => bluntBodyStunChance(d, 20f, true)));
        DebugTables.MakeTablesDialog(
            DefDatabase<ThingDef>.AllDefs.Where(d => d.category == ThingCategory.Pawn), list.ToArray()
        );
    }

    private bool InSameBranch(BodyPartRecord lhs, BodyPartRecord rhs)
    {
        while (lhs.parent != null && lhs.parent.parent != null) lhs = lhs.parent;
        while (rhs.parent != null && rhs.parent.parent != null) rhs = rhs.parent;
        return lhs == rhs;
    }
}
