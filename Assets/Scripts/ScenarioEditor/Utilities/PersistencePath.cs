/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

namespace Simulator.ScenarioEditor.Utilities
{
    using UnityEngine;

    public class PersistencePath
    {
        private readonly string key;
        private string path;

        public string Value
        {
            get
            {
                if (string.IsNullOrEmpty(path))
                    path = PlayerPrefs.GetString(key, Application.persistentDataPath);
                return path;
            }
            set
            {
                path = value;
                PlayerPrefs.SetString(key, value);
            }
        }

        public PersistencePath(string key)
        {
            this.key = key;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}