using OpenH2.Core.Architecture;
using OpenH2.Engine.Stores;
using OpenH2.Engine.Components;
using OpenH2.Engine.Entities;
using OpenH2.Rendering.Abstractions;
using System.Numerics;
using OpenTK.Windowing.Desktop;
using System;

namespace OpenH2.Engine.Systems
{
    public class MouseSelectSystem : WorldSystem
    {

        private readonly GameWindow window;
        private TriggerVolume selectedVolume;
        public MouseSelectSystem(World world, GameWindow window) : base(world)
        {
            selectedVolume = null;
            this.window = window;
        }

        // Find the trigger volume that the mouse is hovering over by raycasting from the camera.
        private TriggerVolume FindNextTriggerVolume(InputStore input_store)
        {
            var cameras = world.Components<CameraComponent>();
            var cam = cameras[0];

            var viewMatrix = cam.ViewMatrix;
            var projectionMatrix = cam.ProjectionMatrix;
            var viewProjectionMat = viewMatrix * projectionMatrix;

            // Get mouse position, and convert it to projection coordinates.
            // That is, scale [0, width) -> [-1, 1] and [0, height) -> [-1, 1].
            var mousePos = input_store.MousePos;
            var scaledMousePos = new Vector2((2 * mousePos.X / window.Size.X - 1),
                                             -(2 * mousePos.Y / window.Size.Y - 1));

            if (!Matrix4x4.Invert(viewProjectionMat, out var viewProjectionInv))
            {
                System.Console.Error.WriteLine($"Unable to invert view+projection matrix: {viewProjectionMat}");
                return null;
            }

            // Convert ray from projection space to world space.
            var rayOrigin = Vector4.Transform(new Vector4(0, 0, 0, 1), viewProjectionInv);
            rayOrigin /= rayOrigin.W;
            var rayTip = Vector4.Transform(new Vector4(scaledMousePos, 1, 1), viewProjectionInv);
            rayTip /= rayTip.W;

            float min = float.MaxValue;
            TriggerVolume newVolume = null;
            foreach (var entity in this.world.Scene.Entities.Values)
            {
                if (entity is not TriggerVolume tv)
                {
                    continue;
                }
                // All trigger volumes have a TriggerGeometryComponent.
                var trigGeomComp = entity.GetChildren<TriggerGeometryComponent>()[0];

                var left_bottom = new Vector3(0, 0, 0);
                var top_right = trigGeomComp.Size;
                var entTransMat = trigGeomComp.Transform.TransformationMatrix;
                if (!Matrix4x4.Invert(entTransMat, out var entTransMatInv))
                {
                    System.Console.Error.WriteLine($"Unable to invert entity transformation mat: {entTransMat} for entity {entity.FriendlyName}");
                    return null;
                }

                // Handle non-axis-aligned triggers: transform the ray into the local space of the trigger.
                var localRayOrigin = Vector4.Transform(rayOrigin, entTransMatInv);
                var localRayTip = Vector4.Transform(rayTip, entTransMatInv);

                // Now do standard axis-aligned bounding-box + ray intersection.
                var localRayDir = localRayTip - localRayOrigin;
                localRayDir /= localRayDir.Length();
                var localRayDirRecip = new Vector3(1.0f / localRayDir.X, 1.0f / localRayDir.Y, 1.0f / localRayDir.Z);

                // Calculate the distance `t` to all 6 planes of the box.
                float t1 = (left_bottom.X - localRayOrigin.X) * localRayDirRecip.X;
                float t2 = (top_right.X - localRayOrigin.X) * localRayDirRecip.X;
                float t3 = (left_bottom.Y - localRayOrigin.Y) * localRayDirRecip.Y;
                float t4 = (top_right.Y - localRayOrigin.Y) * localRayDirRecip.Y;
                float t5 = (left_bottom.Z - localRayOrigin.Z) * localRayDirRecip.Z;
                float t6 = (top_right.Z - localRayOrigin.Z) * localRayDirRecip.Z;

                float tmin = Math.Max(Math.Max(Math.Min(t1, t2), Math.Min(t3, t4)), Math.Min(t5, t6));
                float tmax = Math.Min(Math.Min(Math.Max(t1, t2), Math.Max(t3, t4)), Math.Max(t5, t6));
                if (tmin < 0 || tmax < 0 || tmin > tmax)
                {
                    // Discard the box if we are partially inside it (tmin < 0),
                    // the box is completely behind us (tmax < 0), or the ray missed (tmin > tmax).
                    continue;
                }
                if (tmin < min)
                {
                    // Otherwise, tmin is our total distance (in trigger-local space) to the box.
                    // Pick the lowest one (i.e., closest the camera).
                    min = tmin;
                    newVolume = tv;
                }
            }
            return newVolume;
        }

        public override void Update(double timestep)
        {
            var input_store = this.world.GetGlobalResource<InputStore>();
            if (input_store.RightMouseDown)
            {
                var newVolume = FindNextTriggerVolume(input_store);
                if (newVolume != selectedVolume)
                {
                    if (newVolume != null)
                    {
                        newVolume.ToggleSelected();
                        System.Console.WriteLine($"[TRIG] {newVolume.FriendlyName} selected");
                    }
                    if (selectedVolume != null)
                    {
                        selectedVolume.ToggleSelected();
                    }
                    selectedVolume = newVolume;
                }
            }
        }
    }
}
