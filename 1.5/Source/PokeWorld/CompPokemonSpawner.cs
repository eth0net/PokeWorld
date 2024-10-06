using System;
using RimWorld;
using Verse;

namespace PokeWorld;

public class CompPokemonSpawner : ThingComp
{
    public bool active;
    public bool canSpawn = true;
    public int delay;
    public int timer;

    public CompProperties_PokemonSpawner Props => (CompProperties_PokemonSpawner)props;

    public PawnKindDef PawnKind => Props.pawnKind;

    public override void Initialize(CompProperties props)
    {
        base.Initialize(props);
        delay = Props.delay.RandomInRange;
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref timer, "timer");
        Scribe_Values.Look(ref delay, "delay");
        Scribe_Values.Look(ref active, "active");
        Scribe_Values.Look(ref canSpawn, "canSpawn");
    }

    public override void CompTick()
    {
        base.CompTick();
        if (active)
        {
            timer += 1;
            if (timer > delay && parent.Spawned)
            {
                var pokemon = PokemonGeneratorUtility.GenerateAndSpawnNewPokemon(
                    PawnKind, null, parent.Position, parent.Map
                );
                pokemon.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter);
                Find.LetterStack.ReceiveLetter(
                    "PW_WildRotomLetterLabel".Translate(), "PW_WildRotomLetterText".Translate(), LetterDefOf.ThreatBig,
                    pokemon
                );
                active = false;
                canSpawn = false;
            }
        }
    }

    public void TickAction(JobDriver_WatchTelevision jobDriver)
    {
        var television = jobDriver.job.targetA.Thing;
        var pawn = jobDriver.pawn;
        if (canSpawn && television != null && television.TryGetComp<CompPokemonSpawner>() == this &&
            television.Spawned && pawn != null && pawn.Faction.IsPlayer && PokeWorldSettings.allowGen4)
            if (!active && television.Map.GameConditionManager.ConditionIsActive(GameConditionDefOf.Eclipse))
            {
                var name = pawn.LabelShort;
                Find.LetterStack.ReceiveLetter(
                    "PW_MalevolentTVLetterLabel".Translate(), "PW_MalevolentTVLetterText".Translate(name),
                    LetterDefOf.ThreatSmall, television
                );
                active = true;
            }
    }
}

public class CompProperties_PokemonSpawner : CompProperties
{
    public IntRange delay;
    public PawnKindDef pawnKind;

    public CompProperties_PokemonSpawner()
    {
        compClass = typeof(CompPokemonSpawner);
    }

    public CompProperties_PokemonSpawner(Type compClass) : base(compClass)
    {
        this.compClass = compClass;
    }
}
