using HarmonyLib;
using MissionLibrary;
using MissionLibrary.Controller;
using MissionLibrary.Extension;
using MissionLibrary.View;
using MissionSharedLibrary;
using MissionSharedLibrary.Provider;
using MissionSharedLibrary.Utilities;
using RTSCamera.CampaignGame.Behavior;
using RTSCamera.CampaignGame.Skills;
using RTSCamera.Config;
using RTSCamera.Config.HotKey;
using RTSCamera.Patch;
using RTSCamera.Patch.Fix;
using RTSCamera.src.Patch.Fix;
using SandBox.CampaignBehaviors;
using SandBox.Missions.MissionLogics.Arena;
using SandBox.Objects;
using System;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.MissionViews.SiegeWeapon;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.MountAndBlade.ViewModelCollection.HUD.FormationMarker;
using Module = TaleWorlds.MountAndBlade.Module;

namespace RTSCamera
{
    public class RTSCameraSubModule : MBSubModuleBase
    {
        public const string ModuleId = "RTSCamera";
        public const string OldModuleId = "EnhancedMission";

        private readonly Harmony _harmony = new Harmony("RTSCameraPatch");
        private bool _successPatch;

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

            try
            {
                Module.CurrentModule.GlobalTextManager.LoadGameTexts();
                Initialize();

                _successPatch = true;

                _harmony.Patch(
                    typeof(RangedSiegeWeaponView).GetMethod("HandleUserInput",
                        BindingFlags.Instance | BindingFlags.NonPublic),
                    prefix: new HarmonyMethod(
                        typeof(Patch_RangedSiegeWeaponView).GetMethod("HandleUserInput_Prefix",
                            BindingFlags.Static | BindingFlags.Public)));
                _harmony.Patch(
                    typeof(ArenaPracticeFightMissionController).GetMethod("StartPractice",
                        BindingFlags.Instance | BindingFlags.NonPublic),
                    prefix: new HarmonyMethod(
                        typeof(Patch_ArenaPracticeFightMissionController).GetMethod("StartPractice_Prefix",
                            BindingFlags.Static | BindingFlags.Public)));
                _harmony.Patch(
                    typeof(PassageUsePoint).GetMethod(nameof(PassageUsePoint.IsDisabledForAgent),
                        BindingFlags.Instance | BindingFlags.Public),
                    new HarmonyMethod(typeof(Patch_PassageUsePoint).GetMethod(
                        nameof(Patch_PassageUsePoint.IsDisabledForAgent_Prefix),
                        BindingFlags.Static | BindingFlags.Public)));
                _harmony.Patch(
                    typeof(TeamAIComponent).GetMethod("TickOccasionally",
                        BindingFlags.Instance | BindingFlags.Public),
                    prefix: new HarmonyMethod(typeof(Patch_TeamAIComponent).GetMethod(
                        nameof(Patch_TeamAIComponent.TickOccasionally_Prefix),
                        BindingFlags.Static | BindingFlags.Public)));

                _harmony.Patch(
                    typeof(MissionAgentLabelView).GetMethod("IsAllyInAllyTeam",
                        BindingFlags.Instance | BindingFlags.NonPublic),
                    prefix: new HarmonyMethod(typeof(Patch_MissionAgentLabelView).GetMethod("IsAllyInAllyTeam_Prefix",
                        BindingFlags.Static | BindingFlags.Public)));
                _harmony.Patch(
                    typeof(MissionBoundaryCrossingHandler).GetMethod("TickForMainAgent",
                        BindingFlags.Instance | BindingFlags.NonPublic),
                    prefix: new HarmonyMethod(
                        typeof(Patch_MissionBoundaryCrossingHandler).GetMethod("TickForMainAgent_Prefix",
                            BindingFlags.Static | BindingFlags.Public)));
                _harmony.Patch(
                    typeof(MissionFormationMarkerVM).GetMethod("RefreshFormationPositions",
                        BindingFlags.Instance | BindingFlags.NonPublic),
                    prefix: new HarmonyMethod(typeof(Patch_MissionFormationMarkerVM).GetMethod(
                        nameof(Patch_MissionFormationMarkerVM.RefreshFormationPositions_Prefix),
                        BindingFlags.Static | BindingFlags.Public)));

                var missionListenerOnMissionModeChange = typeof(IMissionListener).GetMethod("OnMissionModeChange", BindingFlags.Instance | BindingFlags.Public);

                var mapping = typeof(MissionScreen).GetInterfaceMap(missionListenerOnMissionModeChange.DeclaringType);
                var index = Array.IndexOf(mapping.InterfaceMethods, missionListenerOnMissionModeChange);
                _harmony.Patch(
                     mapping.TargetMethods[index],
                    prefix: new HarmonyMethod(typeof(Patch_MissionScreen).GetMethod("OnMissionModeChange_Prefix",
                        BindingFlags.Static | BindingFlags.Public)));

                Patch_MissionOrderVM.Patch();
                _successPatch &= Patch_CrosshairVM.Patch();
                _successPatch &= Patch_MissionGauntletSpectatorControl.Patch();
                _successPatch &= Patch_ScoreboardScreenWidget.Patch();
                _successPatch &= Patch_Mission_UpdateSceneTimeSpeed.Patch();
                //_successPatch &= Patch_OrderOfBattleVM.Patch();
                _successPatch &= Patch_MissionGauntletMainAgentEquipDropView.Patch();
                _successPatch &= Patch_MissionGauntletMainAgentEquipmentControllerView.Patch();
                _successPatch &= Patch_DeploymentMissionController.Patch();
                _successPatch &= Patch_SandboxBattleSpawnModel.Patch();
                _successPatch &= Patch_AgentHumanAILogic.Patch();
                _successPatch &= Patch_MissionOrderDeploymentControllerVM.Patch();
                _successPatch &= Patch_OrderTroopPlacer.Patch();
                // Use Patch to add game menu
                WatchBattleBehavior.Patch(_harmony);

                if (!UIConfig.DoNotUseGeneratedPrefabs && RTSCameraConfig.Get().OrderUIClickable)
                {
                    UIConfig.DoNotUseGeneratedPrefabs = true;
                }
            }
            catch (Exception e)
            {
                _successPatch = false;
                MBDebug.ConsolePrint(e.ToString());
            }
        }

