﻿using System;
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
using ActionBuilderMVVM.Models.ActionBuilderMVVM;
using ActionBuilderMVVM.Other;
using ActionBuilderMVVM.Properties;
using ActionBuilderMVVM.Utils;
using Caliburn.Micro;
using Ookii.Dialogs.Wpf;
using Action = System.Action;
using Path = System.IO.Path;

namespace ActionBuilderMVVM.ViewModels
{
    class EditorViewModel : Screen, IHandle<EditorEvent<ActionModel>>, IHandle<EditorEvent<string>>, IHandle<EditorEventType>
    {

        private readonly IEventAggregator _eventAggregator;
        private readonly IConfigProvider _configProvider;

        private int _canvasWidth;
        private int _canvasHeight;
        private int _actionFrame;

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

        public void Handle(EditorEvent<ActionModel> action)
        {
            LoadAction(action.Value);
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

            SwitchFrames(0);

            foreach (var frame in _action.AllBoxes)
            {
                foreach (var box in frame)
                {
                    BoxRects.Add(new BoxRectModel(box.X * 10, box.Y * 10, (float)box.Width * 10, (float)box.Height * 10));
                }
            }
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

        public void Handle(EditorEventType message)
        {
            switch (message)
            {
                case EditorEventType.NextFrameEvent:
                    NextFrame();
                    break;
                case EditorEventType.PreviousFrameEvent:
                    PreviousFrame();
                    break;
            }
        }

        private void SwitchFrames(int distance)
        {
            var newFrame = _actionFrame + distance;

            if (newFrame < 0)
                _actionFrame = 0;
            else if (newFrame > _action.FrameCount)
                _actionFrame = _action.FrameCount;

            if (newFrame == 0)
            {
                _eventAggregator.PublishOnUIThread(new ToolBarEvent<bool>(ToolBarEventType.CanPreviousFrameEvent, false));
                _eventAggregator.PublishOnUIThread(new ToolBarEvent<bool>(ToolBarEventType.CanNextFrameEvent, true));
            }
            else if (newFrame == _action.FrameCount)
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
        }

        private void ReloadSprites()
        {
            if (_actionSpritesPath == null) return;

            var path = Path.Combine(_actionSpritesPath, _actionFrame + ".png");
            ImagePath = File.Exists(path) ? path : new Uri("pack://application:,,,/Textures/NO_TEXTURE.png").AbsolutePath;
        }

        private void NextFrame()
        {
            SwitchFrames(1);
        }

        private void PreviousFrame()
        {
            SwitchFrames(-1);
        }

    }
}