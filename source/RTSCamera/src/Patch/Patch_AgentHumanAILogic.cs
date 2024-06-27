using HarmonyLib;
using MissionSharedLibrary.Utilities;
using RTSCamera.Config;
using System;
using System.Reflection;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace RTSCamera.Patch
{
    public class Patch_AgentHumanAILogic
    {
        private static readonly Harmony Harmony = new Harmony(RTSCameraSubModule.ModuleId + "_" + nameof(Patch_AgentHumanAILogic));

        private static bool _patched;
        public static bool Patch()
        {
            try
            {
                if (_patched)
                    return false;
                _patched = true;

                Harmony.Patch(
                    typeof(AgentHumanAILogic).GetMethod("OnAgentControllerChanged",
                        BindingFlags.Instance | BindingFlags.NonPublic),
                    prefix: new HarmonyMethod(typeof(Patch_AgentHumanAILogic).GetMethod(
                        nameof(Prefix_OnAgentControllerChanged), BindingFlags.Static | BindingFlags.Public)));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Utility.DisplayMessage(e.ToString());
                return false;
            }

            return true;
        }
        public static bool Prefix_OnAgentControllerChanged(AgentHumanAILogic __instance,
            Agent agent,
            Agent.ControllerType oldController)
        {
            try
            {
                if (!agent.IsHuman)
                    return true;

                if (agent.Controller != Agent.ControllerType.AI)
                {
                    if (oldController != Agent.ControllerType.AI || agent.HumanAIComponent == null)
                        return true;
                    // HumanAIComponent registered the following action in constructor, but didn't unregister it.
                    // TODO: Need to check the official code whether this fix affects other behaviors.
                    if (agent.OnAgentWieldedItemChange != null)
                        agent.OnAgentWieldedItemChange -=
                            agent.HumanAIComponent.DisablePickUpForAgentIfNeeded;
                    if (agent.OnAgentMountedStateChanged != null)
                        agent.OnAgentMountedStateChanged -=
                            agent.HumanAIComponent.DisablePickUpForAgentIfNeeded;
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
