using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace PokeWorld;

public class PawnKindColumnDef : Def
{
    private const int IconWidth = 26;

    private static readonly Vector2 IconSize = new(26f, 26f);

    public int gap;

    public bool headerAlwaysInteractable;

    [NoTranslate]
    public string headerIcon;

    public Vector2 headerIconSize;

    [Unsaved]
    private Texture2D headerIconTex;

    [MustTranslate]
    public string headerTip;

    public bool ignoreWhenCalculatingOptimalTableSize;

    public bool moveWorkTypeLabelDown;

    public bool paintable;

    public bool sortable;

    public TrainableDef trainable;

    public int width = -1;

    public int widthPriority;
    public Type workerClass = typeof(PawnKindColumnWorker);

    [Unsaved]
    private PawnKindColumnWorker workerInt;

    public WorkTypeDef workType;

    public PawnKindColumnWorker Worker
    {
        get
        {
            if (workerInt == null)
            {
                workerInt = (PawnKindColumnWorker)Activator.CreateInstance(workerClass);
                workerInt.def = this;
            }

            return workerInt;
        }
    }

    public Texture2D HeaderIcon
    {
        get
        {
            if (headerIconTex == null && !headerIcon.NullOrEmpty())
                headerIconTex = ContentFinder<Texture2D>.Get(headerIcon);
            return headerIconTex;
        }
    }

    public Vector2 HeaderIconSize
    {
        get
        {
            if (headerIconSize != default) return headerIconSize;
            if (HeaderIcon != null) return IconSize;
            return Vector2.zero;
        }
    }

    public bool HeaderInteractable
    {
        get
        {
            if (!sortable && headerTip.NullOrEmpty()) return headerAlwaysInteractable;
            return true;
        }
    }
}
