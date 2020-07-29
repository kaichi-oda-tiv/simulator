/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

namespace Simulator.ScenarioEditor.UI.FileEdit
{
    using System.Collections;
    using System.IO;
    using Data.Deserializer;
    using Data.Serializer;
    using Inspector;
    using Managers;
    using SimpleJSON;
    using UnityEngine;
    using UnityEngine.UI;
    using Utilities;

    public class FileEditPanel : MonoBehaviour, IInspectorContentPanel
    {
        private const string PathsKey = "Simulator/ScenarioEditor/FileEdit/";
        private static readonly PersistencePath LoadPath = new PersistencePath(PathsKey + "Load");
        private static readonly PersistencePath SavePath = new PersistencePath(PathsKey + "Save");
        private static readonly PersistencePath ExportPythonPath = new PersistencePath(PathsKey + "ExportPython");
        
        //Ignoring Roslyn compiler warning for unassigned private field with SerializeField attribute
#pragma warning disable 0649
        [SerializeField]
        private Toggle invertedXRotationToggle;
        
        [SerializeField]
        private Toggle invertedYRotationToggle;
#pragma warning restore 0649

        public string MenuItemTitle => "File";

        private void Start()
        {
            var inputManager = ScenarioManager.Instance.inputManager;
            invertedXRotationToggle.SetIsOnWithoutNotify(inputManager.InvertedXRotation);
            invertedYRotationToggle.SetIsOnWithoutNotify(inputManager.InvertedYRotation);
        }

        public void LoadScenario()
        {
            ScenarioManager.Instance.selectFileDialog.Show(LoadScenarioFromJson, false, LoadPath.Value,
                "Load Scenario From Json", new[] {"json"});
        }

        private void LoadScenarioFromJson(string path)
        {
            LoadPath.Value = path;
            var json = JSONNode.Parse(File.ReadAllText(path));
            if (json != null && json.IsObject)
            {
                ScenarioManager.Instance.ResetScenario();
                JsonScenarioDeserializer.DeserializeScenario(json);
            }
        }

        public void SaveScenario()
        {
            ScenarioManager.Instance.selectFileDialog.Show(SaveScenarioToJson, true, SavePath.Value,
                "Save Scenario To Json", new[] {"json"});
        }

        private void SaveScenarioToJson(string path)
        {
            path = Path.ChangeExtension(path, ".json");
            SavePath.Value = path;
            var scenario = JsonScenarioSerializer.SerializeScenario();
            File.WriteAllText(path, scenario.ScenarioData.ToString());
        }

        public void ExportPythonApi()
        {
            ScenarioManager.Instance.selectFileDialog.Show(ExportPythonApi, true, ExportPythonPath.Value,
                "Export Scenario To Python Script", new[] {"py"});
        }

        private void ExportPythonApi(string path)
        {
            path = Path.ChangeExtension(path, ".py");
            ExportPythonPath.Value = path;
            var scenario = PythonScenarioSerializer.SerializeScenario();
            File.WriteAllText(path, scenario.ScenarioData);
        }

        public void ResetScenario()
        {
            ScenarioManager.Instance.ResetScenario();
        }

        public void ExitEditor()
        {
            ScenarioManager.Instance.ShowLoadingPanel();
            //Delay exiting editor so the loading panel can initialize
            StartCoroutine(DelayedExitEditor());
        }

        private IEnumerator DelayedExitEditor()
        {
            yield return null;
            Loader.ExitScenarioEditor();
        }

        void IInspectorContentPanel.Show()
        {
            gameObject.SetActive(true);
        }

        void IInspectorContentPanel.Hide()
        {
            gameObject.SetActive(false);
        }

        public void ChangeRotationXInversion(bool value)
        {
            ScenarioManager.Instance.inputManager.InvertedXRotation = value;
        }

        public void ChangeRotationYInversion(bool value)
        {
            ScenarioManager.Instance.inputManager.InvertedYRotation = value;
        }
    }
}