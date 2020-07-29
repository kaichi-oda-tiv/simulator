/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

namespace Simulator.ScenarioEditor.UI.FileEdit
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Managers;
    using UnityEngine;
    using UnityEngine.UI;

    public class SelectFileDialog : MonoBehaviour
    {
        private Action<string> callback;
        private string[] viewedExtensions;
        private SelectFileDialogFileButton selectedFile;
        private List<SelectFileDialogFileButton> filesButtons = new List<SelectFileDialogFileButton>();
        private bool allowCustomFilename;
        private string currentPath;

        public Text title;
        public InputField directoryPathInputField;
        public RectTransform filesGrid;
        public SelectFileDialogFileButton fileButtonSample;
        public InputField customFileNameInputField;

        public string DirectoryPath => currentPath;

        public string FilePath => allowCustomFilename
            ? $"{DirectoryPath}{customFileNameInputField.text}"
            : $"{DirectoryPath}{selectedFile.title.text}";

        public void Show(Action<string> pathSelected, bool allowCustomFilename, string directoryPath = null,
            string dialogTitle = "Select File Dialog", string[] extensions = null)
        {
            callback = pathSelected;
            this.allowCustomFilename = allowCustomFilename;
            customFileNameInputField.text = "";
            title.text = dialogTitle;
            viewedExtensions = extensions;
            var path = directoryPath ?? Application.persistentDataPath;
            SelectDirectoryPath(path);
            customFileNameInputField.interactable = allowCustomFilename;
                
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            ClearFilesGrid();
            gameObject.SetActive(false);
        }

        public void SelectDirectoryPath(string path)
        {
            if (path == currentPath)
                return;
            if (!Directory.Exists(path))
            {
                path = Path.GetDirectoryName(path);
                if (!Directory.Exists(path))
                    return;
            }

            path = PathAddBackslash(path);
            directoryPathInputField.text = path;
            currentPath = path;

            ClearFilesGrid();

            var directories = Directory.GetDirectories(path);
            foreach (var directory in directories)
            {
                var buttonGameObject = ScenarioManager.Instance.prefabsPools.GetInstance(fileButtonSample.gameObject);
                buttonGameObject.transform.SetParent(filesGrid);
                buttonGameObject.SetActive(true);
                var button = buttonGameObject.GetComponent<SelectFileDialogFileButton>();
                button.ParentDialog = this;
                button.MarkAsDirectory(Path.GetFileName(directory));
                filesButtons.Add(button);
            }

            var searchPattern = "*";
            if (viewedExtensions != null)
            {
                var sb = new StringBuilder();
                for (var i = 0; i < viewedExtensions.Length; i++)
                {
                    var viewedExtension = viewedExtensions[i];
                    sb.Append("*.");
                    sb.Append(viewedExtension);
                    if (i < viewedExtensions.Length - 1)
                        sb.Append("|");
                }

                searchPattern = sb.ToString();
            }

            var files = Directory.GetFiles(path, searchPattern);
            foreach (var file in files)
            {
                var buttonGameObject = ScenarioManager.Instance.prefabsPools.GetInstance(fileButtonSample.gameObject);
                buttonGameObject.transform.SetParent(filesGrid);
                buttonGameObject.SetActive(true);
                var button = buttonGameObject.GetComponent<SelectFileDialogFileButton>();
                button.ParentDialog = this;
                button.MarkAsFile(Path.GetFileName(file));
                filesButtons.Add(button);
            }
        }

        private void ClearFilesGrid()
        {
            //Clear files grid
            if (selectedFile != null)
                selectedFile.Unselect();
            selectedFile = null;
            for (var i = filesButtons.Count - 1; i >= 0; i--)
            {
                var fileButton = filesButtons[i];
                ScenarioManager.Instance.prefabsPools.ReturnInstance(fileButton.gameObject);
            }

            filesButtons.Clear();
        }

        public void MoveToUpDirectory()
        {
            var separators = new[] {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar};
            var directoryPath = Path.GetDirectoryName(DirectoryPath.TrimEnd(separators));
            if (!string.IsNullOrEmpty(directoryPath))
                SelectDirectoryPath(directoryPath);
        }

        public void ApplyFile()
        {
            if (allowCustomFilename ? !string.IsNullOrEmpty(customFileNameInputField.text) : selectedFile != null)
            {
                callback?.Invoke(FilePath);
                Hide();
            }
        }

        public void SelectFile(SelectFileDialogFileButton button)
        {
            UnselectFile();
            selectedFile = button;
            selectedFile.Select();
            customFileNameInputField.SetTextWithoutNotify(selectedFile.title.text);
        }

        public void UnselectFile()
        {
            if (selectedFile == null) return;
            selectedFile.Unselect();
            selectedFile = null;
        }

        private string PathAddBackslash(string path)
        {
            // They're always one character but EndsWith is shorter than
            // array style access to last path character. Change this
            // if performance are a (measured) issue.
            string separator1 = Path.DirectorySeparatorChar.ToString();
            string separator2 = Path.AltDirectorySeparatorChar.ToString();

            // Trailing white spaces are always ignored but folders may have
            // leading spaces. It's unusual but it may happen. If it's an issue
            // then just replace TrimEnd() with Trim(). Tnx Paul Groke to point this out.
            path = path.TrimEnd();

            // Argument is always a directory name then if there is one
            // of allowed separators then I have nothing to do.
            if (path.EndsWith(separator1) || path.EndsWith(separator2))
                return path;

            // If there is the "alt" separator then I add a trailing one.
            // Note that URI format (file://drive:\path\filename.ext) is
            // not supported in most .NET I/O functions then we don't support it
            // here too. If you have to then simply revert this check:
            // if (path.Contains(separator1))
            //     return path + separator1;
            //
            // return path + separator2;
            if (path.Contains(separator2))
                return path + separator2;

            // If there is not an "alt" separator I add a "normal" one.
            // It means path may be with normal one or it has not any separator
            // (for example if it's just a directory name). In this case I
            // default to normal as users expect.
            return path + separator1;
        }
    }
}