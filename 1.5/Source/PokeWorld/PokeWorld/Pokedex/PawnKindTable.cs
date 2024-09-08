using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace PokeWorld;

public class PawnKindTable
{
    private readonly List<float> cachedColumnWidths = new();

    private readonly List<LookTargets> cachedLookTargets = new();

    private readonly List<float> cachedRowHeights = new();

    private readonly List<bool> columnAtMaxWidth = new();

    private readonly List<bool> columnAtOptimalWidth = new();
    private readonly PawnKindTableDef def;

    private readonly Func<IEnumerable<PawnKindDef>> pawnKindsGetter;

    private float cachedHeaderHeight;

    private float cachedHeightNoScrollbar;

    private List<PawnKindDef> cachedPawnKinds = new();

    private Vector2 cachedSize;

    private bool dirty;

    private Vector2 fixedSize;

    private bool hasFixedSize;

    private int maxTableHeight;

    private int maxTableWidth;

    private int minTableHeight;

    private int minTableWidth;

    private Vector2 scrollPosition;

    private bool sortDescending;

    public PawnKindTable(
        PawnKindTableDef def, Func<IEnumerable<PawnKindDef>> pawnKindsGetter, int uiWidth, int uiHeight
    )
    {
        this.def = def;
        this.pawnKindsGetter = pawnKindsGetter;
        SetMinMaxSize(def.minWidth, uiWidth, 0, uiHeight);
        SetDirty();
    }

    public List<PawnKindColumnDef> ColumnsListForReading => def.columns;

    public PawnKindColumnDef SortingBy { get; private set; }

    public bool SortingDescending
    {
        get
        {
            if (SortingBy != null) return sortDescending;
            return false;
        }
    }

    public Vector2 Size
    {
        get
        {
            RecacheIfDirty();
            return cachedSize;
        }
    }

    public float HeightNoScrollbar
    {
        get
        {
            RecacheIfDirty();
            return cachedHeightNoScrollbar;
        }
    }

    public float HeaderHeight
    {
        get
        {
            RecacheIfDirty();
            return cachedHeaderHeight;
        }
    }

    public List<PawnKindDef> PawnKindsListForReading
    {
        get
        {
            RecacheIfDirty();
            return cachedPawnKinds;
        }
    }

    public void PawnKindTableOnGUI(Vector2 position)
    {
        if (Event.current.type == EventType.Layout) return;
        RecacheIfDirty();
        var num = cachedSize.x - 16f;
        var num2 = 0;
        for (var i = 0; i < def.columns.Count; i++)
        {
            var num3 = i != def.columns.Count - 1 ? (int)cachedColumnWidths[i] : (int)(num - num2);
            var rect = new Rect((int)position.x + num2, (int)position.y, num3, (int)cachedHeaderHeight);
            def.columns[i].Worker.DoHeader(rect, this);
            num2 += num3;
        }

        var outRect = new Rect(
            (int)position.x, (int)position.y + (int)cachedHeaderHeight, (int)cachedSize.x,
            (int)cachedSize.y - (int)cachedHeaderHeight
        );
        var viewRect = new Rect(0f, 0f, outRect.width - 16f, (int)cachedHeightNoScrollbar - (int)cachedHeaderHeight);
        Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
        var num4 = 0;
        for (var j = 0; j < cachedPawnKinds.Count; j++)
        {
            num2 = 0;
            if (!(num4 - scrollPosition.y + (int)cachedRowHeights[j] < 0f) &&
                !(num4 - scrollPosition.y > outRect.height))
            {
                GUI.color = new Color(1f, 1f, 1f, 0.2f);
                Widgets.DrawLineHorizontal(0f, num4, viewRect.width);
                GUI.color = Color.white;
                if (!CanAssignPawn(cachedPawnKinds[j])) GUI.color = Color.gray;
                var rect2 = new Rect(0f, num4, viewRect.width, (int)cachedRowHeights[j]);
                for (var k = 0; k < def.columns.Count; k++)
                {
                    var num5 = k != def.columns.Count - 1 ? (int)cachedColumnWidths[k] : (int)(num - num2);
                    var rect3 = new Rect(num2, num4, num5, (int)cachedRowHeights[j]);
                    def.columns[k].Worker.DoCell(rect3, cachedPawnKinds[j], this);
                    num2 += num5;
                }

                GUI.color = Color.white;
            }

            num4 += (int)cachedRowHeights[j];
        }

        Widgets.EndScrollView();
    }

    public void SetDirty()
    {
        dirty = true;
    }

    public void SetMinMaxSize(int minTableWidth, int maxTableWidth, int minTableHeight, int maxTableHeight)
    {
        this.minTableWidth = minTableWidth;
        this.maxTableWidth = maxTableWidth;
        this.minTableHeight = minTableHeight;
        this.maxTableHeight = maxTableHeight;
        hasFixedSize = false;
        SetDirty();
    }

