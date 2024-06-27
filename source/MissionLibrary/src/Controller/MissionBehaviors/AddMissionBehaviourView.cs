using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace MissionLibrary.Controller.MissionBehaviors
{
    [DefaultView]
    class AddMissionBehaviourView : MissionView
    {
        public override void OnCreated()
        {
            base.OnCreated();

            Global.GetProvider<AMissionStartingManager>().OnCreated(this);
        }

        public override void OnPreMissionTick(float dt)
        {
            base.OnPreMissionTick(dt);

            Global.GetProvider<AMissionStartingManager>().OnPreMissionTick(this, dt);

            var self = Mission.GetMissionBehavior<AddMissionBehaviourView>();
            if (self == this)
            {
                Mission.RemoveMissionBehavior(self);
            }
        }
    }
}
