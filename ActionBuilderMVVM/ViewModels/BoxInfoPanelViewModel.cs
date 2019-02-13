using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ActionBuilderMVVM.Models.ActionBuilderMVVM;
using Caliburn.Micro;

namespace ActionBuilderMVVM.ViewModels
{
    class BoxInfoPanelViewModel : Screen, IHandle<BoxInfoPanelEvent<ActionModel.Box>>
    {
        private ActionModel.Box _selectedBox;

        public ActionModel.Box SelectedBox
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
            SelectedBox = new ActionModel.Box();
        }

        public void Handle(BoxInfoPanelEvent<ActionModel.Box> message)
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
