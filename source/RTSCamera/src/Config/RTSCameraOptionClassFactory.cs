using System.Reflection;
using MissionLibrary.Provider;
using MissionLibrary.View;
using MissionSharedLibrary.Provider;
using MissionSharedLibrary.Utilities;
using MissionSharedLibrary.View.ViewModelCollection;
using MissionSharedLibrary.View.ViewModelCollection.Options;
using MissionSharedLibrary.View.ViewModelCollection.Options.Selection;
using RTSCamera.CampaignGame.Behavior;
using RTSCamera.Logic;
using RTSCamera.View;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace RTSCamera.Config
{
    public class RTSCameraOptionClassFactory
    {
        public static IIdProvider<AOptionClass> CreateOptionClassProvider(IMenuClassCollection menuClassCollection)
        {
            return IdProviderCreator.Create(() =>
            {
                var optionClass = new OptionClass(RTSCameraSubModule.ModuleId,
                    GameTexts.FindText("str_rts_camera_option_class"), menuClassCollection);
                var rtsCameraLogic = Mission.Current.GetMissionBehavior<RTSCameraLogic>();
                var selectCharacterView = Mission.Current.GetMissionBehavior<RTSCameraSelectCharacterView>();
                var hideHudView = Mission.Current.GetMissionBehavior<HideHUDView>();
                var missionScreen = selectCharacterView.MissionScreen;
                var menuManager = AMenuManager.Get();

                var cameraOptionCategory = new OptionCategory("Camera", GameTexts.FindText("str_rts_camera_camera_options"));
                cameraOptionCategory.AddOption(new ActionOptionViewModel(
                    GameTexts.FindText("str_rts_camera_switch_free_camera"),
                    GameTexts.FindText("str_rts_camera_switch_free_camera_hint"),
                    () =>
                    {
                        rtsCameraLogic.SwitchFreeCameraLogic.SwitchCamera();
                        menuManager.RequestToCloseMenu();
                    }));
                cameraOptionCategory.AddOption(new BoolOptionViewModel(
                    GameTexts.FindText("str_rts_camera_use_free_camera_by_default"),
                    GameTexts.FindText("str_rts_camera_use_free_camera_by_default_hint"),
                    () => RTSCameraConfig.Get().UseFreeCameraByDefault,
                    b => RTSCameraConfig.Get().UseFreeCameraByDefault = b));
                cameraOptionCategory.AddOption(new NumericOptionViewModel(
                    GameTexts.FindText("str_rts_camera_raised_height_after_switching_to_free_camera"),
                    GameTexts.FindText("str_rts_camera_raised_height_hint"), () => RTSCameraConfig.Get().RaisedHeight,
                    f =>
                    {
                        RTSCameraConfig.Get().RaisedHeight = f;
                    }, 0, 50, true, true));
                if (!WatchBattleBehavior.WatchMode)
                {
                    if (Campaign.Current != null)
                    {
                        cameraOptionCategory.AddOption(new BoolOptionViewModel(
                            GameTexts.FindText("str_rts_camera_limit_camera_distance"),
                            GameTexts.FindText("str_rts_camera_limit_camera_distance_hint"),
                            () => RTSCameraConfig.Get().LimitCameraDistance,
                            b =>
                            {
                                RTSCameraConfig.Get().LimitCameraDistance = b;
                            }));
                        cameraOptionCategory.AddOption(new NumericOptionViewModel(
                            GameTexts.FindText("str_rts_camera_camera_distance_limit"),
                            new TextObject(RTSCameraSkillBehavior.UpdateCameraMaxDistance(true).GetExplanations()),
                            () => RTSCameraSkillBehavior.CameraDistanceLimit,
                            RTSCameraSkillBehavior.UpdateCameraDistanceLimit, 0, RTSCameraSkillBehavior.CameraDistanceMaxLimit, false, true));
                    }
                }
                cameraOptionCategory.AddOption(new BoolOptionViewModel(
                    GameTexts.FindText("str_rts_camera_camera_height_follows_terrain"),
                    GameTexts.FindText("str_rts_camera_camera_height_follows_terrain_hint"), () => RTSCameraConfig.Get().CameraHeightFollowsTerrain,
                    b =>
                    {
                        RTSCameraConfig.Get().CameraHeightFollowsTerrain = b;
                    }));
                cameraOptionCategory.AddOption(new BoolOptionViewModel(
                    GameTexts.FindText("str_rts_camera_constant_speed"),
                    GameTexts.FindText("str_rts_camera_constant_speed_hint"), () => RTSCameraConfig.Get().ConstantSpeed,
                    b =>
                    {
                        RTSCameraConfig.Get().ConstantSpeed = b;
                    }));
                cameraOptionCategory.AddOption(new BoolOptionViewModel(
                    GameTexts.FindText("str_rts_camera_ignore_terrain"),
                    GameTexts.FindText("str_rts_camera_ignore_terrain_hint"), () => RTSCameraConfig.Get().IgnoreTerrain,
                    b =>
                    {
                        RTSCameraConfig.Get().IgnoreTerrain = b;
                    }));
                cameraOptionCategory.AddOption(new BoolOptionViewModel(
                    GameTexts.FindText("str_rts_camera_ignore_boundaries"),
                        GameTexts.FindText("str_rts_camera_ignore_boundaries_hint"),
                        () => RTSCameraConfig.Get().IgnoreBoundaries,
                        b =>
                        {
                            RTSCameraConfig.Get().IgnoreBoundaries = b;
                        }));
                optionClass.AddOptionCategory(0, cameraOptionCategory);

                var controlOptionCategory = new OptionCategory("Control",
                    GameTexts.FindText("str_rts_camera_control_options"));
                if (!WatchBattleBehavior.WatchMode)
                {
                    controlOptionCategory.AddOption(new SelectionOptionViewModel(
                        GameTexts.FindText("str_rts_camera_player_controller_in_free_camera"),
                        GameTexts.FindText("str_rts_camera_player_controller_in_free_camera_hint"),
                        new SelectionOptionData(i =>
                        {
                            if (i < 0 || i >= (int)Agent.ControllerType.Count)
                                return;

                            RTSCameraConfig.Get().PlayerControllerInFreeCamera = i;
                            if (rtsCameraLogic.SwitchFreeCameraLogic.IsSpectatorCamera && !Utility.IsPlayerDead())
                            {
                                Utilities.Utility.UpdateMainAgentControllerInFreeCamera(Mission.Current.MainAgent, (Agent.ControllerType)i);
                                Utilities.Utility.UpdateMainAgentControllerState(Mission.Current.MainAgent, rtsCameraLogic.SwitchFreeCameraLogic.IsSpectatorCamera, (Agent.ControllerType)i);
                            }
                        }, () =>
                        {
                            if (rtsCameraLogic.SwitchFreeCameraLogic.IsSpectatorCamera && !Utility.IsPlayerDead())
                            {
                                if (Mission.Current.MainAgent.Controller == Agent.ControllerType.AI)
                                    return (int)Agent.ControllerType.AI;

                                var controller = Mission.Current.GetMissionBehavior<MissionMainAgentController>();
                                if (controller == null ||
                                    !((bool?)typeof(MissionMainAgentController)
                                        .GetField("_activated", BindingFlags.Instance | BindingFlags.NonPublic)
                                        ?.GetValue(controller) ?? true) ||
                                    Mission.Current.MainAgent.Controller == Agent.ControllerType.None)
                                {
                                    return (int)Agent.ControllerType.None;
                                }

                                return (int)Agent.ControllerType.Player;
                            }

                            return RTSCameraConfig.Get().PlayerControllerInFreeCamera;
                        }, (int)Agent.ControllerType.Count, new[]
                        {
                            new SelectionItem(true, "str_rts_camera_controller_type", "none"),
                            new SelectionItem(true, "str_rts_camera_controller_type", "AI"),
                            new SelectionItem(true, "str_rts_camera_controller_type", "Player")
                        }), true));
                }
                var playerFormationOption = new SelectionOptionViewModel(
                    GameTexts.FindText("str_rts_camera_player_formation"),
                    GameTexts.FindText("str_rts_camera_player_formation_hint"), new SelectionOptionData(
                        i =>
                        {
                            var config = RTSCameraConfig.Get();
                            if (i >= 0 && i <= (int)FormationClass.NumberOfAllFormations)
                            {
                                FormationClass formationClass = (FormationClass)i;

                                rtsCameraLogic.SwitchFreeCameraLogic.CurrentPlayerFormation = formationClass;
                                config.PlayerFormation = formationClass;

                                Agent mainAgent = Mission.Current.MainAgent;
                                bool? isControllingOwnCharacter = mainAgent.Character?.IsPlayerCharacter;
                                if (isControllingOwnCharacter == true)
                                {
                                    Utility.SetMainAgentFormationClass(formationClass);
                                }
                            }
                        }, () =>
                        {
                            return (int) RTSCameraConfig.Get().PlayerFormation;
                        },
                        (int)FormationClass.NumberOfRegularFormations, new[]
                        {
                            new SelectionItem(true, "str_troop_group_name", "0"),
                            new SelectionItem(true, "str_troop_group_name", "1"),
                            new SelectionItem(true, "str_troop_group_name", "2"),
                            new SelectionItem(true, "str_troop_group_name", "3"),
                            new SelectionItem(true, "str_troop_group_name", "4"),
                            new SelectionItem(true, "str_troop_group_name", "5"),
                            new SelectionItem(true, "str_troop_group_name", "6"),
                            new SelectionItem(true, "str_troop_group_name", "7"),
                            new SelectionItem(true, "str_troop_group_name", "8"),
                            new SelectionItem(true, "str_troop_group_name", "9"),
                            new SelectionItem(true, "str_rts_camera_player_formation_unset")
                        }), true, true);
                controlOptionCategory.AddOption(playerFormationOption);

                controlOptionCategory.AddOption(new SelectionOptionViewModel(
                    GameTexts.FindText("str_rts_camera_watch_another_hero"),
                    GameTexts.FindText("str_rts_camera_watch_another_hero_hint"),
                    new WatchAgentSelectionData(missionScreen).SelectionOptionData, true));
                controlOptionCategory.AddOption(new ActionOptionViewModel(
                    GameTexts.FindText("str_rts_camera_select_character"),
                    GameTexts.FindText("str_rts_camera_select_character_hint"),
                    () =>
                    {
                        selectCharacterView.IsSelectingCharacter = true;
                        menuManager.RequestToCloseMenu();
                    }));
                controlOptionCategory.AddOption(new BoolOptionViewModel(
                    GameTexts.FindText("str_rts_camera_control_ally_after_death"),
                    GameTexts.FindText("str_rts_camera_control_ally_after_death_hint"),
                    () => RTSCameraConfig.Get().ControlAllyAfterDeath,
                    b => RTSCameraConfig.Get().ControlAllyAfterDeath = b));
                controlOptionCategory.AddOption(new BoolOptionViewModel(
                    GameTexts.FindText("str_rts_camera_prefer_unit_in_same_formation"),
                    GameTexts.FindText("str_rts_camera_prefer_unit_in_same_formation_hint"),
                    () => RTSCameraConfig.Get().PreferUnitsInSameFormation,
                    b => RTSCameraConfig.Get().PreferUnitsInSameFormation = b));
                controlOptionCategory.AddOption(new BoolOptionViewModel(
                    GameTexts.FindText("str_rts_camera_control_troops_in_player_party_only"),
                    GameTexts.FindText("str_rts_camera_control_troops_in_player_party_only_hint"),
                    () => RTSCameraConfig.Get().ControlTroopsInPlayerPartyOnly,
                    b => RTSCameraConfig.Get().ControlTroopsInPlayerPartyOnly = b));
                controlOptionCategory.AddOption(new BoolOptionViewModel(
                    GameTexts.FindText("str_rts_camera_ignore_retreating_troops"),
                    GameTexts.FindText("str_rts_camera_ignore_retreating_troops_hint"),
                    () => RTSCameraConfig.Get().IgnoreRetreatingTroops,
                    b => RTSCameraConfig.Get().IgnoreRetreatingTroops = b));
                optionClass.AddOptionCategory(0, controlOptionCategory);

                var miscellaneousOptionCategory = new OptionCategory("Miscellaneous",
                    GameTexts.FindText("str_rts_camera_miscellaneous_options"));
                miscellaneousOptionCategory.AddOption(new ActionOptionViewModel(GameTexts.FindText("str_rts_camera_toggle_pause"), GameTexts.FindText("str_rts_camera_toggle_pause_hint"),
                    () =>
                    {
                        rtsCameraLogic.MissionSpeedLogic?.TogglePause();
                        menuManager.RequestToCloseMenu();
                    }));
                miscellaneousOptionCategory.AddOption(new BoolOptionViewModel(
                    GameTexts.FindText("str_rts_camera_slow_motion_mode"),
                    GameTexts.FindText("str_rts_camera_slow_motion_hint"), () => RTSCameraConfig.Get().SlowMotionMode,
                    b => rtsCameraLogic.MissionSpeedLogic.SetSlowMotionMode(b)));
                miscellaneousOptionCategory.AddOption(new NumericOptionViewModel(
                    GameTexts.FindText("str_rts_camera_slow_motion_factor"),
                    GameTexts.FindText("str_rts_camera_slow_motion_factor_hint"),
                    () => RTSCameraConfig.Get().SlowMotionFactor,
                    f => rtsCameraLogic.MissionSpeedLogic.SetSlowMotionFactor(f), 0, 3, false, true));
                miscellaneousOptionCategory.AddOption(new BoolOptionViewModel(
                    GameTexts.FindText("str_rts_camera_display_mod_message"),
                    GameTexts.FindText("str_rts_camera_display_message_hint"),
                    () => RTSCameraConfig.Get().DisplayMessage, b =>
                    {
                        RTSCameraConfig.Get().DisplayMessage = b;
                        Utility.ShouldDisplayMessage = b;
                    }));
                miscellaneousOptionCategory.AddOption(new ActionOptionViewModel(GameTexts.FindText("str_rts_camera_toggle_ui"), GameTexts.FindText("str_rts_camera_toggle_ui_hint"),
                    () =>
                    {
                        hideHudView?.ToggleUI();
                        menuManager.RequestToCloseMenu();
                    }));
                miscellaneousOptionCategory.AddOption(new BoolOptionViewModel(
                    GameTexts.FindText("str_rts_camera_order_ui_clickable"),
                    GameTexts.FindText("str_rts_camera_order_ui_clickable_hint"),
                    () => RTSCameraConfig.Get().OrderUIClickable,
                    b => RTSCameraConfig.Get().OrderUIClickable = UIConfig.DoNotUseGeneratedPrefabs = b));
                miscellaneousOptionCategory.AddOption(new BoolOptionViewModel(
                    GameTexts.FindText("str_rts_camera_fix_companion_formation"),
                    GameTexts.FindText("str_rts_camera_fix_companion_formation_hint"),
                    () => RTSCameraConfig.Get().FixCompanionFormation,
                    b => RTSCameraConfig.Get().FixCompanionFormation = b));
                optionClass.AddOptionCategory(1, miscellaneousOptionCategory);

                miscellaneousOptionCategory.AddOption(new BoolOptionViewModel(
                    GameTexts.FindText("str_rts_camera_highlight_own_selected_formation"),
                    GameTexts.FindText("str_rts_camera_highlight_own_selected_formation_hint"),
                    () => RTSCameraConfig.Get().HighlightOwnSelectedFormation,
                    b => RTSCameraConfig.Get().HighlightOwnSelectedFormation = b));
                miscellaneousOptionCategory.AddOption(new BoolOptionViewModel(
                    GameTexts.FindText("str_rts_camera_highlight_enemy_target_formation"),
                    GameTexts.FindText("str_rts_camera_highlight_enemy_target_formation_hint"),
                    () => RTSCameraConfig.Get().HighlightEnemyTargetFormation,
                    b => RTSCameraConfig.Get().HighlightEnemyTargetFormation = b));
                miscellaneousOptionCategory.AddOption(new BoolOptionViewModel(
                    GameTexts.FindText("str_rts_camera_more_visible_move_location"),
                    GameTexts.FindText("str_rts_camera_more_visible_move_location_hint"),
                    () => RTSCameraConfig.Get().MoreVisibleMoveLocation,
                    b => RTSCameraConfig.Get().MoreVisibleMoveLocation = b));


                optionClass.AddOptionCategory(1, miscellaneousOptionCategory);
                if (NativeConfig.CheatMode)
                {
                    var cheatOptionCategory = new OptionCategory("Cheat",
                        GameTexts.FindText("str_rts_camera_unbalanced_options_description"));
                    cheatOptionCategory.AddOption(new BoolOptionViewModel(
                        GameTexts.FindText("str_rts_camera_all_invulnerable"),
                        GameTexts.FindText("str_rts_camera_all_invulnerable_hint"),
                        () => rtsCameraLogic.DisableDeathLogic.GetDisableDeath(),
                        b =>
                        {
                            rtsCameraLogic.DisableDeathLogic.SetDisableDeath(b);
                        }));
                    cheatOptionCategory.AddOption(new BoolOptionViewModel(
                        GameTexts.FindText("str_rts_camera_enable_all_invulnerable_hotkey"), null,
                        () => RTSCameraConfig.Get().DisableDeathHotkeyEnabled,
                        b => RTSCameraConfig.Get().DisableDeathHotkeyEnabled = b));
                    cheatOptionCategory.AddOption(new ActionOptionViewModel(GameTexts.FindText("str_rts_camera_switch_team"), GameTexts.FindText("str_rts_camera_switch_team_hint"),
                        () =>
                        {
                            rtsCameraLogic.SwitchTeamLogic.SwapTeam();
                            menuManager.RequestToCloseMenu();
                        }));
                    cheatOptionCategory.AddOption(new BoolOptionViewModel(
                        GameTexts.FindText("str_rts_camera_enable_switch_team_hotkey"), null,
                        () => RTSCameraConfig.Get().SwitchTeamHotkeyEnabled,
                        b => RTSCameraConfig.Get().SwitchTeamHotkeyEnabled = b));
                    optionClass.AddOptionCategory(1, cheatOptionCategory);
                }

                return optionClass;
            }, RTSCameraSubModule.ModuleId);
        }
    }
}
