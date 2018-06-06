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
using System.Windows.Media.Animation;
using MahApps.Metro.Controls;

namespace ActionBuilder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private List<CharacterInfo> characters;
        private List<ActionInfo> actions;
        private List<EditorInfo> editorInfos;

        private List<List<BitmapImage>> actionAnims;

        private bool loadedFromNew = false;
        private CharacterInfo lastSelectedCharacter;

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
            actionAnims = new List<List<BitmapImage>>();

            InitializeComponent();

            loadCharacters("../../Characters/");
            if (File.Exists("../../Editor/lastCharacter.json"))
                loadActions(readFromJson("../../Editor/lastCharacter.json"));

            frameTypeDropdown.Items.Add("Startup");
            frameTypeDropdown.Items.Add("Active");
            frameTypeDropdown.Items.Add("Recovery");
            frameTypeDropdown.Items.Add("Buffer");
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
                editorInfos[i].texturePath = $"Textures/{currentCharacter().name}/{actions[i].name}/";
        }

        private ActionInfo currentAction()
        {
            int index = currentActionDropdown.SelectedIndex;
            return index > -1 && index < actions.Count ? actions[index] : null;
        }

        private CharacterInfo currentCharacter()
        {
            int index = characterList.SelectedIndex;
            return index > -1 && index < characters.Count ? characters[index] : lastSelectedCharacter;
        }

        private void newActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (characterList.SelectedIndex >= 0)
            {
                ActionInfo newAction = new ActionInfo();
                newAction.name = $"new action {actions.Count}";
                actions.Add(newAction);

                EditorInfo newEditorInfo = new EditorInfo();
                newEditorInfo.texturePath = $"Textures/{currentCharacter().name}/{newAction.name}/";

                currentActionDropdown.Items.Add(new ComboBoxItem().Content = newAction.name);

                currentActionDropdown.SelectedIndex = currentActionDropdown.Items.Count - 1;
            }
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            saveAction();
        }

        private void saveAction()
        {
            if (currentActionDropdown.SelectedIndex >= 0)
            {
                MemoryStream outStream = new MemoryStream();
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(ActionInfo));

                ser.WriteObject(outStream, currentAction());

                string filepath = $"../../Actions/{currentCharacter().name}/{currentAction().name}.json";
                writeToJson(filepath, outStream);
            }
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
            if (currentAction() != null)
                frameSlider.Maximum = currentAction().FrameCount;
            if (currentActionDropdown.SelectedIndex >= 0)
                nameTextBox.Text = currentAction().name;
        }

        private void nameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (currentActionDropdown.HasItems && currentActionDropdown.SelectedIndex >= 0 && e.Key == Key.Enter)
            {
                int index = currentActionDropdown.SelectedIndex;
                currentAction().name = nameTextBox.Text;
                updateActionNames();
                currentActionDropdown.SelectedIndex = index;
            }
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (characterList.SelectedIndex >= 0)
            {

                MemoryStream outStream = new MemoryStream();
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(string));

                ser.WriteObject(outStream, currentCharacter().name);

                File.WriteAllText("../../Editor/lastCharacter.json", currentCharacter().name);

                characterNameTextBox.Text = currentCharacter().name;

                lastSelectedCharacter = currentCharacter();

                if (currentActionDropdown.HasItems)
                {
                    actions.Clear();
                    currentActionDropdown.Items.Clear();
                }

                if (currentActionDropdown.SelectedIndex >= 0)
                    saveAction();

                actionAnims.Clear();

                if (loadedFromNew == false)
                    loadActions(currentCharacter().name);
                else
                    loadedFromNew = true;

                for (int i = 0; i < actions.Count; ++i)
                {
                    actionAnims.Add(new List<BitmapImage>());
                    foreach (string file in Directory.GetFiles($"../../Textures/{currentCharacter().name}/"))
                    {
                        BitmapImage tempImg = new BitmapImage();
                        tempImg.BeginInit();
                        tempImg.UriSource = new Uri($@"{AppDomain.CurrentDomain.BaseDirectory}/../{file}", UriKind.Absolute);
                        tempImg.EndInit();
                        actionAnims[i].Add(tempImg);
                    }
                }
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

            ser.WriteObject(outStream, currentCharacter()); 

            string filepath = $"../../Characters/{currentCharacter().name}.json";
            writeToJson(filepath, outStream);

            Directory.CreateDirectory($"../../Actions/{currentCharacter().name}");
            Directory.CreateDirectory($"../../Textures/{currentCharacter().name}");

            loadActions(currentCharacter().name);
        }

        private void characterNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (characterList.HasItems && characterList.SelectedIndex >= 0 && e.Key == Key.Enter)
            {
                var oldName = currentCharacter().name;
                currentCharacter().name = characterNameTextBox.Text;
                updateCharacterNames();

                // write charactr file
                MemoryStream outStream = new MemoryStream();
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(CharacterInfo));

                ser.WriteObject(outStream, currentCharacter());

                string filepath = $"../../Characters/{oldName}.json";
                writeToJson(filepath, outStream);

                Directory.Move($"../../Characters/{oldName}.json", $"../../Characters/{currentCharacter().name}.json");
                Directory.Move($"../../Textures/{oldName}", $"../../Textures/{currentCharacter().name}");
                Directory.Move($"../../Actions/{oldName}", $"../../Actions/{currentCharacter().name}");
            }
        }

        private void frameSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (currentActionDropdown.SelectedIndex >= 0 && frameSlider.Value < currentAction().FrameCount && currentActionDropdown.SelectedIndex < actionAnims.Count)
                if (frameSlider.Value < actionAnims[currentActionDropdown.SelectedIndex].Count)
                    currentFrameImage.Source = actionAnims[currentActionDropdown.SelectedIndex][(int) frameSlider.Value];
        }

        private void prevFrameButton_Click(object sender, RoutedEventArgs e)
        {
            --frameSlider.Value;
        }

        private void nextFrameButton_Click(object sender, RoutedEventArgs e)
        {
            ++frameSlider.Value;
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private void pauseButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void removeFrameButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentAction() != null)
            {
                currentAction().removeFrame((int)frameSlider.Value);
                frameSlider.Maximum = currentAction().FrameCount;
            }
        }

        private void insertFrameButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentAction() != null)
            {
                currentAction().insertFrame((int)frameSlider.Value);
                frameSlider.Maximum = currentAction().FrameCount;
            }
        }
    }
}
