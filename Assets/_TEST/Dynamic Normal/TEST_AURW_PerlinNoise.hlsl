#ifndef ADVANCED_PERLIN_NOISE_V2_INCLUDED
#define ADVANCED_PERLIN_NOISE_V2_INCLUDED

// --- 1. Hash Function (Stable & High-Entropy) ---
float2 hash22(float2 p)
{
    p = float2(
        dot(p, float2(127.1, 311.7)),
        dot(p, float2(269.5, 183.3))
    );
    return -1.0 + 2.0 * frac(sin(p) * 43758.5453);
}

// --- 2. Perlin Noise Function (Modular, Adjustable, Multi-Style) ---
void UltimatePerlinNoise_float(
    float2 UV, // Tiled UV input (from Tiling and Offset node)
    float Scale, // Scale of noise detail
    float Time, // Time offset (for animation)
    int Octaves, // Number of layers
    float Persistence, // Controls amplitude decay
    float Lacunarity, // Controls frequency increase
    float2 FlowDirection, // Direction of animated distortion (optional)
    float Seed, // Random seed input
    float Style, // Style switch: 0 = Classic, 1 = Turbulence, 2 = Ridged, 3 = Cubic
    out float OutNoise // Final noise output (mapped to [0,1] for visuals)
)
{
    UV *= Scale;
    if (Time > 0.0)
    {
        UV += Time * FlowDirection;
    }

    float amplitude = 1.0;
    float frequency = 1.0;
    float total = 0.0;
    float maxAmp = 0.0;

    Octaves = clamp(Octaves, 1, 10);

    [unroll(10)]
    for (int i = 0; i < Octaves; i++)
    {
        float2 p = UV * frequency + Seed * float2(37.1, 17.3) + i * 17.0;
        float2 cell = floor(p);
        float2 local = frac(p);
        float2 fade = local * local * (3.0 - 2.0 * local);

        float2 g00 = hash22(cell);
        float2 g10 = hash22(cell + float2(1.0, 0.0));
        float2 g01 = hash22(cell + float2(0.0, 1.0));
        float2 g11 = hash22(cell + float2(1.0, 1.0));

        float d00 = dot(g00, local);
        float d10 = dot(g10, local - float2(1.0, 0.0));
        float d01 = dot(g01, local - float2(0.0, 1.0));
        float d11 = dot(g11, local - float2(1.0, 1.0));

        float interpX1 = lerp(d00, d10, fade.x);
        float interpX2 = lerp(d01, d11, fade.x);
        float noise = lerp(interpX1, interpX2, fade.y);

        // Apply style logic
        if (Style == 0.0) // Classic
        {
            total += noise * amplitude;
        }
        else if (Style == 1.0) // Turbulence
        {
            total += abs(noise) * amplitude;
        }
        else if (Style == 2.0) // Ridged
        {
            total += (1.0 - abs(noise)) * amplitude;
        }
        else if (Style == 3.0) // Cubic / Pixelated Style
        {
            float stepVal = step(0.0, noise);
            total += stepVal * amplitude;
        }

        maxAmp += amplitude;
        amplitude *= Persistence;
        frequency *= Lacunarity;
    }

    total /= maxAmp;
    total = total * 0.5 + 0.5;
    OutNoise = saturate(total);
}

#endif