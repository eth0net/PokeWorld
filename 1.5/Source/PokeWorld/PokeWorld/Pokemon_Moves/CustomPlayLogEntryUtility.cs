using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace PokeWorld;

public static class CustomPlayLogEntryUtility
{
    public static IEnumerable<Rule> RulesForOptionalMove(string prefix, MoveDef moveDef, ThingDef projectileDef)
    {
        if (moveDef == null) yield break;
        foreach (var item in GrammarUtility.RulesForDef(prefix, moveDef)) yield return item;
        var thingDef = projectileDef;
        if (thingDef == null && moveDef.verb != null) thingDef = moveDef.verb.defaultProjectile;
        if (thingDef == null) yield break;
        foreach (var item2 in GrammarUtility.RulesForDef(prefix + "_projectile", thingDef)) yield return item2;
    }
}
