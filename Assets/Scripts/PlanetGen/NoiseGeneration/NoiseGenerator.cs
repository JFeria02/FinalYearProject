using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseGenerator
{
    private static NoiseLayer[] noiseLayers;

    public static float CalculateNoiseAtPoint(Vector3 point, int layerIndex)
    {
        float noiseValue = 0;
        float amplitude = 1f;
        float frequency = noiseLayers[layerIndex].noiseSettings.roughness;

        for(int i = 0; i < noiseLayers[layerIndex].noiseSettings.octaves; i++)
        {
            // Coordinates to sample from
            float xCoord = noiseLayers[layerIndex].noiseSettings.xOrigin + point.x * noiseLayers[layerIndex].noiseSettings.scale * frequency;
            float yCoord = noiseLayers[layerIndex].noiseSettings.yOrigin + point.y * noiseLayers[layerIndex].noiseSettings.scale * frequency;
            float zCoord = noiseLayers[layerIndex].noiseSettings.zOrigin + point.z * noiseLayers[layerIndex].noiseSettings.scale * frequency;

            // Get the noise sample
            FastNoiseLite generator = new FastNoiseLite(noiseLayers[layerIndex].noiseSettings.seed);
            generator.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            float sample = generator.GetNoise(xCoord, yCoord, zCoord);

            if(noiseLayers[layerIndex].noiseType == NoiseType.Original)
            {
                noiseValue += (sample + 1) * 0.5f * amplitude;
            }
            else if(noiseLayers[layerIndex].noiseType == NoiseType.Ridge)
            {
                sample = 1 - Mathf.Abs(sample);
                sample *= sample;
                noiseValue += sample * amplitude;
            }
            
            frequency *= noiseLayers[layerIndex].noiseSettings.frequencyControl;
            amplitude *= noiseLayers[layerIndex].noiseSettings.amplitudeControl;
        }

        noiseValue = Mathf.Max(0, noiseValue - noiseLayers[layerIndex].noiseSettings.minValue);
        return noiseValue * noiseLayers[layerIndex].noiseSettings.strength;

    }

    public static void SetNoiseLayers(NoiseLayer[] newNoiseLayers)
    {
        noiseLayers = newNoiseLayers;
    }

    public static NoiseLayer[] GetNoiseLayers()
    {
        return noiseLayers;
    }

}
