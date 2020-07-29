/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

namespace Simulator.ScenarioEditor.Data
{
    using SimpleJSON;

    public class JsonScenario
    {
        private JSONNode scenarioData;

        public JsonScenario(JSONNode scenarioData)
        {
            this.scenarioData = scenarioData;
        }

        public JSONNode ScenarioData => scenarioData;
    }
}