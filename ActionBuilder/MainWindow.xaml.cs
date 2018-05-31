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
        private List<CharacterInfo> characters;
        private List<ActionInfo> actions;
        private List<EditorInfo> editorInfos;

        private bool loadedFromNew = false;
        private int lastSelectedCharacter = -1;

        public MainWindow()
        {
            if (!Directory.Exists("../../Editor"))
                Directory.CreateDirectory("../../Editor");

            if (!Directory.Exists("../../Actions"))
                Directory.CreateDirectory("../../Actions");

            if (!Directory.Exists("../../Characters"))
                Directory.CreateDirectory("../../Characters");

            if (!Directory.Exists("../../Textures"))
                Directory.CreateDirectory("../../Textures");

            characters = new List<CharacterInfo>();
            actions = new List<ActionInfo>();
            editorInfos = new List<EditorInfo>();

            InitializeComponent();

            loadCharacters("../../Characters/");
            if (File.Exists("../../Editor/lastCharacter.json"))
                loadActions(readFromJson("../../Editor/lastCharacter.json"));
        }

        private void loadActions(string character)
        {   
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(ActionInfo));

            foreach (string file in Directory.GetFiles($"../../Actions/{character}/"))
                using (StreamReader sr = new StreamReader(file))
                    actions.Add((ActionInfo) ser.ReadObject(sr.BaseStream));

            foreach (ActionInfo action in actions)
                currentActionDropdown.Items.Add(new ComboBoxItem().Content = action.name);

            currentActionDropdown.SelectedIndex = 0;
        }

        private void loadCharacters(string path)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(CharacterInfo));

            if (Directory.Exists(path))
            {
                foreach (string file in Directory.GetFiles(path))
                    using (StreamReader sr = new StreamReader(file))
                        characters.Add((CharacterInfo)ser.ReadObject(sr.BaseStream));

                foreach (CharacterInfo character in characters)
                    characterList.Items.Add(new ListBoxItem().Content = character.name);

                characterList.SelectedIndex = 0;
            }
        }

        private void updateActionNames()
        {
            for (int i = 0; i < actions.Count; ++i)
                currentActionDropdown.Items[i] = new ComboBoxItem().Content = actions[i].name;
        }

        private void updateCharacterNames()
        {
            for (int i = 0; i < characters.Count; ++i)
                characterList.Items[i] = new ComboBoxItem().Content = characters[i].name;
        }

        private void updatePaths()
        {
            for (int i = 0; i < editorInfos.Count; ++i)
                editorInfos[i].texturePath = $"Textures/{characters[characterList.SelectedIndex].name}/{actions[i].name}/";
        }

        private void newActionButton_Click(object sender, RoutedEventArgs e)
        {
            ActionInfo newAction = new ActionInfo();
            newAction.name = $"new action {actions.Count}";
            actions.Add(newAction);

            EditorInfo newEditorInfo = new EditorInfo();
            newEditorInfo.texturePath = $"Textures/{characters[characterList.SelectedIndex].name}/{newAction.name}/";

            currentActionDropdown.Items.Add(new ComboBoxItem().Content = newAction.name);

            currentActionDropdown.SelectedIndex = currentActionDropdown.Items.Count - 1;
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            saveAction();
        }

        private void saveAction()
        {
            MemoryStream outStream = new MemoryStream();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(ActionInfo));

            ser.WriteObject(outStream, actions[currentActionDropdown.SelectedIndex]);

            string filepath = $"../../Actions/{characters[characterList.SelectedIndex].name}/{actions[currentActionDropdown.SelectedIndex].name}.json";
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

            if (!File.Exists(path))
            {
                Directory.CreateDirectory(Directory.GetParent(path).FullName);
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
            if (currentActionDropdown.SelectedIndex >= 0)
                nameTextBox.Text = actions[currentActionDropdown.SelectedIndex].name;
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
            if (characterList.SelectedIndex >= 0)
            {
                lastSelectedCharacter = characterList.SelectedIndex;

                MemoryStream outStream = new MemoryStream();
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(string));

                ser.WriteObject(outStream, characters[lastSelectedCharacter].name);

                File.WriteAllText("../../Editor/lastCharacter.json", characters[lastSelectedCharacter].name);

                characterNameTextBox.Text = characters[lastSelectedCharacter].name;

                if (currentActionDropdown.HasItems)
                {
                    actions.Clear();
                    currentActionDropdown.Items.Clear();
                }

                if (currentActionDropdown.SelectedIndex >= 0)
                    saveAction();

                if (loadedFromNew == false)
                    loadActions(characters[lastSelectedCharacter].name);
                else
                    loadedFromNew = true;
            }
        }

        private void newCharacterButton_Click(object sender, RoutedEventArgs e)
        {
            loadedFromNew = true;

            CharacterInfo newCharacter = new CharacterInfo();
            newCharacter.name = $"new character {characters.Count}";
            characters.Add(newCharacter);

            characterList.Items.Add(new ListBoxItem().Content = newCharacter.name);

            characterList.SelectedIndex = characterList.Items.Count - 1;

            // write charactr file
            MemoryStream outStream = new MemoryStream();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(CharacterInfo));

            ser.WriteObject(outStream, characters[characterList.SelectedIndex]);

            string filepath = $"../../Characters/{characters[characterList.SelectedIndex].name}.json";
            writeToJson(filepath, outStream);

            Directory.CreateDirectory($"../../Actions/{characters[characterList.SelectedIndex].name}");

            loadActions(characters[characterList.SelectedIndex].name);
        }

        private void characterNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (characterList.HasItems && lastSelectedCharacter >= 0 && e.Key == Key.Enter)
            {
                var oldName = characters[lastSelectedCharacter].name;
                characters[lastSelectedCharacter].name = characterNameTextBox.Text;
                updateCharacterNames();

                // write charactr file
                MemoryStream outStream = new MemoryStream();
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(CharacterInfo));

                ser.WriteObject(outStream, characters[lastSelectedCharacter]);

                string filepath = $"../../Characters/{oldName}.json";
                writeToJson(filepath, outStream);

                Directory.Move($"../../Characters/{oldName}.json", $"../../Characters/{characters[lastSelectedCharacter].name}.json");
                Directory.Move($"../../Actions/{oldName}", $"../../Actions/{characters[lastSelectedCharacter].name}");
                characterList.SelectedIndex = lastSelectedCharacter;
            }
        }
    }
}
