using MissionLibrary;
using MissionLibrary.Controller;
using MissionSharedLibrary.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace MissionSharedLibrary.Controller
{
    public class MissionStartingManager : AMissionStartingManager
    {
        private readonly List<AMissionStartingHandler> _handlers = new List<AMissionStartingHandler>();

        public static void AddMissionBehavior(MissionView entranceView, MissionBehavior behaviour)
        {
            behaviour.OnAfterMissionCreated();
            entranceView.Mission.AddMissionBehavior(behaviour);
        }

        public override void OnCreated(MissionView entranceView)
        {
            foreach (var handler in GetHandlers())
            {
                handler.OnCreated(entranceView);
            }
        }

        public override void OnPreMissionTick(MissionView entranceView, float dt)
        {
            foreach (var handler in GetHandlers())
            {
                handler.OnPreMissionTick(entranceView, dt);
            }
        }

        public override void AddHandler(AMissionStartingHandler handler)
        {
            _handlers.Add(handler);
        }

        public override void AddHandler(string key, AMissionStartingHandler handler, Version version)
        {
            Global.RegisterProvider(VersionProviderCreator.Create(() => handler, version), key);
        }

        private IEnumerable<AMissionStartingHandler> GetHandlers()
        {
            return _handlers.Concat(Global.GetProviders<AMissionStartingHandler>());
        }
    }
}
