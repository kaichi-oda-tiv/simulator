/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

namespace Simulator.ScenarioEditor.Input
{
    using System;
    using Agents;
    using Elements;
    using Managers;
    using Network.Core.Threading;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class InputManager : MonoBehaviour
    {
        private enum InputState
        {
            Idle,

            MovingCamera,

            DraggingElement,

            RotatingElement,

            AddingElement
        }

        private const float ZoomFactor = 10.0f;

        private const float RotationFactor = 3.0f;

        private const float KeyMoveFactor = 10.0f;

        private static string XRotationInversionKey = "Simulator/ScenarioEditor/InputManager/XRotationInversion";

        private static string YRotationInversionKey = "Simulator/ScenarioEditor/InputManager/YRotationInversion";


        private Camera scenarioCamera;

        private float targetTiltFree;

        private float targetLookFree;

        private Quaternion mouseFollowRot = Quaternion.identity;

        private int xRotationInversion;

        private int yRotationInversion;

        private int raycastLayerMask = ~0;

        private float raycastDistance;

        private RaycastHit[] raycastHits = new RaycastHit[5];

        private int raycastHitsCount;

        private InputState inputState;

        private bool mouseMoved;

        private Vector3 lastMousePosition;

        private Vector3 lastHitPosition;

        private IDragHandler dragHandler;

        private IRotateHandler rotateHandler;

        private IAddElementsHandler addElementsHandler;

        public LockingSemaphore InputSemaphore { get; } = new LockingSemaphore();

        public bool InvertedXRotation
        {
            get
            {
                // -1 - inverted rotation, 1 - uninverted rotation
                if (xRotationInversion == 0)
                    xRotationInversion = PlayerPrefs.GetInt(XRotationInversionKey, 1);
                return xRotationInversion < 0;
            }
            set
            {
                // -1 - inverted rotation, 1 - uninverted rotation
                var intValue = value ? -1 : 1;
                if (intValue == xRotationInversion) return;
                xRotationInversion = intValue;
                PlayerPrefs.SetInt(XRotationInversionKey, intValue);
            }
        }

        public bool InvertedYRotation
        {
            get
            {
                // -1 - inverted rotation, 1 - uninverted rotation
                if (yRotationInversion == 0)
                    yRotationInversion = PlayerPrefs.GetInt(YRotationInversionKey, 1);
                return yRotationInversion < 0;
            }
            set
            {
                // -1 - inverted rotation, 1 - uninverted rotation
                var intValue = value ? -1 : 1;
                if (intValue == yRotationInversion) return;
                yRotationInversion = intValue;
                PlayerPrefs.SetInt(YRotationInversionKey, intValue);
            }
        }

        private bool IsMouseOverGameWindow => !(0 > Input.mousePosition.x || 0 > Input.mousePosition.y ||
                                                Screen.width < Input.mousePosition.x ||
                                                Screen.height < Input.mousePosition.y);

        private void Start()
        {
            scenarioCamera = FindObjectOfType<ScenarioManager>()?.ScenarioCamera;
            if (scenarioCamera == null)
                throw new ArgumentException("Scenario camera reference is required in the ScenarioManager.");
            raycastDistance = scenarioCamera.farClipPlane - scenarioCamera.nearClipPlane;
            RecacheCameraRotation();
            var ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
            raycastLayerMask &= ~(1 << ignoreRaycastLayer);
        }

        private void Update()
        {
            if (InputSemaphore.IsLocked)
                return;
            RaycastAll();
            HandleMapInput();
        }

        public void RecacheCameraRotation()
        {
            var cameraEuler = scenarioCamera.transform.rotation.eulerAngles;
            targetTiltFree = cameraEuler.x;
            targetLookFree = cameraEuler.y;
        }

        private void RaycastAll()
        {
            //TODO Check if raycast is needed
            mouseMoved = (lastMousePosition - Input.mousePosition).magnitude > 1.0f;
            var ray = scenarioCamera.ScreenPointToRay(Input.mousePosition);
            raycastHitsCount = Physics.RaycastNonAlloc(ray, raycastHits, raycastDistance, raycastLayerMask);
            lastMousePosition = Input.mousePosition;
        }

        private RaycastHit? GetFurthestHit(bool ignoreRigidbodies = false)
        {
            RaycastHit? furthestHit = null;
            var furthestDistance = 0.0f;
            for (var i = 0; i < raycastHitsCount; i++)
                if (raycastHits[i].distance > furthestDistance &&
                    (!ignoreRigidbodies || raycastHits[i].rigidbody == null))
                {
                    furthestHit = raycastHits[i];
                    furthestDistance = furthestHit.Value.distance;
                }

            return furthestHit;
        }

        private void HandleMapInput()
        {
            RaycastHit? furthestHit;
            Vector3 furthestPoint;
            switch (inputState)
            {
                case InputState.Idle:
                    if (Input.GetMouseButtonDown(0) &&
                        !EventSystem.current.IsPointerOverGameObject())
                    {
                        furthestHit = GetFurthestHit();
                        if (furthestHit == null)
                            return;

                        ScenarioElement element = null;
                        for (int i = 0; i < raycastHitsCount; i++)
                        {
                            element = raycastHits[i].collider.gameObject.GetComponentInParent<ScenarioElement>();
                            if (element == null) continue;

                            element.Selected();
                            ScenarioManager.Instance.SelectedElement = element;
                            //Override furthest hit with selected element hit
                            furthestHit = raycastHits[i];
                            break;
                        }

                        if (element == null)
                            inputState = InputState.MovingCamera;

                        lastHitPosition = furthestHit.Value.point;
                    }

                    if (Input.GetMouseButtonDown(1))
                        ScenarioManager.Instance.SelectedElement = null;

                    break;
                case InputState.MovingCamera:
                    if (Input.GetMouseButtonUp(0))
                    {
                        inputState = InputState.Idle;
                        break;
                    }

                    if (IsMouseOverGameWindow && mouseMoved && raycastHitsCount > 0)
                    {
                        furthestHit = GetFurthestHit(true);
                        if (furthestHit == null)
                            return;
                        var cameraTransform = scenarioCamera.transform;
                        var cameraPosition = cameraTransform.position;
                        var deltaPosition = lastHitPosition - furthestHit.Value.point;
                        deltaPosition.y = 0.0f;
                        MoveCameraTo(cameraPosition + deltaPosition);
                    }

                    break;
                case InputState.DraggingElement:
                    //Check if drag was canceled
                    if (Input.GetMouseButtonUp(1) || Input.GetMouseButtonDown(1))
                    {
                        dragHandler.DragCancelled(lastHitPosition);
                        dragHandler = null;
                        inputState = InputState.Idle;
                        break;
                    }
                    //Check for drag finish
                    if (Input.GetMouseButtonUp(0))
                    {
                        if (EventSystem.current.IsPointerOverGameObject())
                            dragHandler.DragCancelled(lastHitPosition);
                        else
                            dragHandler.DragFinished(lastHitPosition);
                        dragHandler = null;
                        inputState = InputState.Idle;
                        break;
                    }
                    
                    //Apply current drag state
                    furthestHit = GetFurthestHit(true);
                    if (furthestHit == null)
                        break;
                    furthestPoint = furthestHit.Value.point;
                    lastHitPosition = furthestPoint;

                    if (mouseMoved)
                        dragHandler.DragMoved(furthestPoint);
                    break;

                case InputState.RotatingElement:
                    if (Input.GetMouseButtonUp(0))
                    {
                        rotateHandler.RotationFinished(scenarioCamera.ScreenToViewportPoint(Input.mousePosition));
                        rotateHandler = null;
                        inputState = InputState.Idle;
                        break;
                    }

                    if (Input.GetMouseButtonUp(1) || Input.GetMouseButtonDown(1))
                    {
                        rotateHandler.RotationCancelled(scenarioCamera.ScreenToViewportPoint(Input.mousePosition));
                        rotateHandler = null;
                        inputState = InputState.Idle;
                        break;
                    }

                    if (mouseMoved)
                        rotateHandler.RotationChanged(scenarioCamera.ScreenToViewportPoint(Input.mousePosition));
                    break;

                case InputState.AddingElement:
                    //Check if adding was canceled
                    if (Input.GetMouseButtonUp(1) || Input.GetMouseButtonDown(1))
                    {
                        addElementsHandler.AddingCancelled(lastHitPosition);
                        inputState = InputState.Idle;
                        break;
                    }
                    
                    //Apply current adding state
                    furthestHit = GetFurthestHit(true);
                    if (furthestHit == null)
                        break;
                    furthestPoint = furthestHit.Value.point;
                    lastHitPosition = furthestPoint;
                    if (Input.GetMouseButtonDown(0))
                    {
                        addElementsHandler.AddElement(furthestPoint);
                        break;
                    }

                    if (mouseMoved)
                        addElementsHandler.AddingMoved(furthestPoint);
                    break;
                default: throw new ArgumentOutOfRangeException();
            }

            //TODO advanced zoom and limit zooming
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                var cameraTransform = scenarioCamera.transform;
                if (Input.GetKey(KeyCode.UpArrow))
                    MoveCameraTo(cameraTransform.position +
                                 cameraTransform.forward * (KeyMoveFactor * Time.unscaledDeltaTime));
                if (Input.GetKey(KeyCode.DownArrow))
                    MoveCameraTo(cameraTransform.position -
                                 cameraTransform.forward * (KeyMoveFactor * Time.unscaledDeltaTime));
                if (Input.GetKey(KeyCode.RightArrow))
                    MoveCameraTo(cameraTransform.position +
                                 cameraTransform.right * (KeyMoveFactor * Time.unscaledDeltaTime));
                if (Input.GetKey(KeyCode.LeftArrow))
                    MoveCameraTo(cameraTransform.position -
                                 cameraTransform.right * (KeyMoveFactor * Time.unscaledDeltaTime));

                if (IsMouseOverGameWindow)
                {
                    var scrollValue = Input.GetAxis("Mouse ScrollWheel");
                    MoveCameraTo(cameraTransform.position + cameraTransform.forward * (scrollValue * ZoomFactor));

                    if (Input.GetMouseButton(1))
                    {
                        targetLookFree += Input.GetAxis("Mouse X") * RotationFactor * xRotationInversion;
                        targetTiltFree += Input.GetAxis("Mouse Y") * RotationFactor * yRotationInversion;
                        targetTiltFree = Mathf.Clamp(targetTiltFree, -90, 90);
                        mouseFollowRot = Quaternion.Euler(targetTiltFree, targetLookFree, 0f);
                        cameraTransform.rotation = mouseFollowRot;
                    }
                }
            }
        }

