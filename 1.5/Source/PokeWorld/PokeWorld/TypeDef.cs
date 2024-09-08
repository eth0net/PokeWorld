using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace PokeWorld;

public class TypeDef : Def
{
    [Unsaved]
    public Texture2D gizmoIcon = BaseContent.BadTex;

    public string gizmoIconPath;
    public float immuneFactor = 0;
    public List<TypeDef> immunities;
    public float notVeryEffeciveFactor = 0.5f;
    public List<TypeDef> resistances;
    public float superEffectiveFactor = 2;

    [Unsaved]
    public Texture2D uiIcon = BaseContent.BadTex;

    [NoTranslate]
    public string uiIconPath;

    public List<TypeDef> weaknesses;

    public override void PostLoad()
    {
        base.PostLoad();
        LongEventHandler.ExecuteWhenFinished(delegate { uiIcon = ContentFinder<Texture2D>.Get(uiIconPath); });
    }

    public float GetDamageMultiplier(TypeDef attackType)
    {
        if (resistances != null)
            foreach (var def in resistances)
                if (attackType == def)
                    return notVeryEffeciveFactor;
        if (weaknesses != null)
            foreach (var def in weaknesses)
                if (attackType == def)
                    return superEffectiveFactor;
        if (immunities != null)
            foreach (var def in immunities)
                if (attackType == def)
                    return immuneFactor;
        return 1;
    }
}
