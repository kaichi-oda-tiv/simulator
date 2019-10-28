using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using SimpleJSON;

namespace BundleScript
{
    public class GetAttribute
    {
        [MenuItem("GameObject/GetAttribute", false, 20)]
        static void OpenWindow()
        {
            var w = new GetAttribute();
            w.Initialize();
        }



        void Initialize()
        {
            GetAttributeScripts(Selection.activeGameObject);
        }

        void GetAttributeScripts(GameObject root)
        {
            Dictionary<string, List<string>> attributeDatas = new Dictionary<string, List<string>>();
            var list = root.GetComponentsInChildren<MonoBehaviour>();
            foreach (var c in list)
            {
                var type = c.GetType();
                var attr = type.GetCustomAttributes(typeof(AssetBundleScriptAttribute), true);
                if (attr.Length < 1)
                {
                    Debug.Log("Custom Attribute Class Not found...");
                }

                string tree = "";
                {
                    Transform objt = c.transform;
                    tree = $"{objt.name}";
                    while (objt.gameObject != root)
                    {
                        objt = objt.parent;
                        tree = $"{objt.name}/{tree}";
                    }
                }

                // treeがkeyでattrがvaluesのdic
                if (attributeDatas.ContainsKey(tree))
                {
                    attributeDatas[tree].Add(type.Name);
                }
                else
                {
                    attributeDatas.Add(tree, new List<string> { type.Name });
                }

                foreach (var n in attr)
                {
                    Debug.Log($"{tree}({c.GetHashCode().ToString("x")}) / {type} has {n.ToString()}");
                }
            }


            SimpleJSON.JSONNode node = new SimpleJSON.JSONObject();
            foreach (var kv in attributeDatas)
            {
                var arr = new SimpleJSON.JSONArray();
                kv.Value.Select(x => new JSONString(x)).ToList().ForEach(x => arr.Add(x));

                node.Add(kv.Key, arr);
            }

            Debug.Log(node.ToString());

            var data = JSON.Parse(node.ToString());
            foreach (var kv in data.Linq)
            {
                string s = $"key: {kv.Key} = {kv.Value.ToString()}";
                Debug.Log(s);
            }

        }

    }
}