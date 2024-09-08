using Verse;

namespace PokeWorld;

internal class DamageWorker_PokemonMeleeMove : DamageWorker_AddInjury
{
    private const float MeleeDamageRandomFactorMin = 0.85f;

    private const float MeleeDamageRandomFactorMax = 1f;

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
                var attack = casterComp.statTracker.attackStat;
                int defense;
                var stab = casterComp.moveTracker.lastUsedMove.IsStab(casterPawn) ? 1.5f : 1f;
                float typeMultiplier = 1;
                var targetPawn = thing as Pawn;
                if (targetPawn != null)
                {
                    var targetComp = targetPawn.TryGetComp<CompPokemon>();
                    if (targetComp != null)
                    {
                        defense = targetComp.statTracker.defenseStat;
                        foreach (var typeDef in targetComp.Types)
                            typeMultiplier *= typeDef.GetDamageMultiplier(casterComp.moveTracker.lastUsedMove.type);
                    }
                    else
                    {
                        defense = PokemonDamageUtility.GetNonPokemonPawnDefense(targetPawn);
                    }
                }
                else
                {
                    defense = 50;
                }

                var damage = (2 * level / 5f + 2) * movePower * (attack / (float)defense) / 10f * stab * typeMultiplier;
                damage = Rand.Range(damage * MeleeDamageRandomFactorMin, damage * MeleeDamageRandomFactorMax);
                dinfo.SetAmount(damage);
                //Log.Message(casterPawn.ToString() + " - " + dinfo.Amount.ToString());
            }
        }

        return base.Apply(dinfo, thing);
    }
}
