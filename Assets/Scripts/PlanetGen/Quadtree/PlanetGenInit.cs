using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PlanetGenInit : MonoBehaviour
{
    public int res;
    public int radius;
    public int maxLevel;
    //private PlanetQuadtree quadtree;
    private GameObject quadtreePy;
    private GameObject quadtreePz;
    private GameObject quadtreePx;
    private GameObject quadtreeNy;
    private GameObject quadtreeNz;
    private GameObject quadtreeNx;

    private Vector3[] initialVerticesPy;
    private Vector3[] initialVerticesPz;
    private Vector3[] initialVerticesPx;
    private Vector3[] initialVerticesNy;
    private Vector3[] initialVerticesNz;
    private Vector3[] initialVerticesNx;




    public bool drawBottomLeftChild;
    public bool drawBottomRightChild;
    public bool drawTopLeftChild;
    public bool drawTopRightChild;

    public bool liveUpdate = false;
    public bool liveUpdateTrigger = false;

    public NoiseLayer[] noiseLayers;



    public void BeginGen()
    {
        if (res % 2 != 0) // System only supports even resolution values, otherwise the vertice calculations will not work
        {
            res += 1;
        }

        // Positive Y (Top of Cube) - Origin = 0-res/2,0+res/2,0-res/2
        initialVerticesPy = new Vector3[((res + 1) * (res + 1))];
        int currentIndex = 0;
        for (int i = 0; i < res + 1; i++) //i for z axis
        {
            for (int j = 0; j < res + 1; j++) //j for x axis
            {
                initialVerticesPy[currentIndex] = new Vector3(j - res / 2, res / 2, i - res / 2);
                currentIndex++;
            }
        }

        // Positive Z
        initialVerticesPz = new Vector3[((res + 1) * (res + 1))];
        currentIndex = 0;
        for (int i = 0; i < res + 1; i++) //i for z axis
        {
            for (int j = 0; j < res + 1; j++) //j for x axis
            {
                initialVerticesPz[currentIndex] = new Vector3(j - res / 2, i - res / 2, 0 - (res / 2));
                currentIndex++;
            }
        }

        // Positive X
        initialVerticesPx = new Vector3[((res + 1) * (res + 1))];
        currentIndex = 0;
        for (int i = 0; i < res + 1; i++) //i for z axis
        {
            for (int j = 0; j < res + 1; j++) //j for x axis
            {
                initialVerticesPx[currentIndex] = new Vector3(res / 2, i - res / 2, j - res / 2);
                currentIndex++;
            }
        }

        // Negative Y -> for negatives, flip completely
        initialVerticesNy = new Vector3[((res + 1) * (res + 1))];
        currentIndex = 0;
        for (int i = res; i > -1; i--) //i for z axis
        {
            for (int j = 0; j < res + 1; j++) //j for x axis
            {
                initialVerticesNy[currentIndex] = new Vector3(j - res / 2, 0 - res / 2, i - res / 2);
                //Debug.Log("Noise: " + noiseTexture.GetPixel(j, i).r);
                currentIndex++;
            }
        }

        // Negative Z -> for negatives, flip completely
        initialVerticesNz = new Vector3[((res + 1) * (res + 1))];
        currentIndex = 0;
        for (int i = 0; i < res + 1; i++) //i for z axis
        {
            for (int j = res; j > -1; j--) //j for x axis
            {
                initialVerticesNz[currentIndex] = new Vector3(j - res / 2, i - res / 2, res / 2);
                currentIndex++;
            }
        }

        // Negative X
        initialVerticesNx = new Vector3[((res + 1) * (res + 1))];
        currentIndex = 0;
        for (int i = 0; i < res + 1; i++) //i for z axis
        {
            for (int j = res; j > -1; j--) //j for x axis
            {
                initialVerticesNx[currentIndex] = new Vector3(0 - res / 2, i - res / 2, j - res / 2);
                currentIndex++;
            }
        }

        PlanetQuadtree.SetNoiseLayers(noiseLayers);
        NoiseGenerator.SetNoiseLayers(noiseLayers);

        quadtreePy = new GameObject("MainTree for +Y");
        quadtreePy.transform.SetParent(transform);
        quadtreePy.AddComponent<PlanetQuadtree>();
        quadtreePy.GetComponent<PlanetQuadtree>().InitPlanetQuadtree(0, maxLevel, res, radius, FaceDirection.pY, initialVerticesPy);
        quadtreePy.GetComponent<PlanetQuadtree>().SetActive(true);

        quadtreePz = new GameObject("MainTree for +Z");
        quadtreePz.transform.SetParent(transform);
        quadtreePz.AddComponent<PlanetQuadtree>();
        quadtreePz.GetComponent<PlanetQuadtree>().InitPlanetQuadtree(0, maxLevel, res, radius, FaceDirection.pZ, initialVerticesPz);
        quadtreePz.GetComponent<PlanetQuadtree>().SetActive(true);

        quadtreePx = new GameObject("MainTree for +X");
        quadtreePx.transform.SetParent(transform);
        quadtreePx.AddComponent<PlanetQuadtree>();
        quadtreePx.GetComponent<PlanetQuadtree>().InitPlanetQuadtree(0, maxLevel, res, radius, FaceDirection.pX, initialVerticesPx);
        quadtreePx.GetComponent<PlanetQuadtree>().SetActive(true);

        quadtreeNy = new GameObject("MainTree for -Y");
        quadtreeNy.transform.SetParent(transform);
        quadtreeNy.AddComponent<PlanetQuadtree>();
        quadtreeNy.GetComponent<PlanetQuadtree>().InitPlanetQuadtree(0, maxLevel, res, radius, FaceDirection.nY, initialVerticesNy);
        quadtreeNy.GetComponent<PlanetQuadtree>().SetActive(true);

        quadtreeNz = new GameObject("MainTree for -Z");
        quadtreeNz.transform.SetParent(transform);
        quadtreeNz.AddComponent<PlanetQuadtree>();
        quadtreeNz.GetComponent<PlanetQuadtree>().InitPlanetQuadtree(0, maxLevel, res, radius, FaceDirection.nZ, initialVerticesNz);
        quadtreeNz.GetComponent<PlanetQuadtree>().SetActive(true);

        quadtreeNx = new GameObject("MainTree for -X");
        quadtreeNx.transform.SetParent(transform);
        quadtreeNx.AddComponent<PlanetQuadtree>();
        quadtreeNx.GetComponent<PlanetQuadtree>().InitPlanetQuadtree(0, maxLevel, res, radius, FaceDirection.nX, initialVerticesNx);
        quadtreeNx.GetComponent<PlanetQuadtree>().SetActive(true);

    }

    // Update is called once per frame
    void Update()
    {

        if (liveUpdate)
        {
            PlanetQuadtree.SetLiveUpdate(liveUpdate);
            PlanetQuadtree.SetNoiseLayers(noiseLayers);
            NoiseGenerator.SetNoiseLayers(noiseLayers);

        }

    }

    private void LateUpdate()
    {
        liveUpdate = false;
        PlanetQuadtree.SetLiveUpdate(liveUpdate);
    }

    public NoiseLayer[] GetNoiseLayers()
    {
        return noiseLayers;
    }



}
