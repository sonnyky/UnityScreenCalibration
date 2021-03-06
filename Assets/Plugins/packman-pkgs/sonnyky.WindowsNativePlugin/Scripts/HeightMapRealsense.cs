﻿using UnityEngine;
using System.Collections;
using System;

public class HeightMapRealsense : MonoBehaviour
{
    public int depthHeight = 512;
    public int depthWidth = 512;
    private bool generate = false;
    private short[] depthData;
    private RealsenseSandparty camInterface;

    [Header("Unity terrain")]
    public int terrainWidth;
    public int terrainHeight;
    private MeshFilter mf;
    private MeshRenderer mr;
    private Mesh mesh;
    private Terrain m_terrain;
    private float[,] m_heightValues, prev_heightValues;
    private bool initFlag = false;

    public float minDepth = 1800;
    public float maxDepth = 2100;

    private WaitForSeconds terrainUpdateDelay;
    private Vector3 thisPixelPos;
    int x, y;

    private IntPtr depthDataPointer;

    // Use this for initialization
    void Start()
    {
        x = y = 0;

        camInterface = GameObject.Find("CameraInterface").GetComponent<RealsenseSandparty>();
        terrainUpdateDelay = new WaitForSeconds(1f);

        m_terrain = (Terrain)gameObject.GetComponent<Terrain>();
        terrainWidth = m_terrain.terrainData.heightmapWidth;
        terrainHeight = m_terrain.terrainData.heightmapHeight;

        // Create the game object containing the renderer
        mf = gameObject.AddComponent(typeof(MeshFilter)) as MeshFilter;
        mr = gameObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;

        // Retrieve a mesh instance
        mesh = gameObject.GetComponent<MeshFilter>().mesh;

        m_heightValues = new float[terrainWidth, terrainHeight];
        prev_heightValues = new float[terrainWidth, terrainHeight];
    }

    // Update is called once per frame
    void Update()
    {
        if (generate)
        {
            GenerateHeightmap();
        }
    }

    void TestHeightMap()
    {
        int y = 0;
        int x = 0;
        for (y = 0; y < terrainHeight; y++)
        {
            for (x = 0; x < terrainWidth; x++)
            {
                m_heightValues[x, y] = UnityEngine.Random.Range(0, 30f) * .5f;
            }
        }
        m_terrain.terrainData.SetHeights(0, 0, m_heightValues);
    }

    void GenerateHeightmap()
    {
        camInterface.GetDepthData();
        camInterface.GetReturnedDepthData(ref depthData);
        for (y = 0; y < 480; y++)
        {
            for (x = 384; x < 896; x++)
           {

                float heightPercentage = 0;
                int depthIndex = ((y) * 1280) + (x);
                float depth = 0;

                depth = depthData[depthIndex];
                heightPercentage = FilterDepthValueToPercent(depth);
                m_heightValues[x - 384, y] = heightPercentage;
            }
        }
        m_terrain.terrainData.SetHeights(0, 0, m_heightValues);
    }

    private float FilterDepthValueToPercent(float inputDepth)
    {
        float outputDepth = 0;

        if (inputDepth < minDepth)
        {
            return 1;
        }
        else if (inputDepth > maxDepth)
        {
            return 0;
        }

        outputDepth = 1 - ((inputDepth - minDepth) / (maxDepth - minDepth));
        return outputDepth;
    }

    IEnumerator UpdateHeights()
    {
        while (true)
        {
            yield return terrainUpdateDelay;
            m_terrain.terrainData.SetHeights(0, 0, m_heightValues);
        }
    }

    public void StartGenerate()
    {
        generate = true;
    }

    public void StopGenerate()
    {
        generate = false;
    }

}