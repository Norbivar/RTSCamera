using System;
using System.Collections.Generic;
using TaleWorlds.MountAndBlade;

namespace MissionLibrary.Event
{
    public class MissionEvent
    {
        private static Dictionary<string, Dictionary<string, Action<object[]>>> _eventMapping =
            new Dictionary<string, Dictionary<string, Action<object[]>>>();


        public static event Action<Agent> MainAgentWillBeChangedToAnotherOne;

        public static event Action<bool> ToggleFreeCamera;

        public delegate void SwitchTeamDelegate();

        public static event SwitchTeamDelegate PreSwitchTeam;
        public static event SwitchTeamDelegate PostSwitchTeam;

        public static event Action MissionMenuClosed;


        public static void Register(string eventId, string receiverId, Action<object[]> callback)
        {
            _eventMapping ??= new Dictionary<string, Dictionary<string, Action<object[]>>>();
            var receivers = _eventMapping[eventId] ??= new Dictionary<string, Action<object[]>>();
            receivers[receiverId] = callback;
        }

        public static void TriggerEvent(string eventId, object[] param)
        {
            var receivers = _eventMapping?[eventId];
            if (receivers == null)
                return;
            foreach (var receiver in _eventMapping?[eventId])
            {
                receiver.Value?.Invoke(param);
            }
        }

        public static void TriggerEvent(string eventId, string receiverId, object[] param)
        {
            _eventMapping?[eventId]?[receiverId]?.Invoke(param);
        }

        public static void Clear()
        {
            MainAgentWillBeChangedToAnotherOne = null;
            _eventMapping = null;
            ToggleFreeCamera = null;
            PreSwitchTeam = null;
            PostSwitchTeam = null;
            MissionMenuClosed = null;
        }

        public static void OnMainAgentWillBeChangedToAnotherOne(Agent newAgent)
        {
            MainAgentWillBeChangedToAnotherOne?.Invoke(newAgent);
        }

        public static void OnToggleFreeCamera(bool freeCamera)
        {
            ToggleFreeCamera?.Invoke(freeCamera);
        }

        public static void OnPreSwitchTeam()
        {
            PreSwitchTeam?.Invoke();
        }

        public static void OnPostSwitchTeam()
        {
            PostSwitchTeam?.Invoke();
        }

        public static void OnMissionMenuClosed()
        {
            MissionMenuClosed?.Invoke();
        }
    }
}
