using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace PokeWorld;

public class IncidentWorker_PokemonInfestation : IncidentWorker
{
    public const float HivePoints = 220f;

    protected override bool CanFireNowSub(IncidentParms parms)
    {
        var map = (Map)parms.target;
        if (base.CanFireNowSub(parms) && HiveUtility.TotalSpawnedHivesCount(map) < 30)
            return InfestationCellFinder.TryFindCell(out _, map);
        return false;
    }

    protected override bool TryExecuteWorker(IncidentParms parms)
    {
        if (PokeWorldSettings.OkforPokemon() && PokeWorldSettings.allowPokemonInfestation)
        {
            var map = (Map)parms.target;
            var hiveDef = PokemonInfestationUtility.GetInfestationPokemonHiveDef();
            var t = PokemonInfestationUtility.SpawnTunnels(
                hiveDef, Mathf.Max(GenMath.RoundRandom(parms.points / 220f), 1), map
            );
            SendStandardLetter(parms, t);
            Find.TickManager.slower.SignalForceNormalSpeedShort();
            return true;
        }

        return false;
    }
}

public static class PokemonInfestationUtility
{
    public static TunnelPokemonHiveSpawner SpawnTunnels(
        ThingDef hiveDef, int hiveCount, Map map, bool spawnAnywhereIfNoGoodCell = false,
        bool ignoreRoofedRequirement = false, string questTag = null
    )
    {
        if (!InfestationCellFinder.TryFindCell(out var cell, map))
        {
            if (!spawnAnywhereIfNoGoodCell) return null;
            if (!RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith(
                    delegate(IntVec3 x)
                    {
                        if (!x.Standable(map) || x.Fogged(map)) return false;
                        var flag = false;
                        var num = GenRadial.NumCellsInRadius(3f);
                        for (var j = 0; j < num; j++)
                        {
                            var c = x + GenRadial.RadialPattern[j];
                            if (c.InBounds(map))
                            {
                                var roof = c.GetRoof(map);
                                if (roof != null && roof.isThickRoof)
                                {
                                    flag = true;
                                    break;
                                }
                            }
                        }

                        return flag;
                    }, map, out cell
                ))
                return null;
        }

        var thing = (TunnelPokemonHiveSpawner)ThingMaker.MakeThing(
            DefDatabase<ThingDef>.GetNamed("PW_TunnelPokemonHiveSpawner")
        );
        thing.pokemonHiveDef = hiveDef;
        GenSpawn.Spawn(thing, cell, map, WipeMode.FullRefund);
        QuestUtility.AddQuestTag(thing, questTag);
        for (var i = 0; i < hiveCount - 1; i++)
        {
            cell = CompSpawnerHives.FindChildHiveLocation(
                thing.Position, map, hiveDef, hiveDef.GetCompProperties<CompProperties_SpawnerHives>(),
                ignoreRoofedRequirement, true
            );
            if (cell.IsValid)
            {
                thing = (TunnelPokemonHiveSpawner)ThingMaker.MakeThing(
                    DefDatabase<ThingDef>.GetNamed("PW_TunnelPokemonHiveSpawner")
                );
                thing.pokemonHiveDef = hiveDef;
                GenSpawn.Spawn(thing, cell, map, WipeMode.FullRefund);
                QuestUtility.AddQuestTag(thing, questTag);
            }
        }

        return thing;
    }

    public static ThingDef GetNaturalPokemonHiveDef()
    {
        var def = DefDatabase<ThingDef>.AllDefs.Where(
            x => x.defName == "PW_PokemonHiveGeodude"
                 || x.defName == "PW_PokemonHiveOnix"
                 || x.defName == "PW_PokemonHiveParas"
                 || x.defName == "PW_PokemonHiveDiglett"
                 || x.defName == "PW_PokemonHiveRhyhorn"
                 || x.defName == "PW_PokemonHivePhanpy"
                 || x.defName == "PW_PokemonHiveAron"
        ).RandomElement();
        return def;
    }

    public static ThingDef GetInfestationPokemonHiveDef()
    {
        var def = DefDatabase<ThingDef>.AllDefs.Where(
            x => x.defName == "PW_InsectPokemonHive"
                 || x.defName == "PW_GroundPokemonHive"
        ).RandomElement();
        return def;
    }
}

[StaticConstructorOnStartup]
public class TunnelPokemonHiveSpawner : ThingWithComps
{
    private static readonly MaterialPropertyBlock matPropertyBlock = new();

    [TweakValue("Gameplay", 0f, 1f)]
    private static readonly float DustMoteSpawnMTB = 0.2f;

    [TweakValue("Gameplay", 0f, 1f)]
    private static readonly float FilthSpawnMTB = 0.3f;

    [TweakValue("Gameplay", 0f, 10f)]
    private static readonly float FilthSpawnRadius = 3f;

    private static readonly Material TunnelMaterial = MaterialPool.MatFrom(
        "Things/Filth/Grainy/GrainyA", ShaderDatabase.Transparent
    );

    private static readonly List<ThingDef> filthTypes = new();

    private readonly FloatRange ResultSpawnDelay = new(26f, 30f);

    public float insectsPoints;

    public ThingDef pokemonHiveDef;
    private int secondarySpawnTick;

    public bool spawnedByInfestationThingComp;

    public bool spawnHive = true;

    private Sustainer sustainer;

