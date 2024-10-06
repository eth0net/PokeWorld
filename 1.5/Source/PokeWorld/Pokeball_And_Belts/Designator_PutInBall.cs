using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace PokeWorld;

internal class Designator_PutInBall : Designator
{
    private readonly List<Pawn> justDesignated = new();

    public Designator_PutInBall()
    {
        defaultLabel = "PW_DesignationPutInPokeball".Translate();
        defaultDesc = "PW_DesignationPutInPokeballDesc".Translate();
        icon = ContentFinder<Texture2D>.Get("UI/Designators/PokeBall");
        soundDragSustain = SoundDefOf.Designate_DragStandard;
        soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
        useMouseIcon = true;
        soundSucceeded = SoundDefOf.Designate_Hunt;
        hotKey = KeyBindingDefOf.Misc7;
    }

    public override int DraggableDimensions => 2;

    protected override DesignationDef Designation => DefDatabase<DesignationDef>.GetNamed("PW_PutInBall");

    public override AcceptanceReport CanDesignateCell(IntVec3 c)
    {
        if (!c.InBounds(Map)) return false;
        if (!PokeBallableInCell(c).Any()) return "PW_DesignationPutInPokeballWarning".Translate();
        return true;
    }

    public override void DesignateSingleCell(IntVec3 loc)
    {
        foreach (var item in PokeBallableInCell(loc)) DesignateThing(item);
    }

    public override AcceptanceReport CanDesignateThing(Thing t)
    {
        var pawn = t as Pawn;
        if (pawn != null && pawn.def.race.Animal && pawn.TryGetComp<CompPokemon>() != null &&
            pawn.Faction == Faction.OfPlayer && Map.designationManager.DesignationOn(pawn, Designation) == null &&
            !pawn.InAggroMentalState) return true;
        return false;
    }

    public override void DesignateThing(Thing t)
    {
        Map.designationManager.AddDesignation(new Designation(t, Designation));
        justDesignated.Add((Pawn)t);
    }

    protected override void FinalizeDesignationSucceeded()
    {
        base.FinalizeDesignationSucceeded();
        justDesignated.Clear();
    }

    private IEnumerable<Pawn> PokeBallableInCell(IntVec3 c)
    {
        if (c.Fogged(Map)) yield break;
        var thingList = c.GetThingList(Map);
        for (var i = 0; i < thingList.Count; i++)
            if (CanDesignateThing(thingList[i]).Accepted)
                yield return (Pawn)thingList[i];
    }
}
