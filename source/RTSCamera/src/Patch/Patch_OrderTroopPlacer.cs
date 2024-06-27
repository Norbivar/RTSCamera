using HarmonyLib;
using MissionSharedLibrary.Utilities;
using RTSCamera.Config;
using RTSCamera.Config.HotKey;
using RTSCamera.Logic;
using RTSCamera.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.GauntletUI.Mission.Singleplayer;
using TaleWorlds.MountAndBlade.View.MissionViews.Order;
using TaleWorlds.MountAndBlade.ViewModelCollection.Order;
using CursorState = TaleWorlds.MountAndBlade.View.MissionViews.Order.OrderTroopPlacer.CursorState;

namespace RTSCamera.Patch
{
    public class Patch_OrderTroopPlacer
    {
        private static readonly Harmony Harmony = new Harmony(RTSCameraSubModule.ModuleId + "_" + nameof(Patch_CrosshairVM));

        private static bool _patched;
        public static bool Patch()
        {
            try
            {
                if (_patched)
                    return false;
                _patched = true;

                Harmony.Patch(
                    typeof(OrderTroopPlacer).GetMethod("InitializeInADisgustingManner",
                        BindingFlags.Instance | BindingFlags.NonPublic),
                    postfix: new HarmonyMethod(typeof(Patch_OrderTroopPlacer).GetMethod(
                        nameof(Postfix_InitializeInADisgustingManner), BindingFlags.Static | BindingFlags.Public)));
                Harmony.Patch(
                    typeof(OrderTroopPlacer).GetMethod("HandleMouseDown",
                        BindingFlags.Instance | BindingFlags.NonPublic),
                    prefix: new HarmonyMethod(typeof(Patch_OrderTroopPlacer).GetMethod(nameof(Prefix_HandleMouseDown),
                        BindingFlags.Static | BindingFlags.Public)));
                Harmony.Patch(
                    typeof(OrderTroopPlacer).GetMethod("GetCursorState",
                        BindingFlags.Instance | BindingFlags.NonPublic),
                    new HarmonyMethod(typeof(Patch_OrderTroopPlacer).GetMethod(nameof(Prefix_GetCursorState),
                        BindingFlags.Static | BindingFlags.Public)));
                Harmony.Patch(
                    typeof(OrderTroopPlacer).GetMethod("AddOrderPositionEntity",
                        BindingFlags.Instance | BindingFlags.NonPublic),
                    new HarmonyMethod(typeof(Patch_OrderTroopPlacer).GetMethod(nameof(Prefix_AddOrderPositionEntity),
                        BindingFlags.Static | BindingFlags.Public)));
                Harmony.Patch(
                    typeof(OrderTroopPlacer).GetMethod(nameof(OrderTroopPlacer.OnMissionScreenTick),
                        BindingFlags.Instance | BindingFlags.Public),
                    new HarmonyMethod(typeof(Patch_OrderTroopPlacer).GetMethod(nameof(Prefix_OnMissionScreenTick),
                        BindingFlags.Static | BindingFlags.Public)));
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Utility.DisplayMessage(e.ToString());
                return false;
            }
        }

        private static readonly FieldInfo _dataSource =
            typeof(MissionGauntletSingleplayerOrderUIHandler).GetField(nameof(_dataSource),
                BindingFlags.Instance | BindingFlags.NonPublic);

        private static CursorState _currentCursorState = CursorState.Invisible;
        private static UIQueryData<CursorState> _cachedCursorState;
        private static FormationColorLogic _contourView;
        private static OrderTroopPlacer _orderTroopPlacer;
        private static readonly RTSCameraConfig _config = RTSCameraConfig.Get();

        public static void Postfix_InitializeInADisgustingManner(OrderTroopPlacer __instance)
        {
            _orderTroopPlacer = __instance;
            _cachedCursorState = new UIQueryData<CursorState>(GetCursorState, 0.05f);
            _contourView = Mission.Current.GetMissionBehavior<RTSCameraLogic>().FormationColorLogic;

            typeof(Input).GetProperty(nameof(Input.DebugInput), BindingFlags.Static | BindingFlags.Public)
                .SetValue(null, __instance.Input);
        }

        public static void OnMissionEnded()
        {
            _orderTroopPlacer = null;
            _contourView = null;
            _cachedCursorState = null;
        }

