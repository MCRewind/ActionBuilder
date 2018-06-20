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
        private List<BoxInfo> boxes;
        private List<List<BitmapImage>> actionAnims;

        private SolidColorBrush hurtBrush, hurtOverBrush, hitBrush, hitOverBrush;
        private CharacterInfo lastSelectedCharacter;
        private Point mouseDownPos;
        private Point currentBoxPos = new Point(0, 0);

        private bool loadedFromNew = false;
        private bool mouseDown = false;

        private int selectedBox;
        private int boxPlaceMode = -1;
        private int currentBoxCount;
        private int gridSize = 4;
        private int previousFrame = 0;

        struct BoxInfo
        {         
            public ActionInfo.Box box;
            public Rectangle rect;

            public BoxInfo(ActionInfo.Box b, Rectangle r)
            {
                box = b;
                rect = r;
            }
        }

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
            boxes = new List<BoxInfo>();

            hurtBrush = new SolidColorBrush();
            hurtBrush.Color = Color.FromRgb(112, 255, 150);

            hurtOverBrush = new SolidColorBrush();
            hurtOverBrush.Color = Color.FromRgb(52, 249, 114);

            hitBrush = new SolidColorBrush();
            hitBrush.Color = Color.FromRgb(255, 66, 116);

            hitOverBrush = new SolidColorBrush();
            hitOverBrush.Color = Color.FromRgb(226, 20, 75);

            InitializeComponent();

            loadCharacters("../../Characters/");
            if (File.Exists("../../Editor/lastCharacter.json"))
                loadActions(readFromJson("../../Editor/lastCharacter.json"));

            zoomBorder.MouseDown += ZoomBorder_MouseDown;
            zoomBorder.MouseWheel += ZoomBorder_MouseWheel;

            frameTypeDropdown.Items.Add("Startup");
            frameTypeDropdown.Items.Add("Active");
            frameTypeDropdown.Items.Add("Recovery");
            frameTypeDropdown.Items.Add("Buffer");

            boxKBAngleSlider.Minimum = 0;
            boxKBAngleSlider.Maximum = 360;
            boxKBAngleSlider.IsSnapToTickEnabled = true;

            actionTypeDropdown.ItemsSource = Enum.GetValues(typeof(Types.ActionType));
        }

        private void loadActions(string character)
        {   
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(ActionInfo));

            if (!Directory.Exists($"../../Actions/{character}/"))
                Directory.CreateDirectory($"../../Actions/{character}/");

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
            boxPlaceMode = 0;
        }

        private void hurtboxButton_Click(object sender, RoutedEventArgs e)
        {
            boxPlaceMode = 1;
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (currentAction() != null)
            {
                frameSlider.Maximum = currentAction().FrameCount;
                infiniteRangeMinDropdown.Items.Clear();
                infiniteRangeMinDropdown.Items.Add("None");
                infiniteRangeMaxDropdown.Items.Clear();
                infiniteRangeMaxDropdown.Items.Add("None");

                currentAction().hitboxes = new List<List<ActionInfo.Box>>();
                currentAction().hurtboxes = new List<List<ActionInfo.Box>>();
                for (int i = 0; i <= currentAction().FrameCount; ++i)
                {
                    infiniteRangeMinDropdown.Items.Add(i);
                    infiniteRangeMaxDropdown.Items.Add(i);

                    currentAction().hitboxes.Add(new List<ActionInfo.Box>());
                    currentAction().hurtboxes.Add(new List<ActionInfo.Box>());
                }
            }
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
            {
                List<ActionInfo.Box> tempHit = new List<ActionInfo.Box>();
                List<ActionInfo.Box> tempHurt = new List<ActionInfo.Box>();
                foreach (var boxInfo in boxes)
                    if (boxInfo.rect.Name.StartsWith("hit"))
                        tempHit.Add(boxInfo.box);
                    else if (boxInfo.rect.Name.StartsWith("hurt"))
                        tempHurt.Add(boxInfo.box);

                currentAction().hitboxes[previousFrame] = tempHit;
                currentAction().hurtboxes[previousFrame] = tempHurt;
                boxes.Clear();
                boxCanvas.Children.Clear();
                foreach (var box in currentAction().hitboxes[(int) frameSlider.Value])
                {
                    Rectangle r = new Rectangle();
                    r.Stroke = hitbox.Stroke;
                    r.Opacity = hitbox.Opacity;
                    r.Fill = hitBrush;
                    r.Name = "hit" + boxes.Count.ToString();
                    r.Width = box.width;
                    r.Height = box.height;
                    r.Visibility = Visibility.Visible;
                    r.MouseEnter += new MouseEventHandler(Box_MouseOver);
                    r.MouseLeave += new MouseEventHandler(Box_MouseLeave);
                    r.MouseLeftButtonDown += new MouseButtonEventHandler(Box_MouseLeftButtonDown);
                    boxCanvas.Children.Add(r);
                    Canvas.SetLeft(r, box.x);
                    Canvas.SetTop(r, box.y);
                    boxes.Add(new BoxInfo(box, r));
                    
                }
                foreach (var box in currentAction().hurtboxes[(int) frameSlider.Value])
                {
                    Rectangle r = new Rectangle();
                    r.Stroke = hurtbox.Stroke;
                    r.Opacity = hurtbox.Opacity;
                    r.Fill = hurtBrush;
                    r.Name = "hit" + boxes.Count.ToString();
                    r.Width = box.width;
                    r.Height = box.height;
                    r.Visibility = Visibility.Visible;
                    r.MouseEnter += new MouseEventHandler(Box_MouseOver);
                    r.MouseLeave += new MouseEventHandler(Box_MouseLeave);
                    r.MouseLeftButtonDown += new MouseButtonEventHandler(Box_MouseLeftButtonDown);
                    boxCanvas.Children.Add(r);
                    Canvas.SetLeft(r, box.x);
                    Canvas.SetTop(r, box.y);
                    boxes.Add(new BoxInfo(box, r));
                }
                if (frameSlider.Value < actionAnims[currentActionDropdown.SelectedIndex].Count)
                    currentFrameImage.Source = actionAnims[currentActionDropdown.SelectedIndex][(int) frameSlider.Value];

                previousFrame = (int)frameSlider.Value;
            }
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
                currentAction().hitboxes.RemoveAt((int)frameSlider.Value);
                currentAction().hurtboxes.RemoveAt((int)frameSlider.Value);
                frameSlider.Maximum = currentAction().FrameCount;
            }
        }

        private void insertFrameButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentAction() != null)
            {
                currentAction().insertFrame((int)frameSlider.Value);
                currentAction().hitboxes.Insert((int)frameSlider.Value, new List<ActionInfo.Box>());
                currentAction().hurtboxes.Insert((int)frameSlider.Value, new List<ActionInfo.Box>());
                frameSlider.Maximum = currentAction().FrameCount;
            }
        }

        private void ZoomBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("DDDDDDDDD");
            editCanvas.Height = 1080;
            editCanvas.Width = 1920;
        }

        private void ZoomBorder_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Console.WriteLine($"{zoomBorder.ZoomX}");
            editCanvas.Height = 1080; 
            editCanvas.Width = 1920; 
        }

        private void Box_MouseOver(object sender, MouseEventArgs e)
        {
            var rect = sender as Rectangle;

            if (rect.Fill.IsEqualTo(hitBrush))
            {
                rect.Fill = hitOverBrush;
            }
            else if (rect.Fill.IsEqualTo(hurtBrush))
            {
                rect.Fill = hurtOverBrush;
            }
        }

        private bool IsSelectedBox(Rectangle rect)
        {
            if (selectedBox < 0 || boxes.Count == 0)
                return false;
            if (boxes[selectedBox].rect.Equals(rect))
                return true;
            return false;
        }

        private int IndexFromRect(Rectangle rect)
        {
            for (int i = 0; i < boxes.Count; ++i)
            {
                if (boxes[i].rect == rect)
                {
                    return i;
                }
            }
            return -1;
        }

        private void Box_MouseLeave(object sender, MouseEventArgs e)
        {
            var rect = sender as Rectangle;

            if (!IsSelectedBox(rect))
            {
                if (rect.Fill.IsEqualTo(hitOverBrush))
                {
                    rect.Fill = hitBrush;
                }
                else if (rect.Fill.IsEqualTo(hurtOverBrush))
                {
                    rect.Fill = hurtBrush;
                }
            }
        }

        private void Box_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var rect = sender as Rectangle;

            if (!IsSelectedBox(rect))
            {
                // if selected box is a selected hitbox
                if (boxes[selectedBox].rect.Fill.IsEqualTo(hitOverBrush))
                {
                    boxes[selectedBox].rect.Fill = hitBrush;
                }
                // else if selected box is a selected hurtbox
                else if (boxes[selectedBox].rect.Fill.IsEqualTo(hurtOverBrush))
                {
                    boxes[selectedBox].rect.Fill = hurtBrush;
                }

                int index = IndexFromRect(rect);
                if (index != -1)
                {
                    selectedBox = index;
                    boxXText.Text = boxes[selectedBox].box.x.ToString();
                    boxYText.Text = boxes[selectedBox].box.y.ToString();
                    boxWidthText.Text = boxes[selectedBox].box.width.ToString();
                    boxHeightText.Text = boxes[selectedBox].box.height.ToString();
                    boxDMGText.Text = boxes[selectedBox].box.damage.ToString();
                    boxKBStrengthText.Text = boxes[selectedBox].box.knockbackStrength.ToString();
                    boxLifespanText.Text = boxes[selectedBox].box.lifespan.ToString();
                    boxIdTextBlock.Text = "ID: " + boxes[selectedBox].rect.Name;
                }
            }
        }

        private void boxXText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (selectedBox != -1)
                {
                    int x = 0;
                    if (int.TryParse(boxXText.Text, out x))
                    {
                        Canvas.SetLeft(boxes[selectedBox].rect, x);
                        boxes[selectedBox].box.setPos(x, boxes[selectedBox].box.y);
                    }
                }
            }
        }

        private void boxYText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (selectedBox != -1)
                {
                    int y = 0;
                    if (int.TryParse(boxYText.Text, out y))
                    {
                        Canvas.SetTop(boxes[selectedBox].rect, y);
                        boxes[selectedBox].box.setPos(boxes[selectedBox].box.x, y);
                    }
                }
            }
        }

        private void boxWidthText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (selectedBox != -1)
                {
                    int width = 0;
                    if (int.TryParse(boxWidthText.Text, out width))
                    {
                        boxes[selectedBox].rect.Width = width;
                        boxes[selectedBox].box.width = width;
                    }
                }
            }
        }

        private void boxHeightText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (selectedBox != -1)
                {
                    int height = 0;
                    if (int.TryParse(boxHeightText.Text, out height))
                    {
                        boxes[selectedBox].rect.Height = height;
                        boxes[selectedBox].box.height = height;
                    }
                }
            }
        }

        private void boxDMGText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (selectedBox != -1)
                {
                    int damage = 0;
                    if (int.TryParse(boxDMGText.Text, out damage))
                    {
                        boxes[selectedBox].box.damage = damage;
                    }
                }
            }
        }

        private void boxKBStrengthText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (selectedBox != -1)
                {
                    int strength = 0;
                    if (int.TryParse(boxKBStrengthText.Text, out strength))
                    {
                        boxes[selectedBox].box.knockbackStrength = strength;
                    }
                }
            }
        }

        private void boxKBAngleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            boxAngleText.Text = boxKBAngleSlider.Value.ToString() + "°";
        }

        private void infiniteRangeMinDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // selected index minus one to account for "none" option (-1)
            currentAction().InfiniteRangeMin = infiniteRangeMinDropdown.SelectedIndex - 1;
        }

        private void infiniteRangeMaxDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentAction().InfiniteRangeMax = infiniteRangeMaxDropdown.SelectedIndex;
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mouseDown = true;
            mouseDownPos = e.GetPosition(boxCanvas);

            if (boxPlaceMode > -1)
            {
                // Capture and track the mouse.
                boxCanvas.CaptureMouse();

                Rectangle box = new Rectangle();
                if (boxPlaceMode == 0)
                {
                    box.Stroke = hitbox.Stroke;
                    box.Opacity = hitbox.Opacity;
                    box.Fill = hitbox.Fill;

                    box.Fill = hitBrush;

                    box.Name = "hit" + boxes.Count.ToString();
                }
                else if (boxPlaceMode == 1)
                {
                    box.Stroke = hurtbox.Stroke;
                    box.Opacity = hurtbox.Opacity;
                    box.Fill = hurtbox.Fill;

                    box.Fill = hurtBrush;

                    box.Name = "hurt" + boxes.Count.ToString();
                }

                box.Visibility = Visibility.Visible;

                box.MouseEnter += new MouseEventHandler(Box_MouseOver);
                box.MouseLeave += new MouseEventHandler(Box_MouseLeave);
                box.MouseLeftButtonDown += new MouseButtonEventHandler(Box_MouseLeftButtonDown);

                BoxInfo info = new BoxInfo(new ActionInfo.Box(true), box);

                boxes.Add(info);

                boxCanvas.Children.Add(boxes.Last().rect);

                //currentBoxPos.Y = Math.Round(mouseDownPos.Y / gridSize) * gridSize;
                //currentBoxPos.X = Math.Round(mouseDownPos.X / gridSize) * gridSize;
                currentBoxPos.X = mouseDownPos.X;
                currentBoxPos.Y = mouseDownPos.Y;

                boxes.Last().box.setPos(currentBoxPos.X, currentBoxPos.Y);

                boxXText.Text = boxes.Last().box.x.ToString();
                boxYText.Text = boxes.Last().box.y.ToString();
                boxDMGText.Text = boxes.Last().box.damage.ToString();
                boxKBStrengthText.Text = boxes.Last().box.knockbackStrength.ToString();

                // Initial placement of the drag selection box.         
                Canvas.SetLeft(boxes.Last().rect, currentBoxPos.X);
                Canvas.SetTop(boxes.Last().rect, currentBoxPos.Y);

                // Make the drag selection box visible.
                boxes.Last().rect.Visibility = Visibility.Visible;
            }
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            mouseDown = false;
            Point mouseUpPos = e.GetPosition(boxCanvas);

            if (boxPlaceMode > -1)
            {
                currentBoxCount = boxes.Count;
                // Release the mouse capture and stop tracking it.
                editCanvas.ReleaseMouseCapture();

                // Hide the drag selection box.
                // selectionBox.Visibility = Visibility.Collapsed;

                Mouse.Capture(null);

                // TODO: 
                //
                // The mouse has been released, check to see if any of the items 
                // in the other canvas are contained within mouseDownPos and 
                // mouseUpPos, for any that are, select them!
                //
                boxPlaceMode = -1;
            }
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(boxCanvas);

            if (boxPlaceMode > -1)
            {
                if (mouseDown)
                {
                    // When the mouse is held down, reposition the drag selection box.
                    if (boxes.Count != currentBoxCount)
                    {
                        if (mouseDownPos.X < mousePos.X)
                        {
                            Canvas.SetLeft(boxes.Last().rect, mouseDownPos.X);
                            //boxes.Last().rect.Width = Math.Round((mousePos.X - mouseDownPos.X) / gridSize) * gridSize;
                            boxes.Last().rect.Width = mousePos.X - mouseDownPos.X;
                        }
                        else
                        {
                            Canvas.SetLeft(boxes.Last().rect, mousePos.X);
                            //boxes.Last().rect.Width = Math.Round((mouseDownPos.X - mousePos.X) / gridSize) * gridSize;
                            boxes.Last().rect.Width = mouseDownPos.X - mousePos.X;
                        }

                        if (mouseDownPos.Y < mousePos.Y)
                        {
                            Canvas.SetTop(boxes.Last().rect, mouseDownPos.Y);
                            //boxes.Last().rect.Height = Math.Round((mousePos.Y - mouseDownPos.Y) / gridSize) * gridSize;
                            boxes.Last().rect.Height = mousePos.Y - mouseDownPos.Y;
                        }
                        else
                        {
                            Canvas.SetTop(boxes.Last().rect, mousePos.Y);
                            //boxes.Last().rect.Height = Math.Round((mouseDownPos.Y - mousePos.Y) / gridSize) * gridSize;
                            boxes.Last().rect.Height = mouseDownPos.Y - mousePos.Y;
                        }

                        
                        boxes.Last().box.setDims(boxes.Last().rect.Width, boxes.Last().rect.Height);
                        boxWidthText.Text = boxes.Last().box.width.ToString();
                        boxHeightText.Text = boxes.Last().box.height.ToString();
                    }
                }
            }
        }
    }
}
