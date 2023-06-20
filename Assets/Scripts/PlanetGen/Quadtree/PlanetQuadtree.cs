using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using System.IO;


public class PlanetQuadtree : MonoBehaviour
{
    private int level;
    private int maxLevel;
    private int res;
    private FaceDirection faceDirection;
    public Vector3 center;
    public Vector3 bottomLeft;
    public Vector3 bottomRight;
    public Vector3 topLeft;
    public Vector3 topRight;
    private Vector3[] vertices;
    private Vector3[] sphereVertices;
    public Vector3 centerPointOnSphere;
    private int[] tris;
    private GameObject[] children;
    private bool childrenCreated = false;
    public bool isActiveLOD = true;
    private bool isMeshActive = false;
    private Mesh mesh;
    private int radius;

    public bool doTop = false;
    public bool doBottom = false;
    public bool doLeft = false;
    public bool doRight = false;

    private Bounds meshBounds;
    private float boundsX;
    private float boundsY;
    private float boundsZ;

    public PlanetQuadtree[] neighbours;
    public PlanetQuadtree[] neighbourTest;

    private static NoiseLayer[] noiseLayers;

    public Texture2D mainTexture;
    private Vector2[] uv;

    private Material noiseMaterial;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;

    public bool saveImage = false;
    public static bool liveUpdate = false;
    public static bool canDoUpdate = false;

    public static bool liveUpdateTrigger = false;

    public void InitPlanetQuadtree(int level, int maxLevel, int res, int radius, FaceDirection faceDirection, Vector3[] vertices)
    {
        gameObject.layer = LayerMask.NameToLayer("PlanetTerrain");
        this.level = level;
        this.maxLevel = maxLevel;

        this.bottomLeft = vertices[0];
        this.bottomRight = vertices[res];
        this.topLeft = vertices[vertices.Length - res - 1];
        this.topRight = vertices[vertices.Length - 1];


        this.res = res;
        this.faceDirection = faceDirection;
        this.vertices = vertices;
        this.radius = radius;
        sphereVertices = ToSphere(vertices);


        centerPointOnSphere = sphereVertices[sphereVertices.Length / 2];

        // Calculate center depending on face
        switch (faceDirection)
        {
            case FaceDirection.pY:
                center = new Vector3((topRight.x + bottomLeft.x) / 2, res / 2, (topRight.z + bottomLeft.z) / 2);
                Vector3[] sphereBLandTR = { bottomLeft, topRight };
                break;

            case FaceDirection.pZ:
                center = new Vector3((topRight.x + bottomLeft.x) / 2, (topRight.y + bottomLeft.y) / 2, 0 - (res / 2));
                break;

            case FaceDirection.pX:
                center = new Vector3(0 - (res / 2), (topRight.y + bottomLeft.y) / 2, (topRight.z + bottomLeft.z) / 2);
                break;

            case FaceDirection.nY:
                center = new Vector3((topRight.x + bottomLeft.x) / 2, 0 - (res / 2), (topRight.z + bottomLeft.z) / 2);
                break;

            case FaceDirection.nZ:
                center = new Vector3((topRight.x + bottomLeft.x) / 2, (topRight.y + bottomLeft.y) / 2, (res / 2));
                break;

            case FaceDirection.nX:
                center = new Vector3((res / 2), (topRight.y + bottomLeft.y) / 2, (topRight.z + bottomLeft.z) / 2);
                break;

        }


    }

    private void Start()
    {
        meshFilter = gameObject.GetComponent<MeshFilter>();
        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
        meshCollider = gameObject.GetComponent<MeshCollider>();
        if (meshCollider == null)
        {
            meshCollider = gameObject.AddComponent<MeshCollider>();
        }

        ShowMesh();

        mesh.vertices = ApplyNoise(sphereVertices);
        mesh.RecalculateBounds();
        meshBounds = meshRenderer.bounds;
        boundsX = meshBounds.size.x;
        boundsY = meshBounds.size.y;
        boundsZ = meshBounds.size.z;

        meshCollider.sharedMesh = mesh;

    }




    private void Update()
    {
        if (!isMeshActive && isActiveLOD)
        {
            ShowMesh();
        }
        if (level < maxLevel + 1 && Vector3.Distance(Camera.main.transform.position, meshBounds.ClosestPoint(Camera.main.transform.position)) < boundsX + boundsY + boundsZ && !childrenCreated && !liveUpdate)   // More detail is needed as camera is closer
        {

            children = new GameObject[4];
            CreateChildren();

            childrenCreated = true;
            children[0].GetComponent<PlanetQuadtree>().SetActive(true);
            children[1].GetComponent<PlanetQuadtree>().SetActive(true);
            children[2].GetComponent<PlanetQuadtree>().SetActive(true);
            children[3].GetComponent<PlanetQuadtree>().SetActive(true);
            isActiveLOD = false;
            HideMesh();
        }
        else if (Vector3.Distance(Camera.main.transform.position, meshBounds.ClosestPoint(Camera.main.transform.position)) >= boundsX + boundsY + boundsZ && childrenCreated && !liveUpdate)
        {

            childrenCreated = false;
            ShowMesh();
            children[0].GetComponent<PlanetQuadtree>().SetActive(false);
            children[1].GetComponent<PlanetQuadtree>().SetActive(false);
            children[2].GetComponent<PlanetQuadtree>().SetActive(false);
            children[3].GetComponent<PlanetQuadtree>().SetActive(false);
            Destroy(children[0]);
            Destroy(children[1]);
            Destroy(children[2]);
            Destroy(children[3]);
            children = null;
            isActiveLOD = true;
        }

        if (isMeshActive && isActiveLOD && !liveUpdate)
        {

            PlanetQuadtree[] neighbourTrees = GetNeighbouringObjects();
            neighbourTest = neighbourTrees;
            if (!ComparePlanetQuadtreeArrays(neighbourTrees, neighbours))
            {

                mesh.vertices = sphereVertices;
                mesh.triangles = tris;
                neighbours = neighbourTrees;


                // Check if a crack in the terrain will form on any of the edges
                if (neighbours[0] != null)
                {
                    doTop = true;
                }
                else
                {
                    doTop = false;
                }

                if (neighbours[1] != null)
                {
                    doBottom = true;
                }
                else
                {
                    doBottom = false;
                }

                if (neighbours[2] != null)
                {
                    doLeft = true;
                }
                else
                {
                    doLeft = false;
                }

                if (neighbours[3] != null)
                {
                    doRight = true;
                }
                else
                {
                    doRight = false;
                }

            }

            if (doTop || doBottom || doLeft || doRight)
            {
                doTop = false;
                doBottom = false;
                doLeft = false;
                doRight = false;
                int[] tris = mesh.triangles;
                int[] newTris = FixTerrainCracks(neighbours, tris, res, sphereVertices.Length);
                mesh.triangles = newTris;
                
                mesh.RecalculateBounds();
                meshCollider.sharedMesh = mesh;

            }


        }


        //Debug
        if (saveImage)
        {
            saveImage = false;
            Texture2D textureToSave = (Texture2D)GetComponent<MeshRenderer>().material.mainTexture;
            File.WriteAllBytes(Application.dataPath + "/../SavedImages/" + "Image" + ".png", textureToSave.EncodeToPNG());

        }

        //Test and Debug
        if (liveUpdate && maxLevel == -1)
        {
            noiseLayers = GetComponentInParent<PlanetGenInit>().GetNoiseLayers();
            sphereVertices = ToSphere(vertices);
            mesh.vertices = ApplyNoise(sphereVertices);

        }

    }

