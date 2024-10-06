using System.Text;
using RimWorld;
using Verse;

namespace PokeWorld;

internal class StatWorker_CatchRate : StatWorker
{
    public override bool ShouldShowFor(StatRequest req)
    {
        if (!base.ShouldShowFor(req)) return false;
        if (req.Def is ThingDef thingDef && thingDef.HasComp(typeof(CompPokemon))) return true;
        return false;
    }

    public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
    {
        if (req.Def is ThingDef thingDef && thingDef.HasComp(typeof(CompPokemon)))
            return thingDef.GetCompProperties<CompProperties_Pokemon>().catchRate;
        return 0;
    }

    public override string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense, float finalVal)
    {
        float currentHealthPercent = 1;
        var catchRate = GetValueUnfinalized(req);
        float bonusBall = 1;
        var aValue = (1 - 2 / 3f * currentHealthPercent) * catchRate * bonusBall;
        var bValue = aValue / 255;
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("PW_StatCatchRateDesc".Translate(bValue.ToStringPercent()));
        return stringBuilder.ToString();
    }
}
