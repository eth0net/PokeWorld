using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace PokeWorld;

public sealed class PokedexManager : WorldComponent
{
    private Dictionary<PawnKindDef, PokemonPokedexState> pokedex = new();

    public PokedexManager(World world) : base(world)
    {
        foreach (var allDef in DefDatabase<PawnKindDef>.AllDefs.Where(x => x.race.HasComp(typeof(CompPokemon)))
                     .OrderBy(x => x.race.GetCompProperties<CompProperties_Pokemon>().pokedexNumber))
            pokedex.Add(allDef, PokemonPokedexState.None);
    }

    public int TotalSeen()
    {
        return pokedex.Values.Where(x => x == PokemonPokedexState.Seen || x == PokemonPokedexState.Caught).Count();
    }

    public int TotalSeen(int generation, bool includeLegendaries = true)
    {
        if (includeLegendaries)
            return pokedex.Where(
                x => x.Key.race.HasComp(typeof(CompPokemon)) &&
                     x.Key.race.GetCompProperties<CompProperties_Pokemon>().generation == generation &&
                     (x.Value == PokemonPokedexState.Seen || x.Value == PokemonPokedexState.Caught)
            ).Count();
        return pokedex.Where(
            x => x.Key.race.HasComp(typeof(CompPokemon)) &&
                 x.Key.race.GetCompProperties<CompProperties_Pokemon>().generation == generation &&
                 !x.Key.race.GetCompProperties<CompProperties_Pokemon>().attributes
                     .Contains(PokemonAttribute.Legendary) &&
                 (x.Value == PokemonPokedexState.Seen || x.Value == PokemonPokedexState.Caught)
        ).Count();
    }

    public int TotalCaught()
    {
        return pokedex.Values.Where(x => x == PokemonPokedexState.Caught).Count();
    }

    public int TotalCaught(int generation, bool includeLegendaries = true)
    {
        if (includeLegendaries)
            return pokedex.Where(
                x => x.Key.race.HasComp(typeof(CompPokemon)) &&
                     x.Key.race.GetCompProperties<CompProperties_Pokemon>().generation == generation &&
                     x.Value == PokemonPokedexState.Caught
            ).Count();
        return pokedex.Where(
            x => x.Key.race.HasComp(typeof(CompPokemon)) &&
                 x.Key.race.GetCompProperties<CompProperties_Pokemon>().generation == generation &&
                 !x.Key.race.GetCompProperties<CompProperties_Pokemon>().attributes
                     .Contains(PokemonAttribute.Legendary) && x.Value == PokemonPokedexState.Caught
        ).Count();
    }

    public override void ExposeData()
    {
        Scribe_Collections.Look(ref pokedex, "PW_pokedex", LookMode.Def, LookMode.Value);
    }

    public bool IsPokemonSeen(PawnKindDef pawnKind)
    {
        pokedex.TryGetValue(pawnKind, out var value);
        return value == PokemonPokedexState.Seen || value == PokemonPokedexState.Caught;
    }

    public bool IsPokemonCaught(PawnKindDef pawnKind)
    {
        pokedex.TryGetValue(pawnKind, out var value);
        return value == PokemonPokedexState.Caught;
    }

    public void AddPokemonKindSeen(PawnKindDef pawnKind)
    {
        if (pawnKind.race.HasComp(typeof(CompPokemon)) && pokedex[pawnKind] == PokemonPokedexState.None)
            pokedex[pawnKind] = PokemonPokedexState.Seen;
    }

    public void AddPokemonKindCaught(PawnKindDef pawnKind)
    {
        if (pawnKind.race.HasComp(typeof(CompPokemon)) && (pokedex[pawnKind] == PokemonPokedexState.None ||
                                                           pokedex[pawnKind] == PokemonPokedexState.Seen))
            pokedex[pawnKind] = PokemonPokedexState.Caught;
    }

    public void DebugFillPokedex()
    {
        foreach (var allDef in DefDatabase<PawnKindDef>.AllDefs.Where(x => x.race.HasComp(typeof(CompPokemon))))
            AddPokemonKindCaught(allDef);
    }

    public void DebugFillPokedexNoLegendary()
    {
        foreach (var allDef in DefDatabase<PawnKindDef>.AllDefs.Where(
                     x => x.race.HasComp(typeof(CompPokemon)) && !x.race.GetCompProperties<CompProperties_Pokemon>()
                         .attributes.Contains(PokemonAttribute.Legendary)
                 )) AddPokemonKindCaught(allDef);
    }
}
