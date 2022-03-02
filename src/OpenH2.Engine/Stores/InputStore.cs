using Silk.NET.Input;
using Silk.NET.Input.Extensions;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;

namespace OpenH2.Engine.Stores
{
    public class InputStore
    {
        public bool MouseDown { get; set; }
        public Vector2 MousePos { get; set; }
        public Vector2 MouseDiff { get; set; }

        private KeyboardState PreviousKeyState { get; set; }
        private KeyboardState KeyState { get; set; }

        public void SetMouse(MouseState mouse)
        {
            MouseDown = mouse.IsButtonPressed(MouseButton.Left);
            MouseDiff = MousePos - mouse.Position;
            MousePos = mouse.Position;
        }

        public void SetKeys(KeyboardState currentDown)
        {
            this.PreviousKeyState = this.KeyState;
            this.KeyState = currentDown;
        }


        /// <summary>
        /// Returns true if the key is down now, but wasn't last frame 
        /// </summary>
        public bool WasPressed(Key key)
        {
            return (this.KeyState?.IsKeyPressed(key) ?? false) && (!(this.PreviousKeyState?.IsKeyPressed(key)) ?? true);
        }

        /// <summary>
        /// Returns if the key is currently down
        /// </summary>
        public bool IsDown(Key key)
        {
            return this.KeyState?.IsKeyPressed(key) ?? false;
        }

        /// <summary>
        /// Returns true if the key is down now, and was last frame as well
        /// </summary>
        public bool Held(Key key)
        {
            return (this.KeyState?.IsKeyPressed(key) ?? false) && this.PreviousKeyState.IsKeyPressed(key);
        }
    }
}