    public bool ComparePlanetQuadtreeArrays(PlanetQuadtree[] array1, PlanetQuadtree[] array2)
    {
        if (array1 == null || array2 == null)
        {
            return false;
        }

        if (array1.Length != array2.Length)
        {
            return false;
        }

        for (int i = 0; i < array1.Length; i++)
        {
            if (array1[i] != array2[i])
            {
                return false;
            }
        }

        return true;
    }

    public int[] FixTerrainCracks(PlanetQuadtree[] neighbours, int[] tris, int res, int sphereVerticesLength)
    {
        // For each side where cracks need to be fixed, remove original tris, create new vertices that will fix cracks, draw tris using these vertices, consider how many sides need to be redrawn

        // Do top and bottom checks first
        // When cheickign left and right -> consider if top and bottom were done and change calculation based on this
        int[] currentTris = tris;
        int rowsRemoved = 0;
        int triRowsLeft;

        // If neighbour above -> remove entire top row of tris
        if (neighbours[0] != null) // Remove last row of tris -> tris per row = res*2 ->
        {
            // Remove last res*2 tris
            int[] newTris = new int[currentTris.Length - (res * 6)];
            System.Array.Copy(currentTris, 0, newTris, 0, currentTris.Length - (res * 6));
            currentTris = newTris;
            rowsRemoved++;
        }

        // if neighbour below -> remove entire bottom row of tris
        if (neighbours[1] != null)
        {
            int[] newTris = new int[currentTris.Length - (res * 6)];
            System.Array.Copy(currentTris, (res * 6), newTris, 0, currentTris.Length - (res * 6));

            currentTris = newTris;
            rowsRemoved++;
        }

        // IF neighbour right -> remove entire right row of tris that remain after previous
        if (neighbours[2] != null && 1 == 2) // Left
        {
            // Remove first 2 tris of each row


            // how many rows are left?
            triRowsLeft = (sphereVerticesLength / (res + 1)) - 1 - rowsRemoved;
            // WE know how many rows are left -> remove first 2 tris of each row for left, remove last 2 tris of each row for right
            int[] newTris = new int[currentTris.Length - (rowsRemoved * 6)];
            Debug.Log("CTlength: " + currentTris.Length + ", rrmd: " + (rowsRemoved * 6));
            int testLength = 0;
            for (int i = 0; i < triRowsLeft; i++) // For each row, remove first 2 tris
            {
                Debug.Log("Left debug: " + (res * 6 * i) + ", " + (res * 6) + ", ntl: " + newTris.Length);
                Debug.Log("TESTLENGTH: " + testLength + ", actualLength: " + newTris.Length);

                System.Array.Copy(currentTris, (res * 6 * i) + 6 - 1, newTris, (res * 6 * i), (res - 1) * 6);
                testLength += res * 6;


            }

            currentTris = newTris;

        }

        int trisRemovedFromLeft = 0;
        int leftTrisRemoved = 0;
        if (neighbours[2] != null)
        {
            // how many rows are left?
            triRowsLeft = (sphereVerticesLength / (res + 1)) - 1 - rowsRemoved;
            // WE know how many rows are left -> remove first 2 tris of each row for left, remove last 2 tris of each row for right
            int[] newTris = new int[currentTris.Length - (rowsRemoved * 6)];
            int rowCount = 0;



            for (int i = 0; i < triRowsLeft; i++)
            {
                System.Array.Copy(currentTris, (res * 6 * i) + 6, newTris, (res * 6 * i), (res - 1) * 6); 
                trisRemovedFromLeft++;
            }
            leftTrisRemoved = 1;
            currentTris = newTris;
        }

        // For right, similar to left but for last 2 tris of each row, index from first tri to tris-6, length = ((res - 1) * 6) - 6 if left tris were removed
        if (neighbours[3] != null)
        {
            triRowsLeft = (sphereVerticesLength / (res + 1)) - 1 - rowsRemoved; 
            int[] newTris = new int[currentTris.Length - (rowsRemoved * 6) - (trisRemovedFromLeft * 6)];

            for (int i = 0; i < triRowsLeft; i++)
            {
                System.Array.Copy(currentTris, (res * 6 * i), newTris, (res * 6 * i), (res - 1 - leftTrisRemoved) * 6); 
            }
            currentTris = newTris;

        }


        // Tri arrays of new tris to be added to currentTris
        int[] topTris;
        int[] bottomTris;
        int[] leftTris;
        int[] rightTris;

        int[] updatedTris = currentTris;

        int updateFromIndex = 0;


        // Next, draw new tris
        // First get existing verts and every other vert where crack occurs
        // Next draw tris as needed, depesinging on what sides have cracks

        // Cracks for even tris first
        List<int> dynamicTris = new List<int>();

        // Add results to currentTris array and then set mesh.triangles
        if (neighbours[0] != null) // Up
        {
            // Get indexes from sphere verts of final 2 rows
            int[] lowerDetailVerts = new int[(res / 2) + 1];
            int[] higherDetailVerts = new int[res + 1];
            int[] fixedTris = new int[(res / 2) * 3 * 6];

            // For each vertice on the final row

            for (int i = 0, currentIndex = 0; i < res + 1; i++) // Get indexes for lower detail verts
            {
                int index = ((res + 1) * res) + i;

                if (index % 2 == 0)
                {
                    lowerDetailVerts[currentIndex] = index;
                    currentIndex++;
                }

            }

            for (int i = 0; i < res + 1; i++)
            {
                int index = ((res + 1) * res) - res - 1 + i;
                higherDetailVerts[i] = index;
            }

            // Create tri array, draw cracksFix without corners first

            // New tris created = (res/2) * 3
            int count = 0;
            for (int i = 0; i < lowerDetailVerts.Length; i++)
            {

                if (i == 0) // Drawing from first vert -> 2 tris drawn, considers if left trsi have also been removed
                {
                    if (neighbours[2] == null) // Draw far left tri only if the left side does not also contain cracks 
                    {
                        dynamicTris.Add(lowerDetailVerts[i]);
                        dynamicTris.Add(higherDetailVerts[i + 1]);
                        dynamicTris.Add(higherDetailVerts[i]);
                    }

                    // Draw the second tri
                    dynamicTris.Add(lowerDetailVerts[i]);
                    dynamicTris.Add(lowerDetailVerts[i + 1]);
                    dynamicTris.Add(higherDetailVerts[i + 1]);

                }
                else if (i != lowerDetailVerts.Length - 1 && i != 0)
                {

                    // Tri 1
                    dynamicTris.Add(lowerDetailVerts[i]);
                    dynamicTris.Add(higherDetailVerts[i + 1 + count]);
                    dynamicTris.Add(higherDetailVerts[i + count]);

                    // Tri 2
                    dynamicTris.Add(lowerDetailVerts[i]);
                    dynamicTris.Add(higherDetailVerts[i + 2 + count]);
                    dynamicTris.Add(higherDetailVerts[i + 1 + count]);

                    // Tri 3
                    dynamicTris.Add(lowerDetailVerts[i]);
                    dynamicTris.Add(lowerDetailVerts[i + 1]);
                    dynamicTris.Add(higherDetailVerts[i + 2 + count]);

                    count++;
                }
                else if (i == lowerDetailVerts.Length - 1 && neighbours[3] == null) // Drawing from last vert
                {
                    dynamicTris.Add(lowerDetailVerts[i]);
                    dynamicTris.Add(higherDetailVerts[higherDetailVerts.Length - 1]);
                    dynamicTris.Add(higherDetailVerts[higherDetailVerts.Length - 2]);
                }
            }

            topTris = new int[dynamicTris.Count];
            for (int i = 0; i < dynamicTris.Count; i++)
            {
                topTris[i] = dynamicTris[i];
            }

            int[] newTriArray = new int[updatedTris.Length + topTris.Length];
            updatedTris.CopyTo(newTriArray, updateFromIndex);
            topTris.CopyTo(newTriArray, updatedTris.Length);

            updateFromIndex += topTris.Length;
            updatedTris = newTriArray;

        }

        if (neighbours[1] != null) // Down
        {
            // Get indexes from sphere verts of first 2 rows
            int[] lowerDetailVerts = new int[(res / 2) + 1];
            int[] higherDetailVerts = new int[res + 1];
            int[] fixedTris = new int[(res / 2) * 3 * 6];

            // For each vertice on the final row

            for (int i = 0, currentIndex = 0; i < res + 1; i++) // Get indexes for lower detail verts
            {
                int index = i;

                if (index % 2 == 0)
                {
                    lowerDetailVerts[currentIndex] = index;
                    currentIndex++;
                }

            }

            for (int i = 0; i < res + 1; i++)  // Get indexes for higher detail verts
            {
                int index = res + 1 + i;
                higherDetailVerts[i] = index;
            }

            // Create tri array, draw cracksFix without corners first

            // New tris created = (res/2) * 3
            int count = 0;
            for (int i = 0; i < lowerDetailVerts.Length; i++)
            {

                // Drawing tris occurs in reverse direction compared to topTris
                if (i == 0) // Drawing from first vert -> 2 tris drawn, considers if left trsi have also been removed
                {
                    if (neighbours[2] == null) // Draw far left tri only if the left side does not also contain cracks 
                    {
                        dynamicTris.Add(higherDetailVerts[i]);
                        dynamicTris.Add(higherDetailVerts[i + 1]);
                        dynamicTris.Add(lowerDetailVerts[i]);
                    }

                    // Draw the second tri
                    dynamicTris.Add(higherDetailVerts[i + 1]);
                    dynamicTris.Add(lowerDetailVerts[i + 1]);
                    dynamicTris.Add(lowerDetailVerts[i]);

                }
                else if (i != lowerDetailVerts.Length - 1 && i != 0)
                {

                    // Tri 1
                    dynamicTris.Add(higherDetailVerts[i + count]);
                    dynamicTris.Add(higherDetailVerts[i + 1 + count]);
                    dynamicTris.Add(lowerDetailVerts[i]);

                    // Tri 2
                    dynamicTris.Add(higherDetailVerts[i + 1 + count]);
                    dynamicTris.Add(higherDetailVerts[i + 2 + count]);
                    dynamicTris.Add(lowerDetailVerts[i]);


                    // Tri 3
                    dynamicTris.Add(higherDetailVerts[i + 2 + count]);
                    dynamicTris.Add(lowerDetailVerts[i + 1]);
                    dynamicTris.Add(lowerDetailVerts[i]);

                    count++;
                }
                else if (i == lowerDetailVerts.Length - 1 && neighbours[3] == null) // Drawing from last vert
                {
                    dynamicTris.Add(higherDetailVerts[higherDetailVerts.Length - 2]);
                    dynamicTris.Add(higherDetailVerts[higherDetailVerts.Length - 1]);
                    dynamicTris.Add(lowerDetailVerts[i]);
                }
            }

            bottomTris = new int[dynamicTris.Count];
            for (int i = 0; i < dynamicTris.Count; i++)
            {
                bottomTris[i] = dynamicTris[i];
            }
            int[] newTriArray = new int[updatedTris.Length + bottomTris.Length];
            updatedTris.CopyTo(newTriArray, updateFromIndex);
            bottomTris.CopyTo(newTriArray, updatedTris.Length);
            updateFromIndex += bottomTris.Length;
            updatedTris = newTriArray;


        }

        if (neighbours[2] != null) // Left
        {
            // Get indexes from sphere verts of first 2 rows
            int[] lowerDetailVerts = new int[(res / 2) + 1];
            int[] higherDetailVerts = new int[res + 1];
            int[] fixedTris = new int[(res / 2) * 3 * 6];

            // For each vertice on the final row

            for (int i = 0, currentIndex = 0; i < res + 1; i++) // Get indexes for lower detail verts
            {
                int index = i * (res + 1);

                if (index % ((res + 1) * 2) == 0)
                {
                    lowerDetailVerts[currentIndex] = index;
                    currentIndex++;

                }

            }

            for (int i = 0; i < res + 1; i++)  // Get indexes for higher detail verts
            {
                int index = i * (res + 1) + 1;
                higherDetailVerts[i] = index;
            }

            // Create tri array, draw cracksFix without corners first

            // New tris created = (res/2) * 3
            int count = 0;
            for (int i = 0; i < lowerDetailVerts.Length; i++)
            {

                if (i == 0) // Drawing from first vert -> 2 tris drawn, considers if left trsi have also been removed
                {
                    if (neighbours[1] == null) // Draw bottom tri only if the bottom side does not also contain cracks 
                    {
                        dynamicTris.Add(lowerDetailVerts[i]);
                        dynamicTris.Add(higherDetailVerts[i + 1]);
                        dynamicTris.Add(higherDetailVerts[i]);
                    }

                    // Draw the second tri
                    dynamicTris.Add(lowerDetailVerts[i]);
                    dynamicTris.Add(lowerDetailVerts[i + 1]);
                    dynamicTris.Add(higherDetailVerts[i + 1]);

                }
                else if (i != lowerDetailVerts.Length - 1 && i != 0)
                {

                    // Tri 1
                    dynamicTris.Add(lowerDetailVerts[i]);
                    dynamicTris.Add(higherDetailVerts[i + 1 + count]);
                    dynamicTris.Add(higherDetailVerts[i + count]);

                    // Tri 2
                    dynamicTris.Add(lowerDetailVerts[i]);
                    dynamicTris.Add(higherDetailVerts[i + 2 + count]);
                    dynamicTris.Add(higherDetailVerts[i + 1 + count]);

                    // Tri 3
                    dynamicTris.Add(lowerDetailVerts[i]);
                    dynamicTris.Add(lowerDetailVerts[i + 1]);
                    dynamicTris.Add(higherDetailVerts[i + 2 + count]);

                    count++;
                }
                else if (i == lowerDetailVerts.Length - 1 && neighbours[0] == null) // Drawing from last vert
                {
                    dynamicTris.Add(lowerDetailVerts[i]);
                    dynamicTris.Add(higherDetailVerts[higherDetailVerts.Length - 1]);
                    dynamicTris.Add(higherDetailVerts[higherDetailVerts.Length - 2]);
                }
            }

            leftTris = new int[dynamicTris.Count];
            for (int i = 0; i < dynamicTris.Count; i++)
            {
                leftTris[i] = dynamicTris[i];
            }
            int[] newTriArray = new int[updatedTris.Length + leftTris.Length];
            updatedTris.CopyTo(newTriArray, updateFromIndex);
            leftTris.CopyTo(newTriArray, updatedTris.Length);
            updateFromIndex += leftTris.Length;
            updatedTris = newTriArray;

        }

        if (neighbours[3] != null) // Right
        {
            // Get indexes from sphere verts of first 2 rows
            int[] lowerDetailVerts = new int[(res / 2) + 1];
            int[] higherDetailVerts = new int[res + 1];
            int[] fixedTris = new int[(res / 2) * 3 * 6];

            // For each vertice on the final row

            for (int i = 0, currentIndex = 0; i < res + 1; i++) // Get indexes for lower detail verts
            {
                int index = i * (res + 1) + res;

                if (index % 2 == 0)
                {
                    lowerDetailVerts[currentIndex] = index;
                    currentIndex++;
                }

            }

            for (int i = 0; i < res + 1; i++)  // Get indexes for higher detail verts
            {
                int index = i * (res + 1) + res - 1;
                higherDetailVerts[i] = index;
            }

            // Create tri array, draw cracksFix without corners first

            // New tris created = (res/2) * 3
            int count = 0;
            for (int i = 0; i < lowerDetailVerts.Length; i++)
            {

                // Drawing tris occurs in reverse direction compared to topTris
                if (i == 0) // Drawing from first vert -> 2 tris drawn, considers if left trsi have also been removed
                {
                    if (neighbours[1] == null) // Draw bottom tri only if bottomSide has no cracks 
                    {
                        dynamicTris.Add(higherDetailVerts[i]);
                        dynamicTris.Add(higherDetailVerts[i + 1]);
                        dynamicTris.Add(lowerDetailVerts[i]);
                    }

                    // Draw the second tri
                    dynamicTris.Add(higherDetailVerts[i + 1]);
                    dynamicTris.Add(lowerDetailVerts[i + 1]);
                    dynamicTris.Add(lowerDetailVerts[i]);

                }
                else if (i != lowerDetailVerts.Length - 1 && i != 0)
                {

                    // Tri 1
                    dynamicTris.Add(higherDetailVerts[i + count]);
                    dynamicTris.Add(higherDetailVerts[i + 1 + count]);
                    dynamicTris.Add(lowerDetailVerts[i]);

                    // Tri 2
                    dynamicTris.Add(higherDetailVerts[i + 1 + count]);
                    dynamicTris.Add(higherDetailVerts[i + 2 + count]);
                    dynamicTris.Add(lowerDetailVerts[i]);


                    // Tri 3
                    dynamicTris.Add(higherDetailVerts[i + 2 + count]);
                    dynamicTris.Add(lowerDetailVerts[i + 1]);
                    dynamicTris.Add(lowerDetailVerts[i]);

                    count++;
                }
                else if (i == lowerDetailVerts.Length - 1 && neighbours[0] == null) // Drawing from last vert
                {
                    dynamicTris.Add(higherDetailVerts[higherDetailVerts.Length - 2]);
                    dynamicTris.Add(higherDetailVerts[higherDetailVerts.Length - 1]);
                    dynamicTris.Add(lowerDetailVerts[i]);
                }
            }

            rightTris = new int[dynamicTris.Count];
            for (int i = 0; i < dynamicTris.Count; i++)
            {
                rightTris[i] = dynamicTris[i];
            }
            int[] newTriArray = new int[updatedTris.Length + rightTris.Length];
            updatedTris.CopyTo(newTriArray, updateFromIndex);
            rightTris.CopyTo(newTriArray, updatedTris.Length);
            updateFromIndex += rightTris.Length;
            updatedTris = newTriArray;
        }

        // Checking tris in updatedTris array
        List<int> validUpdatedTriangles = new List<int>();
        for (int i = 0; i < updatedTris.Length; i += 3)
        {
            bool isValidTri = true;
            for (int j = i + 3; j < updatedTris.Length; j += 3)
            {

                if (updatedTris[i] == updatedTris[j] && updatedTris[i + 1] == updatedTris[j + 1] && updatedTris[i + 2] == updatedTris[j + 2])
                {
                    isValidTri = false;
                }
                else if (updatedTris[i] == 0 && updatedTris[i + 1] == 0 && updatedTris[i + 2] == 0)
                {
                    isValidTri = false;
                }

            }
            if (isValidTri)
            {
                validUpdatedTriangles.Add(updatedTris[i]);
                validUpdatedTriangles.Add(updatedTris[i + 1]);
                validUpdatedTriangles.Add(updatedTris[i + 2]);
            }


        }
        updatedTris = validUpdatedTriangles.ToArray();

        return updatedTris;


    }


