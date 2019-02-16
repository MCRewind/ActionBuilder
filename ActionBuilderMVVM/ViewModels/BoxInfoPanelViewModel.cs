using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ActionBuilderMVVM.Models;
using Caliburn.Micro;

namespace ActionBuilderMVVM.ViewModels
{
    class BoxInfoPanelViewModel : Screen, IHandle<BoxInfoPanelEvent<BoxModel>>
    {
        private BoxModel _selectedBox;

        public BoxModel SelectedBox
        {
            get => _selectedBox;
            set
            {
                _selectedBox = value;
                NotifyOfPropertyChange(nameof(SelectedBox));
            }
        }

        public bool IsBoxSelected => SelectedBox != null;

        public BoxInfoPanelViewModel(IEventAggregator eventAggregator)
        {
            eventAggregator.Subscribe(this);
        }

        public void Handle(BoxInfoPanelEvent<BoxModel> message)
        {
            switch (message.EventType)
            {
                case BoxInfoPanelEventType.UpdateInfo:
                    SelectedBox = message.Value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
