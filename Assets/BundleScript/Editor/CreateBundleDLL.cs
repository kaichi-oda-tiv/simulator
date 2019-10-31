using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEditor;


namespace BundleScript
{
    public class CreateBundleDLL
    {

        string editorPath;

        bool importUnityEngineDLL = true;

        Vector2 scroll;

        public CreateBundleDLL()
        {
            editorPath = Path.GetDirectoryName(EditorApplication.applicationPath);
            Debug.Log($"{editorPath}");
        }

        public void CreateDLLFromSelectedObject(string outputPath, Object[] selectObjects)
        {
            if (selectObjects.Length < 1)
            {
                Debug.Log("object not selected");
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

                CreateDLLSingle(assetPath, opath);
            }
        }

        public void CreateDLLSingle(string scriptPath, string outputPath)
        {
            var baseName = Path.GetFileNameWithoutExtension(scriptPath);
            var process = new System.Diagnostics.Process();

            process.StartInfo.FileName = $"\"{editorPath}/Data/Mono/bin/smcs.bat\"";
            string args = "";
            if (importUnityEngineDLL)
            {
                args += $" -r:\"{editorPath}/Data/Managed/UnityEngine.dll\"";
            }

            args += $" -target:library -out:{outputPath}/{baseName}.bytes {Directory.GetCurrentDirectory()}/{scriptPath}";

            process.StartInfo.Arguments = args;

            Debug.Log($"{process.StartInfo.FileName}{args}");

            process.Start();
            process.WaitForExit();
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