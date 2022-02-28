using System.Numerics;

namespace OpenH2.Engine
{
    public static class EngineGlobals
    {
        public static Vector3 Up = new Vector3(0, 0, 1);
        public static Vector3 Forward = new Vector3(0, 1, 0);
        public static Vector3 Strafe = Vector3.Cross(Forward, Up);

    }
}
