using RimWorld;
using UnityEngine;
using Verse;

namespace PokeWorld;

public static class PokemonMasterUtility
{
    public static float GetMasterObedienceRadius(Pawn pokemon)
    {
        var master = pokemon.playerSettings.Master;
        var skill = master.skills?.GetSkill(SkillDefOf.Animals);
        if (skill?.TotallyDisabled == false)
            return 5.9f + skill.Level;
        return 0;
    }

    public static void DrawObedienceRadiusRingAroundMaster(Pawn pokemon)
    {
        var master = pokemon.playerSettings.Master;
        if (Find.CurrentMap == null || master == null) return;
        var radius = GetMasterObedienceRadius(pokemon);
        if (radius > 0f && radius < GenRadial.MaxRadialPatternRadius)
            GenDraw.DrawRadiusRing(master.Position, radius, Color.blue);
    }

    public static bool IsPokemonMasterDrafted(Pawn pokemon)
    {
        if (pokemon.Faction is not { IsPlayer: true }) return false;
        var master = pokemon.playerSettings?.Master;
        if (master is not { Drafted: true }) return false;
        return master.Map == pokemon.Map;
    }

    public static bool IsPokemonInMasterRange(Pawn pokemon)
    {
        if (!IsPokemonMasterDrafted(pokemon)) return false;
        var distance = pokemon.Position.DistanceTo(pokemon.playerSettings.Master.Position);
        return distance <= GetMasterObedienceRadius(pokemon);
    }
}
