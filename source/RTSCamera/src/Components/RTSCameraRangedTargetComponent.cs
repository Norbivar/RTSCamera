using System;
using System.Runtime.ExceptionServices;
using System.Security;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace RTSCameraAgentComponent
{
    public class RTSCameraRangedTargetComponent : AgentComponent
    {
        public class ComponentAdder : MissionLogic
        {
            public override void OnAgentCreated(Agent agent)
            {
                base.OnAgentCreated(agent);

                agent.AddComponent(new RTSCameraRangedTargetComponent(agent));
            }
        }


        public RTSCameraRangedTargetComponent(Agent agent) : base(agent)
        {
        }

        //public override void OnDismount(Agent mount)
        //{
        //    base.OnDismount(mount);

        //    try
        //    {
        //        if (CurrentColor != null)
        //        {
        //            mount.AgentVisuals?.SetContourColor(null);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        InformationManager.DisplayMessage(new InformationMessage(e.ToString()));
        //    }
        //}
    }
}
