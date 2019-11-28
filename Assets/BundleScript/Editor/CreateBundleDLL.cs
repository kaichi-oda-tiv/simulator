using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using System.Runtime.InteropServices;

namespace BundleScript
{
    public class CreateBundleDLL
    {

        string editorPath;

        bool importUnityEngineDLL = true;

        public CreateBundleDLL()
        {
            editorPath = Path.GetDirectoryName(EditorApplication.applicationPath);
        }

        public void CreateDLLFromSelectedObject(string outputPath, Object[] selectObjects)
        {
            if (selectObjects.Length < 1)
            {
                Debug.LogWarning("object not selected");
                return;
            }

            var opath = outputPath;
            if (string.IsNullOrEmpty(opath))
            {
                opath = Path.GetFullPath($"{Application.dataPath}/Plugins");
            }

            if (!Directory.Exists(opath))
            {
                Directory.CreateDirectory(opath);
            }

            foreach (var obj in selectObjects)
            {
                if (!(obj is TextAsset))
                {
                    Debug.Log($"{obj.name} is not TextAsset({obj.GetType()})");
                    continue;
                }

                var assetPath = AssetDatabase.GetAssetPath(obj);

                CreateDLLSingle("", assetPath, opath);
            }
        }

        /// <summary>
        /// .csからdllを作成します
        /// </summary>
        /// <param name="baselib">ついでに参照したいソースのpathを,区切りで</param>
        /// <param name="scriptPath">dllを生成したい.csへのproject-path</param>
        /// <param name="outputPath">dllを保存するfullpath</param>
        /// <param name="reference"></param>
        public void CreateDLLSingle(string baselib, string scriptPath, string outputPath, List<string> reference = null)
        {
            var baseName = Path.GetFileNameWithoutExtension(scriptPath);
            var process = new System.Diagnostics.Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            process.StartInfo.FileName = $"\"{editorPath}/Data/Mono/bin/smcs.bat\"";
            string args = "";
            if (importUnityEngineDLL)
            {
                args += $" -r:\"{editorPath}/Data/Managed/UnityEngine.dll\"";
            }
            if (reference != null)
            {
                foreach (var r in reference)
                {
                    args += $" -lib:\"{r}\"";
                }
            }


            args += $" -target:library -out:{outputPath}/{baseName}.bytes {Directory.GetCurrentDirectory()}/{scriptPath}";
            if (!string.IsNullOrEmpty(baselib))
            {
                args += $" \"{baselib}\"";
            }

            process.StartInfo.Arguments = args;

            Debug.Log($"{process.StartInfo.FileName}{args}");

            process.Start();
            var stdout = process.StandardOutput.ReadToEnd();
            Debug.Log(stdout);
            var stderr = process.StandardError.ReadToEnd();
            if (!string.IsNullOrEmpty(stderr))
            {
                Debug.LogWarning(stderr);
            }

            process.WaitForExit();
        }

        IEnumerable<string> GetDllPathFromDllImportAttr(DllImportAttribute attr)
        {
            return AssetDatabase.FindAssets(attr.Value)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Select(x => AssetDatabase.GUIDToAssetPath(x));
        }

        List<string> GetDllImportLibPath(System.Type t)
        {
            List<string> r0 = new List<string>();
            BindingFlags bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

            r0.AddRange(t.GetMethods(bf).SelectMany(x => x.GetCustomAttributes<DllImportAttribute>(true)).SelectMany(x => GetDllPathFromDllImportAttr(x)));
            r0.AddRange(t.GetProperties(bf).SelectMany(x => x.GetCustomAttributes<DllImportAttribute>(true)).SelectMany(x => GetDllPathFromDllImportAttr(x)));
            r0.AddRange(t.GetFields(bf).SelectMany(x => x.GetCustomAttributes<DllImportAttribute>(true)).SelectMany(x => GetDllPathFromDllImportAttr(x)));

            r0 = r0.Select(x => Path.GetFullPath(x)).ToList();

            return r0;
        }

        void ParseReference(string classFile)
        {
            try
            {
                var asm = Assembly.LoadFile(Path.GetFullPath(classFile));
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }
    }
}