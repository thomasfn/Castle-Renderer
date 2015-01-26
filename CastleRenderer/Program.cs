using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;

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
        public const int DesiredFPS = 120;
        public const float DesiredFrametime = 1.0f / DesiredFPS;

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
            //root.AddComponent<Sleeper>().TargetFPS = 60.0f;

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
            if (!root.GetComponent<SceneLoader>().LoadSceneFromFile("scene/main.json", true))
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
                // Process a frame, measuring the time taken
                frametimer.Start();
                Application.DoEvents();
                pool.SendMessage(framemsg);
                frametimer.Stop();
                float frametime = (float)frametimer.Elapsed.TotalSeconds;
                frametimer.Reset();

                // Sleep to maintain desired FPS
                float tosleep = DesiredFrametime - frametime;
                if (tosleep > 0.0f)
                {
                    Thread.Sleep((int)(tosleep * 1000.0f));
                    frametime = DesiredFrametime;
                }

                // Update next frame
                framemsg.DeltaTime = frametime;
                framemsg.FrameNumber++;
            }

            // Send the shutdown message
            ShutdownMessage shutdownmsg = new ShutdownMessage();
            pool.SendMessage(shutdownmsg);

            // Delete root actor and clean up
            root.Destroy(true);
        }
    }
}
