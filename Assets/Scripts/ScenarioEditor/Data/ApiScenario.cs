/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

namespace Simulator.ScenarioEditor.Data
{
    using SimpleJSON;

    public class ApiScenario
    {
        private JSONNode scenarioData;

        public ApiScenario(JSONNode scenarioData)
        {
            this.scenarioData = scenarioData;
        }

        public JSONNode ScenarioData => scenarioData;
    }
}