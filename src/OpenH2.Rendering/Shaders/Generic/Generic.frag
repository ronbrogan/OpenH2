#version 450

#extension GL_ARB_bindless_texture : require

layout(std140, binding = 0) uniform GlobalUniform
{
    mat4 ViewMatrix;
    mat4 ProjectionMatrix;
    vec4 ViewPosition;
} Globals;


const int EmissiveTypeEmissiveOnly = 0;   // fusion coil bloom?
const int EmissiveTypeDiffuseBlended = 1; // flag base
const int EmissiveTypeThreeChannel = 2;     // ascension
const int EmissiveTypeDisabled = -1;

layout(std140, binding = 1) uniform GenericUniform
{
    bool UseDiffuseMap;
    float DiffuseAmount;
    sampler2D DiffuseMap;
    vec4 DiffuseColor;
    
    bool UseAlpha;
    float AlphaAmount;
    sampler2D AlphaHandle;
    vec4 AlphaChannel;

    bool UseSpecularMap;
    float SpecularAmount;
    sampler2D SpecularMap;
    vec4 SpecularColor;
    
    bool UseNormalMap;
    float NormalMapAmount;
    sampler2D NormalMap;
    vec4 NormalMapScale;
    
    bool UseEmissiveMap;
    int EmissiveType;
    sampler2D EmissiveMap;
    vec4 EmissiveArguments;
    
    bool UseDetailMap1;
    float DetailMap1Amount;
    sampler2D DetailMap1;
    vec4 DetailMap1Scale;

    bool UseDetailMap2;
    float DetailMap2Amount;
    sampler2D DetailMap2;
    vec4 DetailMap2Scale;

    bool ChangeColor;
    float ColorChangeAmount;
    sampler2D ColorChangeMaskMap;
    vec4 ColorChangeColor;

} Data;

layout(std140, binding = 2) uniform TransformUniform
{
    mat4 ModelMatrix;
    mat4 NormalMatrix;
} Transform;

struct PointLight {
    vec4 Position;
    vec4 ColorAndRange;
};

layout(std140, binding = 3) uniform LightingUniform
{
    PointLight[10] pointLights;
} Lighting;

in Vertex
{
    vec2 texcoord;
    vec3 vertex_color;
    vec3 world_pos;
    vec3 world_normal;
    mat3 TBN;
} vertex;

layout(location = 0) out vec4 out_color;


vec3 calculated_normal;
vec3 specular_color;

vec3 viewDifference = Globals.ViewPosition.xyz - vertex.world_pos;
float viewDistance = length(viewDifference);
vec3 viewDirection = normalize(viewDifference);



// TODO move lighting to global uniform
vec3 light_pos = vec3(-5, 2, 5);
vec3 light_color = vec3(1);
vec3 lightDirection = normalize(light_pos); 

vec4 lightCalculation(in PointLight light, in vec4 textureColor);
vec4 globalLighting(in vec4 textureColor);
vec4 doColorChange(in vec4 baseColor);
vec4 unpackColorChangeComponent(in unsigned int component);