    public void SetFixedSize(Vector2 size)
    {
        fixedSize = size;
        hasFixedSize = true;
        SetDirty();
    }

    public void SortBy(PawnKindColumnDef column, bool descending)
    {
        SortingBy = column;
        sortDescending = descending;
        SetDirty();
    }

    protected virtual bool CanAssignPawn(PawnKindDef p)
    {
        return true;
    }

    private void RecacheIfDirty()
    {
        if (dirty)
        {
            dirty = false;
            RecachePawns();
            RecacheRowHeights();
            cachedHeaderHeight = CalculateHeaderHeight();
            cachedHeightNoScrollbar = CalculateTotalRequiredHeight();
            RecacheSize();
            RecacheColumnWidths();
            RecacheLookTargets();
        }
    }

    private void RecachePawns()
    {
        cachedPawnKinds.Clear();
        cachedPawnKinds.AddRange(pawnKindsGetter());
        cachedPawnKinds = LabelSortFunction(cachedPawnKinds).ToList();
        if (SortingBy != null)
        {
            if (sortDescending)
                cachedPawnKinds.SortStable(SortingBy.Worker.Compare);
            else
                cachedPawnKinds.SortStable((a, b) => SortingBy.Worker.Compare(b, a));
        }

        cachedPawnKinds = PrimarySortFunction(cachedPawnKinds).ToList();
    }

    protected virtual IEnumerable<PawnKindDef> LabelSortFunction(IEnumerable<PawnKindDef> input)
    {
        return input.OrderBy(p => p.label);
    }

    protected virtual IEnumerable<PawnKindDef> PrimarySortFunction(IEnumerable<PawnKindDef> input)
    {
        return input;
    }

    private void RecacheColumnWidths()
    {
        var num = cachedSize.x - 16f;
        var minWidthsSum = 0f;
        RecacheColumnWidths_StartWithMinWidths(out minWidthsSum);
        if (minWidthsSum == num) return;
        if (minWidthsSum > num)
        {
            SubtractProportionally(minWidthsSum - num, minWidthsSum);
            return;
        }

        RecacheColumnWidths_DistributeUntilOptimal(num, ref minWidthsSum, out var noMoreFreeSpace);
        if (!noMoreFreeSpace) RecacheColumnWidths_DistributeAboveOptimal(num, ref minWidthsSum);
    }

    private void RecacheColumnWidths_StartWithMinWidths(out float minWidthsSum)
    {
        minWidthsSum = 0f;
        cachedColumnWidths.Clear();
        for (var i = 0; i < def.columns.Count; i++)
        {
            var minWidth = GetMinWidth(def.columns[i]);
            cachedColumnWidths.Add(minWidth);
            minWidthsSum += minWidth;
        }
    }

    private void RecacheColumnWidths_DistributeUntilOptimal(
        float totalAvailableSpaceForColumns, ref float usedWidth, out bool noMoreFreeSpace
    )
    {
        columnAtOptimalWidth.Clear();
        for (var i = 0; i < def.columns.Count; i++)
            columnAtOptimalWidth.Add(cachedColumnWidths[i] >= GetOptimalWidth(def.columns[i]));
        var num = 0;
        bool flag;
        bool flag2;
        do
        {
            num++;
            if (num >= 10000)
            {
                Log.Error("Too many iterations.");
                break;
            }

            var num2 = float.MinValue;
            for (var j = 0; j < def.columns.Count; j++)
                if (!columnAtOptimalWidth[j])
                    num2 = Mathf.Max(num2, def.columns[j].widthPriority);
            var num3 = 0f;
            for (var k = 0; k < cachedColumnWidths.Count; k++)
                if (!columnAtOptimalWidth[k] && def.columns[k].widthPriority == num2)
                    num3 += GetOptimalWidth(def.columns[k]);
            var num4 = totalAvailableSpaceForColumns - usedWidth;
            flag = false;
            flag2 = false;
            for (var l = 0; l < cachedColumnWidths.Count; l++)
            {
                if (columnAtOptimalWidth[l]) continue;
                if (def.columns[l].widthPriority != num2)
                {
                    flag = true;
                    continue;
                }

                var num5 = num4 * GetOptimalWidth(def.columns[l]) / num3;
                var num6 = GetOptimalWidth(def.columns[l]) - cachedColumnWidths[l];
                if (num5 >= num6)
                {
                    num5 = num6;
                    columnAtOptimalWidth[l] = true;
                    flag2 = true;
                }
                else
                {
                    flag = true;
                }

                if (num5 > 0f)
                {
                    cachedColumnWidths[l] += num5;
                    usedWidth += num5;
                }
            }

            if (usedWidth >= totalAvailableSpaceForColumns - 0.1f)
            {
                noMoreFreeSpace = true;
                break;
            }
        } while (flag && flag2);

        noMoreFreeSpace = false;
    }

