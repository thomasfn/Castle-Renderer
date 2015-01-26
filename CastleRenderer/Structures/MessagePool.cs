using System;
using System.Collections.Generic;

namespace CastleRenderer.Structures
{
    /// <summary>
    /// The base message class from which all other messages will derive
    /// </summary>
    public abstract class Message { }

    /// <summary>
    /// Represents a request to unsubscribe a component
    /// </summary>
    public class SubscribeRequest
    {
        public bool Subscribing { get; set; }
        public BaseComponent Component { get; set; }
        public HashSet<BaseComponent> Set { get; set; }
    }

    /// <summary>
    /// Represents a hub - any components subscribed to this pool will receive messages sent to this pool
    /// </summary>
    public class MessagePool
    {
        private Dictionary<Type, HashSet<BaseComponent>> dctSubs; // Component subscriptions

        private HashSet<SubscribeRequest> subrequests;
        private ResourcePool<SubscribeRequest> requestpool;

        private int iterating;

        public MessagePool()
        {
            // Initialise the subscription dictionary
            dctSubs = new Dictionary<Type, HashSet<BaseComponent>>();
            requestpool = new ResourcePool<SubscribeRequest>();
            subrequests = new HashSet<SubscribeRequest>();
        }

        /// <summary>
        /// Subscribes a component to a specific message type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="component"></param>
        public void Subscribe<T>(BaseComponent component) where T : Message
        {
            // Alias
            Subscribe(component, typeof(T));
        }
        public void Subscribe(BaseComponent component, Type t)
        {
            // Get the type and set
            HashSet<BaseComponent> set;
            if (!dctSubs.TryGetValue(t, out set))
            {
                set = new HashSet<BaseComponent>();
                dctSubs.Add(t, set);
            }

            // Check iteration status
            if (iterating > 0)
            {
                var request = requestpool.Request();
                request.Component = component;
                request.Set = set;
                request.Subscribing = true;
                subrequests.Add(request);
            }
            else
                set.Add(component);
        }

        /// <summary>
        /// Unsubscribes a component from a specific message type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="component"></param>
        public void Unsubscribe<T>(BaseComponent component) where T : Message
        {
            // Alias
            Unsubscribe(component, typeof(T));
        }
        public void Unsubscribe(BaseComponent component, Type t)
        {
            // Get the type and set
            HashSet<BaseComponent> set;
            if (!dctSubs.TryGetValue(t, out set))
            {
                set = new HashSet<BaseComponent>();
                dctSubs.Add(t, set);
            }

            // Check iteration status
            if (iterating > 0)
            {
                var request = requestpool.Request();
                request.Component = component;
                request.Set = set;
                request.Subscribing = false;
                subrequests.Add(request);
            }
            else
                set.Remove(component);
        }

        /// <summary>
        /// Sends a message to all subscribed components
        /// </summary>
        /// <param name="msg"></param>
        public void SendMessage(Message msg)
        {
            // Get the type and set
            Type t = msg.GetType();
            HashSet<BaseComponent> set;
            if (!dctSubs.TryGetValue(t, out set))
            {
                set = new HashSet<BaseComponent>();
                dctSubs.Add(t, set);
            }

            // Send to all components
            iterating++;
            foreach (var c in set)
                if (c.Owner != null) // Edge case where a previous HandleMessage call caused a component later on in set to become unsubbed
                    c.HandleMessage(msg);
            iterating--;

            // Handle any sub requests
            ProcessSubRequests();
        }

        /// <summary>
        /// Processes all subscribe requests
        /// </summary>
        private void ProcessSubRequests()
        {
            // Loop each request
            foreach (var request in subrequests)
            {
                // Perform it
                if (request.Subscribing)
                    request.Set.Add(request.Component);
                else
                    request.Set.Remove(request.Component);

                // Recycle
                requestpool.Recycle(request);
            }

            // Clear
            subrequests.Clear();
        }

    }
}
