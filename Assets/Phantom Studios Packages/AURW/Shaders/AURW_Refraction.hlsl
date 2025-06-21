void AURWSnellsRefractionUV_float(
    float2 ScreenUV, // Raw screen UV (ScreenPosition.xy / ScreenPosition.w)
    float3 WorldPos, // World position of the current fragment
    float3 NormalWS, // World space normal (include normal map if needed)
    float3 ViewDirWS, // World space view direction (from fragment to camera)
    float n1, // Index of refraction of the medium above (usually air = 1.0)
    float n2, // Index of refraction of the medium below (water ~ 1.33)
    float WaterBottomY, // Y position of the water bottom plane (for ray intersection)
    float Strength, // Strength multiplier for visual control
    out float2 RefractionUV // Output UVs to sample the scene color
)
{
    // Normalize input vectors
    float3 N = normalize(NormalWS);
    float3 V = normalize(ViewDirWS);

    // Compute refraction direction using Snell's law
    float eta = n1 / max(n2, 0.0001); // Prevent division by zero
    float3 R = refract(-V, N, eta);

    // Handle total internal reflection (fallback to base UV)
    if (length(R) < 0.001)
    {
        RefractionUV = ScreenUV;
        return;
    }

    // Intersect the refracted ray with a horizontal plane at WaterBottomY
    float t = (WaterBottomY - WorldPos.y) / R.y;
    t = max(t, 0.0); // Clamp to prevent negative distance
    float3 HitPos = WorldPos + R * t;

    // Compute how far the refracted ray travels
    float3 RayOffset = HitPos - WorldPos;
    float offsetLength = length(RayOffset) * Strength;

    // Apply distortion offset to the screen UV (using R.x and R.z projected to screen)
    RefractionUV = ScreenUV + R.xy * offsetLength;
}

void AURW_Refraction_float(float3 Normal, float3 viewDir, float IOR, out float3 Out)
{
    Out = refract(viewDir, Normal, IOR);
}