    public void SetMeshTriangles(int[] triangles)
    {
        mesh.triangles = triangles;
    }


    public PlanetQuadtree[] GetNeighbouringObjects()
    {
        Collider[] upCollider;
        Collider[] downCollider;
        Collider[] leftColldier;
        Collider[] rightCollider;


        PlanetQuadtree[] neighbours = new PlanetQuadtree[4];


        // Up
        Vector3 upCenter = sphereVertices[sphereVertices.Length - 1 - (res / 2)];
        upCollider = Physics.OverlapSphere(upCenter, 24 / Mathf.Pow(2, level));
        for (int i = 0; i < upCollider.Length; i++)
        {
            if (upCollider[i] != null)
            {
                PlanetQuadtree tree = upCollider[i].gameObject.GetComponent<PlanetQuadtree>();
                if (tree.GetLevel() == level - 1 && tree.isActiveLOD)
                {
                    neighbours[0] = tree;
                }
            }
        }


        // Down
        Vector3 downCenter = sphereVertices[(res / 2)];
        downCollider = Physics.OverlapSphere(downCenter, 24 / Mathf.Pow(2, level));
        for (int i = 0; i < downCollider.Length; i++)
        {
            if (downCollider[i] != null)
            {
                PlanetQuadtree tree = downCollider[i].gameObject.GetComponent<PlanetQuadtree>();
                if (tree.GetLevel() == level - 1 && tree.isActiveLOD)
                {
                    neighbours[1] = tree;
                }
            }
        }

        // Left
        Vector3 leftCenter = sphereVertices[(sphereVertices.Length - 1) / 2 - (res / 2)];
        leftColldier = Physics.OverlapSphere(leftCenter, 24 / Mathf.Pow(2, level));
        for (int i = 0; i < leftColldier.Length; i++)
        {
            if (leftColldier[i] != null)
            {
                PlanetQuadtree tree = leftColldier[i].gameObject.GetComponent<PlanetQuadtree>();
                if (tree.GetLevel() == level - 1 && tree.isActiveLOD)
                {
                    neighbours[2] = tree;
                }
            }
        }

        // Right
        Vector3 rightCenter = sphereVertices[(sphereVertices.Length - 1) / 2 + (res / 2)];
        rightCollider = Physics.OverlapSphere(rightCenter, 24 / Mathf.Pow(2, level));
        for (int i = 0; i < rightCollider.Length; i++)
        {
            if (rightCollider[i] != null)
            {
                PlanetQuadtree tree = rightCollider[i].gameObject.GetComponent<PlanetQuadtree>();
                if (tree.GetLevel() == level - 1 && tree.isActiveLOD)
                {
                    neighbours[3] = tree;
                }
            }
        }



        return neighbours;
    }

