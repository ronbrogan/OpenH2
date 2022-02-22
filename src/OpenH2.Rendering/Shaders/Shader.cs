namespace OpenH2.Rendering.Shaders
{
    public enum Shader
    {
        Depth,
        Skybox,
        Generic,
        TextureViewer,
        Wireframe,
        Pointviz,
        ComputeGatherLights,
        ShadowMapping,

        // Always at the end, used to create arrays for mapping purposes
        MAX_VALUE
    }
}
