using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using ICSharpCode.SharpZipLib.Zip;

namespace BundleScript
{
    [System.AttributeUsage(System.AttributeTargets.Parameter, Inherited = false)]
    public class CreateEnvironmentBundle : Attribute
    {
        [MenuItem("Assets/CreateEnvritonmentBundle", false, 20)]
        static void CreateBundle()
        {
            if (Selection.activeObject != null)
            {
                var obj = Selection.activeObject;
                SceneAsset sceneAsset = obj as SceneAsset;
                if (sceneAsset != null)
                {
                    var ceb = new CreateEnvironmentBundle();
                    ceb.GenerateBundle(sceneAsset);
                }
            }
        }

        void GenerateBundle(SceneAsset root, [System.Runtime.CompilerServices.CallerFilePath] string fpath = "")
        {
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

                var scene = EditorSceneManager.OpenScene(rootPath);

                string mapJsonPath = "";
                List<string> genDlls = new List<string>();
                foreach (var editObj in scene.GetRootGameObjects())
                {
                    var ga = new GetAttribute(editObj);
                    var attr = ga.GetAttributeScripts<AssetBundleScriptAttribute>();

                    if (attr.Any())
                    {
                        mapJsonPath = Path.Combine(rootDir, $"_{trueName}.json");
                        var attrList = ga.GetAttributeScripts();

                        CreateBundleUtil.SaveTextAsset(mapJsonPath, ga.ToJSON(attrList));

                        // 2. class名からdllを作る
                        var createdll = new CreateBundleDLL();
                        foreach (var kv in attr)
                        {
                            kv.Value.ForEach(x =>
                            {
                                var t = System.AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).Where(bx => $"{bx.Namespace}.{bx.Name}".EndsWith(x)).FirstOrDefault();
                                var tname = t == null ? "null!" : t.Name;
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
                    }
                }

                var assetBundlesLocation = Path.Combine(Application.dataPath, "..", "AssetBundles");

                // assetbundleを作る
                Simulator.Editor.Build.BuildEnvironmentBundle(assetBundlesLocation, new List<string> { trueName }, (guid, archive) =>
                {
                    if (string.IsNullOrEmpty(mapJsonPath))
                    {
                        return;
                    }
                    // add dllmapping json
                    archive.Add(new StaticDiskDataSource(Path.GetFullPath(mapJsonPath)), "dllmap.json", CompressionMethod.Stored, true);

                    // add managed dll
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


    }
}