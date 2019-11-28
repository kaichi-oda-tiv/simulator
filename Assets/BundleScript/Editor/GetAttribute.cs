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
        GameObject active = null;

        Dictionary<string, List<string>> treeAttributeList = null;
        public GetAttribute(GameObject active = null)
        {
            this.active = active ?? Selection.activeGameObject;
        }

        void Initialize(GameObject active)
        {
            GetAttributeScripts<AssetBundleScriptAttribute>(active);
        }

        public Dictionary<string, List<string>> GetAttributeScripts(GameObject root = null)
        {
            return GetAttributeScripts<AssetBundleScriptAttribute>(root);
        }

        /// <summary>
        /// Dictionary<Hierarcky-path,List<classname>>
        /// </summary>
        /// <param name="root"></param>
        /// <returns>Dictionary<string,List<string>></returns>
        public Dictionary<string, List<string>> GetAttributeScripts<T>(GameObject root = null)
        {
            if (root == null)
            {
                root = active;
            }

            Dictionary<string, List<string>> attributeDatas = new Dictionary<string, List<string>>();
            var list = root.GetComponentsInChildren<MonoBehaviour>();
            foreach (var c in list)
            {
                var type = c.GetType();
                var attr = type.GetCustomAttributes(typeof(T), true);
                if (!attr.Any())
                {
                    continue;
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

                string className = type.Name;
                if (!string.IsNullOrEmpty(type.Namespace))
                {
                    className = $"{type.Namespace}.{className}";
                }

                // treeがkeyでattrがvaluesのdic
                if (attributeDatas.ContainsKey(tree))
                {
                    attributeDatas[tree].Add(className);
                }
                else
                {
                    attributeDatas.Add(tree, new List<string> { className });
                }

            }

            treeAttributeList = attributeDatas;

            return attributeDatas;
        }

        public override string ToString()
        {
            return ToJSON(treeAttributeList);
        }

        public string ToJSON(Dictionary<string, List<string>> dict)
        {
            SimpleJSON.JSONNode node = new SimpleJSON.JSONObject();
            foreach (var kv in dict)
            {
                var arr = new SimpleJSON.JSONArray();
                kv.Value.Select(x => new JSONString(x)).ToList().ForEach(x => arr.Add(x));

                node.Add(kv.Key, arr);
            }
            return node.ToString();
        }


        public JSONNode FromJSON(string jsonString)
        {
            return JSON.Parse(jsonString);
        }

    }
}