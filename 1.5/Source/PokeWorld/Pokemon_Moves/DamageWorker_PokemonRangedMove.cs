using Verse;

namespace PokeWorld;

internal class DamageWorker_PokemonRangedMove : DamageWorker_AddInjury
{
    private const float RangedDamageRandomFactorMin = 0.85f;

    private const float RangedDamageRandomFactorMax = 1f;

    public override DamageResult Apply(DamageInfo dinfo, Thing thing)
    {
        var casterPawn = dinfo.Instigator as Pawn;
        if (casterPawn != null)
        {
            var casterComp = casterPawn.TryGetComp<CompPokemon>();
            if (casterComp != null)
            {
                var level = casterComp.levelTracker.level;
                var movePower = dinfo.Amount;
                var spAttack = casterComp.statTracker.attackSpStat;
                int spDefense;
                var stab = casterComp.moveTracker.lastUsedMove.IsStab(casterPawn) ? 1.5f : 1f;
                float typeMultiplier = 1;
                var targetPawn = thing as Pawn;
                if (targetPawn != null)
                {
                    var targetComp = targetPawn.TryGetComp<CompPokemon>();
                    if (targetComp != null)
                    {
                        spDefense = targetComp.statTracker.defenseSpStat;
                        foreach (var typeDef in targetComp.Types)
                            typeMultiplier *= typeDef.GetDamageMultiplier(casterComp.moveTracker.lastUsedMove.type);
                    }
                    else
                    {
                        spDefense = PokemonDamageUtility.GetNonPokemonPawnDefense(targetPawn);
                    }
                }
                else
                {
                    spDefense = 50;
                }

                var damage = (2 * level / 5f + 2) * movePower * (spAttack / (float)spDefense) / 10f * stab *
                             typeMultiplier;
                damage = Rand.Range(damage * RangedDamageRandomFactorMin, damage * RangedDamageRandomFactorMax);
                dinfo.SetAmount(damage);
                //Log.Message(casterPawn.ToString() + " - " + dinfo.Amount.ToString());
            }
        }

        return base.Apply(dinfo, thing);
    }
}
