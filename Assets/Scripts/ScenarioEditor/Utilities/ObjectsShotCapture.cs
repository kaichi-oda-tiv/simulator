/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

namespace Simulator.ScenarioEditor.Utilities
{
    using UnityEngine;

    public class ObjectsShotCapture : MonoBehaviour
    {
        //Ignoring Roslyn compiler warning for unassigned private field with SerializeField attribute
#pragma warning disable 0649
        [SerializeField]
        private Camera photoBoxCamera;
#pragma warning restore 0649

        private bool isInitialized;

        private void Start()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            Deinitialize();
        }

        private void Initialize()
        {
            if (isInitialized) return;
            photoBoxCamera.targetTexture = new RenderTexture(Screen.width, Screen.height, 16);
            isInitialized = true;
        }

        private void Deinitialize()
        {
            if (!isInitialized) return;
            photoBoxCamera.targetTexture.Release();
            isInitialized = false;
        }

        public Texture2D ShotObject(GameObject objectToShot)
        {
            Initialize();
            gameObject.SetActive(true);
            //Cache previous state
            var previousPosition = objectToShot.transform.position;
            var previousParent = objectToShot.transform.parent;
            var wasActive = objectToShot.activeSelf;
            //Parent the object to the camera
            objectToShot.transform.SetParent(transform);
            objectToShot.transform.localPosition = Vector3.zero;
            //Fit the object in the camera view 
            var b = new Bounds(objectToShot.transform.position, Vector3.zero);
            foreach (Renderer r in objectToShot.GetComponentsInChildren<Renderer>())
                b.Encapsulate(r.bounds);
            const float margin = 1.0f;
            var maxExtent = b.extents.magnitude;
            var minDistance = (maxExtent * margin) / Mathf.Sin(Mathf.Deg2Rad * photoBoxCamera.fieldOfView / 2.0f);
            var cameraPosition = photoBoxCamera.transform.forward * -minDistance;
            cameraPosition.y += b.size.y/2.0f;
            photoBoxCamera.transform.localPosition = cameraPosition;
            //Shot object
            photoBoxCamera.Render();
            var rt = photoBoxCamera.targetTexture;
            var previousActive = RenderTexture.active;
            RenderTexture.active = rt;
            var texture2D = new Texture2D(rt.width, rt.height);
            texture2D.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            //TODO color key background
            // var pixels = texture2D.GetPixels(0, 0, rt.width, rt.height);
            // var colorToClear = pixels[0];
            // for (var i = 0; i < pixels.Length; i++)
            // 	if (pixels[i] == colorToClear)
            // 		pixels[i] = Color.clear;
            // texture2D.SetPixels(pixels);
            texture2D.Apply();
            //Revert changes
            RenderTexture.active = previousActive;
            objectToShot.SetActive(wasActive);
            objectToShot.transform.SetParent(previousParent);
            objectToShot.transform.position = previousPosition;
            gameObject.SetActive(false);
            return texture2D;
        }
    }
}