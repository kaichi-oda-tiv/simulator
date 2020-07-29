/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

namespace Simulator.ScenarioEditor.Data
{
    public class PythonScenario
    {
        private string scenarioData;

        public PythonScenario(string scenarioData)
        {
            this.scenarioData = scenarioData;
        }

        public string ScenarioData => scenarioData;
    }
}