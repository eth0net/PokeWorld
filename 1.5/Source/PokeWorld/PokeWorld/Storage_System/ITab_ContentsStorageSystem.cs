using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace PokeWorld;

internal class ITab_ContentsStorageSystem : ITab
{
    private const float TopPadding = 20f;

    private const float ThingIconSize = 28f;

    private const float ThingRowHeight = 28f;

    private const float ThingLeftX = 36f;

    private const float StandardLineHeight = 22f;

    private const float InitialHeight = 450f;

    public static readonly Color ThingLabelColor = new(0.9f, 0.9f, 0.9f, 1f);

    public static readonly Color HighlightColor = new(0.5f, 0.5f, 0.5f, 1f);

    private static List<Thing> workingInvList = new();

    private bool flagSortDex;
    private bool flagSortLevel;
    private bool flagSortName;

    private List<Thing> listInt = new();
    private Vector2 scrollPosition = Vector2.zero;

    private float scrollViewHeight;

    public ITab_ContentsStorageSystem()
    {
        size = new Vector2(460f, 450f);
        labelKey = "PW_TabPCContents";
    }

    public override bool IsVisible => true;

    public IList<Thing> container
    {
        get
        {
            var portableComputer = SelThing as Building_PortableComputer;
            var storageSystem = Find.World.GetComponent<StorageSystem>();
            listInt.Clear();
            if (portableComputer != null && storageSystem.ContainedThing != null)
                listInt = storageSystem.ContainedThing;
            if (flagSortDex)
                return listInt.OrderBy(x => x.TryGetComp<CompPokemon>().PokedexNumber).ToList();
            if (flagSortName)
                return listInt.OrderBy(x => x.def.label).ToList();
            if (flagSortLevel)
                return listInt.OrderByDescending(x => x.TryGetComp<CompPokemon>().levelTracker.level).ToList();
            return listInt;
        }
    }

    protected override void FillTab()
    {
        Text.Font = GameFont.Small;
        var rect = new Rect(0f, 20f, size.x, size.y - 20f).ContractedBy(10f);
        var position = new Rect(rect.x, rect.y, rect.width, rect.height);
        GUI.BeginGroup(position);
        Text.Font = GameFont.Small;
        GUI.color = Color.white;
        var outRect = new Rect(0f, 0f, position.width, position.height);
        var viewRect = new Rect(0f, 0f, position.width - 16f, scrollViewHeight);
        Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
        var curY = 0f;
        DrawSortCheckbox(position.width - 220, ref curY);
        var storageSystem = Find.World.GetComponent<StorageSystem>();
        string header = "PW_StorageSystemContainedPokemon".Translate(
            storageSystem.GetDirectlyHeldThings().Count, storageSystem.maxCount
        );
        Widgets.ListSeparator(ref curY, viewRect.width, header);
        foreach (var item2 in container) DrawThingRow(ref curY, viewRect.width, item2);
        if (Event.current.type == EventType.Layout)
        {
            if (curY + 70f > 450f)
                size.y = Mathf.Min(curY + 70f, UI.screenHeight - 35 - 165f - 30f);
            else
                size.y = 450f;
            scrollViewHeight = curY + 20f;
        }

        Widgets.EndScrollView();
        GUI.EndGroup();
        GUI.color = Color.white;
        Text.Anchor = TextAnchor.UpperLeft;
    }

