using System;
using System.Linq;
using System.Reflection;
using MissionLibrary.Controller.Camera;
using MissionLibrary.Event;
using MissionSharedLibrary.Utilities;
using RTSCamera.CampaignGame.Behavior;
using RTSCamera.Config;
using RTSCamera.Config.HotKey;
using RTSCamera.Logic;
using RTSCamera.Logic.SubLogic;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.Handlers;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.Screens;
using TaleWorlds.ScreenSystem;

namespace RTSCamera.View
{
    public class FlyCameraMissionView : MissionView, ICameraModeLogic, ICameraController
    {
        private static readonly FieldInfo CameraAddedElevation =
            typeof(MissionScreen).GetField("_cameraAddedElevation", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo SetLastFollowedAgent =
            typeof(MissionScreen).GetProperty("LastFollowedAgent")?.GetSetMethod(true);
        private static readonly MethodInfo SetCameraBearing =
            typeof(MissionScreen).GetProperty("CameraBearing")?.GetSetMethod(true);
        private static readonly MethodInfo SetCameraElevation =
            typeof(MissionScreen).GetProperty("CameraElevation")?.GetSetMethod(true);

        private static readonly FieldInfo CameraSpecialCurrentAddedElevation =
            typeof(MissionScreen).GetField("_cameraSpecialCurrentAddedElevation", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly FieldInfo CameraSpecialCurrentAddedBearing =
            typeof(MissionScreen).GetField("_cameraSpecialCurrentAddedBearing", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly FieldInfo CameraBearingDelta =
            typeof(MissionScreen).GetField("_cameraBearingDelta", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly FieldInfo CameraElevationDelta =
            typeof(MissionScreen).GetField("_cameraElevationDelta", BindingFlags.Instance | BindingFlags.NonPublic);


        private RTSCameraConfig _config;
        private RTSCameraLogic _rtsCameraLogic;
        private SwitchFreeCameraLogic _freeCameraLogic;

        private readonly int _shiftSpeedMultiplier = 3;
        private Vec3 _cameraSpeed;
        private float _cameraSpeedMultiplier;
        private bool _cameraSmoothMode;
        private float _cameraHeightToAdd;
        private float _cameraHeightLimit;
        private float? _previousHeightToTerrain;
        private readonly bool _classicMode = true;
        private bool _levelToEdge;
        private bool _lockToAgent;
        private GauntletLayer _showControlHintLayer;
        private ShowControlHintVM _showControlHintVM;

        public bool LockToAgent
        {
            get => _lockToAgent;
            set
            {
                if (value == _lockToAgent)
                    return;
                _forceMove = false;
                _lockToAgent = value;
                if (_lockToAgent)
                {
                    if (MissionScreen.LastFollowedAgent != null && MissionScreen.LastFollowedAgent.Team == Mission.PlayerTeam)
                    {
                        _showControlHintVM.SetShowText(true, true, MissionScreen.LastFollowedAgent.Name);
                    }
                }
                else if ((_freeCameraLogic == null || !_freeCameraLogic.IsSpectatorCamera))
                {
                    DisableControlHint();
                }
            }
        }

        private bool _forceMove;
        private Vec3 _forceMoveVec;
        private float _forceMoveInvertedProgress;

        private float _cameraBearingDelta;
        private float _cameraElevationDelta;
        private float _viewAngle = 65.0f;
        public float CameraBearing { get; private set; }

        public float CameraElevation { get; private set; }

        public float ViewAngle
        {
            get => _viewAngle;
            set
            {
                _viewAngle = value;
                if (_freeCameraLogic == null || !_freeCameraLogic.IsSpectatorCamera || LockToAgent)
                    return;
                UpdateViewAngle();
            }
        }

        public float Zoom { get; set; } = 1.0f;

        // legacy.
        public bool CameraRotateSmoothMode = true;
        public bool SmoothRotationMode
        {
            get => CameraRotateSmoothMode;
            set => CameraRotateSmoothMode = value;
        }

        // legacy
        public float CameraSpeedFactor = 1;

        // legacy
        public float CameraVerticalSpeedFactor = 1;

        public float MovementSpeedFactor
        {
            get => CameraSpeedFactor;
            set => CameraSpeedFactor = value;
        }
        public float VerticalMovementSpeedFactor
        {
            get => CameraVerticalSpeedFactor;
            set => CameraVerticalSpeedFactor = value;
        }

        public float DepthOfFieldDistance
        {
            get => _depthOfFieldDistance;
            set
            {
                _depthOfFieldDistance = value;
                if (_freeCameraLogic == null || !_freeCameraLogic.IsSpectatorCamera || LockToAgent)
                    return;
                UpdateDof();
            }
        }

        public float DepthOfFieldStart
        {
            get => _depthOfFieldStart;
            set
            {
                _depthOfFieldStart = value;
                if (_freeCameraLogic == null || !_freeCameraLogic.IsSpectatorCamera || LockToAgent)
                    return;
                UpdateDof();
            }
        }

        public float DepthOfFieldEnd
        {
            get => _depthOfFieldEnd;
            set
            {
                _depthOfFieldEnd = value;
                if (_freeCameraLogic == null || !_freeCameraLogic.IsSpectatorCamera || LockToAgent)
                    return;
                UpdateDof();
            }
        }

        private float _depthOfFieldDistance;
        private float _depthOfFieldStart;
        private float _depthOfFieldEnd;

        //public float CameraBearing => MissionScreen.CameraBearing;

        //public float CameraElevation => MissionScreen.CameraElevation;

        public Camera CombatCamera => MissionScreen.CombatCamera;

        public Vec3 CameraPosition { get; set; }

        public FlyCameraMissionView()
        {
            ACameraControllerManager.Get().Instance = this;
        }

        public void FocusOnAgent(Agent agent)
        {
            LockToAgent = true;
            typeof(MissionScreen).GetProperty("LastFollowedAgent")?.GetSetMethod(true)
                ?.Invoke(MissionScreen, new object[] { agent });

            if (!_freeCameraLogic.IsSpectatorCamera)
                _freeCameraLogic.SwitchCamera();

            Utility.SmoothMoveToAgent(MissionScreen, true, false);
        }

        private void LeaveFromAgent()
        {
            LockToAgent = false;
            CameraPosition = MissionScreen.CombatCamera.Frame.origin;
            CameraBearing = MissionScreen.CameraBearing +
                (float?)CameraSpecialCurrentAddedBearing?.GetValue(MissionScreen) ?? 0;
            CameraElevation = MissionScreen.CameraElevation +
                (float?)CameraSpecialCurrentAddedElevation?.GetValue(MissionScreen) ?? 0;
            CameraBearingDelta?.SetValue(MissionScreen, 0);
            CameraElevationDelta?.SetValue(MissionScreen, 0);
            SetLastFollowedAgent?.Invoke(MissionScreen, new object[] { null });
            MissionScreen.LastFollowedAgentVisuals = null;
            _previousHeightToTerrain = null;
        }

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            _cameraSpeed = Vec3.Zero;
            _cameraSpeedMultiplier = 1f;
            _cameraHeightToAdd = 0.0f;
            _cameraHeightLimit = 0.0f;
            _cameraSmoothMode = true;
            ViewOrderPriority = 1;

            _config = RTSCameraConfig.Get();
            _rtsCameraLogic = Mission.GetMissionBehavior<RTSCameraLogic>();
            _freeCameraLogic = _rtsCameraLogic.SwitchFreeCameraLogic;

            _showControlHintVM = new ShowControlHintVM(Mission.GetMissionBehavior<SiegeDeploymentHandler>() == null);
            _showControlHintLayer = new GauntletLayer(ViewOrderPriority);
            _showControlHintLayer.LoadMovie("RTSCameraShowControlHint", _showControlHintVM);
            MissionScreen.AddLayer(_showControlHintLayer);

            MissionEvent.ToggleFreeCamera += OnToggleFreeCamera;

            MissionScreen.OnSpectateAgentFocusIn += MissionScreenOnSpectateAgentFocusIn;
            MissionScreen.OnSpectateAgentFocusOut += MissionScreenOnSpectateAgentFocusOut;
        }

        public override void OnMissionScreenFinalize()
        {
            base.OnMissionScreenFinalize();

            MissionScreen.RemoveLayer(_showControlHintLayer);
            _showControlHintLayer = null;
            _showControlHintVM = null;

            MissionScreen.OnSpectateAgentFocusIn -= MissionScreenOnSpectateAgentFocusIn;
            MissionScreen.OnSpectateAgentFocusOut -= MissionScreenOnSpectateAgentFocusOut;

            MissionEvent.ToggleFreeCamera -= OnToggleFreeCamera;
            _freeCameraLogic = null;
            _config = null;

            ACameraControllerManager.Get().Clear();
        }

        public override bool UpdateOverridenCamera(float dt)
        {
            if (_freeCameraLogic == null || !_freeCameraLogic.IsSpectatorCamera || LockToAgent)
                return base.UpdateOverridenCamera(dt);

            UpdateFlyCamera(dt);
            return true;
        }

        public SpectatorCameraTypes GetMissionCameraLockMode(bool lockedToMainPlayer)
        {
            ICameraModeLogic otherCameraModeLogic =
                Mission.MissionBehaviors.FirstOrDefault(
                        b => !(b is FlyCameraMissionView) && b is ICameraModeLogic) as
                    ICameraModeLogic;
            if (_freeCameraLogic?.IsSpectatorCamera ?? false)
            {
                return LockToAgent ? SpectatorCameraTypes.LockToAnyAgent : SpectatorCameraTypes.Free;
            }
            return otherCameraModeLogic?.GetMissionCameraLockMode(lockedToMainPlayer) ?? SpectatorCameraTypes.Invalid;
        }

        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
        {
            base.OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);

            if (affectedAgent == MissionScreen.LastFollowedAgent && _freeCameraLogic.IsSpectatorCamera && LockToAgent)
            {
                LeaveFromAgent();
                if (!Utility.IsAgentDead(Mission.MainAgent))
                {
                    FocusOnAgent(Mission.MainAgent);
                }
            }
        }

        private void MissionScreenOnSpectateAgentFocusIn(Agent agent)
        {
            _showControlHintVM.SetShowText(true,
                !WatchBattleBehavior.WatchMode && (LockToAgent || Mission.MainAgent == null) && Utility.IsTeamValid(agent.Team) &&
                agent.Team == Mission.PlayerTeam, LockToAgent ? agent.Name : null);
        }

        public void DisableControlHint()
        {
            _showControlHintVM.SetShowText(false, false);
        }

        private void MissionScreenOnSpectateAgentFocusOut(Agent agent)
        {
            _showControlHintVM.SetShowText(false,
                !WatchBattleBehavior.WatchMode && Mission.MainAgent == null &&
                Mission.PlayerTeam?.ActiveAgents.Count > 0);
        }

        private void OnToggleFreeCamera(bool freeCamera)
        {
            if (!freeCamera)
            {
                LockToAgent = false;
            }
            else if (!LockToAgent)
            {
                BeginForcedMove(new Vec3(0, 0, _config.RaisedHeight));
                LeaveFromAgent();
            }
        }

        private void UpdateFlyCamera(float dt)
        {
            HandleRotateInput(dt);
            MatrixFrame cameraFrame = MatrixFrame.Identity;
            cameraFrame.rotation.RotateAboutSide(1.57079637f);
            cameraFrame.rotation.RotateAboutForward(CameraBearing);
            cameraFrame.rotation.RotateAboutSide(CameraElevation);
            if (!MissionScreen.IsPhotoModeEnabled)
                cameraFrame.rotation.RotateAboutSide((float?)CameraAddedElevation?.GetValue(MissionScreen) ?? 0f);
            cameraFrame.origin = CameraPosition;
            float heightFactorForHorizontalMove;
            float heightFactorForVerticalMove;
            var groundHeight = Mission.Scene.GetGroundHeightAtPosition(cameraFrame.origin);
            var terrainHeight = Mission.Scene.GetTerrainHeight(cameraFrame.origin.AsVec2);

            if (!_config.ConstantSpeed)
            {
                float heightAtPosition = _config.IgnoreTerrain ? terrainHeight : groundHeight;
                heightFactorForHorizontalMove = MathF.Clamp((float)(1.0 + (cameraFrame.origin.z - (double)heightAtPosition - 0.5) / 2),
                    1, 30);
                heightFactorForVerticalMove = MathF.Clamp((float)(1.0 + (cameraFrame.origin.z - (double)heightAtPosition - 0.5) / 2),
                    1, 20);
            }
            else
            {
                heightFactorForHorizontalMove = 1;
                heightFactorForVerticalMove = 1;
            }

            if (MissionScreen.InputManager.IsHotKeyPressed("MissionScreenHotkeyIncreaseCameraSpeed"))
                _cameraSpeedMultiplier *= 1.5f;
            if (MissionScreen.InputManager.IsHotKeyPressed("MissionScreenHotkeyDecreaseCameraSpeed"))
                _cameraSpeedMultiplier *= 0.6666667f;
            if (MissionScreen.InputManager.IsHotKeyPressed("ResetCameraSpeed"))
                _cameraSpeedMultiplier = 1f;
            _cameraSpeed *= (float)(1.0 - 10 * (double)dt);
            if (MissionScreen.InputManager.IsControlDown())
            {
                float num = MissionScreen.SceneLayer.Input.GetDeltaMouseScroll() * 0.008333334f;
                if (num > 0.00999999977648258)
                    _cameraSpeedMultiplier *= 1.25f;
                else if (num < -0.00999999977648258)
                    _cameraSpeedMultiplier *= 0.8f;
            }
            float num1 = 3f * _cameraSpeedMultiplier * MovementSpeedFactor;
            if (MissionScreen.SceneLayer.Input.IsGameKeyDown(CombatHotKeyCategory.Zoom))
                num1 *= _shiftSpeedMultiplier;
            if (!_cameraSmoothMode)
            {
                _cameraSpeed.x = 0.0f;
                _cameraSpeed.y = 0.0f;
                _cameraSpeed.z = 0.0f;
            }

            bool hasVerticalInput = false;
            if (!MissionScreen.InputManager.IsControlDown() || !MissionScreen.InputManager.IsAltDown())
            {
                Vec3 keyInput = Vec3.Zero;
                Vec3 mouseInput = Vec3.Zero;
                if (RTSCameraGameKeyCategory.GetKey(GameKeyEnum.CameraMoveForward).IsKeyDown(Input))
                    ++keyInput.y;
                if (RTSCameraGameKeyCategory.GetKey(GameKeyEnum.CameraMoveBackward).IsKeyDown(Input))
                    --keyInput.y;
                if (RTSCameraGameKeyCategory.GetKey(GameKeyEnum.CameraMoveLeft).IsKeyDown(Input))
                    --keyInput.x;
                if (RTSCameraGameKeyCategory.GetKey(GameKeyEnum.CameraMoveRight).IsKeyDown(Input))
                    ++keyInput.x;
                if (RTSCameraGameKeyCategory.GetKey(GameKeyEnum.CameraMoveUp).IsKeyDown(Input))
                    ++keyInput.z;
                if (RTSCameraGameKeyCategory.GetKey(GameKeyEnum.CameraMoveDown).IsKeyDown(Input))
                    --keyInput.z;
                if (MissionScreen.MouseVisible && !MissionScreen.SceneLayer.Input.IsKeyDown(InputKey.RightMouseButton))
                {
                    if (Mission.Mode != MissionMode.Conversation)
                    {
                        float x = MissionScreen.SceneLayer.Input.GetMousePositionRanged().x;
                        float y = MissionScreen.SceneLayer.Input.GetMousePositionRanged().y;
                        if (x <= 0.01 && (y < 0.01 || y > 0.25))
                            --mouseInput.x;
                        else if (x >= 0.99 && (y < 0.01 || y > 0.25))
                            ++mouseInput.x;
                        if (y <= 0.01)
                            ++mouseInput.y;
                        else if (y >= 0.99)
                            --mouseInput.y;
                    }
                }

                if (keyInput.LengthSquared > 0.0)
                    keyInput.Normalize();
                if (mouseInput.LengthSquared > 0.0)
                    mouseInput.Normalize();

                hasVerticalInput = (keyInput.z + mouseInput.z) != 0 || Input.GetDeltaMouseScroll() != 0;
                _cameraSpeed += (keyInput + mouseInput) * num1 * heightFactorForHorizontalMove;
            }
            float horizontalLimit = heightFactorForHorizontalMove * num1;
            float verticalLimit = heightFactorForVerticalMove * num1 * VerticalMovementSpeedFactor;
            _cameraSpeed.x = MBMath.ClampFloat(_cameraSpeed.x, -horizontalLimit, horizontalLimit);
            _cameraSpeed.y = MBMath.ClampFloat(_cameraSpeed.y, -horizontalLimit, horizontalLimit);
            _cameraSpeed.z = MBMath.ClampFloat(_cameraSpeed.z, -verticalLimit, verticalLimit);

            if (_config.CameraHeightFollowsTerrain)
            {
                if (_previousHeightToTerrain.HasValue && !hasVerticalInput)
                {
                    cameraFrame.origin.z = _previousHeightToTerrain.Value + terrainHeight;
                }
            }
            if (_forceMove)
                cameraFrame.origin += ForcedMoveTick(dt);

            if (_classicMode)
            {
                cameraFrame.origin += _cameraSpeed.x * cameraFrame.rotation.s.AsVec2.ToVec3().NormalizedCopy() * dt;
                ref Vec3 local = ref cameraFrame.origin;
                Vec3 vec3_2 = local;
                double y = _cameraSpeed.y;
                Vec3 vec3_1 = cameraFrame.rotation.u.AsVec2.ToVec3();
                Vec3 vec3_3 = vec3_1.NormalizedCopy();
                Vec3 vec3_4 = (float)y * vec3_3 * dt;
                local = vec3_2 - vec3_4;
                cameraFrame.origin.z += _cameraSpeed.z * dt;
                if (!MissionScreen.SceneLayer.Input.IsControlDown())
                    _cameraHeightToAdd -= (float)(Input.GetDeltaMouseScroll() / 200.0) * verticalLimit;
                // hold middle button and move mouse vertically to adjust height
                if (MissionScreen.SceneLayer.Input.IsHotKeyDown("DeploymentCameraIsActive"))
                {
                    if (_levelToEdge == false)
                    {
                        _levelToEdge = true;
                        ScreenManager.FirstHitLayer.InputRestrictions.SetMouseVisibility(true);
                    }
                    _cameraHeightToAdd += 0.5f * TaleWorlds.InputSystem.Input.MouseMoveY;
                }
                else if (_levelToEdge)
                {
                    _levelToEdge = false;
                    ScreenManager.FirstHitLayer.InputRestrictions.SetMouseVisibility(false);
                }
                _cameraHeightToAdd = MathF.Clamp(_cameraHeightToAdd, -verticalLimit, verticalLimit);
                if (MathF.Abs(_cameraHeightToAdd) > 1.0 / 1000.0)
                {
                    cameraFrame.origin.z += (float)(_cameraHeightToAdd * (double)dt);
                    _cameraHeightToAdd *= (float)(1.0 - dt * 5.0);
                }
                else
                {
                    cameraFrame.origin.z += _cameraHeightToAdd * dt;
                    _cameraHeightToAdd = 0.0f;
                }
                if (_cameraHeightLimit > 0.0 && cameraFrame.origin.z > (double)_cameraHeightLimit)
                    cameraFrame.origin.z = _cameraHeightLimit;
            }
            else
            {
                cameraFrame.origin += _cameraSpeed.x * cameraFrame.rotation.s * dt;
                cameraFrame.origin -= _cameraSpeed.y * cameraFrame.rotation.u * dt;
                cameraFrame.origin += _cameraSpeed.z * cameraFrame.rotation.f * dt;
            }

            if (RTSCameraSkillBehavior.ShouldLimitCameraDistance(Mission))
            {
                LimitCameraDistance(ref cameraFrame, dt, num1);
            }

            if (_config.CameraHeightFollowsTerrain)
            {
                _previousHeightToTerrain = cameraFrame.origin.z - terrainHeight;
            }

            if (!MBEditor.IsEditModeOn)
            {
                if (!_config.IgnoreBoundaries && !Mission.IsPositionInsideBoundaries(cameraFrame.origin.AsVec2))
                    cameraFrame.origin.AsVec2 = Mission.GetClosestBoundaryPosition(cameraFrame.origin.AsVec2);
                if (!_config.IgnoreBoundaries && Mission.Mode == MissionMode.Deployment)
                {
                    BattleSideEnum side = Mission.PlayerTeam.Side;
                    IMissionDeploymentPlan deploymentPlan = Mission.DeploymentPlan;
                    if (deploymentPlan?.HasDeploymentBoundaries(side) ?? false)
                    {
                        Vec2 cameraFrameVec2 = cameraFrame.origin.AsVec2;
                        if (!deploymentPlan.IsPositionInsideDeploymentBoundaries(side, in cameraFrameVec2))
                        {
                            Vec2 boundaryPosition = deploymentPlan.GetClosestDeploymentBoundaryPosition(side, in cameraFrameVec2);
                            cameraFrame.origin.AsVec2 = boundaryPosition;
                        }
                    }
                }
                float heightAtPosition = Mission.Scene.GetGroundHeightAtPosition(cameraFrame.origin + new Vec3(0.0f, 0.0f, 100f));
                if (!MissionScreen.IsCheatGhostMode && !_config.IgnoreTerrain && heightAtPosition < 9999.0)
                    cameraFrame.origin.z = Math.Max(cameraFrame.origin.z, heightAtPosition + 0.1f);
                if (cameraFrame.origin.z > heightAtPosition + 80.0)
                    cameraFrame.origin.z = heightAtPosition + 80f;
                if (cameraFrame.origin.z < -100.0)
                    cameraFrame.origin.z = -100f;
            }

            UpdateCameraFrameAndDof(cameraFrame);
        }

        private void LimitCameraDistance(ref MatrixFrame cameraFrame, float dt, float speed)
        {
            if (RTSCameraGameKeyCategory.GetKey(GameKeyEnum.IncreaseCameraDistanceLimit).IsKeyDownInOrder(Input))
            {
                RTSCameraSkillBehavior.UpdateCameraDistanceLimit(RTSCameraSkillBehavior.CameraDistanceLimit + dt * speed);
            }
            if (RTSCameraGameKeyCategory.GetKey(GameKeyEnum.DecreaseCameraDistanceLimit).IsKeyDownInOrder(Input))
            {
                RTSCameraSkillBehavior.UpdateCameraDistanceLimit(RTSCameraSkillBehavior.CameraDistanceLimit - dt * speed);
            }
            var mainAgentPosition = Utility.GetCameraTargetPositionWhenLockedToAgent(MissionScreen, Mission.MainAgent) + Vec3.Up;
            var distanceLimit = RTSCameraSkillBehavior.CameraDistanceLimit;
            var heightDiff = cameraFrame.origin.z - mainAgentPosition.z;
            var verticalScale = 2;
            Vec3 targetPosition = cameraFrame.origin;
            if (heightDiff >= distanceLimit * verticalScale)
            {
                targetPosition = mainAgentPosition;
                targetPosition.z += distanceLimit * verticalScale;
            }
            else if (heightDiff <= -distanceLimit * verticalScale)
            {
                targetPosition = mainAgentPosition;
                targetPosition.z -= distanceLimit * verticalScale;
            }
            var centerPosition = mainAgentPosition.AsVec2.ToVec3(targetPosition.z);
            var distance = targetPosition.Distance(centerPosition);
            var maxDistance = MathF.Sqrt(distanceLimit * distanceLimit - (heightDiff / verticalScale) * (heightDiff / verticalScale));
            if (distance > maxDistance)
            {
                targetPosition = Vec3.Lerp(centerPosition, targetPosition,
                    maxDistance / MathF.Max(distance, 1f));
            }

            cameraFrame.origin = Vec3.Lerp(targetPosition, cameraFrame.origin, MathF.Pow(0.02f, dt));
        }

        private void UpdateCameraFrameAndDof(MatrixFrame matrixFrame)
        {
            CameraPosition = matrixFrame.origin;
            CombatCamera.Frame = matrixFrame;
            UpdateDof();
            UpdateViewAngle();
            Mission.SetCameraFrame(ref matrixFrame, Zoom);
            SetCameraBearing?.Invoke(MissionScreen, new object[1] { CameraBearing });
            SetCameraElevation?.Invoke(MissionScreen, new object[1] { CameraElevation });
        }

        private void UpdateDof()
        {
            if (DepthOfFieldEnd < 0.0001f)
            {
                Mission.Scene.SetDepthOfFieldParameters(0, 0, false);
                Mission.Scene.SetDepthOfFieldFocus(0);
            }
            else
            {
                Mission.Scene.SetDepthOfFieldParameters(DepthOfFieldStart, DepthOfFieldEnd, false);
                if (Math.Abs(DepthOfFieldDistance) < 0.0001f)
                {
                    Mission.Scene.RayCastForClosestEntityOrTerrain(CombatCamera.Position, CombatCamera.Position + CombatCamera.Direction * 3000f, out var terrainCollisionDistance, out GameEntity _, 0.5f, BodyFlags.CameraCollisionRayCastExludeFlags);
                    Mission.RayCastForClosestAgent(CameraPosition,
                        CombatCamera.Position + CombatCamera.Direction * 3000f, out var agentCollisionDistance);
                    if (float.IsNaN(terrainCollisionDistance))
                    {
                        terrainCollisionDistance = float.MaxValue;
                    }

                    if (float.IsNaN(agentCollisionDistance))
                    {
                        agentCollisionDistance = float.MaxValue;
                    }
                    Mission.Scene.SetDepthOfFieldFocus(Math.Min(terrainCollisionDistance, agentCollisionDistance));
                }
                else
                {
                    Mission.Scene.SetDepthOfFieldFocus(DepthOfFieldDistance);
                }
            }
        }

        private void UpdateViewAngle()
        {
            float newDNear = !Mission.CameraIsFirstPerson ? 0.1f : 0.065f;
            CombatCamera.SetFovVertical((float)(ViewAngle * (Math.PI / 180.0)), Screen.AspectRatio, newDNear, 12500f);
            MissionScreen.SceneView?.SetCamera(CombatCamera);
        }

        private void BeginForcedMove(Vec3 vec)
        {
            _forceMove = true;
            _forceMoveInvertedProgress = 1;
            _forceMoveVec = vec;
        }

        private Vec3 ForcedMoveTick(float dt)
        {
            var previousProgress = _forceMoveInvertedProgress;
            _forceMoveInvertedProgress *= (float)Math.Pow(0.00001, dt);
            if (Math.Abs(_forceMoveInvertedProgress) < 0.00001f)
            {
                _forceMove = false;
                _forceMoveInvertedProgress = 1;
                _forceMoveVec = Vec3.Zero;
            }
            return _forceMoveVec * (previousProgress - _forceMoveInvertedProgress);
        }

        private void HandleRotateInput(float dt)
        {
            float mouseSensitivity = MissionScreen.SceneLayer.Input.GetMouseSensitivity();
            float inputXRaw = 0.0f;
            float inputYRaw = 0.0f;
            if (!MBCommon.IsPaused && Mission.Mode != MissionMode.Barter)
            {
                if (MissionScreen.MouseVisible)
                {
                    float x = MissionScreen.SceneLayer.Input.GetMousePositionRanged().x;
                    float y = MissionScreen.SceneLayer.Input.GetMousePositionRanged().y;
                    if (x <= 0.01 && y >= 0.01 && y <= 0.25)
                        inputXRaw = -1000f * dt;
                    else if (x >= 0.99 && y >= 0.01 && y <= 0.25)
                        inputXRaw = 1000f * dt;
                }
                else
                {
                    if (!MissionScreen.SceneLayer.Input.GetIsMouseActive())
                    {
                        double num3 = dt / 0.000600000028498471;
                        inputXRaw = (float)num3 * MissionScreen.SceneLayer.Input.GetGameKeyAxis("CameraAxisX") +
                                    MissionScreen.SceneLayer.Input.GetMouseMoveX();
                        inputYRaw = (float)-num3 * MissionScreen.SceneLayer.Input.GetGameKeyAxis("CameraAxisY") +
                                    MissionScreen.SceneLayer.Input.GetMouseMoveY();
                    }
                    else
                    {
                        inputXRaw = MissionScreen.SceneLayer.Input.GetMouseMoveX();
                        inputYRaw = MissionScreen.SceneLayer.Input.GetMouseMoveY();
                    }
                }
            }
            float num4 = 5.4E-05f;
            float smoothFading;
            if (SmoothRotationMode)
            {
                num4 *= 0.10f;
                smoothFading = (float)Math.Pow(0.000001, dt);
            }
            else
                smoothFading = 0.0f;
            _cameraBearingDelta *= smoothFading;
            _cameraElevationDelta *= smoothFading;
            bool isSessionActive = GameNetwork.IsSessionActive;
            float inputScale = num4 * mouseSensitivity * MissionScreen.CameraViewAngle;
            float inputX = -inputXRaw * inputScale;
            float inputY = (NativeConfig.InvertMouse ? inputYRaw : -inputYRaw) * inputScale;
            if (isSessionActive)
            {
                float maxValue = (float)(0.300000011920929 + 10.0 * dt);
                inputX = MBMath.ClampFloat(inputX, -maxValue, maxValue);
                inputY = MBMath.ClampFloat(inputY, -maxValue, maxValue);
            }
            _cameraBearingDelta += inputX;
            _cameraElevationDelta += inputY;
            if (isSessionActive)
            {
                float maxValue = (float)(0.300000011920929 + 10.0 * dt);
                _cameraBearingDelta = MBMath.ClampFloat(_cameraBearingDelta, -maxValue, maxValue);
                _cameraElevationDelta = MBMath.ClampFloat(_cameraElevationDelta, -maxValue, maxValue);
            }

            CameraBearing += _cameraBearingDelta;
            CameraElevation += _cameraElevationDelta;
            CameraElevation = MBMath.ClampFloat(CameraElevation, -1.36591f, 1.121997f);
        }

        public override void OnPreDisplayMissionTick(float dt)
        {
            base.OnPreDisplayMissionTick(dt);

            //if (!_isOrderViewOpen &&
            //    (Input.IsGameKeyReleased(8) ||
            //     (Input.IsGameKeyReleased(9) && !_rightButtonDraggingMode)))
            //{
            //    LockToAgent = true;
            //}

            if (LockToAgent && (Math.Abs(Input.GetDeltaMouseScroll()) > 0.0001f ||
                                RTSCameraGameKeyCategory.GetKey(GameKeyEnum.CameraMoveForward).IsKeyDown(Input) ||
                                RTSCameraGameKeyCategory.GetKey(GameKeyEnum.CameraMoveBackward).IsKeyDown(Input) ||
                                RTSCameraGameKeyCategory.GetKey(GameKeyEnum.CameraMoveLeft).IsKeyDown(Input) ||
                                RTSCameraGameKeyCategory.GetKey(GameKeyEnum.CameraMoveRight).IsKeyDown(Input) ||
                                RTSCameraGameKeyCategory.GetKey(GameKeyEnum.CameraMoveUp).IsKeyDown(Input) ||
                                RTSCameraGameKeyCategory.GetKey(GameKeyEnum.CameraMoveDown).IsKeyDown(Input) ||
                                Input.GetIsControllerConnected() &&
                                (Input.GetKeyState(InputKey.ControllerLStick).y != 0.0 ||
                                 Input.GetKeyState(InputKey.ControllerLStick).x != 0.0)))
            {
                LeaveFromAgent();
            }
        }


        public bool RequestCameraGoTo(Vec3 position, Vec3 direction = new Vec3())
        {
            if (!_freeCameraLogic.IsSpectatorCamera)
            {
                _freeCameraLogic.SwitchCamera();
                _forceMove = false;
            }
            else if (LockToAgent)
            {
                LeaveFromAgent();
            }

            //Utility.SmoothMoveToPositionAndDirection(MissionScreen, position,
            //    MathF.Asin(Vec3.DotProduct(direction, Vec3.Up)), direction.AsVec2.AngleBetween(Vec2.Forward),
            //    direction.Length > 0.5, direction.Length > 0.5);
            BeginForcedMove(position - MissionScreen.CombatCamera.Position);

            return true;
        }

        public bool RequestCameraGoTo(Vec2 position, Vec2 direction = new Vec2())
        {
            if (!_freeCameraLogic.IsSpectatorCamera)
            {
                _freeCameraLogic.SwitchCamera();
                _forceMove = false;
            }
            else if (LockToAgent)
            {
                LeaveFromAgent();
            }

            //Utility.SmoothMoveToPositionAndDirection(MissionScreen,
            //    position.ToVec3(MissionScreen.CombatCamera.Position.Z), 0, direction.AngleBetween(Vec2.Forward), false,
            //    direction.Length > 0.5);

            BeginForcedMove(position.ToVec3(MissionScreen.CombatCamera.Position.Z) - MissionScreen.CombatCamera.Position);
            return true;
        }
    }
}
