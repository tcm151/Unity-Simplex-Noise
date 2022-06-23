using System.Collections.Generic;
using UnityEngine;


public static class Noise
{
    //> NOISE TYPES
    public enum Type { Simple, Rigid, Brownian, Swiss };

    //> NOISE LAYER SETTINGS
    [System.Serializable]
    public class Layer
    {
        [Header("Properties")]
        public bool enabled = true;
        public bool subtract;
        public Type noiseType;

        [Header("Masking")]
        public bool useMask;
        public Layer mask;

        public SimplexNoise generator = new SimplexNoise();

        [Header("Settings")]
        [Range(01, 004)] public int octaves = 3;
        [Range(00, 001)] public float strength = 0.25f;
        [Range(00, .1f)] public float baseRoughness = 0.001f;
        [Range(00, 010)] public float roughness = 4f;
        [Range(00, 002)] public float persistence = 0.5f;
        [Range(00, 001)] public float threshold = 0.5f;
        public Vector3 offset = Vector3.zero;
    }

    //> GET A NOISE VALUE CREATED FROM MULTIPLE NOISE LAYERS
    public static float GenerateValue(List<Layer> noiseLayers, Vector3 vector3)
    {
        //- return if list is empty
        if (noiseLayers.Count == 0) return default;


        float noiseValue = 0f;
        float firstLayer = GenerateValue(noiseLayers[0], vector3);
        if (noiseLayers[0].enabled) noiseValue += GenerateValue(noiseLayers[0], vector3);

        //- calculate every other layer
        for (int i = 1; i < noiseLayers.Count && noiseLayers[i].enabled; i++)
        {
            float maskValue = (noiseLayers[i].useMask) ? firstLayer : 1;
            noiseValue += GenerateValue(noiseLayers[i], vector3) * maskValue;
        }

        return noiseValue;
    }

    //> GET A NOISE VALUE FOR ANY VECTOR3 BASED ON THE GIVEN LAYER SETTINGS
    public static float GenerateValue(Layer noiseLayer, Vector3 vector3)
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
                        float value = noiseLayer.generator.Generate(vector3 * roughness + noiseLayer.offset);
                        generatedValue += (value + 1) / 2 * amplitude;
                        roughness *= noiseLayer.roughness;
                        amplitude *= noiseLayer.persistence;
                    }
                    break;
                }

            //> MOUNTAINOUS NOISE FILTER
            case { noiseType: Type.Rigid }:
                {
                    for (int i = 0; i < noiseLayer.octaves; i++)
                    {
                        float value = 1 - Mathf.Abs(noiseLayer.generator.Generate(vector3 * roughness + noiseLayer.offset));
                        value *= value * weight;
                        weight = value;
                        generatedValue += value * amplitude;
                        roughness *= noiseLayer.roughness;
                        amplitude *= noiseLayer.persistence;
                    }
                    break;
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