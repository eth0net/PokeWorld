using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace PokeWorld;

internal class Designator_PutInPortableComputer : Designator
{
    private readonly List<CryptosleepBall> justDesignated = new();

    public Designator_PutInPortableComputer()
    {
        defaultLabel = "PW_DesignationStoreInPC".Translate();
        defaultDesc = "PW_DesignationStoreInPCDesc".Translate();
        icon = ContentFinder<Texture2D>.Get("UI/Designators/PortableComputer");
        soundDragSustain = SoundDefOf.Designate_DragStandard;
        soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
        useMouseIcon = true;
        soundSucceeded = SoundDefOf.Designate_Hunt;
        hotKey = KeyBindingDefOf.Misc7;
    }

    public override int DraggableDimensions => 2;

    protected override DesignationDef Designation => DefDatabase<DesignationDef>.GetNamed("PW_PutInPortableComputer");

    public override bool Visible => DefDatabase<ResearchProjectDef>.GetNamed("PW_StorageSystem").IsFinished;

    public override AcceptanceReport CanDesignateCell(IntVec3 c)
    {
        if (!c.InBounds(Map)) return false;
        if (!PCStorableInCell(c).Any()) return "PW_DesignationStoreInPCWarning".Translate();
        return true;
    }

    public override void DesignateSingleCell(IntVec3 loc)
    {
        foreach (var item in PCStorableInCell(loc)) DesignateThing(item);
    }

    public override AcceptanceReport CanDesignateThing(Thing t)
    {
        var ball = t as CryptosleepBall;
        if (ball != null && ball.ContainedThing is Pawn pawn && pawn.Faction == Faction.OfPlayer &&
            Map.designationManager.DesignationOn(ball, Designation) == null) return true;
        return false;
    }

    public override void DesignateThing(Thing t)
    {
        Map.designationManager.AddDesignation(new Designation(t, Designation));
        justDesignated.Add((CryptosleepBall)t);
    }

    protected override void FinalizeDesignationSucceeded()
    {
        base.FinalizeDesignationSucceeded();
        justDesignated.Clear();
    }

    private IEnumerable<CryptosleepBall> PCStorableInCell(IntVec3 c)
    {
        if (c.Fogged(Map)) yield break;
        var thingList = c.GetThingList(Map);
        for (var i = 0; i < thingList.Count; i++)
            if (CanDesignateThing(thingList[i]).Accepted)
                yield return (CryptosleepBall)thingList[i];
    }
}
