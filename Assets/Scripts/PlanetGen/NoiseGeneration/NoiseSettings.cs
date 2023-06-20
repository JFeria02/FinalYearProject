using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NoiseSettings
{
    public int seed;
    public float scale;
    public float xOrigin;
    public float yOrigin;
    public float zOrigin;
    public int octaves;
    public float amplitudeControl;
    public float frequencyControl;
    public float minValue;
    public float strength;
    public float roughness;
}

[System.Serializable]
public class NoiseLayer
{
    public NoiseType noiseType;
    public NoiseSettings noiseSettings;
}

public enum NoiseType
{
    Original,
    Ridge
}