        public static bool IsDraggingFormation(OrderTroopPlacer __instance, Vec2? ____formationDrawingStartingPointOfMouse, float? ____formationDrawingStartingTime)
        {
            if (____formationDrawingStartingPointOfMouse.HasValue)
            {
                Vec2 vec2 = ____formationDrawingStartingPointOfMouse.Value - __instance.Input.GetMousePositionPixel();
                if (Math.Abs(vec2.x) >= 10.0 || Math.Abs(vec2.y) >= 10.0)
                {
                    return true;
                }
            }

            return false;
        }

        public static CursorState GetCursorState()
        {
            return (CursorState)typeof(OrderTroopPlacer).GetMethod("GetCursorState", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(_orderTroopPlacer, new object[] { });
        }
        private static Vec2 GetScreenPoint(OrderTroopPlacer __instance, ref Vec2 ____deltaMousePosition)
        {
            return !__instance.MissionScreen.MouseVisible
                ? new Vec2(0.5f, 0.5f) + ____deltaMousePosition
                : __instance.Input.GetMousePositionRanged() + ____deltaMousePosition;
        }

        private static void BeginFormationDraggingOrClicking(OrderTroopPlacer __instance, ref Vec2 ____deltaMousePosition,
            ref WorldPosition? ____formationDrawingStartingPosition, ref Vec2? ____formationDrawingStartingPointOfMouse, ref float? ____formationDrawingStartingTime)
        {
            Vec3 rayBegin;
            Vec3 rayEnd;
            __instance.MissionScreen.ScreenPointToWorldRay(GetScreenPoint(__instance, ref ____deltaMousePosition), out rayBegin, out rayEnd);
            float collisionDistance;
            if (__instance.Mission.Scene.RayCastForClosestEntityOrTerrain(rayBegin, rayEnd, out collisionDistance,
                    0.3f))
            {
                Vec3 vec3 = rayEnd - rayBegin;
                double num = vec3.Normalize();
                ____formationDrawingStartingPosition = new WorldPosition(Mission.Current.Scene, UIntPtr.Zero, rayBegin + vec3 * collisionDistance,
                    false);
                ____formationDrawingStartingPointOfMouse = __instance.Input.GetMousePositionPixel();
                // Fix the issue that can't drag when slow motion is enabled and mouse is visible.
                ____formationDrawingStartingTime = 0;
                return;
            }

            ____formationDrawingStartingPosition = new WorldPosition?();
            ____formationDrawingStartingPointOfMouse = new Vec2?();
            ____formationDrawingStartingTime = new float?();
        }

        public static bool Prefix_HandleMouseDown(OrderTroopPlacer __instance, ref Formation ____clickedFormation, ref Formation ____mouseOverFormation,
            ref bool ____formationDrawingMode, ref Vec2 ____deltaMousePosition,
            ref WorldPosition? ____formationDrawingStartingPosition, ref Vec2? ____formationDrawingStartingPointOfMouse, ref float? ____formationDrawingStartingTime, bool ____isMouseDown)
        {
            if (__instance.Mission.PlayerTeam.PlayerOrderController.SelectedFormations.IsEmpty() || ____clickedFormation != null)
                return false;
            switch (_currentCursorState)
            {
                case CursorState.Normal:
                case CursorState.Enemy:
                case CursorState.Friend:
                    ____formationDrawingMode = true;
                    BeginFormationDraggingOrClicking(__instance, ref ____deltaMousePosition,
                        ref ____formationDrawingStartingPosition, ref ____formationDrawingStartingPointOfMouse,
                        ref ____formationDrawingStartingTime);
                    break;
                case CursorState.Rotation:
                    return true;
            }

            return false;
        }

        private static void HideNonSelectedOrderRotationEntities(OrderController ___PlayerOrderController, List<GameEntity> ____orderRotationEntities, Formation formation)
        {
            for (int index = 0; index < ____orderRotationEntities.Count; ++index)
            {
                GameEntity orderRotationEntity = ____orderRotationEntities[index];
                if (orderRotationEntity == null && orderRotationEntity.IsVisibleIncludeParents() && ___PlayerOrderController.SelectedFormations.ElementAt(index / 2) != formation)
                {
                    orderRotationEntity.SetVisibilityExcludeParents(false);
                    orderRotationEntity.BodyFlag |= BodyFlags.Disabled;
                }
            }
        }

        private static void TryTransformFromClickingToDragging(OrderTroopPlacer __instance, Vec2? ____formationDrawingStartingPointOfMouse, float? ____formationDrawingStartingTime, OrderController ___PlayerOrderController,
            ref Formation ____clickedFormation, ref bool ____formationDrawingMode, bool ____isMouseDown)
        {
            if (___PlayerOrderController.SelectedFormations.IsEmpty())
                return;

            switch (_currentCursorState)
            {
                case CursorState.Enemy:
                case CursorState.Friend:
                    if (IsDraggingFormation(__instance, ____formationDrawingStartingPointOfMouse, ____formationDrawingStartingTime))
                    {
                        if ((__instance.Input.IsKeyDown(InputKey.LeftMouseButton) ||
                             __instance.Input.IsKeyDown(InputKey.ControllerRTrigger)) && ____isMouseDown)
                        {
                            ____formationDrawingMode = true;
                            ____clickedFormation = null;
                        }
                    }

                    break;
            }
        }

        public static bool Prefix_GetCursorState(OrderTroopPlacer __instance, ref CursorState __result, OrderController ___PlayerOrderController, ref Formation ____clickedFormation, ref Formation ____mouseOverFormation,
            List<GameEntity> ____orderRotationEntities, ref Vec2 ____deltaMousePosition, ref bool ____formationDrawingMode, ref int ____mouseOverDirection, bool ____isMouseDown)
        {
            CursorState cursorState = CursorState.Invisible;
            if (!___PlayerOrderController.SelectedFormations.IsEmpty() && ____clickedFormation == null)
            {
                __instance.MissionScreen.ScreenPointToWorldRay(GetScreenPoint(__instance, ref ____deltaMousePosition), out var rayBegin, out var rayEnd);
                if (!__instance.Mission.Scene.RayCastForClosestEntityOrTerrain(rayBegin, rayEnd, out var collisionDistance,
                    out GameEntity collidedEntity, 0.3f))
                    collisionDistance = 1000f;

                if (cursorState == CursorState.Invisible && collisionDistance < 1000.0)
                {
                    if (!____formationDrawingMode && collidedEntity == null)
                    {
                        for (int index = 0; index < ____orderRotationEntities.Count; ++index)
                        {
                            GameEntity orderRotationEntity = ____orderRotationEntities[index];
                            if (orderRotationEntity.IsVisibleIncludeParents() &&
                                collidedEntity == orderRotationEntity)
                            {
                                ____mouseOverFormation =
                                    ___PlayerOrderController.SelectedFormations.ElementAt(index / 2);
                                ____mouseOverDirection = 1 - (index & 1);
                                cursorState = CursorState.Rotation;
                                break;
                            }
                        }
                    }

                    if (cursorState == CursorState.Invisible)
                    {
                        if (__instance.MissionScreen.OrderFlag.FocusedOrderableObject != null)
                        {
                            cursorState = CursorState.OrderableEntity;
                        }
                        else
                        {
                            var formation = GetMouseOverFormation(__instance, collisionDistance,
                                ___PlayerOrderController, ref ____deltaMousePosition,
                                ____formationDrawingMode);
                            ____mouseOverFormation = formation;
                            if (formation != null)
                            {
                                if (formation.Team.IsEnemyOf(__instance.Mission.PlayerTeam))
                                {
                                    if (_config.HighlightEnemyTargetFormation)
                                    {
                                        cursorState = CursorState.Enemy;
                                    }
                                }
                                else
                                {
                                    if (_config.HighlightOwnSelectedFormation)
                                    {
                                        cursorState = CursorState.Friend;
                                    }
                                }
                            }
                        }
                    }
                    bool anyHighlight = _config.HighlightOwnSelectedFormation || _config.HighlightEnemyTargetFormation;
                    if (cursorState == CursorState.Invisible &&
                        !(RTSCameraGameKeyCategory.GetKey(GameKeyEnum.SelectFormation).IsKeyDown(__instance.Input) && anyHighlight) || // press middle mouse button to avoid accidentally click on ground.
                        ____formationDrawingMode)
                    {
                        cursorState = IsCursorStateGroundOrNormal(____formationDrawingMode);
                    }
                }
            }
            else if (____clickedFormation != null) // click on formation and hold.
            {
                cursorState = _currentCursorState;
            }

            if (cursorState != CursorState.Ground &&
                cursorState != CursorState.Rotation)
                ____mouseOverDirection = 0;

            __result = cursorState;
            return false;
        }

        private static CursorState IsCursorStateGroundOrNormal(bool ____formationDrawingMode)
        {
            return !____formationDrawingMode
                ? CursorState.Normal
                : CursorState.Ground;
        }

        private static Agent RayCastForAgent(OrderTroopPlacer __instance, float distance, ref Vec2 ____deltaMousePosition)
        {
            __instance.MissionScreen.ScreenPointToWorldRay(GetScreenPoint(__instance, ref ____deltaMousePosition), out var rayBegin, out var rayEnd);
            var agent = __instance.Mission.RayCastForClosestAgent(rayBegin, rayEnd, out var agentDistance,
                __instance.MissionScreen.LastFollowedAgent?.Index ?? -1, 0.3f);

            if (agentDistance > distance || agent == null)
            {
                agent = __instance.Mission.RayCastForClosestAgent(rayBegin, rayEnd, out agentDistance,
                    __instance.MissionScreen.LastFollowedAgent?.Index ?? -1, 0.8f);
            }
            return agentDistance > distance ? null : agent;
        }

        private static Formation GetMouseOverFormation(OrderTroopPlacer __instance, float collisionDistance, OrderController ___PlayerOrderController, ref Vec2 ____deltaMousePosition, bool ____formationDrawingMode)
        {
            var agent = RayCastForAgent(__instance, collisionDistance, ref ____deltaMousePosition);
            if (agent != null && agent.IsMount)
                agent = agent.RiderAgent;

            if (agent == null)
                return null;

            if (!__instance.IsDrawingForced && !____formationDrawingMode && agent?.Formation != null &&
                !(___PlayerOrderController.SelectedFormations.Count == 1 &&
                  ___PlayerOrderController.SelectedFormations.Contains(agent.Formation)))
            {
                return agent.Formation;
            }

            return null;
        }

        public static bool Prefix_AddOrderPositionEntity(OrderTroopPlacer __instance, int entityIndex,
            ref Vec3 groundPosition, bool fadeOut, float alpha,
            List<GameEntity> ____orderPositionEntities, ref Material ____meshMaterial)
        {
            if (RTSCameraConfig.Get().MoreVisibleMoveLocation)
            {
                while (____orderPositionEntities.Count <= entityIndex)
                {
                    GameEntity gameEntity = GameEntity.CreateEmpty(__instance.Mission.Scene);
                    gameEntity.EntityFlags |= EntityFlags.NotAffectedBySeason;
                    MetaMesh copy = MetaMesh.GetCopy("barrier_sphere");
                    if (____meshMaterial == null)
                    {
                        ____meshMaterial = copy.GetMeshAtIndex(0).GetMaterial().CreateCopy();
                        ____meshMaterial.SetAlphaBlendMode(Material.MBAlphaBlendMode.Factor);
                    }
                    copy.SetMaterial(____meshMaterial);
                    gameEntity.AddComponent(copy);
                    gameEntity.SetVisibilityExcludeParents(false);
                    ____orderPositionEntities.Add(gameEntity);
                }
                GameEntity orderPositionEntity = ____orderPositionEntities[entityIndex];
                MatrixFrame frame = new MatrixFrame(Mat3.Identity, groundPosition + (Vec3.Up * 1.0f));
                orderPositionEntity.SetFrame(ref frame);
                if (alpha != -1.0)
                {
                    orderPositionEntity.SetVisibilityExcludeParents(true);
                    orderPositionEntity.SetAlpha(alpha);
                }
                else if (fadeOut)
                    orderPositionEntity.FadeOut(0.5f, false);
                else
                    orderPositionEntity.FadeIn();

                return false;
            }
            else
            {
                return true;
            }
        }

        private static void HandleSelectFormationKeyDown(OrderTroopPlacer __instance, ref Formation ____clickedFormation, ref Formation ____mouseOverFormation,
            ref bool ____formationDrawingMode, ref Vec2 ____deltaMousePosition,
            ref WorldPosition? ____formationDrawingStartingPosition, ref Vec2? ____formationDrawingStartingPointOfMouse, ref float? ____formationDrawingStartingTime)
        {
            if (__instance.Mission.PlayerTeam.PlayerOrderController.SelectedFormations.IsEmpty() || ____clickedFormation != null)
                return;

            switch (_currentCursorState)
            {
                case CursorState.Enemy:
                    ____formationDrawingMode = false;
                    ____clickedFormation = ____mouseOverFormation;

                    BeginFormationDraggingOrClicking(__instance, ref ____deltaMousePosition,
                        ref ____formationDrawingStartingPosition, ref ____formationDrawingStartingPointOfMouse,
                        ref ____formationDrawingStartingTime);
                    break;
                case CursorState.Friend:
                    ____formationDrawingMode = false;
                    if (____mouseOverFormation != null && __instance.Mission.PlayerTeam.PlayerOrderController.IsFormationSelectable(____mouseOverFormation))
                    {
                        ____clickedFormation = ____mouseOverFormation;
                    }
                    BeginFormationDraggingOrClicking(__instance, ref ____deltaMousePosition,
                        ref ____formationDrawingStartingPosition, ref ____formationDrawingStartingPointOfMouse,
                        ref ____formationDrawingStartingTime);
                    break;
            }
        }

        private static void HandleSelectFormationKeyUp(OrderTroopPlacer __instance, ref Formation ____clickedFormation, OrderController ___PlayerOrderController, List<GameEntity> ____orderRotationEntities,
            ref bool ____formationDrawingMode, ref Vec2 ____deltaMousePosition,
            ref WorldPosition? ____formationDrawingStartingPosition, ref Vec2? ____formationDrawingStartingPointOfMouse, ref float? ____formationDrawingStartingTime)
        {
            if (!____formationDrawingMode)
            {
                if (____clickedFormation != null)
                {
                    if (____clickedFormation.CountOfUnits > 0)
                    {
                        bool isEnemy = Utility.IsEnemy(____clickedFormation);
                        if (!isEnemy)
                        {
                            HideNonSelectedOrderRotationEntities(___PlayerOrderController, ____orderRotationEntities, ____clickedFormation);

                            if (___PlayerOrderController.IsFormationSelectable(____clickedFormation))
                            {
                                SelectFormationFromController(__instance, ___PlayerOrderController, ____clickedFormation);
                            }
                        }
                        else
                        {
                            ___PlayerOrderController.SetOrderWithFormation(OrderType.Charge, ____clickedFormation);
                            Utility.DisplayChargeToFormationMessage(___PlayerOrderController.SelectedFormations, ____clickedFormation);
                        }
                    }

                    ____clickedFormation = null;

                    ____formationDrawingMode = false;
                    ____formationDrawingStartingPosition = null;
                    ____formationDrawingStartingPointOfMouse = null;
                    ____formationDrawingStartingTime = null;
                    ____deltaMousePosition = Vec2.Zero;
                }
            }
        }

        public static bool Prefix_OnMissionScreenTick(OrderTroopPlacer __instance, ref bool ____initialized, ref OrderController ___PlayerOrderController,
            ref bool ___isDrawnThisFrame, ref bool ____isMouseDown, ref Timer ___formationDrawTimer, ref Vec2? ____formationDrawingStartingPointOfMouse, ref float? ____formationDrawingStartingTime,
            ref Formation ____clickedFormation, ref bool ____formationDrawingMode, Formation ____mouseOverFormation,
            ref List<GameEntity> ____orderPositionEntities, ref List<GameEntity> ____orderRotationEntities,
            ref bool ____wasDrawingForced, ref bool ____wasDrawingFacing, ref bool ____wasDrawingForming, ref bool ___wasDrawnPreviousFrame, ref WorldPosition? ____formationDrawingStartingPosition,
            ref Vec2 ____deltaMousePosition)
        {
            if (!____initialized)
                return false;

            if (!___PlayerOrderController.SelectedFormations.Any())
                return false;

            ___isDrawnThisFrame = false;
            if (__instance.SuspendTroopPlacer)
                return false;

            bool anyHighlight = _config.HighlightOwnSelectedFormation || _config.HighlightEnemyTargetFormation;

            bool isSelectFormationKeyPressed = anyHighlight &&
                                            RTSCameraGameKeyCategory.GetKey(GameKeyEnum.SelectFormation)
                                                .IsKeyPressed(__instance.Input);
            bool isSelectFormationKeyReleased = anyHighlight &&
                                                RTSCameraGameKeyCategory.GetKey(GameKeyEnum.SelectFormation)
                                                    .IsKeyReleased(__instance.Input);
            bool isSelectFormationKeyDown = anyHighlight &&
                                            RTSCameraGameKeyCategory.GetKey(GameKeyEnum.SelectFormation)
                                                .IsKeyDown(__instance.Input);

            _currentCursorState = _cachedCursorState.Value;
            bool isLeftButtonPressed = __instance.Input.IsKeyPressed(InputKey.LeftMouseButton) ||
                              __instance.Input.IsKeyPressed(InputKey.ControllerRTrigger);

            if (isLeftButtonPressed)
            {
                ____isMouseDown = true;
                typeof(OrderTroopPlacer).GetMethod("HandleMouseDown", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.Invoke(__instance, new object[] { });
            }
            if (isSelectFormationKeyPressed)
            {
                HandleSelectFormationKeyDown(__instance, ref ____clickedFormation, ref ____mouseOverFormation,
                    ref ____formationDrawingMode, ref ____deltaMousePosition, ref ____formationDrawingStartingPosition,
                    ref ____formationDrawingStartingPointOfMouse, ref ____formationDrawingStartingTime);
            }
            if (isSelectFormationKeyReleased)
            {
                HandleSelectFormationKeyUp(__instance, ref ____clickedFormation, ___PlayerOrderController,
                    ____orderRotationEntities, ref ____formationDrawingMode, ref ____deltaMousePosition,
                    ref ____formationDrawingStartingPosition, ref ____formationDrawingStartingPointOfMouse,
                    ref ____formationDrawingStartingTime);
            }
            if ((__instance.Input.IsKeyReleased(InputKey.LeftMouseButton) || __instance.Input.IsKeyReleased(InputKey.ControllerRTrigger)) && ____isMouseDown || isSelectFormationKeyReleased)
            {
                ____isMouseDown = false;
                typeof(OrderTroopPlacer).GetMethod("HandleMouseUp", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.Invoke(__instance, new object[] { });
            }
            else if (____isMouseDown && isSelectFormationKeyDown)
            {
                if (!__instance.IsDrawingFacing && !__instance.IsDrawingForming)
                {
                    TryTransformFromClickingToDragging(__instance, ____formationDrawingStartingPointOfMouse,
                        ____formationDrawingStartingTime, ___PlayerOrderController, ref ____clickedFormation,
                        ref ____formationDrawingMode, ____isMouseDown);
                }
            }
            if ((__instance.Input.IsKeyDown(InputKey.LeftMouseButton) || __instance.Input.IsKeyDown(InputKey.ControllerRTrigger)) && ____isMouseDown)
            {
                if (___formationDrawTimer.Check(MBCommon.GetApplicationTime()) &&
                    !__instance.IsDrawingFacing &&
                    !__instance.IsDrawingForming)
                {
                    if (_currentCursorState == CursorState.Ground)
                        typeof(OrderTroopPlacer)
                            .GetMethod("UpdateFormationDrawing", BindingFlags.Instance | BindingFlags.NonPublic)
                            .Invoke(__instance, new object[] { false });
                }
            }
            else if (__instance.IsDrawingForced)
            {
                //Utilities.DisplayMessage("drawing forced");
                Reset(ref ____isMouseDown, ref ____formationDrawingMode, ref ____formationDrawingStartingPosition,
                    ref ____formationDrawingStartingPointOfMouse, ref ____formationDrawingStartingTime,
                    ref ____mouseOverFormation, ref ____clickedFormation);
                ____formationDrawingMode = true;
                BeginFormationDraggingOrClicking(__instance, ref ____deltaMousePosition,
                    ref ____formationDrawingStartingPosition, ref ____formationDrawingStartingPointOfMouse,
                    ref ____formationDrawingStartingTime);
                //HandleMousePressed();
                typeof(OrderTroopPlacer)
                    .GetMethod("UpdateFormationDrawing", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(__instance, new object[] { false });
            }
            else if (__instance.IsDrawingFacing || ____wasDrawingFacing)
            {
                if (__instance.IsDrawingFacing)
                {
                    Reset(ref ____isMouseDown, ref ____formationDrawingMode, ref ____formationDrawingStartingPosition,
                        ref ____formationDrawingStartingPointOfMouse, ref ____formationDrawingStartingTime,
                        ref ____mouseOverFormation, ref ____clickedFormation);

                    typeof(OrderTroopPlacer)
                        .GetMethod("UpdateFormationDrawingForFacingOrder", BindingFlags.Instance | BindingFlags.NonPublic)
                        .Invoke(__instance, new object[] { false });
                }
            }
            else if (__instance.IsDrawingForming || ____wasDrawingForming)
            {
                if (__instance.IsDrawingForming)
                {
                    Reset(ref ____isMouseDown, ref ____formationDrawingMode, ref ____formationDrawingStartingPosition,
                        ref ____formationDrawingStartingPointOfMouse, ref ____formationDrawingStartingTime,
                        ref ____mouseOverFormation, ref ____clickedFormation);

                    typeof(OrderTroopPlacer)
                        .GetMethod("UpdateFormationDrawingForFormingOrder", BindingFlags.Instance | BindingFlags.NonPublic)
                        .Invoke(__instance, new object[] { false });
                }
            }
            else if (____wasDrawingForced)
            {
                Reset(ref ____isMouseDown, ref ____formationDrawingMode, ref ____formationDrawingStartingPosition,
                    ref ____formationDrawingStartingPointOfMouse, ref ____formationDrawingStartingTime,
                    ref ____mouseOverFormation, ref ____clickedFormation);
            }
            else
            {
                typeof(OrderTroopPlacer)
                    .GetMethod("UpdateFormationDrawingForDestination", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(__instance, new object[] { false });
            }

            UpdateInputForContour(____mouseOverFormation);
            foreach (GameEntity orderPositionEntity in ____orderPositionEntities)
                orderPositionEntity.SetPreviousFrameInvalid();
            foreach (GameEntity orderRotationEntity in ____orderRotationEntities)
                orderRotationEntity.SetPreviousFrameInvalid();
            ____wasDrawingForced = __instance.IsDrawingForced;
            ____wasDrawingFacing = __instance.IsDrawingFacing;
            ____wasDrawingForming = __instance.IsDrawingForming;
            ___wasDrawnPreviousFrame = ___isDrawnThisFrame;

            return false;
        }
        private static void UpdateInputForContour(Formation ____mouseOverFormation)
        {
            _contourView?.MouseOver(____mouseOverFormation);
        }
        private static void Reset(ref bool ____isMouseDown, ref bool ____formationDrawingMode, ref WorldPosition? ____formationDrawingStartingPosition,
            ref Vec2? ____formationDrawingStartingPointOfMouse, ref float? ____formationDrawingStartingTime,
            ref Formation ____mouseOverFormation, ref Formation ____clickedFormation)
        {
            ____isMouseDown = false;
            ____formationDrawingMode = false;
            ____formationDrawingStartingPosition = new WorldPosition?();
            ____formationDrawingStartingPointOfMouse = new Vec2?();
            ____formationDrawingStartingTime = new float?();
            ____mouseOverFormation = (Formation)null;
            ____clickedFormation = (Formation)null;
        }

        public static void SelectFormationFromUI(OrderTroopPlacer __instance, Formation ____clickedFormation)
        {
            var uiHandler = __instance.Mission.GetMissionBehavior<MissionGauntletSingleplayerOrderUIHandler>();
            if (uiHandler == null)
                return;

            var dataSource = (MissionOrderVM)_dataSource.GetValue(uiHandler);
            if (dataSource == null)
            {
                return;
            }

            dataSource.OnSelect(____clickedFormation.Index);
        }

        public static void SelectFormationFromController(OrderTroopPlacer __instance, OrderController ___PlayerOrderController, Formation ____clickedFormation)
        {
            if (!__instance.Input.IsControlDown())
            {
                ___PlayerOrderController.ClearSelectedFormations();
                ___PlayerOrderController.SelectFormation(____clickedFormation);
            }
            else if (___PlayerOrderController.IsFormationListening(____clickedFormation))
            {
                ___PlayerOrderController.DeselectFormation(____clickedFormation);
            }
            else
            {
                ___PlayerOrderController.SelectFormation(____clickedFormation);
            }
        }
    }
}
