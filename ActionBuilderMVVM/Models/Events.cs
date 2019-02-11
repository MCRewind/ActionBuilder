using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActionBuilderMVVM
{
    #region ApplicationEvent

    internal enum ApplicationEventType
    {
        NewActionEvent,
        OpenActionEvent,
        SaveActionEvent,
        SaveActionAsEvent,
        ChangeSpriteDirectoryEvent,
        NextFrameEvent,
        PreviousFrameEvent
    }

    class ApplicationEvent<T>
    {
        public ApplicationEventType EventType { get; }

        public T Value { get; }

        public ApplicationEvent(ApplicationEventType eventType, T value = default)
        {
            Value = value;
            EventType = eventType;
        }
    }

    #endregion

    #region ToolBarEvent

    internal enum ToolBarEventType
    {
        CanNextFrameEvent,
        CanPreviousFrameEvent
    }

    class ToolBarEvent<T>
    {
        public ToolBarEventType EventType { get; }

        public T Value { get; }

        public ToolBarEvent(ToolBarEventType eventType, T value = default)
        {
            Value = value;
            EventType = eventType;
        }
    }

#endregion

    #region EditorEvent

    internal enum EditorEventType
    {
        UpdateSpritesEvent,
    }

    internal class EditorEvent<T>
    {

        public EditorEventType EventType { get; }

        public T Value { get; }

        public EditorEvent(EditorEventType eventType, T value = default)
        {
            Value = value;
            EventType = eventType;
        }
    }

#endregion

}
