using System;
using System.Reflection;
using System.Collections.Generic;

namespace CastleRenderer.Structures
{
    /// <summary>
    /// Used to signify that the method is a message handler
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class MessageHandler : Attribute
    {
        protected Type thetype;
        public Type GetMessageType() { return thetype; }
        public MessageHandler(Type messagetype) { thetype = messagetype; }
    }

    /// <summary>
    /// Used to signify that the component has a priority when handling messages
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ComponentPriority : Attribute
    {
        public int Priority { get; private set; }
        public ComponentPriority(int priority)
        {
            Priority = priority;
        }
    }

    /// <summary>
    /// Used to signify that the component requires another component to work
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RequiresComponent : Attribute
    {
        public Type RequiredType { get; private set; }
        public RequiresComponent(Type type)
        {
            RequiredType = type;
        }
    }

    /// <summary>
    /// Represents the base component from which all other components derive
    /// </summary>
    public abstract class BaseComponent
    {
        private Dictionary<Type, MethodInfo> dctMessageHandlers; // All message handlers this component contains
        private object[] tmp; // A temporary object array used to invoke the message handler

        /// <summary>
        /// The priority of this component over other components when handling messages
        /// </summary>
        public int Priority { get; protected set; }

        /// <summary>
        /// Whether or not this component can receive messages
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// The actor that this component belongs to
        /// </summary>
        public Actor Owner { get; set; }

        public BaseComponent()
        {
            // Create message handlers dictionary and temp array
            dctMessageHandlers = new Dictionary<Type, MethodInfo>();
            tmp = new object[1];

            // Loop each method
            MethodInfo[] methods = GetType().GetMethods();
            foreach (var method in methods)
            {
                // Loop all attributes
                foreach (var attrib in method.GetCustomAttributes(true))
                {
                    if (attrib is MessageHandler)
                    {
                        // Get the handler and message type
                        var hdl = attrib as MessageHandler;
                        var t = hdl.GetMessageType();

                        // Store in message handlers dict
                        dctMessageHandlers.Add(t, method);

                        // Done
                        break;
                    }
                }
            }

            // Determine priority
            ComponentPriority priority = GetType().GetCustomAttribute<ComponentPriority>(true);
            if (priority != null)
                Priority = priority.Priority;
            else
                Priority = GetHashCode(); // TODO: Some other not hacky way (if two components have the same hash code and exist on the same actor, one might not get added!)

            // Enable us
            Enabled = true;
        }

        /// <summary>
        /// Called when this component is attached to an Actor
        /// </summary>
        public virtual void OnAttach()
        {
            // Subscribe us to all message handlers
            foreach (var pair in dctMessageHandlers)
                Owner.MessagePool.Subscribe(this, pair.Key);
        }

        /// <summary>
        /// Called when this component is detached from an Actor
        /// </summary>
        public virtual void OnDetach()
        {
            // Unsubscribe us from all message handlers
            foreach (var pair in dctMessageHandlers)
                Owner.MessagePool.Unsubscribe(this, pair.Key);
        }

        /// <summary>
        /// Handles a message from the MessagePool
        /// </summary>
        /// <param name="msg"></param>
        public virtual void HandleMessage(Message msg)
        {
            // Check we're enabled
            if (!Enabled) return;

            // Check we have a handler
            var t = msg.GetType();
            MethodInfo method;
            if (!dctMessageHandlers.TryGetValue(t, out method)) return;

            // Handle it!
            tmp[0] = msg;
            try
            {
                method.Invoke(this, tmp);
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }

        /// <summary>
        /// Sends a message to the MessagePool
        /// </summary>
        /// <param name="msg"></param>
        protected virtual void SendMessage(Message msg)
        {
            // Send it to our owner's message pool
            Owner.MessagePool.SendMessage(msg);
        }

    }
}
