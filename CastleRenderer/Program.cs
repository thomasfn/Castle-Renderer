using System;
using System.Diagnostics;
using System.Windows.Forms;

using CastleRenderer.Structures;
using CastleRenderer.Messages;
using CastleRenderer.Components;

namespace CastleRenderer
{
    /// <summary>
    /// The class containing the entry point for the application
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Entry point for the application
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            // Create the message pool
            MessagePool pool = new MessagePool();
            
            // Create the root actor
            Actor root = new Actor(pool);

            // Attach core systems
            root.AddComponent<UserInputHandler>();
            root.AddComponent<Renderer>();
            root.AddComponent<MaterialSystem>();
            root.AddComponent<SceneManager>();
            root.AddComponent<SceneLoader>();
            root.AddComponent<Sleeper>().TargetFPS = 60.0f;

            // Attach exit listener
            bool exit = false;
            Listener<ExitMessage> exitlistener = root.AddComponent<Listener<ExitMessage>>() as Listener<ExitMessage>;
            exitlistener.OnMessageReceived += (msg) => exit = true;

            // Initialise
            root.Init();

            // Send the initialise message
            InitialiseMessage initmsg = new InitialiseMessage();
            pool.SendMessage(initmsg);

            // Load the scene
            if (!root.GetComponent<SceneLoader>().LoadSceneFromFile("scene.json"))
            {
                Console.WriteLine("Failed to load scene!");
                Console.ReadKey();
                return;
            }

            // Setup the frame message
            FrameMessage framemsg = new FrameMessage();
            framemsg.FrameNumber = 0;
            framemsg.DeltaTime = 0.0f;

            // Setup the timer
            Stopwatch frametimer = new Stopwatch();

            // Loop until done
            while (!exit)
            {
                // Send frame message
                frametimer.Start();
                pool.SendMessage(framemsg);
                frametimer.Stop();
                framemsg.DeltaTime = (float)frametimer.Elapsed.TotalSeconds;
                frametimer.Reset();

                // Increase frame number
                framemsg.FrameNumber++;

                // Process windows events
                Application.DoEvents();
            }

            // Send the shutdown message
            ShutdownMessage shutdownmsg = new ShutdownMessage();
            pool.SendMessage(shutdownmsg);

            // Delete root actor and clean up
            root.Destroy(true);
        }
    }
}
