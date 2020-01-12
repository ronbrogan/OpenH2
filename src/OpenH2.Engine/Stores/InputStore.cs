using OpenTK.Input;
using System.Numerics;

namespace OpenH2.Engine.Stores
{
    public class InputStore
    {
        public bool MouseDown { get; set; }
        public Vector2 MousePos { get; set; }
        public Vector2 MouseDiff { get; set; }
        public KeyboardState Keyboard { get; set; }
        


        public void SetMouse(MouseState mouse)
        {
            MouseDown = mouse.IsButtonDown(MouseButton.Left);
            var currPos = new Vector2(mouse.X, mouse.Y);
            MouseDiff = MousePos - currPos;
            MousePos = currPos;
        }

        public void SetKeys(KeyboardState kb)
        {
            this.Keyboard = kb;
        }
    }
}
