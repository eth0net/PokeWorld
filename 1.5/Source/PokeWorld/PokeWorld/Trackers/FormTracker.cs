using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace PokeWorld.Trackers;

public class FormTracker : IExposable
{
    public CompPokemon comp;

    public int currentFormIndex = -1;
    public Pawn pokemonHolder;

    public FormTracker(CompPokemon comp)
    {
        this.comp = comp;
        pokemonHolder = comp.Pokemon;
        if (comp.formChangerCondition == FormChangerCondition.Fixed)
        {
            ChangeIntoFixed(comp.forms.RandomElementByWeight(form => form.weight));
        }
        else if (currentFormIndex == -1)
        {
            var forms = comp.forms.Where(x => x.isDefault);
            if (forms.Any()) ChangeIntoFixed(forms.First());
        }
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref currentFormIndex, "PW_currentForm", -1);
    }

    public bool TryInheritFormFromPreEvo(FormTracker preEvoFormTracker)
    {
        if (comp.formChangerCondition == FormChangerCondition.Fixed)
        {
            var key = preEvoFormTracker.GetCurrentFormKey();
            if (key == "") return false;
            var forms = comp.forms.Where(form => form.texPathKey == key).ToList();
            if (!forms.Any()) return false;
            ChangeIntoFixed(forms.First());
            return true;
        }

        return false;
    }

    public bool TryInheritFormFromParent(FormTracker parentFormTracker)
    {
        if (comp.formChangerCondition != FormChangerCondition.Fixed) return false;
        var key = parentFormTracker.GetCurrentFormKey();
        if (key == "") return false;
        var forms = comp.forms.Where(form => form.texPathKey == key).ToList();
        if (!forms.Any()) return false;
        ChangeIntoFixed(forms.First());
        return true;
    }

    public IEnumerable<Gizmo> GetGizmos()
    {
        if (pokemonHolder.Faction is not { IsPlayer: true }) yield break;
        if (comp.formChangerCondition != FormChangerCondition.Selectable) yield break;
        var command_Action = new Command_Action
        {
            action = ProcessInput
        };
        command_Action.defaultLabel = currentFormIndex != -1
            ? "PW_FormName".Translate(comp.forms[currentFormIndex].label)
            : "PW_BaseForm".Translate();
        command_Action.defaultDesc = "PW_ChangeForm".Translate();
        command_Action.hotKey = KeyBindingDefOf.Misc3;
        command_Action.icon =
            ContentFinder<Texture2D>.Get(pokemonHolder.Drawer.renderer.BodyGraphic.path +
                                         "_east");
        yield return command_Action;
    }

    private bool CanUseForm(PokemonForm form)
    {
        return comp.forms.Contains(form) && CheckTimeOfDay(form) && CheckWeather(form) && CheckBiome(form);
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
        return comp.forms[currentFormIndex].texPathKey;
    }

    private void ReturnToBaseForm()
    {
        currentFormIndex = -1;
        pokemonHolder.Drawer.renderer.SetAllGraphicsDirty();
    }

    private void ChangeInto(PokemonForm form)
    {
        currentFormIndex = comp.forms.IndexOf(form);
        for (var i = 0; i < 6; i++) FleckMaker.ThrowDustPuff(pokemonHolder.Position, pokemonHolder.Map, 2f);
        pokemonHolder.Drawer.renderer.SetAllGraphicsDirty();
    }

    private void ChangeIntoFixed(PokemonForm form)
    {
        currentFormIndex = comp.forms.IndexOf(form);
    }

    public void FormTick()
    {
        if (comp.formChangerCondition != FormChangerCondition.Environment || !pokemonHolder.Spawned) return;
        if (currentFormIndex != -1 && !comp.forms[currentFormIndex].isDefault &&
            CanUseForm(comp.forms[currentFormIndex])) return;
        foreach (var form in comp.forms.Where(form => !form.isDefault && CanUseForm(form)))
        {
            ChangeInto(form);
            return;
        }

        if (currentFormIndex != -1 && comp.forms[currentFormIndex].isDefault) return;
        var forms = comp.forms.Where(x => x.isDefault).ToList();
        if (forms.Any())
        {
            ChangeInto(forms.First());
            return;
        }

        if (currentFormIndex != -1) ReturnToBaseForm();
    }

    private bool CanKeepCurrentForm(PokemonForm form)
    {
        return CheckTimeOfDay(form) && CheckWeather(form) && CheckBiome(form);
    }

    private void ProcessInput()
    {
        var list = comp.forms.Select(form => new FloatMenuOption(form.label.CapitalizeFirst(), delegate
            {
                if (comp.forms[currentFormIndex] != form) ChangeInto(form);
            }))
            .ToList();

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
