using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ActionBuilderMVVM.Models;
using Caliburn.Micro;

namespace ActionBuilderMVVM.ViewModels
{
    class BoxInfoPanelViewModel : Screen, IHandle<BoxInfoPanelEvent<Box>>
    {
        private Box _selectedBox;

        public Box SelectedBox
        {
            get => _selectedBox;
            set
            {
                _selectedBox = value;
                NotifyOfPropertyChange(nameof(SelectedBox));
            }

        }

        public BoxInfoPanelViewModel()
        {
            SelectedBox = new Box();
        }

        public void Handle(BoxInfoPanelEvent<Box> message)
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
