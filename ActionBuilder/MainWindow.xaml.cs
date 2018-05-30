using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ActionBuilder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<ActionInfo> actions;
        private List<CharacterInfo> characters;
        private List<EditorInfo> editorInfos;

        public MainWindow()
        {

            actions = new List<ActionInfo>();
            editorInfos = new List<EditorInfo>();

            InitializeComponent();

            populateActions();
        }

        private void populateActions()
        {   
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(ActionInfo));

            foreach (string file in Directory.GetFiles("../../Actions/"))
                using (StreamReader sr = new StreamReader(file))
                    actions.Add((ActionInfo) ser.ReadObject(sr.BaseStream));

            foreach (ActionInfo action in actions)
                currentActionDropdown.Items.Add(new ComboBoxItem().Content = action.name);

            currentActionDropdown.SelectedIndex = 0;
        }

        private void updateActionNames()
        {
            for (int i = 0; i < actions.Count; ++i)
                currentActionDropdown.Items[i] = new ComboBoxItem().Content = actions[i].name;
        }

        private void updatePaths()
        {
            for (int i = 0; i < editorInfos.Count; ++i)
                editorInfos[i].texturePath = $"res/textures/{actions[i].name}/";
        }

        private void newActionButton_Click(object sender, RoutedEventArgs e)
        {
            ActionInfo newAction = new ActionInfo();
            newAction.name = "new action";
            actions.Add(newAction);

            EditorInfo newEditorInfo = new EditorInfo();
            newEditorInfo.texturePath = $"res/textures/{newAction.name}/";

            currentActionDropdown.Items.Add(new ComboBoxItem().Content = newAction.name);

            currentActionDropdown.SelectedIndex = currentActionDropdown.Items.Count - 1;
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            MemoryStream outStream = new MemoryStream();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(ActionInfo));

            ser.WriteObject(outStream, actions[currentActionDropdown.SelectedIndex]);

            string filepath = $"../../Actions/{actions[currentActionDropdown.SelectedIndex].name}.json";
            writeToJson(filepath, outStream);
        }

        private string readFromJson(string path)
        {
            var result = string.Empty;

            using (StreamReader r = new StreamReader(path))
            {
                result = r.ReadToEnd();
            }
            return result;
        }

        private void writeToJson(string path, MemoryStream contents)
        {
            string result = string.Empty;

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            contents.Position = 0;

            using (StreamReader r = new StreamReader(contents))
            {
                result = r.ReadToEnd();
            }

            File.WriteAllText(path, result);
        }

        private void hitboxButton_Click(object sender, RoutedEventArgs e)
        {
        
        }

        private void hurtboxButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Console.WriteLine(e.AddedItems);
            //Resources
        }

        private void nameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (currentActionDropdown.HasItems && currentActionDropdown.SelectedIndex >= 0 && e.Key == Key.Enter)
            {
                int index = currentActionDropdown.SelectedIndex;
                actions[currentActionDropdown.SelectedIndex].name = nameTextBox.Text;
                updateActionNames();
                currentActionDropdown.SelectedIndex = index;
            }
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