    private void CalculateUV()
    {
        uv = new Vector2[sphereVertices.Length];
        int index = 0;
        for (int i = 0; i < res + 1; i++)
        {
            for (int j = 0; j < res + 1; j++)
            {
                if (i == 0 && j == 0)
                {
                    uv[index] = new Vector2(0f, 0f);
                }
                else if (j == 0)
                {
                    uv[index] = new Vector2(0f, (float)i / (res + 1));
                }
                else if (i == 0)
                {
                    uv[index] = new Vector2((float)j / (res + 1), 0f);
                }
                else
                {
                    uv[index] = new Vector2((float)j / (res + 1), (float)i / (res + 1));
                }
                index++;
            }
        }
    }
    private void ShowMesh()
    {
        tris = DrawTris();

        meshRenderer.sharedMaterial = noiseMaterial;
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        meshFilter.mesh = mesh;
        CalculateUV();

        mesh.vertices = sphereVertices;

        mesh.triangles = tris;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.uv = uv;
        isMeshActive = true;

        meshCollider.sharedMesh = mesh;
        meshCollider.convex = false;

    }

    private void HideMesh()
    {
        mesh.Clear();
        isMeshActive = false;
    }

    public Vector3[] GetCenterOfSides()
    {
        Vector3 upCenter = sphereVertices[sphereVertices.Length - 1 - (res / 2)];
        Vector3 downCenter = sphereVertices[(res / 2)];
        Vector3 leftCenter = sphereVertices[(sphereVertices.Length - 1) / 2 - (res / 2)];
        Vector3 rightCenter = sphereVertices[(sphereVertices.Length - 1) / 2 + (res / 2)];

        return new Vector3[] { upCenter, downCenter, leftCenter, rightCenter };
    }

