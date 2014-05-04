using System;

using CastleRenderer.Structures;
using CastleRenderer.Messages;

namespace CastleRenderer.Components
{
    public delegate void MessageReceived(Message msg);

    /// <summary>
    /// A message listener that fires an event when the specified message has been received
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Listener<T> : BaseComponent where T : Message
    {
        public event MessageReceived OnMessageReceived;

        public override void OnAttach()
        {
            // Subscribe to the message
            Owner.MessagePool.Subscribe<T>(this);
        }

        public override void HandleMessage(Message msg)
        {
            // Is it the one we're listening for?
            if (msg.GetType() == typeof(T) && OnMessageReceived != null)
                OnMessageReceived(msg);
        }

    }
}
