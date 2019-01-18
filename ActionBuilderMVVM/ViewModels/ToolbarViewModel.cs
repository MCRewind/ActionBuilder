using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;

namespace ActionBuilderMVVM.ViewModels
{
    class ToolbarViewModel : PropertyChangedBase, IHandle<ToolBarEvent<bool>>
    {
        private readonly IEventAggregator _eventAggregator;

        public ToolbarViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            _eventAggregator.Subscribe(this);            
        }

        private bool _canNextFrame;

        public bool CanNextFrame
        {
            get => _canNextFrame;
            set
            {
                _canNextFrame = value;
                NotifyOfPropertyChange(nameof(CanNextFrame));
            }
        }

        private bool _canPreviousFrame;

        public bool CanPreviousFrame
        {
            get => _canPreviousFrame;
            set
            {
                _canPreviousFrame = value;
                NotifyOfPropertyChange(nameof(CanPreviousFrame));
            }
        }

        public void OpenAction()
        {
           _eventAggregator.PublishOnUIThread(ApplicationEventType.OpenActionEvent);
        }

        public void ChangeSpriteDirectory()
        {
            _eventAggregator.PublishOnUIThread(ApplicationEventType.ChangeSpriteDirectoryEvent);
        }

        public void NextFrameAction()
        {
            _eventAggregator.PublishOnUIThread(EditorEventType.NextFrameEvent);
        }

        public void PreviousFrameAction()
        {
            _eventAggregator.PublishOnUIThread(EditorEventType.PreviousFrameEvent);
        }

        public void Handle(ToolBarEvent<bool> message)
        {
            switch (message.EventType)
            {
                case ToolBarEventType.CanNextFrameEvent:
                    CanNextFrame = message.Value;
                    break;
                case ToolBarEventType.CanPreviousFrameEvent:
                    CanPreviousFrame = message.Value;
                    break;
            }
        }
    }
}
