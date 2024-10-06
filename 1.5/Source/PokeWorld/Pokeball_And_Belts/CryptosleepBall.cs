using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace PokeWorld;

public class CryptosleepBall : ThingWithComps, ISuspendableThingHolder, IOpenable
{
    protected bool contentsKnown;
    public ThingOwner innerContainer;
    public bool wantPutInPortableComputer;

    public CryptosleepBall()
    {
        innerContainer = new ThingOwner<Thing>(this, true);
    }

    public bool HasAnyContents => innerContainer.Count > 0;

    public Thing ContainedThing
    {
        get
        {
            if (innerContainer.Count != 0) return innerContainer[0];
            return null;
        }
    }

    public override string Label
    {
        get
        {
            if (contentsKnown && ContainedThing is Pawn pawn)
                return base.Label + " (" + pawn.Name + ")";
            return base.Label;
        }
    }

    public int OpenTicks => 60;

    public bool CanOpen => HasAnyContents;

    public virtual void Open()
    {
        if (HasAnyContents) EjectContents();
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


    public virtual bool Accepts(Thing thing)
    {
        return innerContainer.CanAcceptAnyOf(thing);
    }

    public virtual bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
    {
        if (!Accepts(thing)) return false;
        var flag = false;
        if (thing.holdingOwner != null)
        {
            thing.holdingOwner.TryTransferToContainer(thing, innerContainer, thing.stackCount);
            flag = true;
        }
        else
        {
            flag = innerContainer.TryAdd(thing);
        }

        if (flag)
        {
            contentsKnown = true;
            return true;
        }

        return false;
    }

    public override void TickRare()
    {
        base.TickRare();
        innerContainer.ThingOwnerTickRare();
    }

    public override void Tick()
    {
        base.Tick();
        innerContainer.ThingOwnerTick();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Deep.Look(ref innerContainer, "PW_innerContainer", this);
        Scribe_Values.Look(ref contentsKnown, "PW_contentsKnown");
        Scribe_Values.Look(ref wantPutInPortableComputer, "PW_wantPutInPortableComputer");
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        contentsKnown = true;
        if (innerContainer.NullOrEmpty())
        {
            Log.Error("Error: Tried to spawn empty Poké Ball");
            Destroy();
        }
    }

    public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
    {
        if (innerContainer.Count > 0 && mode == DestroyMode.KillFinalize)
        {
            var list = new List<Pawn>();
            foreach (var item in innerContainer)
            {
                var pawn = item as Pawn;
                if (pawn != null) list.Add(pawn);
            }

            foreach (var item2 in list) HealthUtility.DamageUntilDowned(item2);
            EjectContents();
        }
        else
        {
            innerContainer.ClearAndDestroyContents();
            base.Destroy(mode);
        }
    }

    public virtual void EjectContents()
    {
        contentsKnown = true;
        var pos = InteractionCell;
        var map = Map;
        DeSpawn();
        innerContainer.TryDropAll(pos, map, ThingPlaceMode.Near);
        if (innerContainer.Count == 0) Destroy();
    }

    public override string GetInspectString()
    {
        var text = base.GetInspectString();
        string str;
        if (contentsKnown)
            str = innerContainer.ContentsString;
        else
            str = "UnknownLower".Translate();
        if (contentsKnown && innerContainer.Count > 0)
        {
            var pokemon = innerContainer[0] as Pawn;
            if (pokemon != null)
            {
                var comp = pokemon.TryGetComp<CompPokemon>();
                if (comp != null)
                {
                    var species = pokemon.def.label;
                    var level = comp.levelTracker.level.ToString();
                    if (!text.NullOrEmpty()) text += "\n";
                    if (pokemon.gender != Gender.None)
                        return text + "PW_PokeballContainsLongGendered".Translate(
                            str.CapitalizeFirst(), level, pokemon.gender.ToString().ToLower(), species
                        );
                    return text + "PW_PokeballContainsLongNoGender".Translate(str.CapitalizeFirst(), level, species);
                }
            }
        }

        if (!text.NullOrEmpty()) text += "\n";
        return text + "PW_PokeballContainsShort".Translate(str.CapitalizeFirst());
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var item in base.GetGizmos()) yield return item;
        if (DefDatabase<ResearchProjectDef>.GetNamed("PW_StorageSystem").IsFinished && ContainedThing is Pawn pawn &&
            pawn.Faction == Faction.OfPlayer)
            if (Map.designationManager.DesignationOn(
                    this, DefDatabase<DesignationDef>.GetNamed("PW_PutInPortableComputer")
                ) == null)
            {
                var command_PutInPC = new Command_Action();
                command_PutInPC.defaultLabel = "PW_DesignationStoreInPC".Translate();
                command_PutInPC.defaultDesc = "PW_DesignationStoreInPCDesc".Translate();
                command_PutInPC.hotKey = KeyBindingDefOf.Misc2;
                command_PutInPC.icon = ContentFinder<Texture2D>.Get("UI/Designators/PortableComputer");
                command_PutInPC.action = delegate
                {
                    PutInPortableComputerUtility.UpdatePutInPortableComputerDesignation(this);
                };
                yield return command_PutInPC;
            }
    }
}
