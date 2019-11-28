using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;


namespace BundleScript
{
    public static class CreateBundleUtil
    {
        public static void SaveTextAsset(string path, string data)
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