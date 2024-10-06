using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace PokeWorld;

public sealed class PokedexManager(World world) : WorldComponent(world)
{
    private Dictionary<PawnKindDef, PokemonPokedexState> pokedex = new();

    public int TotalSeen()
    {
        return pokedex.Values.Count(x => x is PokemonPokedexState.Seen or PokemonPokedexState.Caught);
    }

    public int TotalSeen(int generation, bool includeLegendaries = true)
    {
        if (includeLegendaries)
            return pokedex.Count(
                x => x.Key.race.HasComp(typeof(CompPokemon)) &&
                     x.Key.race.GetCompProperties<CompProperties_Pokemon>().generation == generation &&
                     x.Value is PokemonPokedexState.Seen or PokemonPokedexState.Caught
            );

        return pokedex.Count(
            x => x.Key.race.HasComp(typeof(CompPokemon)) &&
                 x.Key.race.GetCompProperties<CompProperties_Pokemon>().generation == generation &&
                 !x.Key.race.GetCompProperties<CompProperties_Pokemon>().attributes
                     .Contains(PokemonAttribute.Legendary) &&
                 x.Value is PokemonPokedexState.Seen or PokemonPokedexState.Caught
        );
    }

    public int TotalCaught()
    {
        return pokedex.Values.Count(x => x == PokemonPokedexState.Caught);
    }

    public int TotalCaught(int generation, bool includeLegendaries = true)
    {
        if (includeLegendaries)
            return pokedex.Count(
                x => x.Key.race.HasComp(typeof(CompPokemon)) &&
                     x.Key.race.GetCompProperties<CompProperties_Pokemon>().generation == generation &&
                     x.Value == PokemonPokedexState.Caught
            );
        return pokedex.Count(
            x => x.Key.race.HasComp(typeof(CompPokemon)) &&
                 x.Key.race.GetCompProperties<CompProperties_Pokemon>().generation == generation &&
                 !x.Key.race.GetCompProperties<CompProperties_Pokemon>().attributes
                     .Contains(PokemonAttribute.Legendary) && x.Value == PokemonPokedexState.Caught
        );
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
        if (!pawnKind.race.HasComp(typeof(CompPokemon))) return;

        if (!pokedex.ContainsKey(pawnKind) || pokedex[pawnKind] == PokemonPokedexState.None)
            pokedex[pawnKind] = PokemonPokedexState.Seen;
    }

    public void AddPokemonKindCaught(PawnKindDef pawnKind)
    {
        if (!pawnKind.race.HasComp(typeof(CompPokemon))) return;
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
