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
                    cvb.GenerateBundle2(Selection.activeGameObject);
                }
            }
        }

        void GenerateBundle2(GameObject root)
        {
            // 元からcopyして
            // copy側を弄って
            // 元を退避させて
            // copyを元のファイル名にして
            // assetbundle作って
            // copyを消して
            // 元を元に戻す

            Debug.Log($"root is {root.GetInstanceID()}");

            var rootPath = AssetDatabase.GetAssetPath(root.GetInstanceID());
            var copyFile = "copy_" + Path.GetFileName(rootPath);
            var copyPath = Path.Combine(Path.GetDirectoryName(rootPath), copyFile);

            var tempFile = "temp_" + Path.GetFileName(rootPath);
            var tempPath = Path.Combine(Path.GetDirectoryName(rootPath), tempFile);

            string log = "";

            log = AssetDatabase.CopyAsset(rootPath, copyPath).ToString();
            Debug.Log($"CopyAsset({rootPath},{copyPath}) => {log}");

            log = AssetDatabase.RenameAsset(rootPath, Path.GetFileNameWithoutExtension(tempFile));
            Debug.Log($"RenameAsset({rootPath},{Path.GetFileNameWithoutExtension(tempFile)} => {log}");
            log = AssetDatabase.RenameAsset(copyPath, Path.GetFileNameWithoutExtension(rootPath));
            Debug.Log($"RenameAsset({copyPath},{Path.GetFileNameWithoutExtension(rootPath)}) => {log}");

            // main処理
            var editObj = AssetDatabase.LoadAssetAtPath<GameObject>(rootPath);
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
                    var t = System.AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).Where(bx => $"{bx.Namespace}.{bx.Name}".EndsWith(x)).FirstOrDefault();
                    var tname = t == null ? "null!" : t.Name;
                    Debug.Log($"{x} , {tname}");
                    var list = editObj.GetComponentsInChildren(t, true);
                    foreach (var c in list)
                    {
                        GameObject.DestroyImmediate(c, true);
                    }
                });

                // createdll.CreateDLLSingle(key, "");
            }


            // 後しまつ

            log = AssetDatabase.DeleteAsset(rootPath).ToString();
            Debug.Log($"DeleteAsset({rootPath}) => {log}");

            log = AssetDatabase.RenameAsset(tempPath, Path.GetFileNameWithoutExtension(rootPath));
            Debug.Log($"RenameAsset({tempPath},{Path.GetFileNameWithoutExtension(rootPath)}) => {log}");

        }


        void GenerateBundle(GameObject root)
        {

            var trueName = root.name;
            Debug.Log($"truename : {trueName}");
            // -2. rootのcloneを作る
            var rootAssetPath = AssetDatabase.GetAssetPath(root.GetInstanceID());

            var cloneFileName = Path.GetFileNameWithoutExtension(rootAssetPath);
            cloneFileName = $"_{cloneFileName}.prefab";
            var clonePath = Path.Combine(Path.GetDirectoryName(rootAssetPath), cloneFileName);

            Debug.Log($"rootName : {root.name} : {rootAssetPath}");
            Debug.Log($"clonePath : {clonePath}");
            // -1. rootを退避させる為にrename
            root.name = $"true_{root.name}"; // 今からお前の名前は千だ！

            var trueNameTempPath = Path.Combine(Path.GetDirectoryName(rootAssetPath), root.name);

            AssetDatabase.CopyAsset(rootAssetPath, clonePath);
            AssetDatabase.Refresh();
            var editObj = AssetDatabase.LoadAssetAtPath<GameObject>(clonePath);
            Debug.Log($"editObj = {editObj}");
            // 0. cloneしたprefabをrootの名前に変更
            editObj.name = trueName;
            var cres = AssetDatabase.RenameAsset(clonePath, Path.GetFileNameWithoutExtension(rootAssetPath));
            Debug.Log($"cres : {cres}");
            AssetDatabase.Refresh();

            var tres = AssetDatabase.RenameAsset(rootAssetPath, Path.GetFileNameWithoutExtension(trueNameTempPath));
            AssetDatabase.Refresh();
            Debug.Log($"root rename  {root.name} / {tres}");

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
                    var t = System.AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).Where(bx => $"{bx.Namespace}.{bx.Name}".EndsWith(x)).FirstOrDefault();
                    var tname = t == null ? "null!" : t.Name;
                    Debug.Log($"{x} , {tname}");
                    var list = editObj.GetComponentsInChildren(t, true);
                    foreach (var c in list)
                    {
                        GameObject.DestroyImmediate(c, true);
                    }
                });

                // createdll.CreateDLLSingle(key, "");
            }

            //AssetDatabase.DeleteAsset(rootAssetPath);
            //            AssetDatabase.DeleteAsset(rootAssetPath);
            //            AssetDatabase.RenameAsset(trueNameTempPath, rootAssetPath);

            AssetDatabase.Refresh();
        }



    }
}
