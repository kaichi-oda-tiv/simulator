using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using ICSharpCode.SharpZipLib.Zip;

namespace BundleScript
{
    [System.AttributeUsage(System.AttributeTargets.Parameter, Inherited = false)]
    public class CreateVehicleBundle : System.Attribute
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

        void GenerateBundle2(GameObject root, [System.Runtime.CompilerServices.CallerFilePath] string fpath = "")
        {
            // 元からcopyして
            // copy側を弄って
            // 元を退避させて
            // copyを元のファイル名にして
            // assetbundle作って
            // copyを消して
            // 元を元に戻す

            // callerfilepathって何か良い方法無いのか??


            Debug.Log($"root is {root.GetInstanceID()}");
            Debug.Log($"fpath is {fpath}\n{Path.GetDirectoryName(fpath)}");

            // 自分自身のpathを取得して、dll作成に必要なscriptのpathを作る
            string packagePath = "";

            var packageDir = Path.Combine(
                Path.GetDirectoryName(Path.Combine(Path.GetDirectoryName(fpath), $"..{Path.DirectorySeparatorChar}")),
                "Scripts"
            );
            Debug.Log($"packageDir is {packageDir}");
            packagePath = Path.Combine(packageDir, "*.cs");

            var rootPath = AssetDatabase.GetAssetPath(root.GetInstanceID());
            var rootDir = Path.GetDirectoryName(rootPath);
            var trueName = Path.GetFileNameWithoutExtension(rootPath);
            var copyFile = "copy_" + Path.GetFileName(rootPath);
            var copyPath = Path.Combine(rootDir, copyFile);

            var tempFile = "temp_" + Path.GetFileName(rootPath);
            var tempPath = Path.Combine(rootDir, tempFile);

            string log = "";
            try
            {

                log = AssetDatabase.CopyAsset(rootPath, copyPath).ToString();
                Debug.Log($"CopyAsset({rootPath},{copyPath}) => {log}");

                log = AssetDatabase.RenameAsset(rootPath, Path.GetFileNameWithoutExtension(tempFile));
                Debug.Log($"RenameAsset({rootPath},{Path.GetFileNameWithoutExtension(tempFile)} => {log}");
                log = AssetDatabase.RenameAsset(copyPath, trueName);
                Debug.Log($"RenameAsset({copyPath},{trueName}) => {log}");

                // main処理
                var editObj = AssetDatabase.LoadAssetAtPath<GameObject>(rootPath);
                var ga = new GetAttribute(editObj);
                var attr = ga.GetAttributeScripts<AssetBundleScriptAttribute>();

                // MEMO:truenameで保存するかそれとも別の名前(ex. map.json)で固定してしまうか?
                var mapJsonPath = Path.Combine(rootDir, $"_{trueName}.json");
                SaveTextAsset(mapJsonPath, ga.ToString());

                // 2. class名からdllを作る
                var createdll = new CreateBundleDLL();
                List<string> genDlls = new List<string>();
                foreach (var kv in attr)
                {
                    // TODO:attrを元にjsonを吐く
                    string s = $"{kv.Key}\n";
                    kv.Value.ForEach(x => { s += $"{x}\n"; });
                    Debug.Log(s);
                    kv.Value.ForEach(x =>
                    {
                        var t = System.AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).Where(bx => $"{bx.Namespace}.{bx.Name}".EndsWith(x)).FirstOrDefault();
                        var tname = t == null ? "null!" : t.Name;
                        Debug.Log($"{x} , {tname}");
                        var list = editObj.GetComponentsInChildren(t, true);

                        foreach (var c in list)
                        {
                            var mono = MonoScript.FromMonoBehaviour(c as MonoBehaviour);
                            var ap = AssetDatabase.GetAssetPath(mono);

                            genDlls.Add(Path.GetFileNameWithoutExtension(ap));

                            createdll.CreateDLLSingle(packagePath, ap, Path.GetFullPath(rootDir));
                            GameObject.DestroyImmediate(c, true);
                        }
                    });

                }

                var assetBundlesLocation = Path.Combine(Application.dataPath, "..", "AssetBundles");


                // assetbundleを作る
                Simulator.Editor.Build.BuildVehiclesBundle(assetBundlesLocation, new List<string> { trueName }, (archive) =>
                {
                    // jsonを追加
                    archive.Add(new StaticDiskDataSource(Path.GetFullPath(mapJsonPath)), Path.GetFileName(mapJsonPath), CompressionMethod.Stored, true);

                    // dll追加
                    foreach (var dllname in genDlls)
                    {
                        var p = Path.Combine(rootDir, $"{dllname}.bytes");
                        archive.Add(new StaticDiskDataSource(Path.GetFullPath(p)), Path.GetFileName(p), CompressionMethod.Stored, true);
                    }

                });
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {

                // 後始末
                log = AssetDatabase.DeleteAsset(rootPath).ToString();
                Debug.Log($"DeleteAsset({rootPath}) => {log}");

                log = AssetDatabase.RenameAsset(tempPath, Path.GetFileNameWithoutExtension(rootPath));
                Debug.Log($"RenameAsset({tempPath},{Path.GetFileNameWithoutExtension(rootPath)}) => {log}");
            }
        }


        void SaveTextAsset(string path, string data)
        {
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(data);

                    sw.Flush();
                }
            }
        }



    }

}
