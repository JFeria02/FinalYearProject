using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using System;

public class PlanetThreadManager : MonoBehaviour
{
    public static NoiseSettings noiseSettings;

    private List<PlanetQuadtree> newActiveObjects = new List<PlanetQuadtree>(); // New objects that need to be processed

    private List<JobHandle> activeJobHandles = new List<JobHandle>();
    private List<PlanetQuadtree> newActiveObjectsProcessing = new List<PlanetQuadtree>(); // Objects currently being processed

    private List<ApplyNoiseJob> noiseJobs = new List<ApplyNoiseJob>();

    private static int res;

    public static void SetRes(int newRes)
    {
        res = newRes;
    }

    // Start is called before the first frame updates
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Setup jobs for ApplyingNoise
        if (newActiveObjects.Count != 0)
        {
            NativeList<JobHandle> applyNoiseJobHandleList = new NativeList<JobHandle>(Allocator.Temp);

            // Generate jobs
            for (int i = 0; i < newActiveObjects.Count; i++)
            {
                
                if (newActiveObjects[i] != null)
                {
                    JobHandle job = ApplyNoiseJob(newActiveObjects[i]);
                    activeJobHandles.Add(job);
                    newActiveObjectsProcessing.Add(newActiveObjects[i]);
                }
            }

            newActiveObjects = new List<PlanetQuadtree>();

        }
    }


    private void LateUpdate()
    {
        //Check if jobs are completed
        List<JobHandle> jobsNotCompleted = new List<JobHandle>();
        List<PlanetQuadtree> newActiveObjectsStillProcessing = new List<PlanetQuadtree>();
        List<ApplyNoiseJob> noiseJobsStillProcessing = new List<ApplyNoiseJob>();

        for (int i = 0; i < activeJobHandles.Count; i++)
        {
            if (activeJobHandles[i].IsCompleted)
            {
                try
                {
                    // ApplyNoise Function no longer compatiable with threading
                    //newActiveObjectsProcessing[i].ApplyNoise(noiseJobs[i].noiseResults.ToArray(), noiseJobs[i].sphereVertices.ToArray());
                    noiseJobs[i].noiseResults.Dispose();
                    noiseJobs[i].sphereVertices.Dispose();
                }
                catch (InvalidOperationException)
                {
                    jobsNotCompleted.Add(activeJobHandles[i]);
                    newActiveObjectsStillProcessing.Add(newActiveObjectsProcessing[i]);
                    noiseJobsStillProcessing.Add(noiseJobs[i]);
                }
                
            }
            else
            {
                jobsNotCompleted.Add(activeJobHandles[i]);
                newActiveObjectsStillProcessing.Add(newActiveObjectsProcessing[i]);
                noiseJobsStillProcessing.Add(noiseJobs[i]);

            }
        }
        activeJobHandles = jobsNotCompleted;
        newActiveObjectsProcessing = newActiveObjectsStillProcessing;
        noiseJobs = noiseJobsStillProcessing;

    }

    private JobHandle ApplyNoiseJob(PlanetQuadtree tree)
    {
        int expectedSize = (res + 1) * (res + 1);

        // Pass values into the job
        ApplyNoiseJob applyNoiseJob = new ApplyNoiseJob
        {
            sphereVertices = new NativeArray<Vector3>(tree.GetSphereVertices(), Allocator.TempJob),
            noiseResults = new NativeArray<float>(expectedSize, Allocator.TempJob),

            seed = noiseSettings.seed,
            octaves = noiseSettings.octaves,
            scale = noiseSettings.scale,
            xOrigin = noiseSettings.xOrigin,
            yOrigin = noiseSettings.xOrigin,
            zOrigin = noiseSettings.xOrigin,
            roughness = noiseSettings.roughness,
            frequencyControl = noiseSettings.frequencyControl,
            amplitudeControl = noiseSettings.amplitudeControl,
            minValue = noiseSettings.minValue,
            strength = noiseSettings.strength,
            radius = tree.GetRadius()
        };
        noiseJobs.Add(applyNoiseJob);
        return applyNoiseJob.Schedule();
    }

    public void AddNewObject(PlanetQuadtree obj)
    {
        newActiveObjects.Add(obj);
    }

    public void RemoveNewObject(PlanetQuadtree obj)
    {
        newActiveObjects.Remove(obj);
    }

    public static void SetNoiseSettings(NoiseSettings newSettings)
    {
        noiseSettings = newSettings;
    }
}

public struct ApplyNoiseJob : IJob
{
    public NativeArray<Vector3> sphereVertices;
    public NativeArray<float> noiseResults;

    // Other settings
    public int radius;

    // Noise Settings
    public int seed;

    public float scale;
    public int octaves;

    public float xOrigin;
    public float yOrigin;
    public float zOrigin;

    public float roughness;

    public float frequencyControl;
    public float amplitudeControl;

    public float minValue;
    public float strength;

    public void Execute()
    {
        for(int i = 0; i < sphereVertices.Length; i++)
        {
            float noiseValue = 0;
            float amplitude = 1f;
            float frequency = roughness;

            for (int j = 0; j < octaves; j++)
            {
                float xCoord = xOrigin + sphereVertices[i].x * scale * frequency;
                float yCoord = yOrigin + sphereVertices[i].y * scale * frequency;
                float zCoord = zOrigin + sphereVertices[i].z * scale * frequency;
                float sample = OpenSimplex2.Noise3_ImproveXY(seed, xCoord, yCoord, zCoord);

                noiseValue += (sample + 1) * 0.5f * amplitude;
                frequency *= frequencyControl;
                amplitude *= amplitudeControl;
            }

            noiseValue = Mathf.Max(0, noiseValue - minValue);
            noiseResults[i] = noiseValue * strength;
            sphereVertices[i] = (1 + noiseResults[i]) * radius * sphereVertices[i];
        }

    }
}


