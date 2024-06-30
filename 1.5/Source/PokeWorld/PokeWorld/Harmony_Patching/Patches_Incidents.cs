using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using PokeWorld.ModSetting;
using RimWorld;
using Verse;

namespace PokeWorld.Harmony_Patching;

[HarmonyPatch(typeof(IncidentWorker_FarmAnimalsWanderIn))]
[HarmonyPatch(nameof(IncidentWorker_FarmAnimalsWanderIn.TryFindRandomPawnKind))]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
internal class IncidentWorker_FarmAnimalsWanderIn_TryFindRandomPawnKind_Patch
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static void Postfix(Map __0, out PawnKindDef __1, ref bool __result)
    {
        if (PokeWorldSettings.OkforPokemon())
        {
            __1 = null;
            __result = false;
        }
        else
        {
            __result = DefDatabase<PawnKindDef>.AllDefs
                .Where(x => x.RaceProps.Animal && !x.race.HasComp(typeof(CompPokemon)) &&
                            x.RaceProps.wildness < 0.35f &&
                            __0.mapTemperature.SeasonAndOutdoorTemperatureAcceptableFor(x.race))
                .TryRandomElementByWeight(k => 0.420000017f - k.RaceProps.wildness, out __1);
        }
    }
}

[HarmonyPatch(typeof(AggressiveAnimalIncidentUtility))]
[HarmonyPatch(nameof(AggressiveAnimalIncidentUtility.TryFindAggressiveAnimalKind))]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
internal class AggressiveAnimalIncidentUtility_TryFindAggressiveAnimalKind_Patch
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static void Postfix(float __0, int __1, out PawnKindDef __2, ref bool __result)
    {
        if (PokeWorldSettings.OkforPokemon())
        {
            __2 = null;
            __result = false;
        }
        else
        {
            var source = DefDatabase<PawnKindDef>.AllDefs.Where(k =>
                k.RaceProps.Animal && !k.race.HasComp(typeof(CompPokemon)) && k.canArriveManhunter && (__1 == -1 ||
                    Find.World.tileTemperatures.SeasonAndOutdoorTemperatureAcceptableFor(__1, k.race)));
            var pawnKindDefs = source as PawnKindDef[] ?? source.ToArray();
            if (pawnKindDefs.Any())
            {
                if (pawnKindDefs.TryRandomElementByWeight(a => AggressiveAnimalIncidentUtility.AnimalWeight(a, __0),
                        out __2))
                {
                    __result = true;
                }
                else if (__0 > pawnKindDefs.Min(a => a.combatPower) * 2f)
                {
                    __2 = pawnKindDefs.MaxBy(a => a.combatPower);
                    __result = true;
                }
                else
                {
                    __2 = null;
                    __result = false;
                }
            }
            else
            {
                __2 = null;
                __result = false;
            }
        }
    }
}

[HarmonyPatch(typeof(IncidentWorker_Infestation))]
[HarmonyPatch(nameof(IncidentWorker_Infestation.TryExecuteWorker))]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
internal class IncidentWorker_Infestation_TryExecuteWorker_Patch
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static bool Prefix(ref bool __result)
    {
        if (!PokeWorldSettings.allowPokemonInfestation || !PokeWorldSettings.OkforPokemon()) return true;
        __result = false;
        return false;
    }
}

[HarmonyPatch(typeof(IncidentWorker_DeepDrillInfestation))]
[HarmonyPatch("TryExecuteWorker")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
internal class IncidentWorker_DeepDrillInfestation_TryExecuteWorker_Patch
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static bool Prefix(ref bool __result)
    {
        if (!PokeWorldSettings.allowPokemonInfestation || !PokeWorldSettings.OkforPokemon()) return true;
        __result = false;
        return false;
    }
}

[HarmonyPatch(typeof(IncidentWorker_ThrumboPasses))]
[HarmonyPatch("TryExecuteWorker")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
internal class IncidentWorker_ThrumboPasses_TryExecuteWorker_Patch
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static bool Prefix(ref bool __result)
    {
        if (!PokeWorldSettings.OkforPokemon()) return true;
        __result = false;
        return false;
    }
}

//For alphabeaver event (work with debug command)
[HarmonyPatch(typeof(Storyteller))]
[HarmonyPatch("TryFire")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
internal class IncidentWorker_HerdMigration_TryFindAnimalKind_Patch
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static bool Prefix(FiringIncident __0, ref bool __result)
    {
        if (__0.def.Worker.def != DefDatabase<IncidentDef>.GetNamed("Alphabeavers")) return true;
        if (!PokeWorldSettings.OkforPokemon()) return true;
        __result = false;
        return false;
    }
}

[HarmonyPatch(typeof(IncidentWorker_SelfTame))]
[HarmonyPatch("Candidates")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
internal class IncidentWorker_SelfTame_Candidates_Patch
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static void Postfix(Map __0, ref IEnumerable<Pawn> __result)
    {
        __result = __0.mapPawns.AllPawnsSpawned.Where(x =>
            x.RaceProps.Animal && x.Faction == null && x.TryGetComp<CompPokemon>() == null &&
            !x.Position.Fogged(x.Map) && !x.InMentalState && !x.Downed && x.RaceProps.wildness > 0f);
    }
}
