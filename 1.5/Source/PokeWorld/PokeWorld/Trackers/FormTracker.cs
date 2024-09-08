using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace PokeWorld;

public class FormTracker : IExposable
{
    public CompPokemon comp;

    public int currentFormIndex = -1;
    public Pawn pokemonHolder;

    public FormTracker(CompPokemon comp)
    {
        this.comp = comp;
        pokemonHolder = comp.Pokemon;
        if (comp.FormChangerCondition == FormChangerCondition.Fixed)
        {
            ChangeIntoFixed(comp.Forms.RandomElementByWeight(form => form.weight));
        }
        else if (currentFormIndex == -1)
        {
            var forms = comp.Forms.Where(x => x.isDefault);
            if (forms.Any()) ChangeIntoFixed(forms.First());
        }
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref currentFormIndex, "PW_currentForm", -1);
    }

    public bool TryInheritFormFromPreEvo(FormTracker preEvoFormTracker)
    {
        if (comp.FormChangerCondition == FormChangerCondition.Fixed)
        {
            var key = preEvoFormTracker.GetCurrentFormKey();
            if (key != "")
            {
                var forms = comp.Forms.Where(form => form.texPathKey == key);
                if (forms.Any())
                {
                    ChangeIntoFixed(forms.First());
                    return true;
                }
            }
        }

        return false;
    }

    public bool TryInheritFormFromParent(FormTracker parentFormTracker)
    {
        if (comp.FormChangerCondition == FormChangerCondition.Fixed)
        {
            var key = parentFormTracker.GetCurrentFormKey();
            if (key != "")
            {
                var forms = comp.Forms.Where(form => form.texPathKey == key);
                if (forms.Any())
                {
                    ChangeIntoFixed(forms.First());
                    return true;
                }
            }
        }

        return false;
    }

    public IEnumerable<Gizmo> GetGizmos()
    {
        if (pokemonHolder.Faction != null && pokemonHolder.Faction.IsPlayer)
            if (comp.FormChangerCondition == FormChangerCondition.Selectable)
            {
                var command_Action = new Command_Action
                {
                    action = delegate { ProcessInput(); }
                };
                if (currentFormIndex != -1)
                    command_Action.defaultLabel = "PW_FormName".Translate(comp.Forms[currentFormIndex].label);
                else
                    command_Action.defaultLabel = "PW_BaseForm".Translate();
                command_Action.defaultDesc = "PW_ChangeForm".Translate();
                command_Action.hotKey = KeyBindingDefOf.Misc3;
                command_Action.icon =
                    ContentFinder<Texture2D>.Get(pokemonHolder.Drawer.renderer.BodyGraphic.path + "_east");
                yield return command_Action;
            }
    }

    private bool CanUseForm(PokemonForm form)
    {
        if (comp.Forms.Contains(form) && CheckTimeOfDay(form) && CheckWeather(form) && CheckBiome(form)) return true;
        return false;
    }

    private bool CheckWeather(PokemonForm form)
    {
        var currentWeather = pokemonHolder.Map.weatherManager.curWeather;
        if ((form.includeWeathers == null || form.includeWeathers.Contains(currentWeather)) &&
            (form.excludeWeathers == null || !form.excludeWeathers.Contains(currentWeather))) return true;
        return false;
    }

    private bool CheckTimeOfDay(PokemonForm form)
    {
        if (form.timeOfDay == TimeOfDay.Any) return true;
        var currentMapTime = GenLocalDate.HourOfDay(pokemonHolder.Map);
        if (form.timeOfDay == TimeOfDay.Day)
            if (7 <= currentMapTime && currentMapTime < 19)
                return true;
        if (form.timeOfDay == TimeOfDay.Night)
            if (19 <= currentMapTime || currentMapTime < 7)
                return true;
        return false;
    }

    private bool CheckBiome(PokemonForm form)
    {
        var currentBiome = pokemonHolder.Map.Biome;
        if ((form.includeBiomes == null || form.includeBiomes.Contains(currentBiome)) &&
            (form.excludeBiomes == null || !form.excludeBiomes.Contains(currentBiome))) return true;
        return false;
    }

    public string GetCurrentFormKey()
    {
        if (currentFormIndex == -1)
            return "";
        return comp.Forms[currentFormIndex].texPathKey;
    }

    private void ReturnToBaseForm()
    {
        currentFormIndex = -1;
        pokemonHolder.Drawer.renderer.SetAllGraphicsDirty();
    }

    private void ChangeInto(PokemonForm form)
    {
        currentFormIndex = comp.Forms.IndexOf(form);
        for (var i = 0; i < 6; i++) FleckMaker.ThrowDustPuff(pokemonHolder.Position, pokemonHolder.Map, 2f);
        pokemonHolder.Drawer.renderer.SetAllGraphicsDirty();
    }

    private void ChangeIntoFixed(PokemonForm form)
    {
        currentFormIndex = comp.Forms.IndexOf(form);
    }

    public void FormTick()
    {
        if (comp.FormChangerCondition == FormChangerCondition.Environment && pokemonHolder.Spawned)
            if (currentFormIndex == -1 || comp.Forms[currentFormIndex].isDefault ||
                !CanUseForm(comp.Forms[currentFormIndex]))
            {
                foreach (var form in comp.Forms)
                    if (!form.isDefault && CanUseForm(form))
                    {
                        ChangeInto(form);
                        return;
                    }

                if (currentFormIndex == -1 || !comp.Forms[currentFormIndex].isDefault)
                {
                    var forms = comp.Forms.Where(x => x.isDefault);
                    if (forms.Any())
                    {
                        ChangeInto(forms.First());
                        return;
                    }

                    if (currentFormIndex != -1) ReturnToBaseForm();
                }
            }
    }

    private bool CanKeepCurrentForm(PokemonForm form)
    {
        if (CheckTimeOfDay(form) && CheckWeather(form) && CheckBiome(form)) return true;
        return false;
    }

    private void ProcessInput()
    {
        var list = new List<FloatMenuOption>();
        foreach (var form in comp.Forms)
        {
            var floatMenuOption = new FloatMenuOption(
                form.label.CapitalizeFirst(), delegate
                {
                    if (comp.Forms[currentFormIndex] != form) ChangeInto(form);
                }
            );
            list.Add(floatMenuOption);
        }

        if (list.Count == 0)
        {
            Messages.Message("PW_NoFormsChangeInto".Translate(), MessageTypeDefOf.RejectInput, false);
            return;
        }

        var floatMenu = new FloatMenu(list)
        {
            vanishIfMouseDistant = true
        };
        Find.WindowStack.Add(floatMenu);
    }
}