    public static void ResetStaticData()
    {
        filthTypes.Clear();
        filthTypes.Add(ThingDefOf.Filth_Dirt);
        filthTypes.Add(ThingDefOf.Filth_Dirt);
        filthTypes.Add(ThingDefOf.Filth_Dirt);
        filthTypes.Add(ThingDefOf.Filth_RubbleRock);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref secondarySpawnTick, "PW_secondarySpawnTick");
        Scribe_Values.Look(ref spawnHive, "PW_spawnHive", true);
        Scribe_Values.Look(ref insectsPoints, "PW_insectsPoints");
        Scribe_Values.Look(ref spawnedByInfestationThingComp, "PW_spawnedByInfestationThingComp");
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        ResetStaticData();
        if (!respawningAfterLoad)
            secondarySpawnTick = Find.TickManager.TicksGame + ResultSpawnDelay.RandomInRange.SecondsToTicks();
        CreateSustainer();
    }

    public override void Tick()
    {
        if (!Spawned) return;
        sustainer.Maintain();
        var vector = Position.ToVector3Shifted();
        if (Rand.MTBEventOccurs(FilthSpawnMTB, 1f, 1.TicksToSeconds()) && CellFinder.TryFindRandomReachableNearbyCell(
                Position, Map, FilthSpawnRadius, TraverseParms.For(TraverseMode.NoPassClosedDoors), null, null,
                out var result
            )) FilthMaker.TryMakeFilth(result, Map, filthTypes.RandomElement());
        if (Rand.MTBEventOccurs(DustMoteSpawnMTB, 1f, 1.TicksToSeconds()))
        {
            var loc = new Vector3(vector.x, 0f, vector.z)
            {
                y = AltitudeLayer.MoteOverhead.AltitudeFor()
            };
            FleckMaker.ThrowDustPuffThick(loc, Map, Rand.Range(1.5f, 3f), new Color(1f, 1f, 1f, 2.5f));
        }

        if (secondarySpawnTick > Find.TickManager.TicksGame) return;
        sustainer.End();
        var map = Map;
        var position = Position;
        Destroy();
        if (spawnHive)
        {
            var obj = (Hive)GenSpawn.Spawn(ThingMaker.MakeThing(pokemonHiveDef), position, map);
            obj.SetFaction(Find.FactionManager.AllFactions.Where(f => f.def.defName == "PW_HostilePokemon").First());
            obj.questTags = questTags;
            foreach (var comp in obj.GetComps<CompSpawner>())
                if (comp.PropsSpawner.thingToSpawn == ThingDefOf.InsectJelly)
                {
                    comp.TryDoSpawn();
                    break;
                }
        }

        if (!(insectsPoints > 0f)) return;
        insectsPoints = Mathf.Max(
            insectsPoints,
            pokemonHiveDef.GetCompProperties<CompProperties_SpawnerPawn>().spawnablePawnKinds.Min(x => x.combatPower)
        );
        var pointsLeft = insectsPoints;
        var list = new List<Pawn>();
        var num = 0;
        PawnKindDef result2;
        for (; pointsLeft > 0f; pointsLeft -= result2.combatPower)
        {
            num++;
            if (num > 1000)
            {
                Log.Error("Too many iterations.");
                break;
            }

            if (!pokemonHiveDef.GetCompProperties<CompProperties_SpawnerPawn>().spawnablePawnKinds
                    .Where(x => x.combatPower <= pointsLeft).TryRandomElement(out result2)) break;
            var pawn = PawnGenerator.GeneratePawn(
                result2, Find.FactionManager.AllFactions.Where(f => f.def.defName == "PW_HostilePokemon").First()
            );
            GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(position, map, 2), map);
            pawn.mindState.spawnedByInfestationThingComp = spawnedByInfestationThingComp;
            list.Add(pawn);
        }

        if (list.Any())
            LordMaker.MakeNewLord(
                Find.FactionManager.AllFactions.Where(f => f.def.defName == "PW_HostilePokemon").First(),
                new LordJob_AssaultColony(
                    Find.FactionManager.AllFactions.Where(f => f.def.defName == "PW_HostilePokemon").First(), true,
                    false
                ), map, list
            );
    }

    protected override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        Rand.PushState();
        Rand.Seed = thingIDNumber;
        for (var i = 0; i < 6; i++)
            DrawDustPart(drawLoc, Rand.Range(0f, 360f), Rand.Range(0.9f, 1.1f) * Rand.Sign * 4f, Rand.Range(1f, 1.5f));
        Rand.PopState();
    }

    private void DrawDustPart(Vector3 drawLoc, float initialAngle, float speedMultiplier, float scale)
    {
        var num = (Find.TickManager.TicksGame - secondarySpawnTick).TicksToSeconds();
        var pos = drawLoc.ToIntVec3().ToVector3ShiftedWithAltitude(AltitudeLayer.Filth);
        pos.y += 3f / 70f * Rand.Range(0f, 1f);
        var value = new Color(0.470588237f, 98f / 255f, 83f / 255f, 0.7f);
        matPropertyBlock.SetColor(ShaderPropertyIDs.Color, value);
        var matrix = Matrix4x4.TRS(
            pos, Quaternion.Euler(0f, initialAngle + speedMultiplier * num, 0f), Vector3.one * scale
        );
        Graphics.DrawMesh(MeshPool.plane10, matrix, TunnelMaterial, 0, null, 0, matPropertyBlock);
    }

    private void CreateSustainer()
    {
        LongEventHandler.ExecuteWhenFinished(
            delegate
            {
                var tunnel = SoundDefOf.Tunnel;
                sustainer = tunnel.TrySpawnSustainer(SoundInfo.InMap(this, MaintenanceType.PerTick));
            }
        );
    }
}