    private void RecacheColumnWidths_DistributeAboveOptimal(float totalAvailableSpaceForColumns, ref float usedWidth)
    {
        columnAtMaxWidth.Clear();
        for (var i = 0; i < def.columns.Count; i++)
            columnAtMaxWidth.Add(cachedColumnWidths[i] >= GetMaxWidth(def.columns[i]));
        var num = 0;
        while (true)
        {
            num++;
            if (num >= 10000)
            {
                Log.Error("Too many iterations.");
                break;
            }

            var num2 = 0f;
            for (var j = 0; j < def.columns.Count; j++)
                if (!columnAtMaxWidth[j])
                    num2 += Mathf.Max(GetOptimalWidth(def.columns[j]), 1f);
            var num3 = totalAvailableSpaceForColumns - usedWidth;
            var flag = false;
            for (var k = 0; k < def.columns.Count; k++)
                if (!columnAtMaxWidth[k])
                {
                    var num4 = num3 * Mathf.Max(GetOptimalWidth(def.columns[k]), 1f) / num2;
                    var num5 = GetMaxWidth(def.columns[k]) - cachedColumnWidths[k];
                    if (num4 >= num5)
                    {
                        num4 = num5;
                        columnAtMaxWidth[k] = true;
                    }
                    else
                    {
                        flag = true;
                    }

                    if (num4 > 0f)
                    {
                        cachedColumnWidths[k] += num4;
                        usedWidth += num4;
                    }
                }

            if (!(usedWidth >= totalAvailableSpaceForColumns - 0.1f))
            {
                if (!flag)
                {
                    DistributeRemainingWidthProportionallyAboveMax(totalAvailableSpaceForColumns - usedWidth);
                    break;
                }

                continue;
            }

            break;
        }
    }

    private void RecacheRowHeights()
    {
        cachedRowHeights.Clear();
        for (var i = 0; i < cachedPawnKinds.Count; i++) cachedRowHeights.Add(CalculateRowHeight(cachedPawnKinds[i]));
    }

    private void RecacheSize()
    {
        if (hasFixedSize)
        {
            cachedSize = fixedSize;
            return;
        }

        var num = 0f;
        for (var i = 0; i < def.columns.Count; i++)
            if (!def.columns[i].ignoreWhenCalculatingOptimalTableSize)
                num += GetOptimalWidth(def.columns[i]);
        var a = Mathf.Clamp(num + 16f, minTableWidth, maxTableWidth);
        var a2 = Mathf.Clamp(cachedHeightNoScrollbar, minTableHeight, maxTableHeight);
        a = Mathf.Min(a, UI.screenWidth);
        a2 = Mathf.Min(a2, UI.screenHeight);
        cachedSize = new Vector2(a, a2);
    }

    private void RecacheLookTargets()
    {
        cachedLookTargets.Clear();
    }

    private void SubtractProportionally(float toSubtract, float totalUsedWidth)
    {
        for (var i = 0; i < cachedColumnWidths.Count; i++)
            cachedColumnWidths[i] -= toSubtract * cachedColumnWidths[i] / totalUsedWidth;
    }

    private void DistributeRemainingWidthProportionallyAboveMax(float toDistribute)
    {
        var num = 0f;
        for (var i = 0; i < def.columns.Count; i++) num += Mathf.Max(GetOptimalWidth(def.columns[i]), 1f);
        for (var j = 0; j < def.columns.Count; j++)
            cachedColumnWidths[j] += toDistribute * Mathf.Max(GetOptimalWidth(def.columns[j]), 1f) / num;
    }

    private float GetOptimalWidth(PawnKindColumnDef column)
    {
        return Mathf.Max(column.Worker.GetOptimalWidth(this), 0f);
    }

    private float GetMinWidth(PawnKindColumnDef column)
    {
        return Mathf.Max(column.Worker.GetMinWidth(this), 0f);
    }

    private float GetMaxWidth(PawnKindColumnDef column)
    {
        return Mathf.Max(column.Worker.GetMaxWidth(this), 0f);
    }

    private float CalculateRowHeight(PawnKindDef pawn)
    {
        var num = 0f;
        for (var i = 0; i < def.columns.Count; i++) num = Mathf.Max(num, def.columns[i].Worker.GetMinCellHeight(pawn));
        return num;
    }

    private float CalculateHeaderHeight()
    {
        var num = 0f;
        for (var i = 0; i < def.columns.Count; i++)
            num = Mathf.Max(num, def.columns[i].Worker.GetMinHeaderHeight(this));
        return num;
    }

    private float CalculateTotalRequiredHeight()
    {
        var num = CalculateHeaderHeight();
        for (var i = 0; i < cachedPawnKinds.Count; i++) num += CalculateRowHeight(cachedPawnKinds[i]);
        return num;
    }
}