        private void Initialize()
        {
            if (!Initializer.Initialize(ModuleId))
                return;
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();

            if (!SecondInitialize())
                return;

            if (!_successPatch)
            {
                InformationManager.DisplayMessage(new InformationMessage("RTS Camera: patch failed"));
            }

            Patch_MissionGauntletSingleplayerOrderUIHandler.Patch();
            Patch_MissionGauntletCrosshair.Patch(_harmony);
            Utility.ShouldDisplayMessage = RTSCameraConfig.Get().DisplayMessage;
            Utility.PrintUsageHint();
        }

        private bool SecondInitialize()
        {
            if (!Initializer.SecondInitialize())
                return false;

            RTSCameraGameKeyCategory.RegisterGameKeyCategory();

            //Global.RegisterProvider(
            //    VersionProviderCreator.Create(() => new RTSCameraAgentComponent.MissionStartingHandler(),
            //        new Version(1, 0, 0)), "RTSCameraAgentComponent.MissionStartingHandler");

            Global.GetProvider<AMissionStartingManager>().AddHandler(new MissionStartingHandler.MissionStartingHandler());

            var menuClassCollection = AMenuManager.Get().MenuClassCollection;
            AMenuManager.Get().OnMenuClosedEvent += RTSCameraConfig.OnMenuClosed;
            menuClassCollection.AddOptionClass(RTSCameraOptionClassFactory.CreateOptionClassProvider(menuClassCollection));
            return true;
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);

            game.GameTextManager.LoadGameTexts();
            AddCampaignBehavior(gameStarterObject);
        }

        public override void RegisterSubModuleObjects(bool isSavedCampaign)
        {
            base.RegisterSubModuleObjects(isSavedCampaign);
            RTSCameraSkillEffects.Initialize();

        }

        private void AddCampaignBehavior(object gameStarter)
        {
            if (gameStarter is CampaignGameStarter campaignGameStarter)
            {
                //campaignGameStarter.AddBehavior(new WatchBattleBehavior());
                campaignGameStarter.AddBehavior(new RTSCameraSkillBehavior());
            }
        }


        protected override void OnSubModuleUnloaded()
        {
            base.OnSubModuleUnloaded();
            MissionExtensionCollection.Clear();
            _harmony.UnpatchAll(_harmony.Id);
            Initializer.Clear();
        }
    }
}
