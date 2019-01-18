using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ActionBuilderMVVM.Models.ActionBuilderMVVM;
using ActionBuilderMVVM.Other;
using ActionBuilderMVVM.Utils;
using Caliburn.Micro;
using Newtonsoft.Json;
using Ookii.Dialogs.Wpf;
using Wpf.Controls.PanAndZoom;

namespace ActionBuilderMVVM.ViewModels
{
    class ShellViewModel : Conductor<IScreen>.Collection.OneActive, IHandle<ApplicationEventType>
    {
        private readonly IEventAggregator _eventAggregator;

        private readonly IConfigProvider _configProvider;

        public ToolbarViewModel ToolbarViewModel { get; }

        public ShellViewModel(IEventAggregator eventAggregator, ToolbarViewModel toolbarViewModel, IConfigProvider configProvider)
        {

            _eventAggregator = eventAggregator;
            _configProvider = configProvider;
            _configProvider.Load();
            ToolbarViewModel = toolbarViewModel;

            _eventAggregator.Subscribe(this);
        }

        public void Handle(ApplicationEventType eventType)
        {
            switch (eventType)
            {
                case ApplicationEventType.OpenActionEvent:
                    var refActionPath = _configProvider.ActionPath;
                    var action = FileUtils.OpenAction(ref refActionPath);

                    if (action != null)
                    {
                        _configProvider.ActionPath = refActionPath;
                        _configProvider.Save();

                        ActivateItem(new EditorViewModel(_eventAggregator, _configProvider)
                        {
                            DisplayName = action.Name
                        });

                        _eventAggregator.PublishOnUIThread(new EditorEvent<ActionModel>(EditorEventType.UpdateActionEvent, action));
                    }

                    break;
                case ApplicationEventType.ChangeSpriteDirectoryEvent:
                    var newPath = FileUtils.ChangeSpriteDirDialog(_configProvider.SpritePath);

                    if (newPath == "NOCHANGE") return;

                    _configProvider.SpritePath = newPath;
                    _configProvider.Save();

                    _eventAggregator.PublishOnUIThread(new EditorEvent<string>(EditorEventType.UpdateSpritesEvent, newPath));
                    break;
                default:
                    break;
            }
        }

    }
}
