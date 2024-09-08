using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace PokeWorld;

public class ShinyTracker : IExposable
{
    public CompPokemon comp;

    public bool femaleGraphicDifference;
    public GraphicData femaleShinyGraphicData;
    public bool flagLetter = true;
    public bool isShiny;
    public Pawn pokemonHolder;
    public float shinyChance;
    public GraphicData shinyGraphicData;

    public ShinyTracker(CompPokemon comp)
    {
        this.comp = comp;
        pokemonHolder = comp.Pokemon;
        shinyChance = comp.ShinyChance;
        TryMakeShiny();
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref isShiny, "PW_isShiny");
        Scribe_Values.Look(ref flagLetter, "PW_flagLetter");
    }

    public void TryMakeShiny()
    {
        var num = Rand.Range(0f, 1f);
        if (num < shinyChance)
            MakeShiny();
        else
            isShiny = false;
    }

    public void MakeShiny()
    {
        isShiny = true;
    }

    public void ShinyTickRare()
    {
        if (isShiny)
        {
            if (flagLetter && pokemonHolder.Spawned && (pokemonHolder.Faction == null || pokemonHolder.Faction ==
                    Find.FactionManager.AllFactions.Where(f => f.def.defName == "PW_HostilePokemon").First()))
            {
                Letter letter = LetterMaker.MakeLetter(
                    "PW_ShinyPokemonLetter".Translate(), "PW_ShinyPokemonLetterDesc".Translate(),
                    LetterDefOf.PositiveEvent, pokemonHolder
                );
                Find.LetterStack.ReceiveLetter(letter);
                flagLetter = false;
            }

            if (PokeWorldSettings.enableShinyMote) TryMakeShinyMote();
        }
    }

    public void TryMakeShinyMote()
    {
        if (pokemonHolder.Spawned && !pokemonHolder.Position.Fogged(pokemonHolder.Map))
        {
            var num = Rand.RangeInclusive(2, 5);
            for (var i = 0; i < num; i++)
                ThrowShinyIcon(
                    pokemonHolder.Position, pokemonHolder.Map, DefDatabase<ThingDef>.GetNamed("Mote_ShinyStar")
                );
        }
    }

    public static Mote ThrowShinyIcon(IntVec3 cell, Map map, ThingDef moteDef)
    {
        if (!cell.ShouldSpawnMotesAt(map) || map.moteCounter.Saturated) return null;
        var obj = (MoteThrown)ThingMaker.MakeThing(moteDef);
        obj.Scale = 0.7f;
        obj.rotationRate = Rand.Range(-3f, 3f);
        obj.exactPosition = cell.ToVector3Shifted();
        obj.exactPosition += new Vector3(0.35f, 0f, 0.35f);
        obj.exactPosition += new Vector3(Rand.Value, 0f, Rand.Value) * 0.1f;
        obj.SetVelocity(Rand.Range(0, 360), 0.42f);
        GenSpawn.Spawn(obj, cell, map);
        return obj;
    }
}
