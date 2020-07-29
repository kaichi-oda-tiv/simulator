/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

namespace Simulator.ScenarioEditor.Utilities
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class PrefabsPools : MonoBehaviour
    {
        private class PrefabPool
        {
            private Transform poolParent;

            private GameObject originPrefab;

            private List<GameObject> instances = new List<GameObject>();

            public PrefabPool(Transform parent, GameObject prefab)
            {
                poolParent = parent;
                originPrefab = prefab;
            }

            public GameObject GetInstance()
            {
                if (instances.Count > 0)
                {
                    var id = instances.Count - 1;
                    var instance = instances[id];
                    instances.RemoveAt(id);
                    return instance;
                }

                return Populate();
            }

            public void ReleaseInstance(GameObject instance)
            {
                instance.transform.SetParent(poolParent);
                instances.Add(instance);
            }

            private GameObject Populate()
            {
                var instance = Instantiate(originPrefab, poolParent);
                if (instance.scene != poolParent.gameObject.scene)
                    Debug.LogWarning("ERROR");
                return instance;
            }
        }

        private Dictionary<GameObject, PrefabPool> prefabPools = new Dictionary<GameObject, PrefabPool>();

        private Dictionary<GameObject, PrefabPool> instanceToPool = new Dictionary<GameObject, PrefabPool>();

        public GameObject GetInstance(GameObject prefab)
        {
            if (!prefabPools.TryGetValue(prefab, out var pool))
            {
                var poolParent = new GameObject(prefab.name);
                SceneManager.MoveGameObjectToScene(poolParent, gameObject.scene);
                poolParent.transform.SetParent(transform);
                pool = new PrefabPool(poolParent.transform, prefab);
                prefabPools.Add(prefab, pool);
            }

            var instance = pool.GetInstance();
            instanceToPool.Add(instance, pool);
            return instance;
        }

        public void ReturnInstance(GameObject instance)
        {
            if (!instanceToPool.TryGetValue(instance, out var pool))
            {
                Debug.LogWarning("Passed instance cannot be returned to the pool as it was not found in the register.");
                return;
            }

            pool.ReleaseInstance(instance);
            instanceToPool.Remove(instance);
        }
    }
}