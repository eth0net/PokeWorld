using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace PokeWorld;

public class LegendaryQuestsTracker : WorldComponent
{
    private DefMap<QuestConditionDef, bool> questConditionDefMap = new();

    public LegendaryQuestsTracker(World world) : base(world)
    {
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Deep.Look(ref questConditionDefMap, "PW_questConditionDefMap");
    }

    public override void WorldComponentTick()
    {
        base.WorldComponentTick();
        if (Find.TickManager.TicksGame % 60000 == 0)
            foreach (var condition in DefDatabase<QuestConditionDef>.AllDefs)
                if (!questConditionDefMap[condition])
                    if (condition.CheckCompletion())
                    {
                        var quest = QuestUtility.GenerateQuestAndMakeAvailable(condition.questScriptDef, new Slate());
                        if (!quest.hidden && condition.questScriptDef.sendAvailableLetter)
                            QuestUtility.SendLetterQuestAvailable(quest);
                        questConditionDefMap[condition] = true;
                    }
    }
}
