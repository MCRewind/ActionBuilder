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

        private EditorViewModel ActiveEditor => ActiveItem as EditorViewModel;

        public ShellViewModel(IEventAggregator eventAggregator, ToolbarViewModel toolbarViewModel, IConfigProvider configProvider)
        {

            _eventAggregator = eventAggregator;
            _configProvider = configProvider;
            _configProvider.Load();
            ToolbarViewModel = toolbarViewModel;

            _eventAggregator.Subscribe(this);
        }

        public void CloseItem(IScreen item)
        {
            if (((EditorViewModel) item).SaveAction())
            {
                DeactivateItem(item, true);
            }
        }

        public void Handle(ApplicationEventType eventType)
        {
            var refActionPath = _configProvider.ActionPath;
            switch (eventType)
            {
                case ApplicationEventType.NewActionEvent:
                    var newAction = new ActionModel
                    {
                        Name = "New Action"
                    };

                    ActivateItem(new EditorViewModel(_eventAggregator, _configProvider)
                    {
                        DisplayName = newAction.Name
                    });

                    ActiveEditor.LoadAction(newAction);
                    break;
                case ApplicationEventType.OpenActionEvent:
                    var action = FileUtils.OpenAction(ref refActionPath);

                    if (action != null)
                    {
                        _configProvider.ActionPath = refActionPath;
                        _configProvider.Save();

                        ActivateItem(new EditorViewModel(_eventAggregator, _configProvider)
                        {
                            DisplayName = action.Name
                        });
                        
                        ActiveEditor.LoadAction(action);
                    }

                    break;
                case ApplicationEventType.ChangeSpriteDirectoryEvent:
                    var newPath = FileUtils.ChangeSpriteDirDialog(_configProvider.SpritePath);

                    if (newPath == "NOCHANGE") return;

                    _configProvider.SpritePath = newPath;
                    _configProvider.Save();

                    _eventAggregator.PublishOnUIThread(new EditorEvent<string>(EditorEventType.UpdateSpritesEvent, newPath));
                    break;
                case ApplicationEventType.SaveActionEvent:
                    ActiveEditor.SaveAction();
                    break;
                case ApplicationEventType.SaveActionAsEvent:
                    ActiveEditor.SaveActionAs();
                    break;
                case ApplicationEventType.NextFrameEvent:
                    ActiveEditor.NextFrame();
                    break;
                case ApplicationEventType.PreviousFrameEvent:
                    ActiveEditor.PreviousFrame();
                    break;
                default:
                    break;
            }
        }
    }
}
