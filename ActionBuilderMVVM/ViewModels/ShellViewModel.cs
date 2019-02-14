using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ActionBuilderMVVM.Models;
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

        private EditorViewModel ActiveEditor => ActiveItem as EditorViewModel;

        public ToolbarViewModel Toolbar { get; }

        public BoxInfoPanelViewModel BoxInfoPanel { get; private set; }

        private readonly Func<EditorViewModel> _editorViewModelFactory;

        public ShellViewModel(
            IEventAggregator eventAggregator,
            IConfigProvider configProvider,
            ToolbarViewModel toolbarViewModel,
            BoxInfoPanelViewModel boxInfoPanelViewModel,
            Func<EditorViewModel> editorViewModelFactory
        ){
            _eventAggregator = eventAggregator;
            _configProvider = configProvider;
            _configProvider.Load();
            Toolbar = toolbarViewModel;
            BoxInfoPanel = boxInfoPanelViewModel;
            this._editorViewModelFactory = editorViewModelFactory;

            _eventAggregator.Subscribe(this);
        }

        public void CloseItem(IScreen item)
        {
            if (((EditorViewModel)item).HasUnsavedChanges)
            {
                if (TaskDialog.OSSupportsTaskDialogs)
                {
                    using (var dialog = new TaskDialog())
                    {
                        dialog.WindowTitle = "Unsaved Changes";
                        dialog.MainInstruction = "This action has unsaved changes";
                        dialog.Content = $"Would you like to save changes to {item.DisplayName}?";
                        var yesButton = new TaskDialogButton(ButtonType.Yes);
                        var noButton = new TaskDialogButton(ButtonType.No);
                        dialog.Buttons.Add(yesButton);
                        dialog.Buttons.Add(noButton);
                        var buttonPressed = dialog.Show();
                        if (buttonPressed.Equals(yesButton))
                        {
                            if (((EditorViewModel)item).SaveAction())
                            {
                                DeactivateItem(item, true);
                            }
                        }

                        if (buttonPressed.Equals(noButton))
                        {
                            DeactivateItem(item, true);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("This operating system does not support task dialogs.", "Task Dialog Sample");
                }
            }
            else
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

                    ActivateEditor(newAction);
                    break;
                case ApplicationEventType.OpenActionEvent:
                    var action = FileUtils.OpenAction(ref refActionPath);

                    if (action != null)
                    {
                        _configProvider.ActionPath = refActionPath;
                        _configProvider.Save();

                        ActivateEditor(action);
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

            void ActivateEditor(ActionModel action)
            {
                var editorViewModel = _editorViewModelFactory();
                editorViewModel.DisplayName = action.Name;

                ActivateItem(editorViewModel);

                ActiveEditor.LoadAction(action);
            }
        }
    }
}
