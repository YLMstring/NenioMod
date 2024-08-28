using BlueprintCore.Actions.Builder;
using BlueprintCore.Blueprints.CustomConfigurators.Classes;
using BlueprintCore.Blueprints.CustomConfigurators.UnitLogic.Abilities;
using BlueprintCore.Blueprints.References;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UI.Common;
using Kingmaker;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.Designers.EventConditionActionSystem.Events;
using Kingmaker.ElementsSystem;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using BlueprintCore.Actions.Builder.AVEx;
using BlueprintCore.Utils;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.View.MapObjects;

namespace NenioMod
{
    internal class PuzzleSkip
    {
        private static readonly string StyleDisplayName = "PuzzleSkip.Name";
        private static readonly string StyleDescription = "PuzzleSkip.Description";

        private const string StyleAbility = "PuzzleSkip.StyleAbility";
        private static readonly string StyleAbilityGuid = "{D468F387-8497-4C4A-B89A-7AD314FF6A90}";
        public static void StyleConfigure()
        {
            var icon = AbilityRefs.EuphoricTranquility.Reference.Get().Icon;

            var ability = AbilityConfigurator.New(StyleAbility, StyleAbilityGuid)
                .CopyFrom(
                AbilityRefs.Insanity,
                typeof(AbilitySpawnFx))
                .SetAnimation(Kingmaker.Visual.Animation.Kingmaker.Actions.UnitAnimationActionCastSpell.CastAnimationStyle.Immediate)
                .AddAbilityEffectRunAction(ActionsBuilder.New()
                    .Add<NenioWinWin>()
                    .Build())
                .SetDisplayName(StyleDisplayName)
                .SetDescription(StyleDescription)
                .SetIcon(icon)
                .SetCanTargetSelf(true)
                .SetSpellResistance(false)
                .SetRange(AbilityRange.Personal)
                .SetType(AbilityType.Special)
                .SetActionType(Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType.Free)
                .AddAbilityCasterInCombat(true)
                .SetActionBarAutoFillIgnored(true)
                .AddHideFeatureInInspect()
                .Configure();

            FeatureConfigurator.For(FeatureRefs.SkillAbilities)
                    .AddFacts([ability])
                    .Configure();
        }
    }

    internal class NenioWinWin : ContextAction
    {
        public override string GetCaption()
        {
            return "NenioWinWin";
        }

        public override void RunAction()
        {
            UIUtility.SendWarning("Detecting puzzles...");
            List<BlueprintEtude> list = new List<BlueprintEtude>();
            foreach (KeyValuePair<BlueprintEtude, EtudesSystem.EtudeState> kvp in Game.Instance.Player.EtudesSystem.m_EtudesData)
            {
                //UIUtility.SendWarning(kvp.Key.NameSafe() + " checked.");
                if (kvp.Key?.NameSafe().Contains("Puzzle") != true)
                {
                    continue;
                }
                if (kvp.Value != EtudesSystem.EtudeState.Started)
                {
                    //UIUtility.SendWarning(kvp.Key.NameSafe() + " not started.");
                    continue;
                }
                UIUtility.SendWarning(kvp.Key.NameSafe() + " detected.");
                list.Add(kvp.Key);
            }
            foreach (var etude in list)
            {
                
                var comps = etude.GetComponents<GenericInteractionTrigger>();
                foreach (var comp in comps)
                {
                    if (comp?.Actions?.Actions == null) continue;
                    FindEtude(comp.Actions);
                }
            }
            foreach (MapObjectEntityData mapObjectEntityData in Game.Instance.State.MapObjects)
            {
                if (!mapObjectEntityData.IsInFogOfWar)
                {
                    float num = Game.Instance.Player.GetMainPartyUnit().DistanceTo(mapObjectEntityData.View.Transform.position);
                    if (num > 30.Feet().Meters)
                    {
                        continue;
                    }
                    else 
                    {
                        InteractionDoorPart interactionDoorPart = mapObjectEntityData.Get<InteractionDoorPart>();
                        if (interactionDoorPart == null || interactionDoorPart.IsOpen)
                        {
                            continue;
                        }
                        UIUtility.SendWarning(mapObjectEntityData.ToString() + " opened.");
                        interactionDoorPart.Open();
                    }
                }
            }

        }
        private void FindEtude(ActionList actions)
        {
            if (actions?.Actions == null) return;
            foreach (var action in actions.Actions)
            {
                if (action is StartEtude std && std.Etude?.NameSafe().Contains("Puzzle") == true)
                {
                    UIUtility.SendWarning("Start " + std.Etude?.NameSafe());
                    action.RunAction();
                }
                if (action is Conditional cond)
                {
                    FindEtude(cond.IfTrue);
                    FindEtude(cond.IfFalse);
                }
            }
        }
    }
}
