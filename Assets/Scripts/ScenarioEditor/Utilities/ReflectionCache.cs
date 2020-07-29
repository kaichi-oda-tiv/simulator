/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

namespace Simulator.ScenarioEditor.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEngine;

    public static class ReflectionCache
    {
        private static readonly Dictionary<string, Type> typesByName = new Dictionary<string, Type>();

        static ReflectionCache()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (var i = 0; i < assemblies.Length; i++)
            {
                var assembly = assemblies[i];
                if (!assembly.ManifestModule.Name.Contains("Simulator"))
                    continue;
                try
                {
                    foreach (var definedType in assembly.GetTypes())
                    {
                        if (string.IsNullOrEmpty(definedType.FullName)) continue;

                        if (!typesByName.ContainsKey(definedType.FullName))
                            typesByName.Add(definedType.FullName, definedType);
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    foreach (Exception inner in ex.LoaderExceptions)
                    {
                        Debug.LogError(inner.Message);
                    }
                }
            }
        }

        public static Type GetType(string fullname)
        {
            return string.IsNullOrEmpty(fullname) ? null : typesByName[fullname];
        }

        public static List<Type> FindTypes(Func<Type, bool> filter)
        {
            return typesByName.Values.Where(type => filter == null || filter.Invoke(type)).ToList();
        }
    }
}