    public void DrawSortCheckbox(float x, ref float y)
    {
        var flag1 = flagSortDex;
        var flag2 = flagSortName;
        var flag3 = flagSortLevel;
        var rect1 = new Rect(x, y, 180, 28);
        Text.Anchor = TextAnchor.MiddleLeft;
        Text.WordWrap = false;
        Text.Font = GameFont.Small;
        Widgets.Label(rect1, "PW_StorageSystemOrderByNumber".Translate());
        Widgets.Checkbox(rect1.xMax, rect1.y, ref flagSortDex, 25, true);
        var rect2 = new Rect(x, y += 28, 180, 28);
        Widgets.Label(rect2, "PW_StorageSystemOrderByName".Translate());
        Widgets.Checkbox(rect2.xMax, rect2.y, ref flagSortName, 25, true);
        var rect3 = new Rect(x, y += 28, 180, 28);
        Widgets.Label(rect3, "PW_StorageSystemOrderByLevel".Translate());
        Widgets.Checkbox(rect3.xMax, rect3.y, ref flagSortLevel, 25, true);
        Text.WordWrap = true;
        if (flag1 != flagSortDex && flagSortDex)
        {
            flagSortName = false;
            flagSortLevel = false;
        }
        else if (flag2 != flagSortName && flagSortName)
        {
            flagSortDex = false;
            flagSortLevel = false;
        }
        else if (flag3 != flagSortLevel && flagSortLevel)
        {
            flagSortDex = false;
            flagSortName = false;
        }

        y += 2;
    }

    private void DrawThingRow(ref float y, float width, Thing thing, bool inventory = false)
    {
        var rect = new Rect(0f, y, width, 28f);
        Widgets.InfoCardButton(rect.width - 24f, y, thing);
        rect.width -= 24f;
        var rect2 = new Rect(rect.width - 24f, y, 24f, 24f);
        var flagPower = SelThing.TryGetComp<CompPowerTrader>().PowerOn;
        if (Mouse.IsOver(rect2))
        {
            if (flagPower)
                TooltipHandler.TipRegion(rect2, "DropThing".Translate());
            else
                TooltipHandler.TipRegion(rect2, "PW_StorageSystemPCNotPowered".Translate());
        }

        var color = flagPower ? Color.white : Color.grey;
        var mouseoverColor = flagPower ? GenUI.MouseoverColor : color;
        if (Widgets.ButtonImage(
                rect2, ContentFinder<Texture2D>.Get("UI/Buttons/Drop"), color, mouseoverColor, flagPower
            ) && flagPower)
        {
            SoundDefOf.Tick_High.PlayOneShotOnCamera();
            InterfaceDrop(thing);
        }

        rect.width -= 24f;

        if (Mouse.IsOver(rect))
        {
            GUI.color = HighlightColor;
            GUI.DrawTexture(rect, TexUI.HighlightTex);
        }

        if (thing.def.DrawMatSingle != null && thing.def.DrawMatSingle.mainTexture != null)
            Widgets.ThingIcon(new Rect(4f, y, 28f, 28f), thing);
        Text.Anchor = TextAnchor.MiddleLeft;
        GUI.color = ThingLabelColor;
        var rect5 = new Rect(36f, y, rect.width - 36f, rect.height);
        var text = thing.LabelCap;
        Text.WordWrap = false;
        Widgets.Label(rect5, text.Truncate(rect5.width));
        Text.WordWrap = true;
        var comp = thing.TryGetComp<CompPokemon>();
        if (comp != null)
        {
            var rect6 = new Rect(170f, y, rect.width - 170f, rect.height);
            var str2 = "Lv." + comp.levelTracker.level;
            Text.WordWrap = false;
            Widgets.Label(rect6, str2.Truncate(rect6.width));
            Text.WordWrap = true;
            var x = 0;
            foreach (var typeDef in comp.Types)
            {
                var rect7 = new Rect(240f + 40f * x, y + 7, 32, 14);
                Widgets.DrawTextureFitted(rect7, typeDef.uiIcon, 1);
                x++;
            }
        }

        if (Mouse.IsOver(rect))
        {
            var text2 = thing.DescriptionDetailed;
            if (thing.def.useHitPoints) text2 = text2 + "\n" + thing.HitPoints + " / " + thing.MaxHitPoints;
            TooltipHandler.TipRegion(rect, text2);
        }

        y += 28f;
    }

    private void InterfaceDrop(Thing t)
    {
        var thingWithComps = t as ThingWithComps;
        var pawn = t as Pawn;
        if (pawn != null)
            if (GenDrop.TryDropSpawn(pawn, SelThing.Position, SelThing.Map, ThingPlaceMode.Near, out var _))
                PutInBallUtility.PutPokemonInBall(pawn);
    }
}
