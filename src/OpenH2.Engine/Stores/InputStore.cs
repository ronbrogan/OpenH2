using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Numerics;

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
            MouseDown = mouse.IsButtonDown(MouseButton.Left);
            var currPos = new Vector2(mouse.X, mouse.Y);
            MouseDiff = MousePos - currPos;
            MousePos = currPos;
        }

        public void SetKeys(KeyboardState currentDown)
        {
            this.PreviousKeyState = this.KeyState;
            this.KeyState = currentDown.GetSnapshot();
        }

        /// <summary>
        /// Returns true if the key is down now, but wasn't last frame 
        /// </summary>
        public bool WasPressed(Keys key)
        {
            return this.KeyState.IsKeyDown(key) && (!this.PreviousKeyState?.IsKeyDown(key) ?? true);
        }

        /// <summary>
        /// Returns if the key is currently down
        /// </summary>
        public bool IsDown(Keys key)
        {
            return this.KeyState.IsKeyDown(key);
        }

        /// <summary>
        /// Returns true if the key is down now, and was last frame as well
        /// </summary>
        public bool Held(Keys key)
        {
            return this.KeyState.IsKeyDown(key) && this.PreviousKeyState.IsKeyDown(key);
        }
    }
}
