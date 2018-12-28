using System;
using System.Collections.Generic;
using System.Text;
using OpenH2.Core.Tags;
using OpenH2.Rendering.Abstractions;
using OpenH2.Rendering.OpenGL;
using OpenTK.Graphics.OpenGL;

namespace OpenH2.Engine
{
    public class Engine
    {
        IGraphicsHost graphicsHost;
        IGameLoopSource gameLoop;

        IRenderAccumulator renderAccumulator;

        Scenario loadedScenario = null;

        public Engine()
        {
            var host = new OpenGLHost();
            graphicsHost = host;
            gameLoop = host;
        }

        public void Start(EngineStartParameters parameters)
        {
            graphicsHost.CreateWindow();

            gameLoop.RegisterCallbacks(Update, Render);
            gameLoop.Start(60, 60);
        }

        public void LoadScenario(object scenario)
        {

        }

        private void Update()
        {
            // Process input

            // Do physics
        }

        private void Render()
        {
            // Accumulate visible items
            
            // Kick off render
        }
    }
}