void main() {

    calculated_normal = vertex.world_normal;
    specular_color = vec3(1);

    if (Data.UseNormalMap) 
    {
        calculated_normal = normalize(texture(Data.NormalMap, vertex.texcoord * Data.NormalMapScale.xy).rgb * 2 - 1);

        lightDirection = vertex.TBN * lightDirection;
        viewDirection = vertex.TBN * viewDirection;
    }

    vec4 detail1Tex = texture(Data.DetailMap1, vertex.texcoord * Data.DetailMap1Scale.xy);
    vec4 detail2Tex = texture(Data.DetailMap2, vertex.texcoord * Data.DetailMap2Scale.xy);
    vec4 diffuseTex = texture(Data.DiffuseMap, vertex.texcoord);

    vec4 diffuseColor = Data.DiffuseColor;
    
    if(Data.UseDiffuseMap)
    {
        vec4 detailColor = vec4(0.4);

        if(Data.UseDetailMap1 && Data.UseDetailMap2)
        {
            detailColor = mix(detail1Tex, detail2Tex, diffuseTex.a);
        }
        else if(Data.UseDetailMap1 || Data.UseDetailMap2)
        {
            // If one is empty (vec4(0)), detailColor will be set to the other
            detailColor = detail1Tex + detail2Tex;
        }

        diffuseColor = vec4((diffuseTex * detailColor * 2.5).rgb, 1);
    }
    
    vec4 finalColor;

    // Sets ambient baseline
    finalColor = vec4(diffuseColor.rgb * 0.3, diffuseColor.a);

    // Adds global lighting
    finalColor += globalLighting(diffuseColor);

    // Accumulates point lights
    for(int i = 0; i < 10; i++)
    {
        if(Lighting.pointLights[i].ColorAndRange.a <= 0.0) 
        {
            continue;
        }
        finalColor += lightCalculation(Lighting.pointLights[i], diffuseColor);
    }

    if(Data.UseEmissiveMap)
    {
        vec4 emissiveSample = texture(Data.EmissiveMap, vertex.texcoord);

        if(Data.EmissiveType == EmissiveTypeEmissiveOnly)
        {
            float a = emissiveSample.r + emissiveSample.b + emissiveSample.g;
            finalColor = vec4(emissiveSample.rgb, a/3.0);
        }
        // TODO: figure out how to do the 3 channel stuff better?
        else if(Data.EmissiveType == EmissiveTypeThreeChannel)
        {
            float r = emissiveSample.r * Data.EmissiveArguments.r;
            float g = emissiveSample.g * Data.EmissiveArguments.g;
            float b = emissiveSample.b * Data.EmissiveArguments.b;

            float winner = max(max(r,g),b);

            finalColor += vec4(winner,winner,winner,0);
        }
        else
        {
            finalColor += vec4(emissiveSample.r);
        }
    }
    
    if(Data.ChangeColor)
    {
        finalColor = doColorChange(finalColor);
    }

    if(Data.UseAlpha)
    {
        vec4 alphaSample = texture(Data.AlphaHandle, vertex.texcoord);
        float alpha = min(alphaSample.a, finalColor.a);
    
        if(Data.AlphaChannel.r == 1.0)
        {
            alpha = alphaSample.r;
        }

        finalColor.a = alpha;
    }

    out_color = finalColor;
}

vec4 globalLighting(in vec4 textureColor)
{
    float cosTheta = clamp(dot(lightDirection, calculated_normal), 0.0, 1.0);
    vec4 light_diffuse = textureColor * vec4(light_color,1) * cosTheta;

    float specularFalloff = 1 / (viewDistance * viewDistance );

    vec3 halfwayDirection = normalize(lightDirection + viewDirection);
    float specularAngle = max(dot(calculated_normal, halfwayDirection), 0.0);

    // TODO: replace 100 with specular amount/intensity
    float specularModifier = pow(specularAngle, 100);

    // TODO: specular term is not working correctly
    vec4 light_specular = vec4(light_color,1) * vec4(light_color,1) * 0; //specularModifier;
    
    return light_diffuse + light_specular;
}

vec4 lightCalculation(in PointLight light, in vec4 textureColor)
{
    vec3 posDiff = light.Position.xyz - vertex.world_pos;

    if(length(posDiff) > light.ColorAndRange.a)
    {
        return vec4(0);
    }

    vec3 norm = normalize(vertex.world_normal);

    float falloff = 1/exp2(length(posDiff));

    vec4 diffuse = (textureColor/2 + vec4(light.ColorAndRange.xyz, 1)/2) * falloff;

    return diffuse;
}

vec4 doColorChange(in vec4 baseColor)
{
    vec4 maskSample = texture(Data.ColorChangeMaskMap, vertex.texcoord);

    if(maskSample.a < 0.1) return baseColor;

    vec4 primary = unpackColorChangeComponent(floatBitsToUint(Data.ColorChangeColor.r));
    vec4 secondary = unpackColorChangeComponent(floatBitsToUint(Data.ColorChangeColor.g));
    vec4 tertiary = unpackColorChangeComponent(floatBitsToUint(Data.ColorChangeColor.b));
    vec4 quaternary = unpackColorChangeComponent(floatBitsToUint(Data.ColorChangeColor.a));

    if(primary.a > 0)
    {
        baseColor = maskSample * primary;
    }

    return baseColor;
}

vec4 unpackColorChangeComponent(in unsigned int component)
{
    float r = float(component >> 24);
    float g = float(component >> 16 & 255);
    float b = float(component >> 8 & 255);
    float a = float(component & 255);

    return vec4(r / 255.0, g / 255.0, b / 255.0, a / 1.0);
}