    private void CreateChildren() // Currently done for +Y face only
    {
        // Regenerate face of sphere, starting from cube vertives into sphere vertices
        Vector3 childBottomLeft;
        Vector3 childBottomRight;
        Vector3 childTopLeft;
        Vector3 childTopRight;
        Vector3[] childVertives;

        if (faceDirection == FaceDirection.pY)
        {
            // Bottom left - for +Y only
            childBottomLeft = bottomLeft;
            //childBottomRight = new Vector3(bottomRight.x - (res / (Mathf.Pow(2, level + 1))), bottomRight.y, bottomRight.z);
            //childTopLeft = new Vector3(bottomLeft.x, topLeft.y, topLeft.z - (res / (Mathf.Pow(2, level + 1))));
            //childTopRight = center;
            childVertives = CreateChildVertices(childBottomLeft);
            children[0] = new GameObject("ChildTree1");
            children[0].transform.SetParent(transform);
            children[0].AddComponent<PlanetQuadtree>();
            children[0].GetComponent<PlanetQuadtree>().InitPlanetQuadtree(level + 1, maxLevel, res, radius, FaceDirection.pY, childVertives);

            // Bottom Right - for +Y only
            childBottomLeft = new Vector3(bottomRight.x - (res / (Mathf.Pow(2, level + 1))), bottomRight.y, bottomRight.z);
            //childBottomRight = bottomRight;
            //childTopLeft = center;
            //childTopRight = new Vector3(topRight.x, topRight.y, topRight.z - (res / (Mathf.Pow(2, level + 1))));
            childVertives = CreateChildVertices(childBottomLeft);
            children[1] = new GameObject("ChildTree2");
            children[1].transform.SetParent(transform);
            children[1].AddComponent<PlanetQuadtree>();
            children[1].GetComponent<PlanetQuadtree>().InitPlanetQuadtree(level + 1, maxLevel, res, radius, FaceDirection.pY, childVertives);

            // Top Left - for +Y only
            childBottomLeft = new Vector3(topLeft.x, topLeft.y, topLeft.z - (res / (Mathf.Pow(2, level + 1))));
            //childBottomRight = center;
            //childTopLeft = topLeft;
            //childTopRight = new Vector3(topRight.x - (res / (Mathf.Pow(2, level + 1))), topRight.y, topRight.z);
            childVertives = CreateChildVertices(childBottomLeft);
            children[2] = new GameObject("ChildTree3");
            children[2].transform.SetParent(transform);
            children[2].AddComponent<PlanetQuadtree>();
            children[2].GetComponent<PlanetQuadtree>().InitPlanetQuadtree(level + 1, maxLevel, res, radius, FaceDirection.pY, childVertives);

            // TOP RIGHT - for +Y only
            childBottomLeft = center;
            //childBottomRight = new Vector3(topRight.x, topRight.y, topRight.z - (res / (Mathf.Pow(2, level + 1))));
            //childTopLeft = new Vector3(topRight.x - (res / (Mathf.Pow(2, level + 1))), topRight.y, topRight.z);
            //childTopRight = topRight;
            childVertives = CreateChildVertices(childBottomLeft);
            children[3] = new GameObject("ChildTree4");
            children[3].transform.SetParent(transform);
            children[3].AddComponent<PlanetQuadtree>();
            children[3].GetComponent<PlanetQuadtree>().InitPlanetQuadtree(level + 1, maxLevel, res, radius, FaceDirection.pY, childVertives);
        }

        else if (faceDirection == FaceDirection.pZ)
        {
            // Bottom left - for +Z only
            childBottomLeft = bottomLeft;
            // = new Vector3(bottomRight.x - (res / (Mathf.Pow(2, level + 1))), bottomRight.y, bottomRight.z);
            //childTopLeft = new Vector3(bottomLeft.x, topLeft.y - (res / (Mathf.Pow(2, level + 1))), topLeft.z);
            //childTopRight = center;
            childVertives = CreateChildVertices(childBottomLeft);
            children[0] = new GameObject("ChildTree1");
            children[0].transform.SetParent(transform);
            children[0].AddComponent<PlanetQuadtree>();
            children[0].GetComponent<PlanetQuadtree>().InitPlanetQuadtree(level + 1, maxLevel, res, radius, FaceDirection.pZ, childVertives);

            // Bottom Right - for +Z only
            childBottomLeft = new Vector3(bottomRight.x - (res / (Mathf.Pow(2, level + 1))), bottomRight.y, bottomRight.z);
            //childBottomRight = bottomRight;
            //childTopLeft = center;
            //childTopRight = new Vector3(topRight.x, topRight.y - (res / (Mathf.Pow(2, level + 1))), topRight.z);
            childVertives = CreateChildVertices(childBottomLeft);
            children[1] = new GameObject("ChildTree2");
            children[1].transform.SetParent(transform);
            children[1].AddComponent<PlanetQuadtree>();
            children[1].GetComponent<PlanetQuadtree>().InitPlanetQuadtree(level + 1, maxLevel, res, radius, FaceDirection.pZ, childVertives);

            // Top Left - for +Z only
            childBottomLeft = new Vector3(topLeft.x, topLeft.y - (res / (Mathf.Pow(2, level + 1))), topLeft.z);
            //childBottomRight = center;
            //childTopLeft = topLeft;
            //childTopRight = new Vector3(topRight.x - (res / (Mathf.Pow(2, level + 1))), topRight.y, topRight.z);
            childVertives = CreateChildVertices(childBottomLeft);
            children[2] = new GameObject("ChildTree3");
            children[2].transform.SetParent(transform);
            children[2].AddComponent<PlanetQuadtree>();
            children[2].GetComponent<PlanetQuadtree>().InitPlanetQuadtree(level + 1, maxLevel, res, radius, FaceDirection.pZ, childVertives);

            // TOP RIGHT - for +Z only
            childBottomLeft = center;
            //childBottomRight = new Vector3(topRight.x, topRight.y - (res / (Mathf.Pow(2, level + 1))), topRight.z);
            //childTopLeft = new Vector3(topRight.x - (res / (Mathf.Pow(2, level + 1))), topRight.y, topRight.z);
            //childTopRight = topRight;
            childVertives = CreateChildVertices(childBottomLeft);
            children[3] = new GameObject("ChildTree4");
            children[3].transform.SetParent(transform);
            children[3].AddComponent<PlanetQuadtree>();
            children[3].GetComponent<PlanetQuadtree>().InitPlanetQuadtree(level + 1, maxLevel, res, radius, FaceDirection.pZ, childVertives);
        }

        else if (faceDirection == FaceDirection.pX)
        {
            // Bottom left - for +Z only
            childBottomLeft = bottomLeft;
            //childBottomRight = new Vector3(bottomRight.x, bottomRight.y, bottomRight.z - (res / (Mathf.Pow(2, level + 1))));
            //childTopLeft = new Vector3(bottomLeft.x, topLeft.y - (res / (Mathf.Pow(2, level + 1))), topLeft.z);
            //childTopRight = center;
            childVertives = CreateChildVertices(childBottomLeft);
            children[0] = new GameObject("ChildTree1");
            children[0].transform.SetParent(transform);
            children[0].AddComponent<PlanetQuadtree>();
            children[0].GetComponent<PlanetQuadtree>().InitPlanetQuadtree(level + 1, maxLevel, res, radius, FaceDirection.pX, childVertives);

            // Bottom Right - for +Z only
            childBottomLeft = new Vector3(bottomRight.x, bottomRight.y, bottomRight.z - (res / (Mathf.Pow(2, level + 1))));
            //childBottomRight = bottomRight;
            //childTopLeft = center;
            //childTopRight = new Vector3(topRight.x, topRight.y - (res / (Mathf.Pow(2, level + 1))), topRight.z);
            childVertives = CreateChildVertices(childBottomLeft);
            children[1] = new GameObject("ChildTree2");
            children[1].transform.SetParent(transform);
            children[1].AddComponent<PlanetQuadtree>();
            children[1].GetComponent<PlanetQuadtree>().InitPlanetQuadtree(level + 1, maxLevel, res, radius, FaceDirection.pX, childVertives);

            // Top Left - for +Z only
            childBottomLeft = new Vector3(topLeft.x, topLeft.y - (res / (Mathf.Pow(2, level + 1))), topLeft.z);
            //childBottomRight = center;
            //childTopLeft = topLeft;
            //childTopRight = new Vector3(topRight.x, topRight.y, topRight.z - (res / (Mathf.Pow(2, level + 1))));
            childVertives = CreateChildVertices(childBottomLeft);
            children[2] = new GameObject("ChildTree3");
            children[2].transform.SetParent(transform);
            children[2].AddComponent<PlanetQuadtree>();
            children[2].GetComponent<PlanetQuadtree>().InitPlanetQuadtree(level + 1, maxLevel, res, radius, FaceDirection.pX, childVertives);

            // TOP RIGHT - for +Z only
            childBottomLeft = center;
            //childBottomRight = new Vector3(topRight.x, topRight.y - (res / (Mathf.Pow(2, level + 1))), topRight.z);
            //childTopLeft = new Vector3(topRight.x, topRight.y, topRight.z - (res / (Mathf.Pow(2, level + 1))));
            //childTopRight = topRight;
            childVertives = CreateChildVertices(childBottomLeft);
            children[3] = new GameObject("ChildTree4");
            children[3].transform.SetParent(transform);
            children[3].AddComponent<PlanetQuadtree>();
            children[3].GetComponent<PlanetQuadtree>().InitPlanetQuadtree(level + 1, maxLevel, res, radius, FaceDirection.pX, childVertives);
        }

        else if (faceDirection == FaceDirection.nY)
        {
            // Bottom left - for -Y only
            childBottomLeft = bottomLeft; // Z from bottmLeft is different from what it should be -> problem in childVertice creation
            //childBottomRight = new Vector3(bottomRight.x - (res / (Mathf.Pow(2, level + 1))), bottomRight.y, bottomRight.z);
            //childTopLeft = new Vector3(bottomLeft.x, topLeft.y, topLeft.z);
            //childTopRight = center;
            childVertives = CreateChildVertices(childBottomLeft);
            children[0] = new GameObject("ChildTree1");
            children[0].transform.SetParent(transform);
            children[0].AddComponent<PlanetQuadtree>();
            children[0].GetComponent<PlanetQuadtree>().InitPlanetQuadtree(level + 1, maxLevel, res, radius, FaceDirection.nY, childVertives);

            // Bottom Right - for -Y only
            childBottomLeft = new Vector3(bottomRight.x - (res / (Mathf.Pow(2, level + 1))), bottomRight.y, bottomRight.z);
            //childBottomRight = bottomRight;
            //childTopLeft = center;
            //childTopRight = new Vector3(topRight.x, topRight.y, topRight.z - (res / (Mathf.Pow(2, level + 1))));
            childVertives = CreateChildVertices(childBottomLeft);
            children[1] = new GameObject("ChildTree2");
            children[1].transform.SetParent(transform);
            children[1].AddComponent<PlanetQuadtree>();
            children[1].GetComponent<PlanetQuadtree>().InitPlanetQuadtree(level + 1, maxLevel, res, radius, FaceDirection.nY, childVertives);

            // Top Left - for -Y only
            childBottomLeft = new Vector3(topLeft.x, topLeft.y, topLeft.z + (res / (Mathf.Pow(2, level + 1))));  // Problem with BottomLeft not calculated correctly: - turned to +
            //childBottomRight = center;
            //childTopLeft = topLeft;
            //childTopRight = new Vector3(topRight.x - (res / (Mathf.Pow(2, level + 1))), topRight.y, topRight.z);
            childVertives = CreateChildVertices(childBottomLeft);
            children[2] = new GameObject("ChildTree3");
            children[2].transform.SetParent(transform);
            children[2].AddComponent<PlanetQuadtree>();
            children[2].GetComponent<PlanetQuadtree>().InitPlanetQuadtree(level + 1, maxLevel, res, radius, FaceDirection.nY, childVertives);

            // TOP RIGHT - for -Y only
            childBottomLeft = center;
            //childBottomRight = new Vector3(topRight.x, topRight.y, topRight.z - (res / (Mathf.Pow(2, level + 1))));
            //childTopLeft = new Vector3(topRight.x - (res / (Mathf.Pow(2, level + 1))), topRight.y, topRight.z);
            //childTopRight = topRight;
            childVertives = CreateChildVertices(childBottomLeft);
            children[3] = new GameObject("ChildTree4");
            children[3].transform.SetParent(transform);
            children[3].AddComponent<PlanetQuadtree>();
            children[3].GetComponent<PlanetQuadtree>().InitPlanetQuadtree(level + 1, maxLevel, res, radius, FaceDirection.nY, childVertives);

        }

        else if (faceDirection == FaceDirection.nZ)
        {
            // Bottom left - for -Z only
            childBottomLeft = bottomLeft;
            //childBottomRight = new Vector3(bottomRight.x - (res / (Mathf.Pow(2, level + 1))), bottomRight.y, bottomRight.z);
            //childTopLeft = new Vector3(bottomLeft.x, topLeft.y - (res / (Mathf.Pow(2, level + 1))), topLeft.z);
            //childTopRight = center;
            childVertives = CreateChildVertices(childBottomLeft);
            children[0] = new GameObject("ChildTree1");
            children[0].transform.SetParent(transform);
            children[0].AddComponent<PlanetQuadtree>();
            children[0].GetComponent<PlanetQuadtree>().InitPlanetQuadtree(level + 1, maxLevel, res, radius, FaceDirection.nZ, childVertives);

            // Bottom Right - for -Z only
            childBottomLeft = new Vector3(bottomRight.x + (res / (Mathf.Pow(2, level + 1))), bottomRight.y, bottomRight.z);
            //childBottomRight = bottomRight;
            //childTopLeft = center;
            //childTopRight = new Vector3(topRight.x, topRight.y - (res / (Mathf.Pow(2, level + 1))), topRight.z);
            childVertives = CreateChildVertices(childBottomLeft);
            children[1] = new GameObject("ChildTree2");
            children[1].transform.SetParent(transform);
            children[1].AddComponent<PlanetQuadtree>();
            children[1].GetComponent<PlanetQuadtree>().InitPlanetQuadtree(level + 1, maxLevel, res, radius, FaceDirection.nZ, childVertives);

            // Top Left - for -Z only
            childBottomLeft = new Vector3(topLeft.x, topLeft.y - (res / (Mathf.Pow(2, level + 1))), topLeft.z);
            //childBottomRight = center;
            //childTopLeft = topLeft;
            //childTopRight = new Vector3(topRight.x - (res / (Mathf.Pow(2, level + 1))), topRight.y, topRight.z);
            childVertives = CreateChildVertices(childBottomLeft);
            children[2] = new GameObject("ChildTree3");
            children[2].transform.SetParent(transform);
            children[2].AddComponent<PlanetQuadtree>();
            children[2].GetComponent<PlanetQuadtree>().InitPlanetQuadtree(level + 1, maxLevel, res, radius, FaceDirection.nZ, childVertives);

            // TOP RIGHT - for -Z only
            childBottomLeft = center;
            //childBottomRight = new Vector3(topRight.x, topRight.y - (res / (Mathf.Pow(2, level + 1))), topRight.z);
            //childTopLeft = new Vector3(topRight.x - (res / (Mathf.Pow(2, level + 1))), topRight.y, topRight.z);
            //childTopRight = topRight;
            childVertives = CreateChildVertices(childBottomLeft);
            children[3] = new GameObject("ChildTree4");
            children[3].transform.SetParent(transform);
            children[3].AddComponent<PlanetQuadtree>();
            children[3].GetComponent<PlanetQuadtree>().InitPlanetQuadtree(level + 1, maxLevel, res, radius, FaceDirection.nZ, childVertives);
        }

        else if (faceDirection == FaceDirection.nX)
        {
            // Bottom left - for -Z only
            childBottomLeft = bottomLeft;
            //childBottomRight = new Vector3(bottomRight.x, bottomRight.y, bottomRight.z - (res / (Mathf.Pow(2, level + 1))));
            //childTopLeft = new Vector3(bottomLeft.x, topLeft.y - (res / (Mathf.Pow(2, level + 1))), topLeft.z);
            //childTopRight = center;
            childVertives = CreateChildVertices(childBottomLeft);
            children[0] = new GameObject("ChildTree1");
            children[0].transform.SetParent(transform);
            children[0].AddComponent<PlanetQuadtree>();
            children[0].GetComponent<PlanetQuadtree>().InitPlanetQuadtree(level + 1, maxLevel, res, radius, FaceDirection.nX, childVertives);

            // Bottom Right - for -Z only
            childBottomLeft = new Vector3(bottomRight.x, bottomRight.y, bottomRight.z + (res / (Mathf.Pow(2, level + 1)))); // Changed - sing to +
            //childBottomRight = bottomRight;
            //childTopLeft = center;
            //childTopRight = new Vector3(topRight.x, topRight.y - (res / (Mathf.Pow(2, level + 1))), topRight.z);
            childVertives = CreateChildVertices(childBottomLeft);
            children[1] = new GameObject("ChildTree2");
            children[1].transform.SetParent(transform);
            children[1].AddComponent<PlanetQuadtree>();
            children[1].GetComponent<PlanetQuadtree>().InitPlanetQuadtree(level + 1, maxLevel, res, radius, FaceDirection.nX, childVertives);

            // Top Left - for -Z only
            childBottomLeft = new Vector3(topLeft.x, topLeft.y - (res / (Mathf.Pow(2, level + 1))), topLeft.z);
            //childBottomRight = center;
            //childTopLeft = topLeft;
            //childTopRight = new Vector3(topRight.x, topRight.y, topRight.z - (res / (Mathf.Pow(2, level + 1))));
            childVertives = CreateChildVertices(childBottomLeft);
            children[2] = new GameObject("ChildTree3");
            children[2].transform.SetParent(transform);
            children[2].AddComponent<PlanetQuadtree>();
            children[2].GetComponent<PlanetQuadtree>().InitPlanetQuadtree(level + 1, maxLevel, res, radius, FaceDirection.nX, childVertives);

            // TOP RIGHT - for -Z only
            childBottomLeft = center;
            //childBottomRight = new Vector3(topRight.x, topRight.y - (res / (Mathf.Pow(2, level + 1))), topRight.z);
            //childTopLeft = new Vector3(topRight.x, topRight.y, topRight.z - (res / (Mathf.Pow(2, level + 1))));
            //childTopRight = topRight;
            childVertives = CreateChildVertices(childBottomLeft);
            children[3] = new GameObject("ChildTree4");
            children[3].transform.SetParent(transform);
            children[3].AddComponent<PlanetQuadtree>();
            children[3].GetComponent<PlanetQuadtree>().InitPlanetQuadtree(level + 1, maxLevel, res, radius, FaceDirection.nX, childVertives);
        }

    }

