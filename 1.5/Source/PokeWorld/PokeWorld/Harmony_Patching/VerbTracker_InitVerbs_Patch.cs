using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace PokeWorld;

[HarmonyPatch(typeof(VerbTracker))]
[HarmonyPatch("InitVerbs")]
internal class VerbTracker_InitVerbs_Patch
{
    public static void Postfix(Func<Type, string, Verb> __0, VerbTracker __instance)
    {
        if (!(__instance.directOwner is Pawn pawn)) return;
        var comp = pawn.TryGetComp<CompPokemon>();
        if (comp == null) return;
        foreach (var kvp in comp.moveTracker.unlockableMoves)
        {
            var verbProperties = kvp.Key.verb;
            if (verbProperties != null)
                try
                {
                    var text = Verb.CalculateUniqueLoadID(
                        pawn, comp.moveTracker.unlockableMoves.Keys.ToList().IndexOf(kvp.Key)
                    );

                    var methodInitVerb = __instance.GetType().GetMethod(
                        "InitVerb", BindingFlags.NonPublic | BindingFlags.Instance
                    );
                    methodInitVerb.Invoke(
                        __instance,
                        new object[] { __0(verbProperties.verbClass, text), verbProperties, null, null, text }
                    );
                }
                catch (Exception ex)
                {
                    Log.Error("Could not instantiate Verb (directOwner=" + pawn.ToStringSafe() + "): " + ex);
                }

            var tool = kvp.Key.tool;
            if (tool != null)
            {
                var maneuver = tool.Maneuvers.First();
                try
                {
                    var verb = maneuver.verb;
                    var text2 = Verb.CalculateUniqueLoadID(
                        pawn, comp.moveTracker.unlockableMoves.Keys.ToList().IndexOf(kvp.Key)
                    );

                    var methodInitVerb = __instance.GetType().GetMethod(
                        "InitVerb", BindingFlags.NonPublic | BindingFlags.Instance
                    );
                    methodInitVerb.Invoke(
                        __instance, new object[] { __0(verb.verbClass, text2), verb, tool, maneuver, text2 }
                    );
                }
                catch (Exception ex2)
                {
                    Log.Error("Could not instantiate Verb (directOwner=" + pawn.ToStringSafe() + "): " + ex2);
                }
            }
        }
    }
}
