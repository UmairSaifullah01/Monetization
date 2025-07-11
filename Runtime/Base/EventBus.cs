using System;
using System.Collections.Generic;

namespace THEBADDEST.MonetizationApi
{
    /// <summary>
    /// Marker interface for all event types used with the EventBus.
    /// </summary>
    public interface IEvent { }

    /// <summary>
    /// Event published when an ad is shown.
    /// </summary>
    public class AdShownEvent : IEvent
    {
        public string AdType;
        public string Placement;
        public DateTime Time;
    }

    /// <summary>
    /// A simple, type-safe event bus for decoupled communication between modules.
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> _subscribers = new();

        /// <summary>
        /// Subscribe to an event type.
        /// </summary>
        /// <typeparam name="T">The event type.</typeparam>
        /// <param name="handler">The handler to invoke when the event is published.</param>
        public static void Subscribe<T>(Action<T> handler) where T : IEvent
        {
            var type = typeof(T);
            if (!_subscribers.ContainsKey(type))
                _subscribers[type] = new List<Delegate>();
            _subscribers[type].Add(handler);
        }

        /// <summary>
        /// Unsubscribe from an event type.
        /// </summary>
        /// <typeparam name="T">The event type.</typeparam>
        /// <param name="handler">The handler to remove.</param>
        public static void Unsubscribe<T>(Action<T> handler) where T : IEvent
        {
            var type = typeof(T);
            if (_subscribers.TryGetValue(type, out var handlers))
                handlers.Remove(handler);
        }

        /// <summary>
        /// Publish an event to all subscribers.
        /// </summary>
        /// <typeparam name="T">The event type.</typeparam>
        /// <param name="evt">The event instance.</param>
        public static void Publish<T>(T evt) where T : IEvent
        {
            var type = typeof(T);
            if (_subscribers.TryGetValue(type, out var handlers))
            {
                // Copy to avoid modification during iteration
                var handlersCopy = handlers.ToArray();
                foreach (var handler in handlersCopy)
                {
                    if (handler is Action<T> action)
                        action(evt);
                }
            }
        }
    }
} 