    private Vector3[] CreateChildVertices(Vector3 childBottomLeft)
    {
        Vector3[] childVertices = new Vector3[((res + 1) * (res + 1))];
        float chunkSpace = res / (Mathf.Pow(2, level + 1));
        float interval = chunkSpace / res;

        if (faceDirection == FaceDirection.pY)
        {
            // Positive Y (Top of Cube) - Origin = 0-res/2,0+res/2,0-res/2
            childVertices = new Vector3[((res + 1) * (res + 1))];
            int currentIndex = 0;
            for (float i = 0; i <= chunkSpace; i += interval) //i for z axis
            {
                for (float j = 0; j <= chunkSpace; j += interval) //j for x axis
                {
                    childVertices[currentIndex] = new Vector3(j + childBottomLeft.x, res / 2, i + childBottomLeft.z);
                    currentIndex++;
                }
            }
        }

        else if (faceDirection == FaceDirection.pZ)
        {
            childVertices = new Vector3[((res + 1) * (res + 1))];
            int currentIndex = 0;
            for (float i = 0; i <= chunkSpace; i += interval) //i for z axis
            {
                for (float j = 0; j <= chunkSpace; j += interval) //j for x axis
                {
                    childVertices[currentIndex] = new Vector3(j + childBottomLeft.x, i + childBottomLeft.y, 0 - (res / 2));
                    currentIndex++;
                }
            }
        }

        else if (faceDirection == FaceDirection.pX)
        {
            childVertices = new Vector3[((res + 1) * (res + 1))];
            int currentIndex = 0;
            for (float i = 0; i <= chunkSpace; i += interval) //i for z axis
            {
                for (float j = 0; j <= chunkSpace; j += interval) //j for x axis
                {
                    childVertices[currentIndex] = new Vector3((res / 2), i + childBottomLeft.y, j + childBottomLeft.z);
                    currentIndex++;
                }
            }
        }

        else if (faceDirection == FaceDirection.nY)
        {
            childVertices = new Vector3[((res + 1) * (res + 1))];
            int currentIndex = 0;
            for (float i = 0; i <= chunkSpace; i += interval) //i for z axis
            {
                for (float j = 0; j <= chunkSpace; j += interval) //j for x axis
                {
                    childVertices[currentIndex] = new Vector3(j + childBottomLeft.x, 0 - res / 2, childBottomLeft.z - i); // Problem when top right is done? z problem
                    currentIndex++;
                }
            }

        }

        else if (faceDirection == FaceDirection.nZ)
        {
            childVertices = new Vector3[((res + 1) * (res + 1))];
            int currentIndex = 0;
            for (float i = 0; i <= chunkSpace; i += interval) //i for z axis
            {
                for (float j = 0; j <= chunkSpace; j += interval) //j for x axis
                {
                    childVertices[currentIndex] = new Vector3(childBottomLeft.x - j, i + childBottomLeft.y, res / 2);
                    currentIndex++;
                }
            }
        }

        else if (faceDirection == FaceDirection.nX)
        {
            childVertices = new Vector3[((res + 1) * (res + 1))];
            int currentIndex = 0;
            for (float i = 0; i <= chunkSpace; i += interval) //i for z axis
            {
                for (float j = 0; j <= chunkSpace; j += interval) //j for x axis
                {
                    childVertices[currentIndex] = new Vector3(0 - (res / 2), i + childBottomLeft.y, childBottomLeft.z - j);
                    currentIndex++;
                }
            }
        }

        return childVertices;
    }

