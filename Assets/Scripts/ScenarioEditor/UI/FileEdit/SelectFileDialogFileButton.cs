/**
 * Copyright (c) 2020 LG Electronics, Inc.
 *
 * This software contains code licensed as described in LICENSE.
 *
 */

namespace Simulator.ScenarioEditor.UI.FileEdit
{
    using System;
    using System.IO;
    using UnityEngine;
    using UnityEngine.UI;

    public class SelectFileDialogFileButton : MonoBehaviour
    {
        private enum ButtonType
        {
            None,
            File,
            Directory
        }

        private ButtonType buttonType;
        public Button button;
        public Text title;
        public Text Icon;

        public string FolderIcon;
        public string FileIcon;

        public SelectFileDialog ParentDialog { get; set; }

        public void MarkAsFile(string fileName)
        {
            buttonType = ButtonType.File;
            title.text = fileName;
            Icon.text = FileIcon;
        }

        public void MarkAsDirectory(string directoryName)
        {
            buttonType = ButtonType.Directory;
            title.text = directoryName + Path.DirectorySeparatorChar;
            Icon.text = FolderIcon;
        }

        public void Select()
        {
            button.interactable = false;
        }

        public void Unselect()
        {
            button.interactable = true;
        }

        public void Clicked()
        {
            switch (buttonType)
            {
                case ButtonType.None:
                    break;
                case ButtonType.File:
                    ParentDialog.SelectFile(this);
                    break;
                case ButtonType.Directory:
                    ParentDialog.SelectDirectoryPath($"{ParentDialog.DirectoryPath}{title.text}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}