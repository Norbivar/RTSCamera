using MissionLibrary.Controller;
using MissionLibrary.Extension;
using MissionSharedLibrary.Controller;
using RTSCamera.ActorComponents;
using RTSCamera.Logic;
using RTSCamera.View;
using RTSCameraAgentComponent;
using System.Collections.Generic;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace RTSCamera.MissionStartingHandler
{
    public class MissionStartingHandler : AMissionStartingHandler
    {
        public override void OnCreated(MissionView entranceView)
        {

            List<MissionBehavior> list = new List<MissionBehavior>
            {
                new RTSCameraSelectCharacterView(),

                new RTSCameraLogic(),

                new HideHUDView(),
                new FlyCameraMissionView()
            };


            foreach (var missionBehavior in list)
            {
                MissionStartingManager.AddMissionBehavior(entranceView, missionBehavior);
            }

            foreach (var extension in MissionExtensionCollection.Extensions)
            {
                foreach (var missionBehavior in extension.CreateMissionBehaviors(entranceView.Mission))
                {
                    MissionStartingManager.AddMissionBehavior(entranceView, missionBehavior);
                }
            }

            MissionStartingManager.AddMissionBehavior(entranceView, new ColouredOutlineComponent.ComponentAdder());
            MissionStartingManager.AddMissionBehavior(entranceView, new RTSCameraRangedTargetComponent.ComponentAdder());
        }

        public override void OnPreMissionTick(MissionView entranceView, float dt)
        {
        }
    }
}
