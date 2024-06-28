using BlueprintCore.Blueprints.Configurators;
using BlueprintCore.Blueprints.CustomConfigurators.Classes;
using BlueprintCore.Blueprints.References;
using BlueprintCore.Utils;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Kingmaker.UnitLogic.Parts;
using BlueprintCore.Actions.Builder;
using Kingmaker.Designers;
using Kingmaker.Utility;
using Kingmaker.Controllers.Units;
using Kingmaker.RuleSystem.Rules.Abilities;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic;
using TurnBased.Controllers;
using Unity.Burst.CompilerServices;
using HarmonyLib;
using Kingmaker.GameModes;
using Kingmaker.Visual;
using Kingmaker.ElementsSystem;
using BlueprintCore.Actions.Builder.ContextEx;

namespace NenioMod
{
    internal class Deskari
    {
        private static readonly string Feat2Name = "FeatDeskari";
        public static readonly string Feat2Guid = "{1D8CD198-9A8F-4676-A33B-D9EC31FE8B69}";

        private static readonly string DisplayName2 = "FeatDeskari.Name";
        private static readonly string Description2 = "FeatDeskari.Description";

        public static void DeskariConfigure()
        {
            var icon = AbilityRefs.MountTargetAbility.Reference.Get().Icon;

            var action = ActionsBuilder.New().Add<ContextActionBaphometSummon>().Build();

            var feat = FeatureConfigurator.New(Feat2Name, Feat2Guid)
                    .SetDisplayName(DisplayName2)
                    .SetDescription(Description2)
                    .SetIcon(icon)
                    .AddCombatStateTrigger(combatStartActions: action)
                    .Configure();

            UnitConfigurator.For(UnitRefs.DemonLordDeskari)
                .AddFacts([feat])
                .Configure();

            UnitConfigurator.For(UnitRefs.Baphomet)
                .AddFacts([feat])
                .Configure();
        }

        internal class ContextActionBaphometSummon : ContextAction
        {
            public override string GetCaption()
            {
                return "Baphomet Summon";
            }

            public override void RunAction()
            {
                if (Context.MaybeCaster.Blueprint == UnitRefs.Baphomet.Reference.Get())
                {
                    if (Game.Instance.Player.MainCharacter.Value.Progression.MythicLevel < 10)
                    {
                        return;
                    }
                    foreach (var unit in GameHelper.GetTargetsAround(Target.Point, 100.Feet(), false, true))
                    {
                        if (unit.Blueprint == UnitRefs.DemonLordDeskari.Reference.Get())
                        {
                            dispel.Run();
                            return;
                        }
                    }
                    return;
                }
                foreach (var unit in GameHelper.GetTargetsAround(Target.Point, 100.Feet(), false, true))
                {
                    if (unit.Blueprint == UnitRefs.Baphomet.Reference.Get())
                    {
                        return;
                    }
                }
                Summon(Context.MaybeCaster, Target.Point);
            }
            public static void Summon(UnitEntityData caster, Vector3 position)
            {
                var unit = UnitRefs.Baphomet.Reference.Get();
                UnitEntityData maybeCaster = caster;
                Vector3 vector = position;
                UnitEntityView unitEntityView = unit.Prefab.Load(false, false);
                float radius = (unitEntityView != null) ? unitEntityView.Corpulence : 0.5f;
                FreePlaceSelector.PlaceSpawnPlaces(1, radius, vector);
                UnitEntityData unitEntityData = Game.Instance.EntityCreator.SpawnUnit(unit, vector, Quaternion.identity, maybeCaster.HoldingState, null);
                unitEntityData.Descriptor.SwitchFactions(caster.Faction, true);
                unitEntityData.GroupId = maybeCaster.GroupId;
                unitEntityData.UpdateGroup();
                //unitEntityData.Ensure<UnitPartRider>().Mount(maybeCaster, false);
            }

            public ActionList dispel = ActionsBuilder.New().CastSpell(AbilityRefs.Baphomet_BaphometDisjunction.ToString()).Build();
        }

        internal class BondController : BaseUnitController
        {
            public override void TickOnUnit(UnitEntityData unit)
            {
                
                try
                {
                    if (!unit.IsInCombat || !unit.View.IsMoving() || !unit.HasFact(Bond)) return;
                    foreach (var unit2 in GameHelper.GetTargetsAround(unit.Position, 100.Feet(), false, false))
                    {
                        if (unit2.HasFact(Bond))
                        {
                            unit2.Position = unit.Position;
                            break;
                        }
                    }
                }
                catch (Exception ex) { Main.Logger.Error("fail tick", ex); }

            }

            private static BlueprintFeatureReference Bond = BlueprintTool.GetRef<BlueprintFeatureReference>(Feat2Guid);
        }

        [HarmonyPatch(typeof(GameModesFactory), nameof(GameModesFactory.Initialize))]
        internal class PatchBond
        {
            static void Postfix()
            {
                GameModesFactory.Register(new BondController(), [GameModesFactory.Default]);
            }
        }
    }
}
