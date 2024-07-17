using System;
using System.Runtime.ExceptionServices;
using System.Security;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace RTSCamera
{
    namespace ActorComponents
    {

        public struct Contour
        {
            public Contour(uint? color, bool alwaysVisible)
            {
                Color = color;
                AlwaysVisible = alwaysVisible;
            }
            public uint? Color;
            public bool AlwaysVisible;
        }

        public enum ColorLevel
        {
            TargetFormation,
            SelectedFormation,
            MouseOverFormation,
            TargetAgent,
            SelectedAgent,
            MouseOverAgent,
            NumberOfLevel
        }

        public class ColouredOutlineComponent : AgentComponent
        {
            public class ComponentAdder : MissionLogic
            {
                public override void OnAgentCreated(Agent agent)
                {
                    base.OnAgentCreated(agent);

                    agent.AddComponent(new ColouredOutlineComponent(agent));
                }
            }

            private readonly Contour[] _colors = new Contour[(int)ColorLevel.NumberOfLevel];
            private int _currentLevel = -1;

            private uint? CurrentColor => _currentLevel < 0 ? null : _colors[_currentLevel].Color;
            private bool CurrentAlwaysVisible => _currentLevel < 0 || _colors[_currentLevel].AlwaysVisible;

            private bool _shouldUpdateColor = false;

            public ColouredOutlineComponent(Agent agent) : base(agent)
            {
                for (int i = 0; i < _colors.Length; ++i)
                {
                    _colors[i] = new Contour(null, false);
                }
            }

            public void SetContourColor(int level, uint? color, bool alwaysVisible, bool updateInstantly)
            {
                if (SetContourColorWithoutUpdate(level, color, alwaysVisible))
                {
                    _currentLevel = color.HasValue ? level : EffectiveLevel(level - 1);
                    if (updateInstantly)
                    {
                        _shouldUpdateColor = false;
                        SetColor();
                    }
                    else
                    {
                        _shouldUpdateColor = true;
                    }
                }
            }

            private bool SetContourColorWithoutUpdate(int level, uint? color, bool alwaysVisible)
            {
                if (level < 0 || level >= _colors.Length)
                    return false;
                if (_colors[level].Color == color)
                    return false;
                _colors[level].Color = color;
                _colors[level].AlwaysVisible = alwaysVisible;
                return _currentLevel <= level; // needs update.
            }

            private void UpdateColor()
            {
                _currentLevel = EffectiveLevel();
                _shouldUpdateColor = true;
            }

            public void ClearContourColor()
            {
                try
                {
                    for (int i = 0; i < _colors.Length; ++i)
                    {
                        _colors[i].Color = null;
                    }

                    Agent.AgentVisuals?.SetContourColor(null);
                    if (Agent.HasMount)
                        Agent.MountAgent.AgentVisuals?.SetContourColor(null);
                }
                catch (Exception e)
                {
                    InformationManager.DisplayMessage(new InformationMessage(e.ToString()));
                }
            }

            public void ClearTargetOrSelectedFormationColor()
            {
                bool needUpdate = SetContourColorWithoutUpdate((int)ColorLevel.TargetFormation, null, true);
                needUpdate |= SetContourColorWithoutUpdate((int)ColorLevel.SelectedFormation, null, true);
                if (needUpdate)
                    UpdateColor();
            }

            public void ClearFormationColor()
            {
                bool needUpdate = SetContourColorWithoutUpdate((int)ColorLevel.TargetFormation, null, true);
                needUpdate |= SetContourColorWithoutUpdate((int)ColorLevel.SelectedFormation, null, true);
                needUpdate |= SetContourColorWithoutUpdate((int)ColorLevel.MouseOverFormation, null, true);
                if (needUpdate)
                    UpdateColor();
            }

            public override void OnMount(Agent mount)
            {
                base.OnMount(mount);

                try
                {
                    mount.AgentVisuals?.SetContourColor(CurrentColor, CurrentAlwaysVisible);
                }
                catch (Exception e)
                {
                    InformationManager.DisplayMessage(new InformationMessage(e.ToString()));
                }
            }

            public override void OnDismount(Agent mount)
            {
                base.OnDismount(mount);

                try
                {
                    if (CurrentColor != null)
                    {
                        mount.AgentVisuals?.SetContourColor(null);
                    }
                }
                catch (Exception e)
                {
                    InformationManager.DisplayMessage(new InformationMessage(e.ToString()));
                }
            }

            public void UpdateContour()
            {
                if (_shouldUpdateColor)
                {
                    _shouldUpdateColor = false;
                    SetColor();
                }
            }

            private int EffectiveLevel(int maxLevel = (int)ColorLevel.NumberOfLevel - 1)
            {
                for (int i = maxLevel; i > -1; --i)
                {
                    if (_colors[i].Color.HasValue)
                        return i;
                }

                return -1;
            }

            private void SetColor()
            {
                try
                {
                    Agent.AgentVisuals?.SetContourColor(CurrentColor, CurrentAlwaysVisible);
                    if (Agent.HasMount)
                        Agent.MountAgent.AgentVisuals?.SetContourColor(CurrentColor, CurrentAlwaysVisible);
                }
                catch (Exception e)
                {
                    InformationManager.DisplayMessage(new InformationMessage(e.ToString()));
                }
            }

            public override void OnAgentRemoved()
            {
                ClearContourColor();

                base.OnAgentRemoved();
            }
        }
    }
}
