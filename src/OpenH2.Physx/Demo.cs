using PhysX;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Physx
{
    public static class Demo
    {
        public static void Main(string[] args)
        {
            var f = new Foundation();

            var p = new Physics(f);

            var cooking = p.CreateCooking();
        }
    }
}
