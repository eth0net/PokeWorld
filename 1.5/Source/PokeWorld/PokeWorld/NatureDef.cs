using RimWorld;
using Verse;

namespace PokeWorld;

public class NatureDef : Def
{
    public StatDef decreasedStat;
    public float decreaseMult = 0.9f;
    public StatDef increasedStat;
    public float increaseMult = 1.1f;

    public float GetMultiplier(StatDef stat)
    {
        if (increasedStat != null && decreasedStat != null && increasedStat != decreasedStat)
        {
            if (stat == increasedStat)
                return increaseMult;
            if (stat == decreasedStat) return decreaseMult;
        }

        return 1;
    }
}
