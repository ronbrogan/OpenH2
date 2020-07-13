using OpenToolkit.Windowing.Common.Input;
using System;
using System.Linq;
using System.Numerics;

namespace OpenH2.Engine.Stores
{
    public class InputStore
    {
        public bool MouseDown { get; set; }
        public Vector2 MousePos { get; set; }
        public Vector2 MouseDiff { get; set; }

        public KeyboardState DownKeys { get; private set; }

        /// <summary>
        /// Keys that weren't down last frame, but now are
        /// </summary>
        public KeyboardState PressedKeys { get; private set; }



        public void SetMouse(MouseState mouse)
        {
            MouseDown = mouse.IsButtonDown(MouseButton.Left);
            var currPos = new Vector2(mouse.X, mouse.Y);
            MouseDiff = MousePos - currPos;
            MousePos = currPos;
        }

        private Key[] allKeys = Enum.GetValues(typeof(Key)).Cast<Key>().ToArray();

        public void SetKeys(KeyboardState currentDown)
        {
            var previousDown = this.DownKeys;
            var pressed = this.PressedKeys;

            foreach(var key in allKeys)
            {
                if (key == Key.Unknown)
                    continue;

                var curr = currentDown.IsKeyDown(key);
                var prev = previousDown.IsKeyDown(key);

                // TODO: held keys curr && prev
                
                pressed.SetKeyState(key, curr && prev == false);
            }

            this.PressedKeys = pressed;
            this.DownKeys = currentDown;
        }
    }
}
