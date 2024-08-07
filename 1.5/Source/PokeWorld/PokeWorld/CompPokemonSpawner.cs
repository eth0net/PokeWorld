﻿using RimWorld;
using System;
using Verse;

namespace PokeWorld
{
    public class CompPokemonSpawner : ThingComp
    {
        public bool active = false;
        public bool canSpawn = true;
        public int timer = 0;
        public int delay;

        public CompProperties_PokemonSpawner Props => (CompProperties_PokemonSpawner)this.props;

        public PawnKindDef PawnKind => Props.pawnKind;

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            delay = Props.delay.RandomInRange;
        }
        
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref timer, "timer", defaultValue: 0);
            Scribe_Values.Look(ref delay, "delay", defaultValue: 0);
            Scribe_Values.Look(ref active, "active", defaultValue: false);
            Scribe_Values.Look(ref canSpawn, "canSpawn", defaultValue: false);
        }
        
        public override void CompTick()
        {
            base.CompTick();
            if (active)
            {
                timer += 1;
                if (timer > delay && parent.Spawned)
                {
                    Pawn pokemon = PokemonGeneratorUtility.GenerateAndSpawnNewPokemon(PawnKind, null, parent.Position, parent.Map);
                    pokemon.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter);
                    Find.LetterStack.ReceiveLetter("PW_WildRotomLetterLabel".Translate(), "PW_WildRotomLetterText".Translate(), LetterDefOf.ThreatBig, pokemon);
                    active = false;
                    canSpawn = false;
                }
            }
        }
        
        public void TickAction(JobDriver_WatchTelevision jobDriver)
        {
            Thing television = jobDriver.job.targetA.Thing;
            Pawn pawn = jobDriver.pawn;
            if (canSpawn && television != null && television.TryGetComp<CompPokemonSpawner>() == this && television.Spawned && pawn != null && pawn.Faction.IsPlayer && PokeWorldSettings.allowGen4)
            {
                if (!active && television.Map.GameConditionManager.ConditionIsActive(GameConditionDefOf.Eclipse))
                {
                    string name = pawn.LabelShort;
                    Find.LetterStack.ReceiveLetter("PW_MalevolentTVLetterLabel".Translate(), "PW_MalevolentTVLetterText".Translate(name), LetterDefOf.ThreatSmall, television);
                    active = true;
                }
            }
        }
    }
    
    public class CompProperties_PokemonSpawner : CompProperties
    {
        public PawnKindDef pawnKind;
        public IntRange delay;

        public CompProperties_PokemonSpawner()
        {
            this.compClass = typeof(CompPokemonSpawner);
        }

        public CompProperties_PokemonSpawner(Type compClass) : base(compClass)
        {
            this.compClass = compClass;
        }
    }
}
