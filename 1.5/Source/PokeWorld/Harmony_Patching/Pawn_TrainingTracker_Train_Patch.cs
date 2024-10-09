using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace PokeWorld;

[HarmonyPatch(typeof(Pawn_TrainingTracker))]
[HarmonyPatch("Train")]
public class Pawn_TrainingTracker_Train_Patch
{
    public static bool Prefix(TrainableDef __0, Pawn __1, Pawn_TrainingTracker __instance)
    {
        if (__0 != DefDatabase<TrainableDef>.GetNamed("PW_TrainXp")) return true;
        var comp = __instance.pawn.TryGetComp<CompPokemon>();
        if (comp == null || __1 == null || __1.skills.GetSkill(SkillDefOf.Animals).TotallyDisabled) return false;
        var skillValue = __1.skills.GetSkill(SkillDefOf.Animals).Level;
        var expAmount = 10 + skillValue * 20 + (int)Mathf.Lerp(
            0, comp.levelTracker.totalExpForNextLevel / 5f, skillValue / 20f
        );
        expAmount = Mathf.Clamp(expAmount, 0, comp.levelTracker.totalExpForNextLevel * 3);
        comp.levelTracker.IncreaseExperience(expAmount);
        comp.friendshipTracker.ChangeFriendship(1);

        return false;

    }
}