        private void MoveCameraTo(Vector3 position)
        {
            var mapBounds = ScenarioManager.Instance.MapManager.CurrentMapBounds;
            position.x = Mathf.Clamp(position.x, mapBounds.min.x, mapBounds.max.x);
            position.y = Mathf.Clamp(position.y, 5.0f, 200.0f);
            position.z = Mathf.Clamp(position.z, mapBounds.min.z, mapBounds.max.z);
            scenarioCamera.transform.position = position;
        }

        public void StartDraggingElement(IDragHandler dragHandler)
        {
            if (inputState != InputState.Idle) return;
            inputState = InputState.DraggingElement;
            this.dragHandler = dragHandler;
            RaycastAll();
            var furthestHit = GetFurthestHit();
            this.dragHandler.DragStarted(furthestHit?.point ?? Vector3.zero);
        }

        public void CancelDraggingElement(IDragHandler dragHandler)
        {
            if (this.dragHandler != dragHandler)
            {
                Debug.LogWarning("Cannot cancel dragging as passed element is currently not handled.");
            }

            RaycastAll();
            var furthestHit = GetFurthestHit();
            this.dragHandler.DragCancelled(furthestHit?.point ?? Vector3.zero);
            this.dragHandler = null;
            inputState = InputState.Idle;
        }

