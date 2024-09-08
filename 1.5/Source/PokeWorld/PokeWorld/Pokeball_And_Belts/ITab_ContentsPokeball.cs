using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace PokeWorld;

internal class ITab_ContentsPokeball : ITab
{
    protected const float TopPadding = 20f;

    protected const float SpaceBetweenItemsLists = 10f;

    protected const float ThingRowHeight = 28f;

    protected const float ThingIconSize = 28f;

    protected const float ThingLeftX = 36f;

    protected static List<Thing> tmpSingleThing = new();

    protected static readonly Color ThingLabelColor = ITab_Pawn_Gear.ThingLabelColor;

    protected static readonly Color ThingHighlightColor = ITab_Pawn_Gear.HighlightColor;


    private readonly List<Thing> listInt = new();

    private readonly List<Thing> thingsToSelect = new();

    public bool canRemoveThings = true;

    public string containedItemsKey;

    private float lastDrawnHeight;
    private Vector2 scrollPosition;

    public ITab_ContentsPokeball()
    {
        labelKey = "PW_TabPokeballContents";
        containedItemsKey = "PW_ContainedPokemon";
        canRemoveThings = false;
        size = new Vector2(460f, 450f);
    }

    public override bool IsVisible
    {
        get
        {
            if (SelThing is CryptosleepBall ball && ball.ContainedThing is Pawn pawn)
                return pawn.Faction == Faction.OfPlayer;
            return false;
        }
    }

    public IList<Thing> container
    {
        get
        {
            var cryptosleepBall = SelThing as CryptosleepBall;
            listInt.Clear();
            if (cryptosleepBall != null && cryptosleepBall.ContainedThing != null)
                listInt.Add(cryptosleepBall.ContainedThing);
            return listInt;
        }
    }

    protected override void FillTab()
    {
        thingsToSelect.Clear();
        var outRect = new Rect(default, size).ContractedBy(10f);
        outRect.yMin += 20f;
        var rect = new Rect(0f, 0f, outRect.width - 16f, Mathf.Max(lastDrawnHeight, outRect.height));
        Text.Font = GameFont.Small;
        Widgets.BeginScrollView(outRect, ref scrollPosition, rect);
        var curY = 0f;
        DoItemsLists(rect, ref curY);
        lastDrawnHeight = curY;
        Widgets.EndScrollView();
        if (thingsToSelect.Any())
        {
            ITab_Pawn_FormingCaravan.SelectNow(thingsToSelect);
            thingsToSelect.Clear();
        }
    }

    protected virtual void DoItemsLists(Rect inRect, ref float curY)
    {
        GUI.BeginGroup(inRect);
        Widgets.ListSeparator(ref curY, inRect.width, containedItemsKey.Translate());
        var container = this.container;
        var flag = false;
        for (var i = 0; i < container.Count; i++)
        {
            var t = container[i];
            if (t != null)
            {
                flag = true;
                tmpSingleThing.Clear();
                tmpSingleThing.Add(t);
                DoThingRow(
                    t.def, t.stackCount, tmpSingleThing, inRect.width, ref curY, delegate(int x) { OnDropThing(t, x); }
                );
                tmpSingleThing.Clear();
            }
        }

        if (!flag) Widgets.NoneLabel(ref curY, inRect.width);
        GUI.EndGroup();
    }

    protected virtual void OnDropThing(Thing t, int count)
    {
        GenDrop.TryDropSpawn(t.SplitOff(count), SelThing.Position, SelThing.Map, ThingPlaceMode.Near, out var _);
    }

    protected void DoThingRow(
        ThingDef thingDef, int count, List<Thing> things, float width, ref float curY, Action<int> discardAction
    )
    {
        var rect = new Rect(0f, curY, width, 28f);
        if (canRemoveThings)
        {
            if (count != 1 && Widgets.ButtonImage(
                    new Rect(rect.x + rect.width - 24f, rect.y + (rect.height - 24f) / 2f, 24f, 24f),
                    CaravanThingsTabUtility.AbandonSpecificCountButtonTex
                ))
                Find.WindowStack.Add(
                    new Dialog_Slider("RemoveSliderText".Translate(thingDef.label), 1, count, discardAction)
                );
            rect.width -= 24f;
            if (Widgets.ButtonImage(
                    new Rect(rect.x + rect.width - 24f, rect.y + (rect.height - 24f) / 2f, 24f, 24f),
                    CaravanThingsTabUtility.AbandonButtonTex
                ))
            {
                var value = thingDef.label;
                if (things.Count == 1 && things[0] is Pawn) value = ((Pawn)things[0]).LabelShortCap;
                Find.WindowStack.Add(
                    Dialog_MessageBox.CreateConfirmation(
                        "ConfirmRemoveItemDialog".Translate(value), delegate { discardAction(count); }
                    )
                );
            }

            rect.width -= 24f;
        }

        if (things.Count == 1)
            Widgets.InfoCardButton(rect.width - 24f, curY, things[0]);
        else
            Widgets.InfoCardButton(rect.width - 24f, curY, thingDef);
        rect.width -= 24f;
        if (Mouse.IsOver(rect))
        {
            GUI.color = ThingHighlightColor;
            GUI.DrawTexture(rect, TexUI.HighlightTex);
        }

        if (thingDef.DrawMatSingle != null && thingDef.DrawMatSingle.mainTexture != null)
        {
            var rect2 = new Rect(4f, curY, 28f, 28f);
            if (things.Count == 1)
                Widgets.ThingIcon(rect2, things[0]);
            else
                Widgets.ThingIcon(rect2, thingDef);
        }

        Text.Anchor = TextAnchor.MiddleLeft;
        GUI.color = ThingLabelColor;
        var rect3 = new Rect(36f, curY, rect.width - 36f, rect.height);
        var str = things.Count != 1 || count != things[0].stackCount
            ? GenLabel.ThingLabel(thingDef, null, count).CapitalizeFirst()
            : things[0].LabelCap;
        Text.WordWrap = false;
        Widgets.Label(rect3, str.Truncate(rect3.width));
        Text.WordWrap = true;
        TooltipHandler.TipRegion(rect, str);
        var comp = things[0].TryGetComp<CompPokemon>();
        if (comp != null)
        {
            var rect4 = new Rect(170f, curY, rect.width - 170f, rect.height);
            string str2 = "PW_LevelShort".Translate(comp.levelTracker.level.ToString());
            Text.WordWrap = false;
            Widgets.Label(rect4, str2.Truncate(rect4.width));
            Text.WordWrap = true;
            if (comp.Types != null)
            {
                var x = 0;
                foreach (var typeDef in comp.Types)
                {
                    var rect5 = new Rect(240f + 40f * x, curY + 7, 32, 14);
                    Widgets.DrawTextureFitted(rect5, typeDef.uiIcon, 1);
                }
            }
        }

        Text.Anchor = TextAnchor.UpperLeft;
        if (Widgets.ButtonInvisible(rect)) SelectLater(things);
        if (Mouse.IsOver(rect))
            for (var i = 0; i < things.Count; i++)
                TargetHighlighter.Highlight(things[i]);
        curY += 28f;
    }

    private void SelectLater(List<Thing> things)
    {
        thingsToSelect.Clear();
        thingsToSelect.AddRange(things);
    }
}
