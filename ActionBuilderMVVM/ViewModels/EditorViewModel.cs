using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ActionBuilderMVVM.Models;
using ActionBuilderMVVM.Other;
using ActionBuilderMVVM.Properties;
using ActionBuilderMVVM.Utils;
using Caliburn.Micro;
using Ookii.Dialogs.Wpf;
using Action = System.Action;
using Path = System.IO.Path;

namespace ActionBuilderMVVM.ViewModels
{
    class EditorViewModel : Screen, IHandle<EditorEvent<string>>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IConfigProvider _configProvider;

        private int _canvasWidth;
        private int _canvasHeight;
        private int _actionFrame;

        private bool _hasUnsavedChanges;

        private string _actionSpritesPath;

        private Rect _gridRect;

        private ActionModel _action = null;

        public ObservableCollection<BoxRectModel> BoxRects { get; set; }

        public EditorViewModel(IEventAggregator eventAggregator, IConfigProvider configProvider)
        {
            BoxRects = new ObservableCollection<BoxRectModel>();

            _eventAggregator = eventAggregator;
            _configProvider = configProvider;

            _eventAggregator.Subscribe(this);

            CanvasWidth = 200;
            CanvasHeight = 200;
        }

        public bool HasUnsavedChanges
        {
            get => _hasUnsavedChanges;
            set
            {
                _hasUnsavedChanges = value;
                UpdateTabTitle();
            }
        }

        public int CanvasWidth
        {
            get => _canvasWidth;
            set
            {
                _canvasWidth = value;
                NotifyOfPropertyChange(nameof(CanvasWidth));
            }
        }

        public int CanvasHeight
        {
            get => _canvasHeight;
            set
            {
                _canvasHeight = value;
                NotifyOfPropertyChange(nameof(CanvasHeight));
            }
        }

        public Rect GridRect
        {
            get => _gridRect;
            set
            {
                _gridRect = value;
                NotifyOfPropertyChange(nameof(GridRect));
            }
        }

        public void SetCanvasWidth(int width) => CanvasWidth = width;

        public void SetCanvasHeight(int height) => CanvasHeight = height;

        private string _imagePath;

        public string ImagePath
        {
            get => _imagePath;
            set
            {
                _imagePath = value;
                NotifyOfPropertyChange(nameof(ImagePath));
            }
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            //TODO add menu items for hitboxes
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            //TODO remove menu items for hitboxes
        }

        private void UpdateTabTitle()
        {
            DisplayName = _action.Name + (HasUnsavedChanges ? "*" : "");
        }

        public void Handle(EditorEvent<string> newPathEvent)
        {
            _actionSpritesPath = Path.Combine(newPathEvent.Value, _action.Name);

            if (TaskDialog.OSSupportsTaskDialogs)
            {
                using (var dialog = new TaskDialog())
                {
                    dialog.WindowTitle = "Sprite Update";
                    dialog.MainInstruction = "The sprite directory has been updated.";
                    dialog.Content = "Would you like to reload sprites?";
                    var yesButton = new TaskDialogButton(ButtonType.Yes);
                    var noButton = new TaskDialogButton(ButtonType.No);
                    dialog.Buttons.Add(yesButton);
                    dialog.Buttons.Add(noButton);
                    var buttonPressed = dialog.Show();
                    if (buttonPressed.Equals(yesButton))
                    {
                        ReloadSprites();
                    }
                }
            }
            else
            {
                MessageBox.Show("This operating system does not support task dialogs.", "Task Dialog Sample");
            }
        }

        public bool SaveAction()
        {
            if (_action.Path == null)
            {
                var refActionPath = _configProvider.ActionPath;
                if (FileUtils.SaveActionAs(_action, ref refActionPath))
                {
                    HasUnsavedChanges = false;
                    return true;
                }
            }
            else
            {
                FileUtils.SaveAction(_action);
                return true;
            }

            return false;
        }

        public bool SaveActionAs()
        {
            var refActionPath = _configProvider.ActionPath;
            if (FileUtils.SaveActionAs(_action, ref refActionPath))
            {
                HasUnsavedChanges = false;
                return true;
            }

            return false;
        }

        public void LoadAction(ActionModel action)
        {
            _action = action;
            if (_configProvider.SpritePath == null)
            {
                ImagePath = new Uri("pack://application:,,,/Textures/NO_TEXTURE.png").AbsolutePath;
            }
            else
            {
                _actionSpritesPath = Path.Combine(_configProvider.SpritePath, _action.Name);
                ReloadSprites();
            }
            Console.WriteLine($"frameCount: {_action.FrameCount}");

            HasUnsavedChanges = action.Path == null;

            _actionFrame = 0;
            SwitchFrames(0);
        }

        private void SwitchFrames(int distance)
        {
            var newFrame = _actionFrame + distance;

            if (newFrame < 0)
                newFrame = 0;
            else if (newFrame >= _action.FrameCount)
                newFrame = Math.Max(0, _action.FrameCount - 1);

            if (newFrame == 0)
            {
                _eventAggregator.PublishOnUIThread(new ToolBarEvent<bool>(ToolBarEventType.CanPreviousFrameEvent, false));
                _eventAggregator.PublishOnUIThread(new ToolBarEvent<bool>(ToolBarEventType.CanNextFrameEvent, true));
            }
            else if (newFrame == _action.FrameCount - 1 )
            {
                _eventAggregator.PublishOnUIThread(new ToolBarEvent<bool>(ToolBarEventType.CanNextFrameEvent, false));
                _eventAggregator.PublishOnUIThread(new ToolBarEvent<bool>(ToolBarEventType.CanPreviousFrameEvent, true));
            }
            else
            {
                _eventAggregator.PublishOnUIThread(new ToolBarEvent<bool>(ToolBarEventType.CanNextFrameEvent, true));
                _eventAggregator.PublishOnUIThread(new ToolBarEvent<bool>(ToolBarEventType.CanPreviousFrameEvent, true));
            }

            _actionFrame = newFrame;

            ReloadSprites();
            ReloadBoxes();
        }

        private void ReloadBoxes()
        {
            BoxRects.Clear();
            if (_actionFrame >= _action.Hurtboxes.Count)
            {
                return;
            }
            foreach (var box in _action.Hurtboxes[_actionFrame])
                BoxRects.Add(new BoxRectModel(Box.BoxType.Hurt, box.X * 10, box.Y * 10, (float)box.Width * 10, (float)box.Height * 10));
        }

        private void ReloadSprites()
        {
            if (_actionSpritesPath == null) return;

            var path = Path.Combine(_actionSpritesPath, _actionFrame + ".png");
            ImagePath = File.Exists(path) ? path : new Uri("pack://application:,,,/Textures/NO_TEXTURE.png").AbsolutePath;
        }

        public void NextFrame()
        {
            SwitchFrames(1);
        }

        public void PreviousFrame()
        {
            SwitchFrames(-1);
        }

    }
}
