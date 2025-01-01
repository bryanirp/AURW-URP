#ifndef WATER_CUSTOM_COLOR
#define WATER_CUSTOM_COLOR
void AdjustColorByPH_float(float3 baseColor, float pH, float intensity, out float3 adjustedColor)
{
    // Normalizar el pH al rango [-1, 1]
    float normalizedPH = saturate((pH - 7.0) / 7.0) * 2.0 - 1.0;

    // Definir los colores cálidos (ácidos) y fríos (alcalinos)
    float3 warmShift = float3(1.0, 0.8, 0.2); // Amarillo/Verde claro para ácidos
    float3 coolShift = float3(0.2, 0.6, 1.0); // Azul/Verde para alcalinos

    // Interpolar entre cálidos y fríos según el pH normalizado
    float3 colorShift = lerp(warmShift, coolShift, (normalizedPH + 1.0) * 0.5);

    // Aplicar el ajuste al color base con una intensidad ajustable
  
    adjustedColor = saturate(baseColor + colorShift * abs(normalizedPH) * intensity);
}
#endif