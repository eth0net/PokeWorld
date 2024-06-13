using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace PokeWorld
{
    public class FormTracker : IExposable
    {
        public CompPokemon comp;
        public Pawn pokemonHolder;

        public int currentFormIndex = -1;

        public FormTracker(CompPokemon comp)
        {
            this.comp = comp;
            pokemonHolder = comp.Pokemon;
            if (comp.FormChangerCondition == FormChangerCondition.Fixed)
            {
                ChangeIntoFixed(comp.Forms.RandomElementByWeight((PokemonForm form) => form.weight));
            }
            else if (currentFormIndex == -1)
            {
                IEnumerable<PokemonForm> forms = comp.Forms.Where((PokemonForm x) => x.isDefault);
                if (forms.Any())
                {
                    ChangeIntoFixed(forms.First());
                    return;
                }
            }
        }
        public bool TryInheritFormFromPreEvo(FormTracker preEvoFormTracker)
        {
            if (comp.FormChangerCondition == FormChangerCondition.Fixed)
            {
                string key = preEvoFormTracker.GetCurrentFormKey();
                if (key != "")
                {
                    IEnumerable<PokemonForm> forms = comp.Forms.Where((PokemonForm form) => form.texPathKey == key);
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
                string key = parentFormTracker.GetCurrentFormKey();
                if (key != "")
                {
                    IEnumerable<PokemonForm> forms = comp.Forms.Where((PokemonForm form) => form.texPathKey == key);
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
            {
                if (comp.FormChangerCondition == FormChangerCondition.Selectable)
                {
                    Command_Action command_Action = new Command_Action
                    {
                        action = delegate
                        {
                            ProcessInput();
                        }
                    };
                    if (currentFormIndex != -1)
                    {
                        command_Action.defaultLabel = "PW_FormName".Translate(comp.Forms[currentFormIndex].label);
                    }
                    else
                    {
                        command_Action.defaultLabel = "PW_BaseForm".Translate();
                    }
                    command_Action.defaultDesc = "PW_ChangeForm".Translate();
                    command_Action.hotKey = KeyBindingDefOf.Misc3;
                    command_Action.icon = ContentFinder<Texture2D>.Get(pokemonHolder.Drawer.renderer.BodyGraphic.path + "_east");
                    yield return command_Action;
                }
            }
        }
        private bool CanUseForm(PokemonForm form)
        {
            if (comp.Forms.Contains(form) && CheckTimeOfDay(form) && CheckWeather(form) && CheckBiome(form))
            {
                return true;
            }
            return false;
        }
        private bool CheckWeather(PokemonForm form)
        {
            WeatherDef currentWeather = pokemonHolder.Map.weatherManager.curWeather;
            if ((form.includeWeathers == null || form.includeWeathers.Contains(currentWeather)) && (form.excludeWeathers == null || !form.excludeWeathers.Contains(currentWeather)))
            {
                return true;
            }
            return false;
        }
        private bool CheckTimeOfDay(PokemonForm form)
        {
            if (form.timeOfDay == TimeOfDay.Any)
            {
                return true;
            }
            int currentMapTime = GenLocalDate.HourOfDay(pokemonHolder.Map);
            if (form.timeOfDay == TimeOfDay.Day)
            {
                if (7 <= currentMapTime && currentMapTime < 19)
                {
                    return true;
                }
            }
            if (form.timeOfDay == TimeOfDay.Night)
            {
                if (19 <= currentMapTime || currentMapTime < 7)
                {
                    return true;
                }
            }
            return false;
        }
        private bool CheckBiome(PokemonForm form)
        {
            BiomeDef currentBiome = pokemonHolder.Map.Biome;
            if ((form.includeBiomes == null || form.includeBiomes.Contains(currentBiome)) && (form.excludeBiomes == null || !form.excludeBiomes.Contains(currentBiome)))
            {
                return true;
            }
            return false;
        }
        public string GetCurrentFormKey()
        {
            if (currentFormIndex == -1)
            {
                return "";
            }
            else
            {
                return comp.Forms[currentFormIndex].texPathKey;
            }
        }
        private void ReturnToBaseForm()
        {
            currentFormIndex = -1;
            pokemonHolder.Drawer.renderer.SetAllGraphicsDirty();
        }
        private void ChangeInto(PokemonForm form)
        {
            currentFormIndex = comp.Forms.IndexOf(form);
            for (int i = 0; i < 6; i++)
            {
                FleckMaker.ThrowDustPuff(pokemonHolder.Position, pokemonHolder.Map, 2f);
            }
            pokemonHolder.Drawer.renderer.SetAllGraphicsDirty();
        }
        private void ChangeIntoFixed(PokemonForm form)
        {
            currentFormIndex = comp.Forms.IndexOf(form);
        }
        public void FormTick()
        {
            if (comp.FormChangerCondition == FormChangerCondition.Environment && pokemonHolder.Spawned)
            {
                if (currentFormIndex == -1 || comp.Forms[currentFormIndex].isDefault || !CanUseForm(comp.Forms[currentFormIndex]))
                {
                    foreach (PokemonForm form in comp.Forms)
                    {
                        if (!form.isDefault && CanUseForm(form))
                        {
                            ChangeInto(form);
                            return;
                        }
                    }
                    if (currentFormIndex == -1 || !comp.Forms[currentFormIndex].isDefault)
                    {
                        IEnumerable<PokemonForm> forms = comp.Forms.Where((PokemonForm x) => x.isDefault);
                        if (forms.Any())
                        {
                            ChangeInto(forms.First());
                            return;
                        }
                        if (currentFormIndex != -1)
                        {
                            ReturnToBaseForm();
                        }
                    }
                }
            }
        }
        private bool CanKeepCurrentForm(PokemonForm form)
        {
            if (CheckTimeOfDay(form) && CheckWeather(form) && CheckBiome(form))
            {
                return true;
            }
            return false;
        }
        private void ProcessInput()
        {
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            foreach (PokemonForm form in comp.Forms)
            {
                FloatMenuOption floatMenuOption = new FloatMenuOption(form.label.CapitalizeFirst(), delegate
                {
                    if (comp.Forms[currentFormIndex] != form)
                    {
                        ChangeInto(form);
                    }
                });
                list.Add(floatMenuOption);
            }
            if (list.Count == 0)
            {
                Messages.Message("PW_NoFormsChangeInto".Translate(), MessageTypeDefOf.RejectInput, historical: false);
                return;
            }
            FloatMenu floatMenu = new FloatMenu(list)
            {
                vanishIfMouseDistant = true
            };
            Find.WindowStack.Add(floatMenu);
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref currentFormIndex, "PW_currentForm", -1);
        }
    }
}