    private Vector3[] ToSphere(Vector3[] verts) 
    {
        Vector3[] sphereVerts = new Vector3[verts.Length];
        int currentIndex = 0;


        for (int i = 0; i < verts.Length; i++)
        {
            Vector3 v = verts[currentIndex] * 2f / res;
            float x2 = v.x * v.x;
            float y2 = v.y * v.y;
            float z2 = v.z * v.z;


            float newX = v.x * Mathf.Sqrt((1f - y2 / 2f - z2 / 2f + y2 * z2 / 3f));
            float newY = v.y * Mathf.Sqrt((1f - x2 / 2f - z2 / 2f + x2 * z2 / 3f));
            float newZ = v.z * Mathf.Sqrt((1f - x2 / 2f - y2 / 2f + x2 * y2 / 3f));

            sphereVerts[currentIndex] = new Vector3(newX, newY, newZ);
            currentIndex++;
        }

        return sphereVerts;
    }

    private int[] DrawTris()
    {

        int[] tris = new int[(res * res) * 6];

        int i = 0;
        int trisGenerated = 0;
        for (int j = 0; j < ((res * res) * 2); j++) // for each point along x axis * 2
        {

            if (faceDirection == FaceDirection.pY || faceDirection == FaceDirection.pZ || faceDirection == FaceDirection.pX || 1 == 1)
            {
                if (j % 2 == 0)
                {
                    tris[(j * 3)] = res + (j / 2) + i + 1;
                    tris[(j * 3) + 1] = (j / 2) + 1 + i;
                    tris[(j * 3) + 2] = (j / 2) + i;

                }
                else
                {
                    tris[(j * 3)] = (j / 2) + 1 + i;
                    tris[(j * 3) + 1] = res + (j / 2) + i + 1;
                    tris[(j * 3) + 2] = res + (j / 2) + i + 2;

                }
            }
            else
            {
                if (j % 2 == 0)
                {
                    tris[(j * 3) + 2] = res + (j / 2) + i + 1;
                    tris[(j * 3) + 1] = (j / 2) + 1 + i;
                    tris[(j * 3)] = (j / 2) + i;

                }
                else
                {
                    tris[(j * 3) + 2] = (j / 2) + 1 + i;
                    tris[(j * 3) + 1] = res + (j / 2) + i + 1;
                    tris[(j * 3)] = res + (j / 2) + i + 2;

                }
            }


            trisGenerated++;
            if (trisGenerated == res * 2)
            {
                trisGenerated = 0;
                i++;
            }

        }
        return tris;
    }

