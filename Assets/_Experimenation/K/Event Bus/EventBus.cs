using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace _Experimenation.K.Event_Bus
{
    public static class EventBus
    {
        private interface ISubscription
        {
        }

        /// <summary>
        /// Holds a combined UnityAction<T> for one event type. Raising an
        /// event invokes the delegate directly - no DynamicInvoke reflection.
        /// </summary>
        private sealed class Subscription<T> : ISubscription
        {
            private UnityAction<T> _combined;

            public bool IsEmpty => _combined == null;

            public void Add(UnityAction<T> action)
            {
                // Preserve the original dedupe: subscribing the identical
                // delegate twice must not invoke it twice per Raise.
                if (_combined != null &&
                    Array.IndexOf(_combined.GetInvocationList(), action) >= 0)
                    return;

                _combined += action;
            }

            public void Remove(UnityAction<T> action)
            {
                _combined -= action;
            }

            public void Raise(T eventData)
            {
                _combined?.Invoke(eventData);
            }
        }

        private static readonly Dictionary<Type, ISubscription> Subscriptions = new();

        public static void Subscribe<T>(UnityAction<T> action)
        {
            if (action == null)
                return;

            if (Subscriptions.TryGetValue(typeof(T), out var existing) &&
                existing is Subscription<T> typed)
            {
                typed.Add(action);
                return;
            }

            var subscription = new Subscription<T>();
            subscription.Add(action);
            Subscriptions[typeof(T)] = subscription;
        }

        public static void Unsubscribe<T>(UnityAction<T> action)
        {
            if (action == null)
                return;

            if (!Subscriptions.TryGetValue(typeof(T), out var existing) ||
                existing is not Subscription<T> typed)
                return;

            typed.Remove(action);

            if (typed.IsEmpty)
                Subscriptions.Remove(typeof(T));
        }

        public static void Raise<T>(T eventData)
        {
            if (!Subscriptions.TryGetValue(typeof(T), out var existing) ||
                existing is not Subscription<T> typed)
                return;

            typed.Raise(eventData);
        }
    }
}