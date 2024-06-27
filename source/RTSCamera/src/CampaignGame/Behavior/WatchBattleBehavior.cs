using HarmonyLib;
using Helpers;
using MissionSharedLibrary.Utilities;
using System;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace RTSCamera.CampaignGame.Behavior
{
    public class WatchBattleBehavior : CampaignBehaviorBase
    {
        public static bool WatchMode;

        public override void SyncData(IDataStore dataStore)
        {
        }

        public override void RegisterEvents()
        {
            //CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, starter =>
            //    {
            //        AddMenuOption(Game.Current, starter);
            //    });
        }

        // Use harmony to add menu option to "encounter" menu because the original way (through OnSessionLauncherEvent doesn't work any more)
        public static void Patch(Harmony harmony)
        {
            try
            {
                harmony.Patch(
                    typeof(EncounterGameMenuBehavior).GetMethod("AddGameMenus",
                        BindingFlags.Instance | BindingFlags.NonPublic),
                    postfix: new HarmonyMethod(typeof(WatchBattleBehavior).GetMethod(nameof(Postfix),
                        BindingFlags.Static | BindingFlags.Public)));

            }
            catch (Exception e)
            {
                Utility.DisplayMessage(e.ToString());
            }
        }

        public static void Postfix(CampaignGameStarter gameSystemInitializer)
        {
            AddMenuOption(Game.Current, gameSystemInitializer);
        }


        private static void AddMenuOption(Game game, CampaignGameStarter gameStarter)
        {
            try
            {
                gameStarter.AddGameMenuOption("encounter", "rts_camera_watch_battle",
                    game.GameTextManager.FindText("str_rts_camera_watch_battle").ToString(), args =>
                    {
                        try
                        {
                            CheckEnemyAttackableHonorably(args);
                            return EncounterAttackCondition(args);
                        }
                        catch (Exception e)
                        {
                            Utility.DisplayMessage(e.ToString());
                        }

                        return false;
                    }, args =>
                    {
                        try
                        {
                            WatchMode = true;
                            MenuHelper.EncounterAttackConsequence(args);
                        }
                        catch (Exception e)
                        {
                            WatchMode = false;
                            Utility.DisplayMessage(e.ToString());
                        }
                    });

                gameStarter.AddGameMenuOption("menu_siege_strategies", "rts_camera_watch_battle",
                    game.GameTextManager.FindText("str_rts_camera_watch_battle").ToString(), WatchSiegeCondition, WatchSiegeConsequence);
            }
            catch (Exception e)
            {
                Utility.DisplayMessage(e.ToString());
            }

        }

        private static bool WatchSiegeCondition(MenuCallbackArgs args)
        {
            try
            {
                args.optionLeaveType = GameMenuOption.LeaveType.Mission;
                var mainPartySiegedSettlement = MobileParty.MainParty.BesiegedSettlement;
                
                if (mainPartySiegedSettlement == null || !Hero.MainHero.IsWounded)
                {
                    args.IsEnabled = false;
                    return false;
                }
                if (mainPartySiegedSettlement != null 
                    && mainPartySiegedSettlement.SiegeEvent != null
                    && mainPartySiegedSettlement.SiegeEvent.BesiegerCamp != null 
                    && mainPartySiegedSettlement.SiegeEvent.BesiegerCamp.LeaderParty == MobileParty.MainParty)
                {
                    Settlement settlement = PlayerEncounter.EncounteredParty != null ? PlayerEncounter.EncounteredParty.Settlement : PlayerSiege.PlayerSiegeEvent.BesiegedSettlement;
                    if (PlayerSiege.PlayerSide == BattleSideEnum.Attacker && !settlement.SiegeEvent.BesiegerCamp.IsPreparationComplete)
                    {
                        args.IsEnabled = false;
                        args.Tooltip = new TextObject("{=bCuxzp1N}You need to wait for the siege equipment to be prepared.");
                        return true;
                    }

                    args.IsEnabled = true;
                    args.Tooltip = GameTexts.FindText("str_rts_camera_watch_mode_tool_tip");
                    return true;
                }
            }
            catch (Exception e)
            {
                Utility.DisplayMessage(e.ToString());
            }

            return false;
        }

        private static void WatchSiegeConsequence(MenuCallbackArgs args)
        {
            try
            {
                if (PlayerEncounter.IsActive)
                    PlayerEncounter.LeaveEncounter = false;
                else
                    EncounterManager.StartSettlementEncounter(MobileParty.MainParty, PlayerSiege.PlayerSiegeEvent.BesiegedSettlement);
                WatchMode = true;
                GameMenu.SwitchToMenu("assault_town");
            }
            catch (Exception e)
            {
                WatchMode = false;
                Utility.DisplayMessage(e.ToString());
            }
        }

        private static readonly TextObject EnemyNotAttackableTooltip = new TextObject("{=oYUAuLO7}You have sworn not to attack.");

        private static void CheckEnemyAttackableHonorably(MenuCallbackArgs args)
        {
            if (MobileParty.MainParty.Army != null && MobileParty.MainParty.Army.LeaderParty != MobileParty.MainParty || PlayerEncounter.PlayerIsDefender)
                return;
            IFaction mapFaction;
            if (PlayerEncounter.EncounteredMobileParty != null)
            {
                mapFaction = PlayerEncounter.EncounteredMobileParty.MapFaction;
            }
            else
            {
                if (PlayerEncounter.EncounteredParty == null)
                    return;
                mapFaction = PlayerEncounter.EncounteredParty.MapFaction;
            }
            if (mapFaction == null || !mapFaction.NotAttackableByPlayerUntilTime.IsFuture)
                return;
            args.IsEnabled = false;
            args.Tooltip = EnemyNotAttackableTooltip;
        }

        private static bool EncounterAttackCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.HostileAction;
            if (MapEvent.PlayerMapEvent == null)
                return false;

            MapEvent battle = PlayerEncounter.Battle;
            Settlement mapEventSettlement = battle?.MapEventSettlement;

            var non_battle_pred = (battle == null
                || mapEventSettlement == null
                || !mapEventSettlement.IsFortification
                || !battle.IsSiegeAssault
                || PlayerSiege.PlayerSiegeEvent == null
                || PlayerSiege.PlayerSiegeEvent.BesiegerCamp.IsPreparationComplete);

            var battle_pred = battle != null
                && (battle.HasTroopsOnBothSides() || battle.IsSiegeAssault) 
                && MapEvent.PlayerMapEvent.GetLeaderParty(PartyBase.MainParty.OpponentSide) != null;

            return non_battle_pred && battle_pred;
        }
    }
}
