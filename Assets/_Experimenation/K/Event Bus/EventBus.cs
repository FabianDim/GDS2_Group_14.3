using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

namespace _Experimenation.K.Event_Bus
{
    public static class EventBus
    {
        private static readonly Dictionary<Type, Delegate> Events = new();

        public static void Subscribe<T>(UnityAction<T> action)
        {
            var actionType = typeof(T);
            if (
                action == null||
                Events.TryAdd(actionType, action) || 
                Events[actionType].GetInvocationList().Contains(action)) return;
            Events[actionType] = Delegate.Combine(Events[actionType], action);
        }

        public static void Unsubscribe<T>(UnityAction<T> action)
        {
            var actionType = typeof(T);
            if (!Events.ContainsKey(actionType)) return;
            Events[actionType] = Delegate.Remove(Events[actionType], action);
            if(Events[actionType] == null) Events.Remove(actionType);
        }

        public static void Raise<T>(T eventData)
        {
            var actionType = typeof(T);
            if (!Events.TryGetValue(actionType, out var @event)) return;
            @event.DynamicInvoke(eventData);
        }
    }
}