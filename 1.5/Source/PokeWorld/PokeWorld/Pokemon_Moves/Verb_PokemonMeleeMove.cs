using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace PokeWorld;

public class Verb_PokemonMeleeMove : Verb_MeleeAttack
{
    public override bool Available()
    {
        var comp = ((Pawn)caster).TryGetComp<CompPokemon>();
        if (comp != null)
        {
            var moveDef = comp.moveTracker.unlockableMoves.Keys.Where(x => x.tool == tool).First();
            return PokemonAttackGizmoUtility.ShouldUseMove((Pawn)caster, moveDef);
        }

        return false;
    }

    private IEnumerable<DamageInfo> DamageInfosToApply(LocalTargetInfo target)
    {
        var num = verbProps.AdjustedMeleeDamageAmount(this, CasterPawn);
        var armorPenetration = verbProps.AdjustedArmorPenetration(this, CasterPawn);
        var def = verbProps.meleeDamageDef;
        BodyPartGroupDef bodyPartGroupDef = null;
        HediffDef hediffDef = null;

        if (CasterIsPawn)
        {
            bodyPartGroupDef = verbProps.AdjustedLinkedBodyPartsGroup(tool);
            if (num >= 1f)
            {
                if (HediffCompSource != null) hediffDef = HediffCompSource.Def;
            }
            else
            {
                num = 1f;
                def = DamageDefOf.Blunt;
            }
        }

        var source = EquipmentSource == null ? CasterPawn.def : EquipmentSource.def;
        var direction = (target.Thing.Position - CasterPawn.Position).ToVector3();
        var damageInfo = new DamageInfo(def, num, armorPenetration, -1f, caster, null, source);
        damageInfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
        damageInfo.SetWeaponBodyPartGroup(bodyPartGroupDef);
        damageInfo.SetWeaponHediff(hediffDef);
        damageInfo.SetAngle(direction);
        yield return damageInfo;
        if (tool != null && tool.extraMeleeDamages != null)
            foreach (var extraMeleeDamage in tool.extraMeleeDamages)
                if (Rand.Chance(extraMeleeDamage.chance))
                {
                    num = extraMeleeDamage.amount;
                    num = Rand.Range(num * 0.8f, num * 1.2f);
                    damageInfo = new DamageInfo(
                        extraMeleeDamage.def, num, extraMeleeDamage.AdjustedArmorPenetration(this, CasterPawn), -1f,
                        caster, null, source
                    );
                    damageInfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
                    damageInfo.SetWeaponBodyPartGroup(bodyPartGroupDef);
                    damageInfo.SetWeaponHediff(hediffDef);
                    damageInfo.SetAngle(direction);
                    yield return damageInfo;
                }

        if (!surpriseAttack ||
            ((verbProps.surpriseAttack == null || verbProps.surpriseAttack.extraMeleeDamages.NullOrEmpty()) &&
             (tool == null || tool.surpriseAttack == null ||
              tool.surpriseAttack.extraMeleeDamages.NullOrEmpty()))) yield break;
        var enumerable = Enumerable.Empty<ExtraDamage>();
        if (verbProps.surpriseAttack != null && verbProps.surpriseAttack.extraMeleeDamages != null)
            enumerable = enumerable.Concat(verbProps.surpriseAttack.extraMeleeDamages);
        if (tool != null && tool.surpriseAttack != null && !tool.surpriseAttack.extraMeleeDamages.NullOrEmpty())
            enumerable = enumerable.Concat(tool.surpriseAttack.extraMeleeDamages);
        foreach (var item in enumerable)
        {
            var num2 = GenMath.RoundRandom(item.AdjustedDamageAmount(this, CasterPawn));
            var armorPenetration2 = item.AdjustedArmorPenetration(this, CasterPawn);
            var damageInfo2 = new DamageInfo(item.def, num2, armorPenetration2, -1f, caster, null, source);
            damageInfo2.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
            damageInfo2.SetWeaponBodyPartGroup(bodyPartGroupDef);
            damageInfo2.SetWeaponHediff(hediffDef);
            damageInfo2.SetAngle(direction);
            yield return damageInfo2;
        }
    }

    protected override DamageWorker.DamageResult ApplyMeleeDamageToTarget(LocalTargetInfo target)
    {
        var result = new DamageWorker.DamageResult();
        foreach (var item in DamageInfosToApply(target))
        {
            if (!target.ThingDestroyed)
            {
                var comp = caster.TryGetComp<CompPokemon>();
                if (comp != null)
                    comp.moveTracker.lastUsedMove =
                        comp.moveTracker.unlockableMoves.Keys.Where(x => x.tool == tool).First();
                result = target.Thing.TakeDamage(item);
                continue;
            }

            return result;
        }

        return result;
    }
}
