using System;
using System.Collections.Generic;
using Verse;

namespace PokeWorld;

public class PawnKindTableDef : Def
{
    public List<PawnKindColumnDef> columns;

    public int minWidth = 500;

    public Type workerClass = typeof(PawnKindTable);
}
