using System;
using System.Collections.Generic;

using CastleRenderer.Structures;
using CastleRenderer.Messages;
using CastleRenderer.Graphics;

using SlimDX;

namespace CastleRenderer.Components
{
    /// <summary>
    /// Represents the user input handler
    /// </summary>
    [ComponentPriority(0)]
    public class UserInputHandler : BaseComponent
    {
        /// <summary>
        /// Called when the window has been created
        /// </summary>
        /// <param name="msg"></param>
        [MessageHandler(typeof(WindowCreatedMessage))]
        public void OnWindowCreated(WindowCreatedMessage msg)
        {
            // Add event handlers
            msg.Form.KeyDown += Form_KeyDown;
            msg.Form.KeyUp += Form_KeyUp;
            msg.Form.MouseDown += Form_MouseDown;
            msg.Form.MouseUp += Form_MouseUp;
            msg.Form.MouseMove += Form_MouseMove;

            // TODO: Optimise by caching message objects (reduces mem allocations)
        }

        private void Form_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            // Send mousemove message
            MouseMoveMessage msg = new MouseMoveMessage();
            msg.X = e.X;
            msg.Y = e.Y;
            Owner.MessagePool.SendMessage(msg);
        }

        private void Form_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            // Send mousepress message
            MousePressMessage msg = new MousePressMessage();
            msg.Button = e.Button;
            msg.Depressed = true;
            Owner.MessagePool.SendMessage(msg);
        }

        private void Form_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            // Send mousepress message
            MousePressMessage msg = new MousePressMessage();
            msg.Button = e.Button;
            msg.Depressed = false;
            Owner.MessagePool.SendMessage(msg);
        }

        private void Form_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            // Send keypress message
            KeyPressMessage msg = new KeyPressMessage();
            msg.Key = e.KeyCode;
            msg.Depressed = true;
            Owner.MessagePool.SendMessage(msg);
        }
        private void Form_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            // Send keypress message
            KeyPressMessage msg = new KeyPressMessage();
            msg.Key = e.KeyCode;
            msg.Depressed = false;
            Owner.MessagePool.SendMessage(msg);
        }

    }
}
