using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace BundleScript
{
    public class CreateVehicleBundle
    {
        // Projectの右クリックメニューから出す
        [MenuItem("Assets/CreateVehicleBundle", false, 20)]
        static void CreateBundle()
        {
            if (Selection.activeGameObject != null)
            {
                var vi = Selection.activeGameObject.GetComponent<Simulator.VehicleInfo>();
                if (vi != null)
                {
                    var cvb = new CreateVehicleBundle();
                    cvb.GenerateBundle(Selection.activeGameObject);
                }
            }
        }


        void GenerateBundle(GameObject root)
        {
            var trueName = root.name;
            // -2. rootのcloneを作る
            var rootAssetPath = AssetDatabase.GetAssetPath(root.GetInstanceID());

            var cloneFileName = Path.GetFileNameWithoutExtension(rootAssetPath);
            cloneFileName = $"_{cloneFileName}.prefab";
            var clonePath = Path.Combine($"{Path.GetDirectoryName(rootAssetPath)}", $"{cloneFileName}");

            Debug.Log($"{root.name} : {rootAssetPath}");
            Debug.Log($"{clonePath}");
            // -1. rootを退避させる為にrename
            root.name = $"true_{root.name}"; // 今からお前の名前は千だ！
            AssetDatabase.CopyAsset(rootAssetPath, clonePath);
            AssetDatabase.Refresh();
            var editObj = AssetDatabase.LoadAssetAtPath<GameObject>(clonePath);
            // 0. cloneしたprefabをrootの名前に変更
            editObj.name = trueName;
            // 1. 特定のAttributeが付いたclass名とtreeinfoを作成する
            var ga = new GetAttribute(editObj);
            var attr = ga.GetAttributeScripts<AssetBundleScriptAttribute>();

            // 2. class名からdllを作る
            var createdll = new CreateBundleDLL();
            foreach (var kv in attr)
            {
                string s = $"{kv.Key}\n";
                kv.Value.ForEach(x => { s += $"{x}\n"; });
                Debug.Log(s);
                // 3. prefabからclassをremoveする
                kv.Value.ForEach(x =>
                {
                    var t = Type.GetType(x);
                    var tname = t == null ? "null!" : t.Name;
                    Debug.Log($"{x} , {tname}");
                    var list = editObj.GetComponentsInChildren(t, true);
                    foreach (var c in list)
                    {
                        GameObject.Destroy(c);
                    }
                });
                // createdll.CreateDLLSingle(key, "");

            }

            //AssetDatabase.DeleteAsset(rootAssetPath);
            //root.name = trueName;
        }



    }
}
