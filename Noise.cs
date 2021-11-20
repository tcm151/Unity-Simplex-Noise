using System.Collections.Generic;
using UnityEngine;


namespace ProcessControl.Procedural
{
    public static class Noise
    {
        //> NOISE TYPES
        public enum Type { Simple, Rigid, Brownian, Swiss };

        //> NOISE LAYER SETTINGS
        [System.Serializable] public class Layer
        {
            public bool enabled = true;
            public bool subtract;
            
            public bool useMask;
            public Layer mask;

            public Type noiseType;
            public SimplexNoise generator = new SimplexNoise();

            [Range(01, 004)] public int octaves = 3;
            [Range(00, 001)] public float strength = 0.25f;
            [Range(00, .1f)] public float baseRoughness = 0.001f;
            [Range(00, 010)] public float roughness = 4f;
            [Range(00, 002)] public float persistence = 0.5f;
            [Range(00, 001)] public float threshold = 0.5f;
            public Vector3 offset = Vector3.zero;
        }
        
        //> GET A NOISE VALUE FOR ANY VECTOR3
        public static float GenerateValue(List<Layer> noiseLayers, Vector3 v3)
        {
            float noiseValue = 0f;
            float firstLayerValue = 0f;

            //- calculate first layer
            if (noiseLayers.Count > 0)
            {
                firstLayerValue = GenerateValue(noiseLayers[0], v3);
                if (noiseLayers[0].enabled) noiseValue = firstLayerValue;
            }

            //- calculate every other layer
            for (int i = 1; i < noiseLayers.Count; i++)
            {
                // ignore if not enabled
                if (!noiseLayers[i].enabled) continue;

                float firstLayerMask = (noiseLayers[i].useMask) ? firstLayerValue : 1;
                noiseValue += GenerateValue(noiseLayers[i], v3) * firstLayerMask;
            }

            return noiseValue;
        }

        //> GET A NOISE VALUE FROM A VECTOR3
        public static float GenerateValue(Layer noiseLayer, Vector3 v3)
        {
            float generatedValue = 0f;
            float roughness = noiseLayer.baseRoughness;
            float amplitude = 1;
            // float frequency = 1;
            float weight = 1;

            switch (noiseLayer)
            {
                //> SIMPLE NOISE FILTER
                case { noiseType: Type.Simple }:
                {
                    for (int i = 0; i < noiseLayer.octaves; i++)
                    {
                        float value = noiseLayer.generator.Generate(v3 * roughness + noiseLayer.offset);
                        generatedValue += (value + 1) / 2 * amplitude;
                        roughness *= noiseLayer.roughness;
                        amplitude *= noiseLayer.persistence;
                    } break;
                }

                //> MOUNTAINOUS NOISE FILTER
                case { noiseType: Type.Rigid }:
                {
                    for (int i = 0; i < noiseLayer.octaves; i++)
                    {
                        float value = 1 - Mathf.Abs(noiseLayer.generator.Generate(v3 * roughness + noiseLayer.offset));
                        value *= value * weight;
                        weight = value;
                        generatedValue += value * amplitude;
                        roughness *= noiseLayer.roughness;
                        amplitude *= noiseLayer.persistence;
                    } break;
                }

                //> BROWNIAN NOISE FILTER
                // case { noiseType: Type.Brownian }:
                // {
                //     float acceleration = 2f;
                //
                //     for (int i = 0; i < noise.octaves; i++)
                //     {
                //         generatedValue += noise.generator.Generate(vector3 * Mathf.Pow(acceleration, i)) / Mathf.Pow(acceleration, i);
                //     } break;
                // }
                
                //> LAYER NOT ENABLED
                case { enabled: false }:
                {
                    return 0;
                }
                
            }

            generatedValue = Mathf.Max(0, generatedValue - noiseLayer.threshold);
            return (noiseLayer.subtract) ? -generatedValue * noiseLayer.strength : generatedValue * noiseLayer.strength;
        }
    }
}