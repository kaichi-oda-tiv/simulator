/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

namespace Simulator.ScenarioEditor.Agents
{
    using Managers;
    using UnityEngine;

    public class AgentVariant
    {
        public ScenarioAgentSource source;

        public string name;

        public GameObject prefab;

        private Texture2D iconTexture;

        private Sprite iconSprite;

        public Texture2D IconTexture
        {
            get
            {
                if (iconTexture == null)
                    iconTexture = ShotTexture();
                return iconTexture;
            }
        }

        public Sprite IconSprite
        {
            get
            {
                if (iconSprite == null)
                    iconSprite = Sprite.Create(IconTexture, new Rect(0.0f, 0.0f, IconTexture.width, IconTexture.height),
                        new Vector2(0.5f, 0.5f), 100.0f);
                return iconSprite;
            }
        }

        private Texture2D ShotTexture()
        {
            var instance = source.GetModelInstance(this);
            var texture = ScenarioManager.Instance.objectsShotCapture.ShotObject(instance);
            ScenarioManager.Instance.prefabsPools.ReturnInstance(instance);
            return texture;
        }
    }
}