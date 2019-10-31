using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BundleScript
{
    public class CreateVehicleBundle
    {
        [MenuItem("Assets/CreateVehicleBundle", false, 20)]
        static void CreateBundle()
        {
            Debug.Log($"active? {Selection.activeGameObject != null}");
            if (Selection.activeGameObject != null)
            {
                var vi = Selection.activeGameObject.GetComponent<Simulator.VehicleInfo>();
                Debug.Log($"VehicleInfo ? {vi != null}");
                if (vi != null)
                {

                }
            }
        }


        void GenerateBundle(GameObject root)
        {
            // 1. 特定のAttributeが付いたclass名とtreeinfoを作成する
            var ga = new GetAttribute(root);
            var attr = ga.GetAttributeScripts();
            // 2. class名からdllを作る
            var createdll = new CreateBundleDLL();
            // 3. prefabからclassをremoveする
            // 4. 
        }



    }
}
