using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace PokeWorld;

public class PawnKindTable_Pokedex : PawnKindTable
{
    public PawnKindTable_Pokedex(
        PawnKindTableDef def, Func<IEnumerable<PawnKindDef>> pawnKindsGetter, int uiWidth, int uiHeight
    )
        : base(def, pawnKindsGetter, uiWidth, uiHeight)
    {
    }

    protected override IEnumerable<PawnKindDef> LabelSortFunction(IEnumerable<PawnKindDef> input)
    {
        return from p in input
            orderby p.race.GetCompProperties<CompProperties_Pokemon>().pokedexNumber
            select p;
    }

    protected override IEnumerable<PawnKindDef> PrimarySortFunction(IEnumerable<PawnKindDef> input)
    {
        return from p in input
            orderby p.race.GetCompProperties<CompProperties_Pokemon>().pokedexNumber
            select p;
    }
}
