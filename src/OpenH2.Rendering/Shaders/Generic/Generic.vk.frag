#version 450

#extension GL_EXT_nonuniform_qualifier : enable

const int EmissiveTypeEmissiveOnly = 0;   // fusion coil bloom?
const int EmissiveTypeDiffuseBlended = 1; // flag base
const int EmissiveTypeThreeChannel = 2;     // ascension
const int EmissiveTypeDisabled = -1;
layout(std140, binding = 0) uniform GlobalUniform
{
    mat4 ViewMatrix;
    mat4 ProjectionMatrix;
    mat4 SunLightMatrix[4];
    vec4 SunLightDistances;
    vec4 SunLightDirection;
    vec4 ViewPosition;
} Globals;

layout(std140, binding = 1) uniform TransformUniform
{
    mat4 ModelMatrix;
    mat4 NormalMatrix;
} Transform;

layout(std140, binding = 2) uniform GenericUniform
{
    bool UseDiffuseMap;
    float DiffuseAmount;
    ivec2 DiffuseMap;
    vec4 DiffuseColor;
    
    bool UseAlpha;
    float AlphaAmount;
    ivec2 AlphaHandle;
    vec4 AlphaChannel;

    bool UseSpecularMap;
    float SpecularAmount;
    ivec2 SpecularMap;
    vec4 SpecularColor;
    
    bool UseNormalMap;
    float NormalMapAmount;
    ivec2 NormalMap;
    vec4 NormalMapScale;
    
    bool UseEmissiveMap;
    int EmissiveType;
    ivec2 EmissiveMap;
    vec4 EmissiveArguments;
    
    bool UseDetailMap1;
    float DetailMap1Amount;
    ivec2 DetailMap1;
    vec4 DetailMap1Scale;

    bool UseDetailMap2;
    float DetailMap2Amount;
    ivec2 DetailMap2;
    vec4 DetailMap2Scale;

    bool ChangeColor;
    float ColorChangeAmount;
    ivec2 ColorChangeMaskMap;
    vec4 ColorChangeColor;
} Data;

layout(set = 1, binding = 3) uniform sampler2D Textures[];

layout(binding = 16) uniform sampler2DArray shadowMap;

layout(location = 0) in vec3 frag_pos;
layout(location = 1) in vec2 texcoord;
layout(location = 2) in vec3 color;
layout(location = 3) in vec3 world_pos;
layout(location = 4) in vec3 world_normal;
layout(location = 5) in mat3 TBN;

layout(location = 0) out vec4 out_color;


vec3 calculated_normal;
vec3 specular_color;

vec3 viewDifference = Globals.ViewPosition.xyz - world_pos;
float viewDistance = length(viewDifference);
vec3 viewDirection = normalize(viewDifference);


vec3 light_color = vec3(1);
vec3 lightDirection = normalize(vec3(Globals.SunLightDirection)); 

vec4 globalLighting(in vec4 textureColor);
vec4 doColorChange(in vec4 baseColor);
vec4 unpackColorChangeComponent(in uint component);
float shadowCalculation(in vec3 fragPosWorldSpace);

void main() {

    calculated_normal = world_normal;
    specular_color = vec3(1);

    if (Data.UseNormalMap) 
    {
        calculated_normal = normalize(texture(Textures[Data.NormalMap.x], texcoord * Data.NormalMapScale.xy).rgb * 2 - 1);

        lightDirection = TBN * lightDirection;
        viewDirection = TBN * viewDirection;
    }

    vec4 detail1Tex = texture(Textures[Data.DetailMap1.x], texcoord * Data.DetailMap1Scale.xy);
    vec4 detail2Tex = texture(Textures[Data.DetailMap2.x], texcoord * Data.DetailMap2Scale.xy);
    vec4 diffuseTex = texture(Textures[Data.DiffuseMap.x], texcoord);

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
    float shadow = shadowCalculation(frag_pos);
    finalColor += (1.0 - shadow) * globalLighting(diffuseColor) * 0.5;

    if(Data.UseEmissiveMap)
    {
        vec4 emissiveSample = texture(Textures[Data.EmissiveMap.x], texcoord);

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
        vec4 alphaSample = texture(Textures[Data.AlphaHandle.x], texcoord);
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
    float cosTheta = clamp(dot(-lightDirection, calculated_normal), 0.0, 1.0);
    vec4 light_diffuse = textureColor * vec4(light_color,1) * cosTheta;

    float specularFalloff = 1 / (viewDistance * viewDistance );

    vec3 halfwayDirection = normalize(-lightDirection + viewDirection);
    float specularAngle = max(dot(calculated_normal, halfwayDirection), 0.0);

    // TODO: replace 100 with specular amount/intensity
    float specularModifier = pow(specularAngle, 100);

    // TODO: specular term is not working correctly
    vec4 light_specular = vec4(light_color,1) * vec4(light_color,1) * 0; //specularModifier;
    
    return light_diffuse + light_specular;
}

vec4 doColorChange(in vec4 baseColor)
{
    vec4 maskSample = texture(Textures[Data.ColorChangeMaskMap.x], texcoord);

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

vec4 unpackColorChangeComponent(in uint component)
{
    float r = float(component >> 24);
    float g = float(component >> 16 & 255);
    float b = float(component >> 8 & 255);
    float a = float(component & 255);

    return vec4(r / 255.0, g / 255.0, b / 255.0, a / 1.0);
}


float shadowCalculation(vec3 fragPosWorldSpace)
{
    // select cascade layer
    vec4 fragPosViewSpace = Globals.ViewMatrix * vec4(fragPosWorldSpace, 1.0);
    float depthValue = abs(fragPosViewSpace.z);

    int layer = -1;
    for (int i = 0; i < 3; ++i)
    {
        if (depthValue < Globals.SunLightDistances[i])
        {
            layer = i;
            break;
        }
    }
    if (layer == -1)
    {
        layer = 3;
    }

    vec4 fragPosLightSpace = Globals.SunLightMatrix[layer] * vec4(fragPosWorldSpace, 1.0);

    // perform perspective divide
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;

    // transform to [0,1] range for sampling, depth is already 0,1
    vec3 texCoords = projCoords * 0.5 + 0.5;

    // get depth of current fragment from light's perspective
    float currentDepth = projCoords.z;

    // keep the shadow at 0.0 when outside the far_plane region of the light's frustum.
    if (currentDepth > 1.0)
    {
        return 0.0;
    }

    // calculate bias (based on depth map resolution and slope)
    //vec3 normal = normalize(fs_in.Normal);
    //float bias = max(0.05 * (1.0 - dot(normal, lightDir)), 0.005);
    //const float biasModifier = 0.5f;
    //if (layer == cascadeCount)
    //{
    //    bias *= 1 / (farPlane * biasModifier);
    //}
    //else
    //{
    //    bias *= 1 / (cascadePlaneDistances[layer] * biasModifier);
    //}

    float bias = 0.0005;

    // PCF
    float shadow = 0.0;
    vec2 texelSize = 1.0 / vec2(textureSize(shadowMap, 0));
    for(int x = -1; x <= 1; ++x)
    {
        for(int y = -1; y <= 1; ++y)
        {
            float pcfDepth = texture(shadowMap, vec3(texCoords.xy + vec2(x, y) * texelSize, layer)).r;
            shadow += (currentDepth - bias) > pcfDepth ? 1.0 : 0.0;
        }    
    }
    shadow /= 9.0;
        
    return clamp(shadow, 0, 1);
}  
