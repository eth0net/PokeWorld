using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace PokeWorld.Incidents;

internal class IncidentWorker_Ambush_PokemonManhunterPack : IncidentWorker_Ambush
{
    private const float ManhunterAmbushPointsFactor = 0.75f;

    public override bool CanFireNowSub(IncidentParms parms)
    {
        if (!base.CanFireNowSub(parms)) return false;
        List<PawnKindDef> animalKind;
        return Pokemon_ManhunterPackIncidentUtility.TryFindManhunterAnimalKind(AdjustedPoints(parms.points), -1,
            out animalKind);
    }

    public override List<Pawn> GeneratePawns(IncidentParms parms)
    {
        if (Pokemon_ManhunterPackIncidentUtility.TryFindManhunterAnimalKind(AdjustedPoints(parms.points),
                parms.target.Tile, out var animalKind) ||
            Pokemon_ManhunterPackIncidentUtility.TryFindManhunterAnimalKind(AdjustedPoints(parms.points), -1,
                out animalKind))
            return Pokemon_ManhunterPackIncidentUtility.GeneratePokemonFamily_NewTmp(animalKind, parms.target.Tile,
                AdjustedPoints(parms.points));
        Log.Error(string.Concat("Could not find any valid animal kind for ", def, " incident."));
        return [];
    }

    public override void PostProcessGeneratedPawnsAfterSpawning(List<Pawn> generatedPawns)
    {
        foreach (var t in generatedPawns)
        {
            t.health.AddHediff(HediffDefOf.Scaria);
            t.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent);
        }
    }

    private static float AdjustedPoints(float basePoints)
    {
        return basePoints * 0.75f;
    }

    public override string GetLetterText(Pawn anyPawn, IncidentParms parms)
    {
        var caravan = parms.target as Caravan;
        return string.Format(def.letterText, caravan != null ? caravan.Name : "yourCaravan".TranslateSimple(),
            anyPawn.GetKindLabelPlural()).CapitalizeFirst();
    }
}
