using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class FindComponent
{

    [MenuItem("Assets/FindComponentType", false, 20), MenuItem("GameObject/FindComponentType", false, 20)]
    static void FindComponentType()
    {
        new FindComponent().SearchComponent<CallHello>(Selection.activeGameObject);

    }

    [MenuItem("Assets/FindComponentString", false, 20), MenuItem("GameObject/FindComponentString", false, 20)]
    static void FindComponentString()
    {
        new FindComponent().SearchComponentString(Selection.activeGameObject, "CallHello");
    }

    [MenuItem("Assets/FindComponentHybrid", false, 20), MenuItem("GameObject/FindComponentHybrid", false, 20)]
    static void FindComponentHybrid()
    {
        var root = Selection.activeGameObject;

        var list = root.GetComponentsInChildren(typeof(CallHello), true);
        new FindComponent().Log(list.Select(x => x.ToString()));
    }

    void SearchComponent<T>(GameObject root)
    {
        var list = root.GetComponentsInChildren<T>(true);
        Log(list.Select(x => x.ToString()));
    }

    void SearchComponentString(GameObject root, string classname)
    {
        Debug.Log($"{classname} / " + (System.Type.GetType(classname) == null ? "null!" : "yes"));
        var list = root.GetComponentsInChildren(System.Type.GetType(classname), true);
        Log(list.Select(x => x.ToString()));
    }

    void Log(IEnumerable<string> list)
    {
        foreach (var l in list)
        {
            Debug.Log(l);
        }
    }

}
