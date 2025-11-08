using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace CustomEventBus
{
    public class EventBus
    {
        private readonly Dictionary<Type, List<Subscription>> _callbacks = new();
        private readonly object _locker = new();

        public IDisposable Subscribe<T>(Action<T> callback, int priority = 0)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            var subscription = new Subscription(this, typeof(T), callback, priority);

            lock (_locker)
            {
                if (!_callbacks.TryGetValue(subscription.EventType, out var list))
                {
                    list = new List<Subscription>();
                    _callbacks.Add(subscription.EventType, list);
                }

                list.Add(subscription);
                list.Sort(CompareSubscriptions);
            }

            return subscription;
        }

        public void Invoke<T>(T signal)
        {
            List<Subscription> snapshot;
            lock (_locker)
            {
                if (!_callbacks.TryGetValue(typeof(T), out var list) || list.Count == 0)
                {
                    return;
                }

                snapshot = list.Where(s => !s.IsDisposed).ToList();
            }

            foreach (var subscription in snapshot)
            {
                subscription.Invoke(signal);
            }
        }

        public void Unsubscribe<T>(Action<T> callback)
        {
            if (callback == null)
            {
                return;
            }

            lock (_locker)
            {
                if (!_callbacks.TryGetValue(typeof(T), out var list) || list.Count == 0)
                {
                    Debug.LogErrorFormat("Trying to unsubscribe for not existing key! {0}", typeof(T).Name);
                    return;
                }

                var subscription = list.FirstOrDefault(s => s.Matches(callback));
                if (subscription == null)
                {
                    Debug.LogErrorFormat("Trying to unsubscribe missing callback for key {0}", typeof(T).Name);
                    return;
                }

                subscription.Dispose();
            }
        }

        private void Remove(Subscription subscription)
        {
            lock (_locker)
            {
                if (!_callbacks.TryGetValue(subscription.EventType, out var list))
                {
                    return;
                }

                list.Remove(subscription);
                if (list.Count == 0)
                {
                    _callbacks.Remove(subscription.EventType);
                }
            }
        }

        private static int CompareSubscriptions(Subscription left, Subscription right)
        {
            if (left == null && right == null) return 0;
            if (left == null) return 1;
            if (right == null) return -1;

            var priorityCompare = right.Priority.CompareTo(left.Priority);
            if (priorityCompare != 0)
            {
                return priorityCompare;
            }

            return left.Sequence.CompareTo(right.Sequence);
        }

        private sealed class Subscription : IDisposable
        {
            private static long _sequenceGenerator;
            private readonly Delegate _callback;
            private readonly EventBus _owner;
            private bool _isDisposed;

            public Subscription(EventBus owner, Type eventType, Delegate callback, int priority)
            {
                _owner = owner ?? throw new ArgumentNullException(nameof(owner));
                EventType = eventType ?? throw new ArgumentNullException(nameof(eventType));
                _callback = callback ?? throw new ArgumentNullException(nameof(callback));
                Priority = priority;
                Sequence = Interlocked.Increment(ref _sequenceGenerator);
            }

            public Type EventType { get; }
            public int Priority { get; }
            public long Sequence { get; }
            public bool IsDisposed => _isDisposed;

            public void Invoke<T>(T signal)
            {
                if (_isDisposed)
                {
                    return;
                }

                if (_callback is Action<T> typed)
                {
                    typed(signal);
                }
            }

            public bool Matches(Delegate callback)
            {
                return !_isDisposed && _callback.Equals(callback);
            }

            public void Dispose()
            {
                if (_isDisposed)
                {
                    return;
                }

                _isDisposed = true;
                _owner.Remove(this);
            }
        }
    }
}
