using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ActionBuilderMVVM.Models;
using Newtonsoft.Json;
using Ookii.Dialogs.Wpf;

namespace ActionBuilderMVVM.Utils
{
    static class FileUtils
    {
        public static bool SaveActionAs(ActionModel action, ref string initialDirectory)
        {
            var saveActionDialog = new VistaSaveFileDialog
            {
                AddExtension = true,
                DefaultExt = ".act",
                Filter = "Action files (*.act)|*.act",
                InitialDirectory = initialDirectory,
                OverwritePrompt = true,
                Title = "Save Action"
            };
            if (saveActionDialog.ShowDialog() != true) return false;

            initialDirectory = saveActionDialog.FileName;
            action.Path = saveActionDialog.FileName;

            SaveAction(action);
            return true;
        }

        public static void SaveAction(ActionModel action)
        {
            var actionJson = JsonConvert.SerializeObject(action);
            File.WriteAllText(action.Path, actionJson);
        }

        public static ActionModel OpenAction(ref string initialDirectory)
        {
            var openFileDialog = new VistaOpenFileDialog
            {
                Filter = "Action files (*.act)|*.act|All files (*.*)|*.*",
                
                FileName = initialDirectory == string.Empty
                    ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    : initialDirectory
            };

            if (openFileDialog.ShowDialog() != true) return null;

            initialDirectory = openFileDialog.FileName;

            var contents = File.ReadAllText(openFileDialog.FileName);
            var action = JsonConvert.DeserializeObject<ActionModel>(contents);
            action.Path = openFileDialog.FileName;
            return action;
        }

        public static string ChangeSpriteDirDialog(string initialDirectory)
        {
            var dialog = new VistaFolderBrowserDialog
            {
                Description = "Please select a folder.",
                UseDescriptionForTitle = true,
                SelectedPath = initialDirectory == string.Empty
                    ? Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
                    : initialDirectory
            };

            if (!VistaFolderBrowserDialog.IsVistaFolderDialogSupported)
                MessageBox.Show("Because you are not using Windows Vista or later, the regular folder browser dialog will be used. Please use Windows Vista to see the new dialog.", "Sample folder browser dialog");

            if (dialog.ShowDialog() == true)
                return dialog.SelectedPath;
            else
                return "NOCHANGE";
        }
    }
}
