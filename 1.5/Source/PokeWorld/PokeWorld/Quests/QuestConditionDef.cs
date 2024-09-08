using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace PokeWorld;

public class QuestConditionDef : Def
{
    public List<GenerationRequirement> generationsRequirements;
    public QuestScriptDef questScriptDef;
    public int requiredCaughtMinCount = -1;
    public List<PawnKindDef> requiredKindCaught;
    public List<PawnKindDef> requiredKindSeen;
    public int requiredSeenMinCount = -1;

    public bool CheckCompletion()
    {
        var pokedex = Find.World.GetComponent<PokedexManager>();
        if (requiredKindSeen != null)
        {
            if (requiredSeenMinCount == -1) requiredSeenMinCount = requiredKindSeen.Count();
            var count = 0;
            foreach (var kindDef in requiredKindSeen)
                if (pokedex.IsPokemonSeen(kindDef))
                    count += 1;
            if (count < requiredSeenMinCount) return false;
        }

        if (requiredKindCaught != null)
        {
            if (requiredCaughtMinCount == -1) requiredCaughtMinCount = requiredKindCaught.Count();
            var count = 0;
            foreach (var kindDef in requiredKindCaught)
                if (pokedex.IsPokemonCaught(kindDef))
                    count += 1;
            if (count < requiredCaughtMinCount) return false;
        }

        if (generationsRequirements != null)
            foreach (var genReq in generationsRequirements)
                if (pokedex.TotalSeen(genReq.generation, false) < genReq.minNoLegSeen ||
                    pokedex.TotalSeen(genReq.generation) < genReq.minSeen ||
                    pokedex.TotalCaught(genReq.generation, false) < genReq.minNoLegCaught ||
                    pokedex.TotalCaught(genReq.generation) < genReq.minCaught)
                    return false;
        return true;
    }
}
