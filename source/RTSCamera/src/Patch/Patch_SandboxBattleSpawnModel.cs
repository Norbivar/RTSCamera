using HarmonyLib;
using MissionSharedLibrary.Utilities;
using RTSCamera.Config;
using SandBox.GameComponents;
using System;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace RTSCamera.Patch
{
    public class Patch_SandboxBattleSpawnModel
    {
        private static readonly Harmony Harmony = new Harmony(RTSCameraSubModule.ModuleId + "_" + nameof(Patch_SandboxBattleSpawnModel));

        private static bool _patched;
        public static bool Patch()
        {
            try
            {
                if (_patched)
                    return false;
                _patched = true;

                Harmony.Patch(
                    typeof(SandboxBattleSpawnModel).GetMethod("FindBestOrderOfBattleFormationClassAssignmentForTroop",
                        BindingFlags.Static | BindingFlags.NonPublic),
                    prefix: new HarmonyMethod(typeof(Patch_SandboxBattleSpawnModel).GetMethod(
                        nameof(Prefix_FindBestOrderOfBattleFormationClassAssignmentForTroop), BindingFlags.Static | BindingFlags.Public)));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Utility.DisplayMessage(e.ToString());
                return false;
            }

            return true;
        }
        public static bool Prefix_FindBestOrderOfBattleFormationClassAssignmentForTroop(SandboxBattleSpawnModel __instance, IAgentOriginBase origin)
        {
            try
            {
                if (RTSCameraConfig.Get().FixCompanionFormation)
                {
                    var character = origin.Troop as CharacterObject;
                    if (character == null)
                        return true;
                    typeof(BasicCharacterObject).GetProperty(nameof(BasicCharacterObject.DefaultFormationClass))
                        .GetSetMethod(true).Invoke(origin.Troop, new object[] { character.GetFormationClass() });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Utility.DisplayMessage(e.ToString());
            }
            return true;
        }
    }
}
