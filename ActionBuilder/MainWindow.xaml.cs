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

namespace ActionBuilder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private readonly List<CharacterInfo> _characters;
        private readonly List<ActionInfo> _actions;
        private readonly List<EditorInfo> _editorInfos;
        private readonly List<MainWindow.BoxInfo> _boxes;
        private readonly List<List<BitmapImage>> _actionAnims;

        private readonly SolidColorBrush _hurtBrush;
        private readonly SolidColorBrush _hurtOverBrush;
        private readonly SolidColorBrush _hitBrush;
        private readonly SolidColorBrush _hitOverBrush;

        private CharacterInfo _lastSelectedCharacter;
        private Point _mouseDownPos;
        private Point _currentBoxPos = new Point(0, 0);

        private bool _loadedFromNew;
        private bool _mouseDown;

        private int _selectedBox;
        private int _boxPlaceMode = -1;
        private int _currentBoxCount;
        private int _previousFrame = 0;

        private const int GridSize = 4;

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

            _characters = new List<CharacterInfo>();
            _actions = new List<ActionInfo>();
            _editorInfos = new List<EditorInfo>();
            _actionAnims = new List<List<BitmapImage>>();
            _boxes = new List<MainWindow.BoxInfo>();

            _hurtBrush = new SolidColorBrush { Color = Color.FromRgb(112, 255, 150) };
            _hurtOverBrush = new SolidColorBrush { Color = Color.FromRgb(52, 249, 114) };
            _hitBrush = new SolidColorBrush { Color = Color.FromRgb(255, 66, 116) };
            _hitOverBrush = new SolidColorBrush { Color = Color.FromRgb(226, 20, 75) };

            InitializeComponent();

            LoadCharacters("../../Characters/");
            if (File.Exists("../../Editor/lastCharacter.json"))
                LoadActions(JsonUtils.ReadFromJson("../../Editor/lastCharacter.json"));

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
        }

        private void LoadActions(string character)
        {   
            var ser = new DataContractJsonSerializer(typeof(ActionInfo));

            if (!Directory.Exists($"../../Actions/{character}/"))
                Directory.CreateDirectory($"../../Actions/{character}/");

            foreach (var file in Directory.GetFiles($"../../Actions/{character}/"))
                using (var sr = new StreamReader(file))
                    _actions.Add((ActionInfo) ser.ReadObject(sr.BaseStream));

            foreach (var action in _actions)
                CurrentActionDropdown.Items.Add(new ComboBoxItem().Content = action.Name);

            CurrentActionDropdown.SelectedIndex = 0;
        }

        private void LoadCharacters(string path)
        {
            var ser = new DataContractJsonSerializer(typeof(CharacterInfo));

            if (!Directory.Exists(path)) return;

            foreach (var file in Directory.GetFiles(path))
                using (var sr = new StreamReader(file))
                    _characters.Add((CharacterInfo)ser.ReadObject(sr.BaseStream));

            foreach (var character in _characters)
                CharacterList.Items.Add(new ListBoxItem().Content = character.Name);

            CharacterList.SelectedIndex = 0;
        }

        private void UpdateActionNames()
        {
            for (var i = 0; i < _actions.Count; ++i)
                CurrentActionDropdown.Items[i] = new ComboBoxItem().Content = _actions[i].Name;
        }

        private void UpdateCharacterNames()
        {
            for (var i = 0; i < _characters.Count; ++i)
                CharacterList.Items[i] = new ComboBoxItem().Content = _characters[i].Name;
        }

        private void UpdatePaths()
        {
            for (var i = 0; i < _editorInfos.Count; ++i)
                _editorInfos[i].TexturePath = $"Textures/{CurrentCharacter().Name}/{_actions[i].Name}/";
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

        private void NewActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (CharacterList.SelectedIndex < 0) return;

            var newAction = new ActionInfo { Name = $"new action {_actions.Count}" };
            _actions.Add(newAction);

            var newEditorInfo = new EditorInfo { TexturePath = $"Textures/{CurrentCharacter().Name}/{newAction.Name}/" };

            CurrentActionDropdown.Items.Add(new ComboBoxItem().Content = newAction.Name);
            CurrentActionDropdown.SelectedIndex = CurrentActionDropdown.Items.Count - 1;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e) => SaveAction();

        private void SaveAction()
        {
            if (CurrentActionDropdown.SelectedIndex < 0) return;

            var filepath = $"../../Actions/{CurrentCharacter().Name}/{CurrentAction().Name}.json";

            using (var outStream = new MemoryStream())
            {
                var ser = new DataContractJsonSerializer(typeof(ActionInfo));

                ser.WriteObject(outStream, CurrentAction());

                JsonUtils.WriteToJson(filepath, outStream);
            }
        }

        private void HitboxButton_Click(object sender, RoutedEventArgs e) => _boxPlaceMode = 0;

        private void HurtboxButton_Click(object sender, RoutedEventArgs e) => _boxPlaceMode = 1;

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CurrentAction() == null) return;

            NameTextBox.Text = CurrentAction().Name;
            FrameSlider.Maximum = CurrentAction().FrameCount;
            InfiniteRangeMinDropdown.Items.Clear();
            InfiniteRangeMinDropdown.Items.Add("None");
            InfiniteRangeMaxDropdown.Items.Clear();
            InfiniteRangeMaxDropdown.Items.Add("None");

            CurrentAction().Hitboxes = new List<List<Box>>();
            CurrentAction().Hurtboxes = new List<List<Box>>();
            for (var i = 0; i <= CurrentAction().FrameCount; ++i)
            {
                InfiniteRangeMinDropdown.Items.Add(i);
                InfiniteRangeMaxDropdown.Items.Add(i);

                CurrentAction().Hitboxes.Add(new List<Box>());
                CurrentAction().Hurtboxes.Add(new List<Box>());
            }
        }

        private void NameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (CurrentAction() == null || e.Key != Key.Enter) return;

            CurrentAction().Name = NameTextBox.Text;
            UpdateActionNames();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CharacterList.SelectedIndex < 0) return;

            using (var outStream = new MemoryStream())
            {
                var ser = new DataContractJsonSerializer(typeof(string));

                ser.WriteObject(outStream, CurrentCharacter().Name);
            }

            File.WriteAllText("../../Editor/lastCharacter.json", CurrentCharacter().Name);

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

            if (_loadedFromNew == false)
                LoadActions(CurrentCharacter().Name);
            else
                _loadedFromNew = true;

            for (var i = 0; i < _actions.Count; ++i)
            {
                _actionAnims.Add(new List<BitmapImage>());
                foreach (var file in Directory.GetFiles($"../../Textures/{CurrentCharacter().Name}/"))
                {
                    var tempImg = new BitmapImage();
                    tempImg.BeginInit();
                    tempImg.UriSource = new Uri($@"{AppDomain.CurrentDomain.BaseDirectory}/../{file}", UriKind.Absolute);
                    tempImg.EndInit();
                    _actionAnims[i].Add(tempImg);
                }
            }
        }

        private void NewCharacterButton_Click(object sender, RoutedEventArgs e)
        {
            _loadedFromNew = true;

            var newCharacter = new CharacterInfo { Name = $"new character {_characters.Count}" };
            _characters.Add(newCharacter);

            CharacterList.Items.Add(new ListBoxItem().Content = newCharacter.Name);

            CharacterList.SelectedIndex = CharacterList.Items.Count - 1;

            var filepath = $"../../Characters/{CurrentCharacter().Name}.json";
            var ser = new DataContractJsonSerializer(typeof(CharacterInfo));

            // write character file
            using (var outStream = new MemoryStream())
            {
                ser.WriteObject(outStream, CurrentCharacter()); 
                JsonUtils.WriteToJson(filepath, outStream);
            }

            Directory.CreateDirectory($"../../Actions/{CurrentCharacter().Name}");
            Directory.CreateDirectory($"../../Textures/{CurrentCharacter().Name}");

            LoadActions(CurrentCharacter().Name);
        }

        private void CharacterNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (!CharacterList.HasItems || CharacterList.SelectedIndex < 0 || e.Key != Key.Enter) return;

            var oldName = CurrentCharacter().Name;
            CurrentCharacter().Name = CharacterNameTextBox.Text;
            UpdateCharacterNames();

            var ser = new DataContractJsonSerializer(typeof(CharacterInfo));
            var filepath = $"../../Characters/{oldName}.json";

            // write charactr file
            using (var outStream = new MemoryStream())
            {
                ser.WriteObject(outStream, CurrentCharacter());
                JsonUtils.WriteToJson(filepath, outStream);
            }

            Directory.Move($"../../Characters/{oldName}.json", $"../../Characters/{CurrentCharacter().Name}.json");
            Directory.Move($"../../Textures/{oldName}", $"../../Textures/{CurrentCharacter().Name}");
            Directory.Move($"../../Actions/{oldName}", $"../../Actions/{CurrentCharacter().Name}");
        }

        private void FrameSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CurrentActionDropdown.SelectedIndex < 0 || !(FrameSlider.Value < CurrentAction().FrameCount) ||
                CurrentActionDropdown.SelectedIndex >= _actionAnims.Count) return;

            var tempHit = new List<Box>();
            var tempHurt = new List<Box>();

            foreach (var boxInfo in _boxes)
                if (boxInfo.Rect.Name.StartsWith("hit"))
                    tempHit.Add(boxInfo.Box);
                else if (boxInfo.Rect.Name.StartsWith("hurt"))
                    tempHurt.Add(boxInfo.Box);

            CurrentAction().Hitboxes[_previousFrame] = tempHit;
            CurrentAction().Hurtboxes[_previousFrame] = tempHurt;

            _boxes.Clear();
            BoxCanvas.Children.Clear();

            foreach (var box in CurrentAction().Hitboxes[(int) FrameSlider.Value])
            {
                var r = new Rectangle
                {
                    Stroke = Hitbox.Stroke,
                    Opacity = Hitbox.Opacity,
                    Fill = _hitBrush,
                    Name = "hit" + _boxes.Count.ToString(),
                    Width = box.Width,
                    Height = box.Height,
                    Visibility = Visibility.Visible
                };
                r.MouseEnter += Box_MouseOver;
                r.MouseLeave += Box_MouseLeave;
                r.MouseLeftButtonDown += Box_MouseLeftButtonDown;

                BoxCanvas.Children.Add(r);
                Canvas.SetLeft(r, box.X);
                Canvas.SetTop(r, box.Y);

                _boxes.Add(new BoxInfo(box, r));    
            }
            foreach (var box in CurrentAction().Hurtboxes[(int) FrameSlider.Value])
            {
                var r = new Rectangle
                {
                    Stroke = Hurtbox.Stroke,
                    Opacity = Hurtbox.Opacity,
                    Fill = _hurtBrush,
                    Name = "hurt" + _boxes.Count.ToString(),
                    Width = box.Width,
                    Height = box.Height,
                    Visibility = Visibility.Visible
                };
                r.MouseEnter += Box_MouseOver;
                r.MouseLeave += Box_MouseLeave;
                r.MouseLeftButtonDown += Box_MouseLeftButtonDown;

                BoxCanvas.Children.Add(r);
                Canvas.SetLeft(r, box.X);
                Canvas.SetTop(r, box.Y);

                _boxes.Add(new BoxInfo(box, r));
            }

            if (FrameSlider.Value < _actionAnims[CurrentActionDropdown.SelectedIndex].Count)
                CurrentFrameImage.Source = _actionAnims[CurrentActionDropdown.SelectedIndex][(int) FrameSlider.Value];

            _previousFrame = (int)FrameSlider.Value;
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

            CurrentAction().RemoveFrame((int)FrameSlider.Value);
            CurrentAction().Hitboxes.RemoveAt((int)FrameSlider.Value);
            CurrentAction().Hurtboxes.RemoveAt((int)FrameSlider.Value);

            FrameSlider.Maximum = CurrentAction().FrameCount;
        }

        private void InsertFrameButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentAction() == null) return;

            CurrentAction().InsertFrame((int)FrameSlider.Value);
            CurrentAction().Hitboxes.Insert((int)FrameSlider.Value, new List<Box>());
            CurrentAction().Hurtboxes.Insert((int)FrameSlider.Value, new List<Box>());

            FrameSlider.Maximum = CurrentAction().FrameCount;
        }

        private void EditGridZoomBorderMouseDown(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void EditGridZoomBorderMouseWheel(object sender, MouseWheelEventArgs e)
        {
            Console.WriteLine($"{EditGridZoomBorder.ZoomX}");
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
            if (_selectedBox < 0 || _boxes.Count == 0)
                return false;
            return _boxes[_selectedBox].Rect.Equals(rect);
        }

        private int IndexFromRect(Rectangle rect)
        {
            for (var i = 0; i < _boxes.Count; ++i)
                if (_boxes[i].Rect.Equals(rect))
                    return i;

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

        private void Box_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is Rectangle rect)) return;

            if (IsSelectedBox(rect)) return;
           
            // if selected box is a selected hitbox
            if (_boxes[_selectedBox].Rect.Fill.IsEqualTo(_hitOverBrush))
                _boxes[_selectedBox].Rect.Fill = _hitBrush;
            else if (_boxes[_selectedBox].Rect.Fill.IsEqualTo(_hurtOverBrush))
                _boxes[_selectedBox].Rect.Fill = _hurtBrush;

            var index = IndexFromRect(rect);

            if (index == -1) return;

            _selectedBox = index;
            BoxXText.Text = _boxes[_selectedBox].Box.X.ToString();
            BoxYText.Text = _boxes[_selectedBox].Box.Y.ToString();
            BoxWidthText.Text = _boxes[_selectedBox].Box.Width.ToString();
            BoxHeightText.Text = _boxes[_selectedBox].Box.Height.ToString();
            BoxDmgText.Text = _boxes[_selectedBox].Box.Damage.ToString();
            BoxKbStrengthText.Text = _boxes[_selectedBox].Box.KnockbackStrength.ToString();
            BoxLifespanText.Text = _boxes[_selectedBox].Box.Lifespan.ToString();
            BoxIdTextBlock.Text = "ID: " + _boxes[_selectedBox].Rect.Name;
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
                            Canvas.SetLeft(_boxes[_selectedBox].Rect, x);
                            _boxes[_selectedBox].Box.SetPos(x, _boxes[_selectedBox].Box.Y);
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
                            Canvas.SetTop(_boxes[_selectedBox].Rect, y);
                            _boxes[_selectedBox].Box.SetPos(_boxes[_selectedBox].Box.X, y);
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
                            _boxes[_selectedBox].Rect.Width = width;
                            _boxes[_selectedBox].Box.Width = width;
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
                            _boxes[_selectedBox].Rect.Height = height;
                            _boxes[_selectedBox].Box.Height = height;
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
                            _boxes[_selectedBox].Box.Damage = damage;
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
                            _boxes[_selectedBox].Box.KnockbackStrength = strength;
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
            => CurrentAction().InfiniteRangeMax = InfiniteRangeMaxDropdown.SelectedIndex;

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _mouseDown = true;
            _mouseDownPos = e.GetPosition(BoxCanvas);

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

                    box.Name = "hit" + _boxes.Count;
                    break;
                case 1:
                    box.Stroke = Hurtbox.Stroke;
                    box.Opacity = Hurtbox.Opacity;
                    box.Fill = Hurtbox.Fill;

                    box.Fill = _hurtBrush;

                    box.Name = "hurt" + _boxes.Count;
                    break;
            }

            box.Visibility = Visibility.Visible;

            box.MouseEnter += Box_MouseOver;
            box.MouseLeave += Box_MouseLeave;
            box.MouseLeftButtonDown += Box_MouseLeftButtonDown;
            
            _boxes.Add(new BoxInfo(new Box(), box));

            BoxCanvas.Children.Add(_boxes.Last().Rect);

            //currentBoxPos.Y = Math.Round(mouseDownPos.Y / gridSize) * gridSize;
            //currentBoxPos.X = Math.Round(mouseDownPos.X / gridSize) * gridSize;
            _currentBoxPos.X = _mouseDownPos.X;
            _currentBoxPos.Y = _mouseDownPos.Y;

            _boxes.Last().Box.SetPos(_currentBoxPos.X, _currentBoxPos.Y);

            BoxXText.Text = _boxes.Last().Box.X.ToString();
            BoxYText.Text = _boxes.Last().Box.Y.ToString();
            BoxDmgText.Text = _boxes.Last().Box.Damage.ToString();
            BoxKbStrengthText.Text = _boxes.Last().Box.KnockbackStrength.ToString();

            // Initial placement of the drag selection box.         
            Canvas.SetLeft(_boxes.Last().Rect, _currentBoxPos.X);
            Canvas.SetTop(_boxes.Last().Rect, _currentBoxPos.Y);

            // Make the drag selection box visible.
            _boxes.Last().Rect.Visibility = Visibility.Visible;
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _mouseDown = false;
            var mouseUpPos = e.GetPosition(BoxCanvas);

            if (_boxPlaceMode <= -1) return;

            _currentBoxCount = _boxes.Count;
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
            _boxPlaceMode = -1;
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            var mousePos = e.GetPosition(BoxCanvas);

            if (_boxPlaceMode <= -1)              return;
            if (!_mouseDown)                      return;
            if (_boxes.Count == _currentBoxCount) return;

            // When the mouse is held down, reposition the drag selection box
            if (_mouseDownPos.X < mousePos.X)
            {
                Canvas.SetLeft(_boxes.Last().Rect, _mouseDownPos.X);
                //boxes.Last().rect.Width = Math.Round((mousePos.X - mouseDownPos.X) / gridSize) * gridSize;
                _boxes.Last().Rect.Width = mousePos.X - _mouseDownPos.X;
            }
            else
            {
                Canvas.SetLeft(_boxes.Last().Rect, mousePos.X);
                //boxes.Last().rect.Width = Math.Round((mouseDownPos.X - mousePos.X) / gridSize) * gridSize;
                _boxes.Last().Rect.Width = _mouseDownPos.X - mousePos.X;
            }

            if (_mouseDownPos.Y < mousePos.Y)
            {
                Canvas.SetTop(_boxes.Last().Rect, _mouseDownPos.Y);
                //boxes.Last().rect.Height = Math.Round((mousePos.Y - mouseDownPos.Y) / gridSize) * gridSize;
                _boxes.Last().Rect.Height = mousePos.Y - _mouseDownPos.Y;
            }
            else
            {
                Canvas.SetTop(_boxes.Last().Rect, mousePos.Y);
                //boxes.Last().rect.Height = Math.Round((mouseDownPos.Y - mousePos.Y) / gridSize) * gridSize;
                _boxes.Last().Rect.Height = _mouseDownPos.Y - mousePos.Y;
            }

            _boxes.Last().Box.SetDims(_boxes.Last().Rect.Width, _boxes.Last().Rect.Height);
            BoxWidthText.Text = _boxes.Last().Box.Width.ToString();
            BoxHeightText.Text = _boxes.Last().Box.Height.ToString();
        }
    }
}