        public void StartRotatingElement(IRotateHandler rotateHandler)
        {
            if (inputState != InputState.Idle) return;
            inputState = InputState.RotatingElement;
            this.rotateHandler = rotateHandler;
            this.rotateHandler.RotationStarted(scenarioCamera.ScreenToViewportPoint(Input.mousePosition));
        }

        public void CancelRotatingElement(IRotateHandler rotateHandler)
        {
            if (this.rotateHandler != rotateHandler)
            {
                Debug.LogWarning("Cannot cancel rotating as passed element is currently not handled.");
            }

            this.rotateHandler.RotationCancelled(scenarioCamera.ScreenToViewportPoint(Input.mousePosition));
            this.rotateHandler = null;
            inputState = InputState.Idle;
        }

        public void StartAddingElements(IAddElementsHandler addElementsHandler)
        {
            if (inputState != InputState.Idle) return;
            inputState = InputState.AddingElement;
            this.addElementsHandler = addElementsHandler;
            RaycastAll();
            var furthestHit = GetFurthestHit();
            this.addElementsHandler.AddingStarted(furthestHit?.point ?? Vector3.zero);
        }

        public void CancelAddingElements(IAddElementsHandler addElementsHandler)
        {
            if (this.addElementsHandler != addElementsHandler)
            {
                Debug.LogWarning("Cannot cancel adding elements as passed element is currently not handled.");
            }

            RaycastAll();
            var furthestHit = GetFurthestHit();
            this.addElementsHandler.AddingCancelled(furthestHit?.point ?? Vector3.zero);
            this.addElementsHandler = null;
            inputState = InputState.Idle;
        }
    }
}