    private Vector3[] ApplyNoise(Vector3[] sphereVertices)
    {
        float[] noiseValues = new float[sphereVertices.Length];
        Texture2D noiseTexture = new Texture2D(res + 1, res + 1);
        Color[] pixels = new Color[noiseTexture.width * noiseTexture.height];
        int pixelIndex = 0;

        for (int i = 0; i < sphereVertices.Length; i++)
        {
            float noiseValue = 0f;

            // The base layer from which other layers build upon
            float baseLayerValue = NoiseGenerator.CalculateNoiseAtPoint(sphereVertices[i], 0);
            noiseValue = baseLayerValue;
            for(int j = 1; j < noiseLayers.Length; j++)
            {
                noiseValue += NoiseGenerator.CalculateNoiseAtPoint(sphereVertices[i], j) * baseLayerValue;
            }
            noiseValues[i] = noiseValue;

            sphereVertices[i] = (1 + noiseValue) * radius * sphereVertices[i];

        }
        pixels = GenerateTexture(noiseValues);
        noiseTexture.SetPixels(pixels);
        noiseTexture.Apply();

        mainTexture = noiseTexture;
        mainTexture.wrapMode = TextureWrapMode.Clamp;
        noiseTexture.wrapMode = TextureWrapMode.Clamp;

        Material material = new Material(Shader.Find("Unlit/Texture"));
        material.mainTexture = noiseTexture; 

        GetComponent<MeshRenderer>().material = material;
        noiseMaterial = material;

        return sphereVertices;

    }

    private Color[] GenerateTexture(float[] noiseValues)
    {
        Color[] pixels = new Color[(res + 1) * (res + 1)];

        for (int i = 0; i < noiseValues.Length; i++)
        {
            if(noiseValues[i] == 0)
            {
                pixels[i] = Color.blue;
            }
            else if(noiseValues[i] > 0 && noiseValues[i] <= 0.002)
            {
                pixels[i] = Color.yellow;
            }
            else if (noiseValues[i] > 0.001 && noiseValues[i] <= 0.05)
            {
                pixels[i] = Color.green;
            }
            else if (noiseValues[i] > 0.05 && noiseValues[i] <= 0.055)
            {
                pixels[i] = new Color32(0, 240, 0, 1);
            }
            else if (noiseValues[i] > 0.055 && noiseValues[i] <= 0.1)
            {
                pixels[i] = new Color32(0, 210, 0, 1);
            }
            else if (noiseValues[i] > 0.1 && noiseValues[i] <= 0.13)
            {
                pixels[i] = Color.gray;
            }
            else if (noiseValues[i] > 0.13 && noiseValues[i] <= 1)
            {
                pixels[i] = Color.white;
            }
        }
        return pixels;
    }

    public Vector3[] GetVertices()
    {
        return vertices;
    }

    public Vector3[] GetSphereVertices()
    {
        return sphereVertices;
    }

    public Vector3 GetCenterPointOnSphere()
    {
        return centerPointOnSphere;
    }

    public void SetActive(bool isActiveLOD)
    {
        this.isActiveLOD = isActiveLOD;
    }

    public bool IsActiveLOD()
    {
        return isActiveLOD;
    }

    public PlanetQuadtree GetChild(int index)
    {
        return children[index].GetComponent<PlanetQuadtree>();
    }

    public int GetLevel()
    {
        return level;
    }

    public PlanetQuadtree[] GetPreviousNeighbourTrees()
    {
        return neighbours;
    }

    public void UpdateNeighboursTree(PlanetQuadtree[] newNeighbours)
    {
        neighbours = newNeighbours;
    }

    public int GetRes()
    {
        return res;
    }

    public int GetRadius()
    {
        return radius;
    }

    public static void SetNoiseLayers(NoiseLayer[] newNoiseLayers)
    {
        noiseLayers = newNoiseLayers;
    }

    public static void SetLiveUpdate(bool newBool)
    {
        liveUpdate = newBool;
    }

    public static void SetCanDoUpdate(bool newBool)
    {
        canDoUpdate = newBool;
    }



}
