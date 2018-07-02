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
using static ActionBuilder.ActionInfo;
using Microsoft.Win32;
using Newtonsoft.Json;
using Path = System.IO.Path;

namespace ActionBuilder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        #region VARIABLES
        private readonly List<CharacterInfo> _characters;
        private readonly List<ActionInfo> _actions;
        private readonly List<EditorInfo> _editorInfos;
        private List<BoxInfo> _hitboxes, _hurtboxes;
        private readonly List<List<BitmapImage>> _actionAnims;

        private readonly SolidColorBrush _hurtBrush;
        private readonly SolidColorBrush _hurtOverBrush;
        private readonly SolidColorBrush _hitBrush;
        private readonly SolidColorBrush _hitOverBrush;

        private CharacterInfo _lastSelectedCharacter;
        private IntCouple _mouseDownPos, _anchoredMouseDownPos;
        private IntCouple _currentBoxPos = new IntCouple(0, 0);

        private bool _loadedFromNew;
        private bool _mouseDown;

        private int _selectedBox;
        private int _boxPlaceMode = -1;
        private int _currentBoxCount;
        private int _previousFrame;

        private const int GridSize = 4;

        private struct IntCouple
        {
            public int X { get; set; }
            public int Y { get; set; }

            public IntCouple(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        private struct BoxInfo
        {
            public Box Box { get; }
            public Rectangle Rect { get; }

            public BoxInfo(Box b, Rectangle r)
            {
                Box = b;
                Rect = r;
            }
        }
#endregion

        public MainWindow()
        {
            if (!Directory.Exists("../../Editor"))
                try
                {
                    Directory.CreateDirectory("../../Editor");
                }
                catch (IOException e)
                {
                    Console.WriteLine(e);
                    throw;
                }

            if (!Directory.Exists("../../Actions"))
                try
                {
                    Directory.CreateDirectory("../../Actions");
                }
                catch (IOException e)
                {
                    Console.WriteLine(e);
                    throw;
                }

            if (!Directory.Exists("../../Characters"))
                try
                {
                    Directory.CreateDirectory("../../Characters");
                }
                catch (IOException e)
                {
                    Console.WriteLine(e);
                    throw;
                }

            if (!Directory.Exists("../../Textures"))
                try
                {
                    Directory.CreateDirectory("../../Textures");
                }
                catch (IOException e)
                {
                    Console.WriteLine(e);
                    throw;
                }

            _characters = new List<CharacterInfo>();
            _actions = new List<ActionInfo>();
            _editorInfos = new List<EditorInfo>();
            _actionAnims = new List<List<BitmapImage>>();
            _hitboxes = new List<BoxInfo>();
            _hurtboxes = new List<BoxInfo>();

            _hurtBrush = new SolidColorBrush { Color = Color.FromRgb(112, 255, 150) };
            _hurtOverBrush = new SolidColorBrush { Color = Color.FromRgb(52, 249, 114) };
            _hitBrush = new SolidColorBrush { Color = Color.FromRgb(255, 66, 116) };
            _hitOverBrush = new SolidColorBrush { Color = Color.FromRgb(226, 20, 75) };

            

            InitializeComponent();

            LoadCharacters("../../Characters/");
            //if (File.Exists("../../Editor/lastCharacter.json"))
               // LoadActions(JsonUtils.ReadFromJson("../../Editor/lastCharacter.json"));

            EditGridZoomBorder.MouseDown += EditGridZoomBorderMouseDown;
            EditGridZoomBorder.MouseWheel += EditGridZoomBorderMouseWheel;

            FrameTypeDropdown.Items.Add("Startup");
            FrameTypeDropdown.Items.Add("Active");
            FrameTypeDropdown.Items.Add("Recovery");
            FrameTypeDropdown.Items.Add("Buffer");

            BoxKbAngleSlider.Minimum = 0;
            BoxKbAngleSlider.Maximum = 360;
            BoxKbAngleSlider.IsSnapToTickEnabled = true;

            ActionTypeDropdown.ItemsSource = Enum.GetValues(typeof(Types.ActionType));

            UpdateBoxUiState();
        }

        private void LoadActions(string character)
        {
            if (!Directory.Exists($"../../Actions/{character}/"))
            {
                try
                {
                    Directory.CreateDirectory($"../../Actions/{character}/");
                }
                catch (IOException e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }

            foreach (var file in Directory.GetFiles($"../../Actions/{character}/"))
            {
                var contents = File.ReadAllText(file);
                var action = JsonConvert.DeserializeObject<ActionInfo>(contents);
                Console.WriteLine($"FRAME COUNT{action.FrameCount}");
                _actions.Add(action);
            }

            foreach (var action in _actions)
            {
                CurrentActionDropdown.Items.Clear();
                _editorInfos.Clear();
                _actionAnims.Clear();
                CurrentActionDropdown.Items.Add(new ComboBoxItem().Content = action.Name);

                var newEditorInfo = new EditorInfo { TexturePath = $"../../Textures/{CurrentCharacter().Name}/{action.Name}/" };
                _editorInfos.Add(newEditorInfo);
                _actionAnims.Add(new List<BitmapImage>());
            }

            CurrentActionDropdown.SelectedIndex = 0;
        }

        private void LoadCharacters(string path)
        {
            if (!Directory.Exists(path)) return;

            foreach (var file in Directory.GetFiles(path))
            {
                var contents = File.ReadAllText(file);
                _characters.Add(JsonConvert.DeserializeObject<CharacterInfo>(contents));
            }

            foreach (var character in _characters)
                CharacterList.Items.Add(new ListBoxItem().Content = character.Name);

            CharacterList.SelectedIndex = 0;
        }

        private void UpdateActionNames()
        {
            CurrentActionDropdown.Items.Clear();
            foreach (var action in _actions)
                CurrentActionDropdown.Items.Add(new ComboBoxItem().Content = action.Name);
        }

        private void UpdateCharacterNames()
        {
            for (var i = 0; i < _characters.Count; ++i)
                CharacterList.Items[i] = new ComboBoxItem().Content = _characters[i].Name;
        }

        private void UpdatePaths()
        {
            for (var i = 0; i < _actions.Count; ++i)
                _editorInfos[i].TexturePath = $"../../Textures/{CurrentCharacter().Name}/{_actions[i].Name}/";
        }

        private ActionInfo CurrentAction()
        {
            var index = CurrentActionDropdown.SelectedIndex;
            return index > -1 && index < _actions.Count ? _actions[index] : null;
        }

        private CharacterInfo CurrentCharacter()
        {
            var index = CharacterList.SelectedIndex;
            return index > -1 && index < _characters.Count ? _characters[index] : _lastSelectedCharacter;
        }

        private EditorInfo CurrentEditorInfo()
        {
            var index = CurrentActionDropdown.SelectedIndex;
            return index > -1 && index < _editorInfos.Count ? _editorInfos[index] : null;
        }

        private void NewActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (CharacterList.SelectedIndex < 0) return;

            var newAction = new ActionInfo { Name = $"new action {_actions.Count}" };
            _actions.Add(newAction);
            _actionAnims.Add(new List<BitmapImage>());

            var newEditorInfo = new EditorInfo { TexturePath = $"../../Textures/{CurrentCharacter().Name}/{newAction.Name}" };
            _editorInfos.Add(newEditorInfo);

            try
            {
                Directory.CreateDirectory(newEditorInfo.TexturePath);
            }
            catch (IOException exception)
            {
                Console.WriteLine(exception);
                throw;
            }
            
            CurrentActionDropdown.Items.Add(new ComboBoxItem().Content = newAction.Name);
            CurrentActionDropdown.SelectedIndex = CurrentActionDropdown.Items.Count - 1;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e) => SaveAction();

        private void SaveAction()
        {
            if (CurrentActionDropdown.SelectedIndex < 0) return;

            var filepath = $"../../Actions/{CurrentCharacter().Name}/{CurrentAction().Name}.json";

            var actionJson = JsonConvert.SerializeObject(CurrentAction(), Formatting.Indented);

            JsonUtils.WriteToJson(filepath, actionJson);
        }

        private void HitboxButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentAction().FrameCount <= 0) return;

            HitboxButton.Background = new SolidColorBrush { Color = Color.FromRgb(140, 30, 74) };
            HurtboxButton.Background = new SolidColorBrush { Color = Color.FromRgb(67, 249, 170) };

            _boxPlaceMode = _boxPlaceMode == 0 ? -1 : 0;
            HurtboxButton.IsChecked = false;
        }

        private void HurtboxButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentAction().FrameCount <= 0) return;

            HurtboxButton.Background = new SolidColorBrush { Color = Color.FromRgb(35, 132, 90) };
            HitboxButton.Background = new SolidColorBrush { Color = Color.FromRgb(247, 56, 133) };
            _boxPlaceMode = _boxPlaceMode == 1 ? -1 : 1;
            HitboxButton.IsChecked = false;
        }
        
        private void HitboxButton_OnMouseEnter(object sender, MouseEventArgs e)
        {
            if (!HitboxButton.IsChecked.HasValue) return;
            if (!HitboxButton.IsChecked.Value)
                HitboxButton.Background = new SolidColorBrush { Color = Color.FromRgb(193, 44, 104) };
        }

        private void HitboxButton_OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (!HitboxButton.IsChecked.HasValue) return;
            if (!HitboxButton.IsChecked.Value)
                HitboxButton.Background = new SolidColorBrush { Color = Color.FromRgb(247, 56, 133) };
        }

        private void HurtboxButton_OnMouseEnter(object sender, MouseEventArgs e)
        {
            if (!HurtboxButton.IsChecked.HasValue) return;
            if (!HurtboxButton.IsChecked.Value)
                HurtboxButton.Background = new SolidColorBrush { Color = Color.FromRgb(51, 191, 130) };
        }

        private void HurtboxButton_OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (!HurtboxButton.IsChecked.HasValue) return;
            if (!HurtboxButton.IsChecked.Value)
                HurtboxButton.Background = new SolidColorBrush { Color = Color.FromRgb(67, 249, 170) };
        }

        // action dropdown selection changed
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CurrentAction() == null) return;

            NameTextBox.Text = CurrentAction().Name;

            UpdateUiState();

            ActionTypeDropdown.SelectedIndex = (int) CurrentAction().Type;

            InfiniteRangeMinDropdown.Items.Clear();
            InfiniteRangeMinDropdown.Items.Add("None");
            InfiniteRangeMaxDropdown.Items.Clear();
            InfiniteRangeMaxDropdown.Items.Add("None");
            InfiniteRangeMinDropdown.SelectedIndex = (int) CurrentAction().InfiniteRangeMin + 1;
            InfiniteRangeMaxDropdown.SelectedIndex = (int) CurrentAction().InfiniteRangeMax + 1;

            Canvas.SetLeft(AnchorPoint, CurrentAction().Anchor.X);
            Canvas.SetTop(AnchorPoint, CurrentAction().Anchor.Y);
            AnchorXTextBox.Text = Canvas.GetLeft(AnchorPoint).ToString();
            AnchorYTextBox.Text = Canvas.GetTop(AnchorPoint).ToString();

            CurrentAction().Hitboxes = new List<List<Box>>();
            CurrentAction().Hurtboxes = new List<List<Box>>();
            for (var i = 0; i < CurrentAction().FrameCount; ++i)
            {
                InfiniteRangeMinDropdown.Items.Add(i);
                InfiniteRangeMaxDropdown.Items.Add(i);

                CurrentAction().Hitboxes.Add(new List<Box>());
                CurrentAction().Hurtboxes.Add(new List<Box>());
            }
        }

        // action name changed
        private void NameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (CurrentAction() == null || e.Key != Key.Enter) return;

            var oldName = CurrentAction().Name;
            var selectedIndex = CurrentActionDropdown.SelectedIndex;

            if (oldName.Equals(NameTextBox.Text)) return;
            if (Directory.Exists($"../../Textures/{CurrentCharacter().Name}/{NameTextBox.Text}"))
            {
                MessageBox.Show(this, "An action with this name already exists", "Can Not Rename");
                return;
            }

            CurrentAction().Name = NameTextBox.Text;

            CurrentActionDropdown.Items[selectedIndex] = new ComboBoxItem().Content = CurrentAction().Name;
            _editorInfos[selectedIndex].TexturePath = $"../../Textures/{CurrentCharacter().Name}/{_actions[selectedIndex].Name}/";

            CurrentActionDropdown.SelectedIndex = selectedIndex;
            try
            {
                Directory.Move($"../../Textures/{CurrentCharacter().Name}/{oldName}", _editorInfos[selectedIndex].TexturePath);
            }
            catch (IOException exception)
            {
                Console.WriteLine(exception);
                throw;
            }

        }

        // character selection changed
        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CharacterList.SelectedIndex < 0) return;

            try
            {
                File.WriteAllText("../../Editor/lastCharacter.json", CurrentCharacter().Name);
            }
            catch (IOException exception)
            {
                Console.WriteLine(exception);
                throw;
            }
            
            CharacterNameTextBox.Text = CurrentCharacter().Name;

            _lastSelectedCharacter = CurrentCharacter();

            if (CurrentActionDropdown.HasItems)
            {
                _actions.Clear();
                CurrentActionDropdown.Items.Clear();
            }

            if (CurrentActionDropdown.SelectedIndex >= 0)
                SaveAction();

            _actionAnims.Clear();

            if (!_loadedFromNew)
                LoadActions(CurrentCharacter().Name);
            else
                _loadedFromNew = true;

            for (var i = 0; i < _actions.Count; ++i)
            {
                _actionAnims.Add(new List<BitmapImage>());
                foreach (var file in Directory.GetFiles($"../../Textures/{CurrentCharacter().Name}/{CurrentAction().Name}/"))
                {
                    var tempImg = new BitmapImage();
                    tempImg.BeginInit();
                    tempImg.UriSource = new Uri($@"{AppDomain.CurrentDomain.BaseDirectory}/../{file}", UriKind.Absolute);
                    tempImg.CacheOption = BitmapCacheOption.OnLoad;
                    tempImg.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    tempImg.EndInit();
                    var index = int.Parse(Path.GetFileNameWithoutExtension(file));
                    if (index >= _actionAnims[i].Count)
                        for (var j = 0; j <= index; j++)
                            _actionAnims[i].Add(new BitmapImage());
                    _actionAnims[i][index] = tempImg;
                }
            }

            if (_actionAnims.Count <= 0) return;
            if (_actionAnims.Count <= CurrentActionDropdown.SelectedIndex) return;
            if (_actionAnims[CurrentActionDropdown.SelectedIndex].Count <= 0) return;

            CurrentFrameImage.Source = _actionAnims[CurrentActionDropdown.SelectedIndex][0];
        }

        private void NewCharacterButton_Click(object sender, RoutedEventArgs e)
        {
            _loadedFromNew = true;

            var newCharacter = new CharacterInfo { Name = $"new character {_characters.Count}" };
            _characters.Add(newCharacter);

            CharacterList.Items.Add(new ListBoxItem().Content = newCharacter.Name);

            CharacterList.SelectedIndex = CharacterList.Items.Count - 1;

            var filepath = $"../../Characters/{CurrentCharacter().Name}.json";

            // write character file
            var jsonString = JsonConvert.SerializeObject(CurrentCharacter(), Formatting.Indented);
            JsonUtils.WriteToJson(filepath, jsonString);

            try
            {
                Directory.CreateDirectory($"../../Actions/{CurrentCharacter().Name}");
                Directory.CreateDirectory($"../../Textures/{CurrentCharacter().Name}");
            }
            catch (IOException exception)
            {
                Console.WriteLine(exception);
                throw;
            }
           
            LoadActions(CurrentCharacter().Name);
        }

        private void CharacterNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (!CharacterList.HasItems || CharacterList.SelectedIndex < 0 || e.Key != Key.Enter) return;

            var oldName = CurrentCharacter().Name;
            CurrentCharacter().Name = CharacterNameTextBox.Text;
            UpdateCharacterNames();
            UpdatePaths();

            var filepath = $"../../Characters/{oldName}.json";

            // write charactr file
            var jsonString = JsonConvert.SerializeObject(CurrentCharacter(), Formatting.Indented);
            JsonUtils.WriteToJson(filepath, jsonString);

            try
            {
                Directory.Move($"../../Characters/{oldName}.json", $"../../Characters/{CurrentCharacter().Name}.json");
                Directory.Move($"../../Textures/{oldName}", $"../../Textures/{CurrentCharacter().Name}");
                Directory.Move($"../../Actions/{oldName}", $"../../Actions/{CurrentCharacter().Name}");
            }
            catch (IOException exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }

        private void FrameSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var currentAction = CurrentAction();

            if (CurrentActionDropdown.SelectedIndex < 0 || FrameSlider.Value > currentAction.FrameCount ||
                CurrentActionDropdown.SelectedIndex >= _actionAnims.Count) return;

            FrameTypeDropdown.SelectedIndex = (int) currentAction.FrameTypeAt((int) FrameSlider.Value);

            if (_previousFrame < currentAction.Hitboxes.Count)
                currentAction.Hitboxes[_previousFrame].Clear();
            if (_previousFrame < currentAction.Hurtboxes.Count)
                currentAction.Hurtboxes[_previousFrame].Clear();    
            foreach (var boxInfo in _hitboxes)
                currentAction.Hitboxes[_previousFrame].Add(boxInfo.Box);
            foreach (var boxInfo in _hurtboxes)
                currentAction.Hurtboxes[_previousFrame].Add(boxInfo.Box);

            _currentBoxCount = 0;
            _hitboxes.Clear();
            _hurtboxes.Clear();
            BoxCanvas.Children.Clear();
           
            foreach (var box in currentAction.Hitboxes[(int) FrameSlider.Value])
            {
                var r = new Rectangle
                {
                    Stroke = Hitbox.Stroke,
                    Opacity = Hitbox.Opacity,
                    Fill = _hitBrush,
                    Name = "I" + _hitboxes.Count,
                    Visibility = Visibility.Visible,

                    Width = box.Width,
                    Height = box.Height,
                };
                r.MouseEnter += Box_MouseOver;
                r.MouseLeave += Box_MouseLeave;
                r.MouseLeftButtonDown += Box_MouseLeftButtonDown;

                Canvas.SetLeft(r, box.X);
                Canvas.SetTop(r, box.Y);

                BoxCanvas.Children.Add(r);

                _hitboxes.Add(new BoxInfo(box, r));
            }
            foreach (var box in currentAction.Hurtboxes[(int) FrameSlider.Value])
            {
                var r = new Rectangle
                {
                    Stroke = Hurtbox.Stroke,
                    Opacity = Hurtbox.Opacity,
                    Fill = _hurtBrush,
                    Name = "R" + _hurtboxes.Count,
                    Visibility = Visibility.Visible,
                    Width = box.Width,
                    Height = box.Height,
                };
                r.MouseEnter += Box_MouseOver;
                r.MouseLeave += Box_MouseLeave;
                r.MouseLeftButtonDown += Box_MouseLeftButtonDown;

                Canvas.SetLeft(r, box.X);
                Canvas.SetTop(r, box.Y);

                BoxCanvas.Children.Add(r);

                _hurtboxes.Add(new BoxInfo(box, r));
            }

            CurrentFrameImage.Source = 
                FrameSlider.Value < _actionAnims[CurrentActionDropdown.SelectedIndex].Count
                    ? _actionAnims[CurrentActionDropdown.SelectedIndex][(int) FrameSlider.Value] 
                    : null;

            _previousFrame = (int)FrameSlider.Value;

            UpdateBoxUiState();
        }

        private void PrevFrameButton_Click(object sender, RoutedEventArgs e) => --FrameSlider.Value;

        private void NextFrameButton_Click(object sender, RoutedEventArgs e) => ++FrameSlider.Value;

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RemoveFrameButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentAction() == null) return;

            CurrentAction().RemoveFrame((int) FrameSlider.Value);
            CurrentAction().RemoveBoxList((int) FrameSlider.Value, 0);
            CurrentAction().RemoveBoxList((int) FrameSlider.Value, 1);

            CurrentFrameImage.Source = null;
            _currentBoxCount = 0;
            _hitboxes.Clear();
            _hurtboxes.Clear();
            BoxCanvas.Children.Clear();

            UpdateUiState();
        }

        private void InsertFrameButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentAction() == null) return;

            CurrentAction().InsertFrame((int) FrameSlider.Value);
            CurrentAction().InsertBoxList((int) FrameSlider.Value, 0);
            CurrentAction().InsertBoxList((int) FrameSlider.Value, 1);

            UpdateUiState();
        }

        private void UpdateUiState()
        {
            FrameSlider.Maximum = CurrentAction().FrameCount > 1 ? CurrentAction().FrameCount - 1 : 0;
            if (CurrentAction().FrameCount == 0)
            {
                //FrameSlider.Value = 0;
                FrameSlider.IsEnabled = false;
                ImportSpriteButton.IsEnabled = false;
                HitboxButton.IsEnabled = false;
                HurtboxButton.IsEnabled = false;
                FrameTypeDropdown.IsEnabled = false;
            }
            else
            {
                FrameSlider.IsEnabled = true;
                ImportSpriteButton.IsEnabled = true;
                HitboxButton.IsEnabled = true;
                HurtboxButton.IsEnabled = true;
                FrameTypeDropdown.IsEnabled = true;
            }
        }

        private void UpdateBoxUiState()
        {
            if (_selectedBox < 0 || CurrentBoxList().Count == 0)
            {
                BoxLifespanText.IsEnabled = false;
                BoxXText.IsEnabled = false;
                BoxYText.IsEnabled = false;
                BoxWidthText.IsEnabled = false;
                BoxHeightText.IsEnabled = false;
                BoxDmgText.IsEnabled = false;
                BoxKbStrengthText.IsEnabled = false;
                BoxKbAngleSlider.IsEnabled = false;
                return;
            }

            BoxLifespanText.IsEnabled = true;
            BoxXText.IsEnabled = true;
            BoxYText.IsEnabled = true;
            BoxWidthText.IsEnabled = true;
            BoxHeightText.IsEnabled = true;
            BoxDmgText.IsEnabled = true;
            BoxKbStrengthText.IsEnabled = true;
            BoxKbAngleSlider.IsEnabled = true;
        }

        private void EditGridZoomBorderMouseDown(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void EditGridZoomBorderMouseWheel(object sender, MouseWheelEventArgs e)
        {
            Console.WriteLine($@"{EditGridZoomBorder.ZoomX}");
            EditCanvas.Height = 1080; 
            EditCanvas.Width = 1920; 
        }

        private void ZoomBorder_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space:
                    EditGridZoomBorder.PanButton = PanAndZoom.ButtonName.Left;
                    break;
                default:
                    EditGridZoomBorder.PanButton = PanAndZoom.ButtonName.Middle;
                    break;
            }
        }

        private void Box_MouseOver(object sender, MouseEventArgs e)
        {
            if (!(sender is Rectangle rect)) return;

            if (rect.Fill.IsEqualTo(_hitBrush))
                rect.Fill = _hitOverBrush;
            else if (rect.Fill.IsEqualTo(_hurtBrush))
                rect.Fill = _hurtOverBrush;
        }

        private bool IsSelectedBox(Rectangle rect)
        {
            if (_selectedBox < 0 || CurrentBoxList().Count == 0)
                return false;
            return SelectedBox().Rect.Equals(rect);
        }

        private int IndexFromRect(Rectangle rect)
        {
            for (var i = 0; i < _hitboxes.Count; ++i)
                if (_hitboxes[i].Rect.Equals(rect))
                    return i;

            for (var i = 0; i < _hurtboxes.Count; ++i)
                if (_hurtboxes[i].Rect.Equals(rect))
                    return _hitboxes.Count + i;

            return -1;
        }

        private void Box_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!(sender is Rectangle rect)) return;

            if (IsSelectedBox(rect)) return;

            if (rect.Fill.IsEqualTo(_hitOverBrush))
                rect.Fill = _hitBrush;
            else if (rect.Fill.IsEqualTo(_hurtOverBrush))
                rect.Fill = _hurtBrush;
        }

        private BoxInfo SelectedBox()
        {
            return _selectedBox >= _hitboxes.Count ? _hurtboxes[_selectedBox - _hitboxes.Count] : _hitboxes[_selectedBox];
        }

        private void Box_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is Rectangle rect)) return;

            if (IsSelectedBox(rect)) return;

            // if selected box is a selected hitbox
            if (SelectedBox().Rect.Fill.IsEqualTo(_hitOverBrush))
                SelectedBox().Rect.Fill = _hitBrush;
            else if (SelectedBox().Rect.Fill.IsEqualTo(_hurtOverBrush))
                SelectedBox().Rect.Fill = _hurtBrush;

            var index = IndexFromRect(rect);

            if (index == -1) return;

            _selectedBox = index;
            BoxXText.Text = SelectedBox().Box.X.ToString();
            BoxYText.Text = SelectedBox().Box.Y.ToString();
            BoxWidthText.Text = SelectedBox().Box.Width.ToString();
            BoxHeightText.Text = SelectedBox().Box.Height.ToString();
            BoxDmgText.Text = SelectedBox().Box.Damage.ToString();
            BoxKbStrengthText.Text = SelectedBox().Box.KnockbackStrength.ToString();
            BoxLifespanText.Text = SelectedBox().Box.Lifespan.ToString();
            BoxIdTextBlock.Text = "ID: " + SelectedBox().Rect.Name;

            UpdateBoxUiState();
        }

        private void BoxXText_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    if (_selectedBox != -1)
                    {
                        if (int.TryParse(BoxXText.Text, out var x))
                        {
                            Canvas.SetLeft(SelectedBox().Rect, x);
                            SelectedBox().Box.SetPos(x, SelectedBox().Box.Y);
                        }
                    }
   
                    break;
            }
        }

        private void BoxYText_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    if (_selectedBox != -1)
                    {
                        if (int.TryParse(BoxYText.Text, out var y))
                        {
                            Canvas.SetTop(SelectedBox().Rect, y);
                            SelectedBox().Box.SetPos(SelectedBox().Box.X, y);
                        }
                    }

                    break;
            }
        }

        private void BoxWidthText_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    if (_selectedBox != -1)
                    {
                        if (int.TryParse(BoxWidthText.Text, out var width))
                        {
                            SelectedBox().Rect.Width = width;
                            SelectedBox().Box.Width = width;
                        }
                    }

                    break;
            }
        }

        private void BoxHeightText_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    if (_selectedBox != -1)
                    {
                        if (int.TryParse(BoxHeightText.Text, out var height))
                        {
                            SelectedBox().Rect.Height = height;
                            SelectedBox().Box.Height = height;
                        }
                    }

                    break;
            }
        }

        private void BoxDMGText_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    if (_selectedBox != -1)
                    {
                        if (int.TryParse(BoxDmgText.Text, out var damage))
                        {
                            SelectedBox().Box.Damage = damage;
                        }
                    }

                    break;
            }
        }

        private void BoxKBStrengthText_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    if (_selectedBox != -1)
                    {
                        if (int.TryParse(BoxKbStrengthText.Text, out var strength))
                        {
                            SelectedBox().Box.KnockbackStrength = strength;
                        }
                    }

                    break;
            }
        }

        private void BoxKBAngleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) 
            => BoxAngleText.Text = BoxKbAngleSlider.Value + "°";

        private void InfiniteRangeMinDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
            // selected index minus one to account for "none" option (-1)
            => CurrentAction().InfiniteRangeMin = InfiniteRangeMinDropdown.SelectedIndex - 1;

        private void InfiniteRangeMaxDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => CurrentAction().InfiniteRangeMax = InfiniteRangeMaxDropdown.SelectedIndex - 1;

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _mouseDownPos = new IntCouple((int) e.GetPosition(BoxCanvas).X, (int) e.GetPosition(BoxCanvas).Y);
            _anchoredMouseDownPos = new IntCouple((int)e.GetPosition(AnchorPoint).X, (int)e.GetPosition(AnchorPoint).Y);

            if (_boxPlaceMode <= -1) return;
            
            // Capture and track the mouse.
            BoxCanvas.CaptureMouse();

            var box = new Rectangle();

            switch (_boxPlaceMode)
            {
                case 0:
                    box.Stroke = Hitbox.Stroke;
                    box.Opacity = Hitbox.Opacity;
                    box.Fill = Hitbox.Fill;

                    box.Fill = _hitBrush;

                    box.Name = "hit" + _hitboxes.Count;

                    box.MouseEnter += Box_MouseOver;
                    box.MouseLeave += Box_MouseLeave;
                    box.MouseLeftButtonDown += Box_MouseLeftButtonDown;                
                    break;

                case 1:
                    box.Stroke = Hurtbox.Stroke;
                    box.Opacity = Hurtbox.Opacity;
                    box.Fill = Hurtbox.Fill;

                    box.Fill = _hurtBrush;

                    box.Name = "hurt" + _hurtboxes.Count;

                    box.MouseEnter += Box_MouseOver;
                    box.MouseLeave += Box_MouseLeave;
                    box.MouseLeftButtonDown += Box_MouseLeftButtonDown;
                    break;
            }

            CurrentBoxList().Add(new BoxInfo(new Box(), box));
            BoxCanvas.Children.Add(CurrentBoxList().Last().Rect);

            //currentBoxPos.Y = Math.Round(mouseDownPos.Y / gridSize) * gridSize;
            //currentBoxPos.X = Math.Round(mouseDownPos.X / gridSize) * gridSize;
            _currentBoxPos.X =  _mouseDownPos.X;
            _currentBoxPos.Y =  _mouseDownPos.Y;

            // set box x to ANCHORED position, not render position
            CurrentBoxList().Last().Box.SetPos(_anchoredMouseDownPos.X, _anchoredMouseDownPos.Y);

            BoxXText.Text = CurrentBoxList().Last().Box.X.ToString();
            BoxYText.Text = CurrentBoxList().Last().Box.Y.ToString();
            BoxDmgText.Text = CurrentBoxList().Last().Box.Damage.ToString();
            BoxKbStrengthText.Text = CurrentBoxList().Last().Box.KnockbackStrength.ToString();

            // Initial placement of the drag selection box.         
            Canvas.SetLeft(CurrentBoxList().Last().Rect, _mouseDownPos.X);
            Canvas.SetTop(CurrentBoxList().Last().Rect, _mouseDownPos.Y);
            //Canvas.SetLeft(CurrentBoxList().Last().Rect, Math.Round((_currentBoxPos.X) / GridSize) * GridSize);
            //Canvas.SetTop(CurrentBoxList().Last().Rect, Math.Round((_currentBoxPos.Y) / GridSize) * GridSize);
            // Make the drag selection box visible.
            CurrentBoxList().Last().Rect.Visibility = Visibility.Visible;
            _mouseDown = true;
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _mouseDown = false;
            var mouseUpPos = e.GetPosition(BoxCanvas);

            switch (_boxPlaceMode)
            {
                case -1:
                    return;
                case 0:
                    _currentBoxCount = _hitboxes.Count;
                    break;
                case 1:
                    _currentBoxCount = _hurtboxes.Count;
                    break;
            }

            // Release the mouse capture and stop tracking it.
            EditCanvas.ReleaseMouseCapture();

            // Hide the drag selection box.
            // selectionBox.Visibility = Visibility.Collapsed;

            Mouse.Capture(null);

            // TODO: 
            //
            // The mouse has been released, check to see if any of the items 
            // in the other canvas are contained within mouseDownPos and 
            // mouseUpPos, for any that are, select them!
            //
            UpdateBoxUiState();
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            var mousePos = new IntCouple((int) e.GetPosition(BoxCanvas).X, (int) e.GetPosition(BoxCanvas).Y);
            var anchoredMousePos = new IntCouple((int)e.GetPosition(AnchorPoint).X, (int)e.GetPosition(AnchorPoint).Y);

            if (_boxPlaceMode <= -1) return;
            if (!_mouseDown) return;
            if (_boxPlaceMode == 0)
                if (_hitboxes.Count == _currentBoxCount) return;
                else if (_boxPlaceMode == 1)
                    if (_hurtboxes.Count == _currentBoxCount) return;


            Canvas.SetLeft(CurrentBoxList().Last().Rect, _mouseDownPos.X);
            Canvas.SetTop(CurrentBoxList().Last().Rect, _mouseDownPos.Y);
            // set position to ANCHORED position not rendered position
            CurrentBoxList().Last().Box.SetPos(_anchoredMouseDownPos.X, _anchoredMouseDownPos.Y);
            BoxXText.Text = CurrentBoxList().Last().Box.X.ToString();
            BoxYText.Text = CurrentBoxList().Last().Box.Y.ToString();
            CurrentBoxList().Last().Rect.Width = Math.Abs(mousePos.X - _mouseDownPos.X);
            CurrentBoxList().Last().Rect.Height = Math.Abs(mousePos.Y - _mouseDownPos.Y);

            // When the mouse is held down, reposition the drag selection box
            if (mousePos.X < _mouseDownPos.X)
            {
                Canvas.SetLeft(CurrentBoxList().Last().Rect, _mouseDownPos.X - CurrentBoxList().Last().Rect.Width);

                CurrentBoxList().Last().Box.X = _anchoredMouseDownPos.X - (int) CurrentBoxList().Last().Rect.Width;

                CurrentBoxList().Last().Rect.Width = Math.Abs(mousePos.X - _mouseDownPos.X);

                BoxXText.Text = CurrentBoxList().Last().Box.X.ToString();
            }
           
            if (mousePos.Y < _mouseDownPos.Y)
            {
                Canvas.SetTop(CurrentBoxList().Last().Rect, _mouseDownPos.Y - CurrentBoxList().Last().Rect.Height);

                CurrentBoxList().Last().Box.Y = _anchoredMouseDownPos.Y - (int)CurrentBoxList().Last().Rect.Height;
                
                CurrentBoxList().Last().Rect.Height = Math.Abs(mousePos.Y - _mouseDownPos.Y);

                BoxYText.Text = CurrentBoxList().Last().Box.Y.ToString();
            }
          
            CurrentBoxList().Last().Box.SetDims((int) CurrentBoxList().Last().Rect.Width, (int) CurrentBoxList().Last().Rect.Height);
            BoxWidthText.Text = CurrentBoxList().Last().Box.Width.ToString();
            BoxHeightText.Text = CurrentBoxList().Last().Box.Height.ToString();
        }

        private ref List<BoxInfo> CurrentBoxList()
        {
            switch (_boxPlaceMode)
            {
                case 0:
                    return ref _hitboxes;
                case 1:
                    return ref _hurtboxes;
                default:
                    return ref _hitboxes;
            }
        }

        private void ImportSpriteButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentAction().FrameCount <= 0) return;

            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpeg;*.jpg;*.bmp)|*.png;*.jpeg;*.jpg;*.bmp|All files (*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)

            };
            if (openFileDialog.ShowDialog() != true) return;

            //if (File.Exists($"{CurrentEditorInfo().TexturePath}/{FrameSlider.Value}.png"))
            //File.Delete($"{CurrentEditorInfo().TexturePath}/{FrameSlider.Value}.png");
            try
            {
                File.Copy(openFileDialog.FileName, $"{CurrentEditorInfo().TexturePath}/{FrameSlider.Value}.png", true);
            }
            catch (IOException exception)
            {
                Console.WriteLine(exception);
                throw;
            }
           
            var tempImg = new BitmapImage();
            tempImg.BeginInit();
            tempImg.UriSource = new Uri($@"{AppDomain.CurrentDomain.BaseDirectory}/../{CurrentEditorInfo().TexturePath}/{FrameSlider.Value}.png", UriKind.Absolute);
            tempImg.CacheOption = BitmapCacheOption.OnLoad;
            tempImg.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            tempImg.EndInit();

            if (_actionAnims[CurrentActionDropdown.SelectedIndex].Count <= (int) FrameSlider.Value)
                for (var i = 0; i <= (int) FrameSlider.Value; i++)
                    _actionAnims[CurrentActionDropdown.SelectedIndex].Add(new BitmapImage());
            _actionAnims[CurrentActionDropdown.SelectedIndex][(int) FrameSlider.Value] = tempImg;

            if (FrameSlider.Value < _actionAnims[CurrentActionDropdown.SelectedIndex].Count)
                CurrentFrameImage.Source = _actionAnims[CurrentActionDropdown.SelectedIndex][(int)FrameSlider.Value];

        }

        private void AnchorPoint_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void ActionTypeDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => CurrentAction().Type = (Types.ActionType) ActionTypeDropdown.SelectedIndex;

        private void FrameTypeDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => CurrentAction().SetFrameType((int) FrameSlider.Value, (FrameType) FrameTypeDropdown.SelectedIndex);

        private void AnchorXTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    if (_selectedBox != -1)
                    {
                        if (int.TryParse(AnchorXTextBox.Text, out var x))
                        {
                            var oldX = Canvas.GetLeft(AnchorPoint);
                            var difference = x - oldX;
                            foreach (var box in _hitboxes)
                            {
                                box.Box.X += (int) difference;
                            }
                            foreach (var box in _hurtboxes)
                            {
                                box.Box.X += (int)difference;
                            }

                            Canvas.SetLeft(AnchorPoint, x);

                            CurrentAction().Anchor.X = x;

                            if (_selectedBox > 0)
                                BoxXText.Text = SelectedBox().Box.X.ToString();
                        }
                    }

                    break;
            }
        }

        private void AnchorYTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    if (int.TryParse(AnchorYTextBox.Text, out var y))
                    {
                        var oldY = Canvas.GetTop(AnchorPoint);
                        var difference = y - oldY;
                        foreach (var box in _hitboxes)
                        {
                            box.Box.Y += (int) difference;
                        }
                        foreach (var box in _hurtboxes)
                        {
                            box.Box.Y += (int) difference;
                        }

                        Canvas.SetTop(AnchorPoint, y);
                        CurrentAction().Anchor.Y = y;

                        if (_selectedBox > 0)
                            BoxYText.Text = SelectedBox().Box.Y.ToString();
                    }
            
                    break;
            }
        }

        private void ClearSpriteButton_OnClick(object sender, RoutedEventArgs e)
        {
            CurrentFrameImage.Source = null;
            _actionAnims[CurrentActionDropdown.SelectedIndex][(int) FrameSlider.Value] = new BitmapImage();
            try
            {
                File.Delete($"../../Textures/{CurrentCharacter().Name}/{CurrentAction().Name}/{FrameSlider.Value}.png");
            }
            catch (IOException exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }
    }
}
