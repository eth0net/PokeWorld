using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Verse;

namespace PokeWorld.Harmony_Patching;

[HarmonyPatch(typeof(Pawn))]
[HarmonyPatch(nameof(Pawn.LabelNoCount))]
[HarmonyPatch(MethodType.Getter)]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class Pawn_get_LabelNoCount_Patch
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static void Postfix(Pawn __instance, ref string __result)
    {
        __result = PokemonNamePatchUtility.TryPatchName(__instance, __result);
    }
}

[HarmonyPatch(typeof(Pawn))]
[HarmonyPatch("get_LabelShort")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class Pawn_get_LabelShort_Patch
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static void Postfix(Pawn __instance, ref string __result)
    {
        if (__instance.Name != null) __result = PokemonNamePatchUtility.TryPatchName(__instance, __result);
    }
}

[HarmonyPatch(typeof(Pawn))]
[HarmonyPatch("get_LabelNoCountColored")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class Pawn_get_LabelNoCountColored_Patch
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static void Postfix(Pawn __instance, ref TaggedString __result)
    {
        __result = PokemonNamePatchUtility.TryPatchName(__instance, __result);
    }
}

[HarmonyPatch(typeof(Pawn))]
[HarmonyPatch("get_NameShortColored")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class Pawn_get_NameShortColored_Patch
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static void Postfix(Pawn __instance, ref TaggedString __result)
    {
        __result = PokemonNamePatchUtility.TryPatchName(__instance, __result);
    }
}

[HarmonyPatch(typeof(Pawn))]
[HarmonyPatch("get_NameFullColored")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class Pawn_get_NameFullColored_Patch
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static void Postfix(Pawn __instance, ref TaggedString __result)
    {
        __result = PokemonNamePatchUtility.TryPatchName(__instance, __result);
    }
}

public static class PokemonNamePatchUtility
{
    public static string TryPatchName(Pawn pawn, string name)
    {
        var comp = pawn.TryGetComp<CompPokemon>();
        if (comp == null) return name;
        if (comp.shinyTracker is { isShiny: true }) name = GetShinyStar() + name;
        if (comp.formTracker != null && comp.showFormLabel && (pawn.Faction == null ||
                                                               (pawn.Faction != null &&
                                                                (!pawn.Faction.IsPlayer ||
                                                                 pawn.Name.Numerical))))
            name += " " + GetFormLabel(comp);

        return name;
    }

    private static string GetShinyStar()
    {
        return "★";
    }

    private static string GetFormLabel(CompPokemon comp)
    {
        return comp.formTracker.currentFormIndex != -1 ? comp.forms[comp.formTracker.currentFormIndex].label : "";
    }
}
