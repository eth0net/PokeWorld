using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace PokeWorld;

public class StorageSystem : WorldComponent, ISuspendableThingHolder
{
    public readonly int maxCount = 999;
    protected bool contentsKnown;
    public ThingOwner innerContainer;

    public StorageSystem(World world) : base(world)
    {
        innerContainer = new ThingOwner<Pawn>(this);
    }

    public bool HasAnyContents => innerContainer.Count > 0;

    public List<Thing> ContainedThing
    {
        get
        {
            if (innerContainer.Count != 0) return innerContainer.ToList();
            return null;
        }
    }

    public bool IsContentsSuspended => true;

    public ThingOwner GetDirectlyHeldThings()
    {
        return innerContainer;
    }

    public void GetChildHolders(List<IThingHolder> outChildren)
    {
        ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
    }

    public IThingHolder ParentHolder => null;

    public virtual bool Accepts(CryptosleepBall cryptosleepBall)
    {
        if (cryptosleepBall.ContainedThing != null && cryptosleepBall.ContainedThing is Pawn pawn &&
            pawn.Faction == Faction.OfPlayer) return true;
        return false;
    }

    public virtual bool TryAcceptThing(CryptosleepBall cryptosleepBall, bool allowSpecialEffects = true)
    {
        if (!Accepts(cryptosleepBall)) return false;
        var flag = false;
        if (cryptosleepBall.ContainedThing != null)
        {
            cryptosleepBall.innerContainer.TryTransferToContainer(
                cryptosleepBall.ContainedThing, innerContainer, cryptosleepBall.stackCount
            );
            cryptosleepBall.Destroy();
            flag = true;
        }

        if (flag)
        {
            contentsKnown = true;
            return true;
        }

        return false;
    }

    public static Building_PortableComputer FindPortableComputerFor(
        CryptosleepBall p, Pawn traveler, bool ignoreOtherReservations = false
    )
    {
        foreach (var item in DefDatabase<ThingDef>.AllDefs.Where(
                     def => typeof(Building_PortableComputer).IsAssignableFrom(def.thingClass)
                 ))
        {
            var building_PortableComputer = (Building_PortableComputer)GenClosest.ClosestThingReachable(
                p.Position, p.Map, ThingRequest.ForDef(item), PathEndMode.InteractionCell, TraverseParms.For(traveler)
            );
            if (building_PortableComputer != null && building_PortableComputer.TryGetComp<CompPowerTrader>().PowerOn &&
                traveler.CanReserve(building_PortableComputer)) return building_PortableComputer;
        }

        return null;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Deep.Look(ref innerContainer, "PW_innerContainer");
        Scribe_Values.Look(ref contentsKnown, "PW_contentsKnown");
    }
}
