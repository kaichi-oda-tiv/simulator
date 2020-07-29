/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

namespace Simulator.ScenarioEditor.Elements
{
    using Input;
    using UnityEngine;

    public abstract class ScenarioElement : MonoBehaviour, IDragHandler, IRotateHandler
    {
        private const float ScreenWidthRotation = 360 * 2;

        private Vector3 positionBeforeDrag;

        private Quaternion rotationBeforeRotating;

        private Vector2 rotationStartViewportPosition;

        private string uid;

        public virtual string Uid
        {
            get => uid ?? (uid = System.Guid.NewGuid().ToString());
            set => uid = value;
        }

        public virtual Transform TransformToDrag => transform;

        public virtual Transform TransformToRotate => transform;

        public abstract void Selected();

        public abstract void Destroy();

        void IDragHandler.DragStarted(Vector3 dragPosition)
        {
            positionBeforeDrag = TransformToDrag.position;
        }

        void IDragHandler.DragMoved(Vector3 dragPosition)
        {
            TransformToDrag.position = dragPosition;
            OnDragged();
        }

        void IDragHandler.DragFinished(Vector3 dragPosition)
        {
            TransformToDrag.position = dragPosition;
            OnDragged();
        }

        void IDragHandler.DragCancelled(Vector3 dragPosition)
        {
            TransformToDrag.position = positionBeforeDrag;
            OnDragged();
        }

        void IRotateHandler.RotationStarted(Vector2 viewportPosition)
        {
            rotationStartViewportPosition = viewportPosition;
            rotationBeforeRotating = TransformToRotate.localRotation;
        }

        void IRotateHandler.RotationChanged(Vector2 viewportPosition)
        {
            var rotationValue = (viewportPosition.x - rotationStartViewportPosition.x) * ScreenWidthRotation;
            TransformToRotate.localRotation = rotationBeforeRotating * Quaternion.Euler(0.0f, rotationValue, 0.0f);
            OnRotated();
        }

        void IRotateHandler.RotationFinished(Vector2 viewportPosition)
        {
            var rotationValue = (viewportPosition.x - rotationStartViewportPosition.x) * ScreenWidthRotation;
            TransformToRotate.localRotation = rotationBeforeRotating * Quaternion.Euler(0.0f, rotationValue, 0.0f);
            OnRotated();
        }

        void IRotateHandler.RotationCancelled(Vector2 viewportPosition)
        {
            TransformToRotate.localRotation = rotationBeforeRotating;
            OnRotated();
        }

        protected virtual void OnDragged()
        {
        }

        protected virtual void OnRotated()
        {
        }
    }
}