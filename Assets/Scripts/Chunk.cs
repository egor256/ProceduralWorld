using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public GameObject treePrefab;

    public int chunkX;
    public int chunkZ;

    public static int sizeX = 64;
    public static int sizeZ = 64;
    public static int scaleX = 4;
    public static int scaleZ = 4;

    private float heightMultiplier = 128;

    private Vector3[] vertices;
    private int[] triangles;
    private Color[] colors;

    private static int Hash32shift(int key)
    {
        key = ~key + (key << 15);
        key = key ^ (key >> 12);
        key = key + (key << 2);
        key = key ^ (key >> 4);
        key = key * 2057;
        key = key ^ (key >> 16);
        return key;
    }

    private static float GenerateWhiteNoise(int x, int y)
    {
        return Mathf.Abs(Hash32shift(x ^ Hash32shift(y))) / (float) int.MaxValue; // [0.0f - 1.0f]
    }

    private static float Interpolate(float x0, float x1, float alpha)
    {
        return x0 * (1 - alpha) + alpha * x1;
    }

    private static float GenerateSmoothNoise(float x, float y)
    {
        int intX = (int) Mathf.Floor(x);
        float horizontalBlend = x - intX;
        int intY = (int) Mathf.Floor(y);
        float verticalBlend = y - intY;
        float top = Interpolate(GenerateWhiteNoise(intX, intY), GenerateWhiteNoise(intX + 1, intY), horizontalBlend);
        float bottom = Interpolate(GenerateWhiteNoise(intX, intY + 1), GenerateWhiteNoise(intX + 1, intY + 1), horizontalBlend);
        return Interpolate(top, bottom, verticalBlend);
    }

    private static float GeneratePerlinNoise(float x, float y, int octaveCount)
    {
        float persistance = 0.2f;
        float amplitude = 1.0f;
        float totalAmplitude = 0.0f;
        float perlinNoise = 0.0f;
        for (int i = 0; i < octaveCount; i++)
        {
            amplitude *= persistance;
            totalAmplitude += amplitude;
            float m = Mathf.Pow(2, i - 4);
            perlinNoise += GenerateSmoothNoise(x * m, y * m) * amplitude;
        }
        return perlinNoise / totalAmplitude;
    }

    private static Gradient MakeGradient(Color32 color1, Color32 color2)
    {
        Gradient gradient = new Gradient();
        GradientColorKey[] colorKey = new GradientColorKey[2];
        colorKey[0].color = color1;
        colorKey[0].time = 0.35f;
        colorKey[1].color = color2;
        colorKey[1].time = 0.65f;
        GradientAlphaKey[] alphaKey = new GradientAlphaKey[2];
        alphaKey[0].alpha = 1.0f;
        alphaKey[0].time = 0.0f;
        alphaKey[1].alpha = 0.0f;
        alphaKey[1].time = 1.0f;
        gradient.SetKeys(colorKey, alphaKey);
        return gradient;
    }

    private void MakeVertices()
    {
        vertices = new Vector3[(sizeX + 1) * (sizeZ + 1)];

        float noiseScale = 0.1f;

        int i = 0;
        for (int z = 0; z < sizeZ + 1; z++)
        {
            for (int x = 0; x < sizeX + 1; x++)
            {
                float noiseX = (chunkX * sizeX + x) * scaleX * noiseScale;
                float noiseZ = (chunkZ * sizeZ + z) * scaleZ * noiseScale;
                float h = GeneratePerlinNoise(noiseX, noiseZ, 2);
                vertices[i] = new Vector3(x * scaleX, h * heightMultiplier, z * scaleZ);
                i++;
            }
        }
    }

    private void MakeTriangles()
    {
        triangles = new int[sizeX * sizeZ * 6];

        int v = 0;
        int t = 0;
        for (int z = 0; z < sizeZ; z++)
        {
            for (int x = 0; x < sizeX; x++)
            {
                triangles[t] = v;
                triangles[t + 1] = v + sizeX + 1;
                triangles[t + 2] = v + 1;
                triangles[t + 3] = v + 1;
                triangles[t + 4] = v + sizeX + 1;
                triangles[t + 5] = v + sizeX + 2;
                v++;
                t += 6;
            }
            v++;
        }
    }

    private void MakeColors()
    {
        colors = new Color[(sizeX + 1) * (sizeZ + 1)];

        Color32 sandColor = new Color32(250, 242, 120, 255);
        Color32 grassColor = new Color32(100, 230, 0, 255);
        Color32 rockColor = new Color32(128, 128, 128, 255);

        Gradient gradient1 = MakeGradient(sandColor, grassColor);
        Gradient gradient2 = MakeGradient(grassColor, rockColor);

        for (int z = 0; z < sizeZ + 1; z++)
        {
            for (int x = 0; x < sizeX + 1; x++)
            {
                float h = vertices[z * (sizeZ + 1) + x].y / heightMultiplier;
                if (h < 0.75f)
                {
                    colors[z * (sizeZ + 1) + x] = gradient1.Evaluate((h - 0.45f) * 16);
                }
                else
                {
                    colors[z * (sizeZ + 1) + x] = gradient2.Evaluate((h - 0.75f) * 16);
                }
            }
        }
    }

    private void MakeTrees()
    {
        for (int z = 0; z < sizeZ + 1; z++)
        {
            for (int x = 0; x < sizeX + 1; x++)
            {
                int whiteNoiseX = (chunkX * sizeX + x) * scaleX + 1024;
                int whiteNoiseZ = (chunkZ * sizeZ + z) * scaleZ + 1024;
                float randVal = GenerateWhiteNoise(whiteNoiseX, whiteNoiseZ);
                if (randVal > 0.994f)
                {
                    float h = vertices[z * (sizeZ + 1) + x].y / heightMultiplier;
                    if (h > 0.5f)
                    {
                        GameObject player = GameObject.Find("FPSController");
                        float treeX = (chunkX * sizeX + x) * scaleX;
                        float treeY = h * heightMultiplier + 5.5f;
                        float treeZ = (chunkZ * sizeZ + z) * scaleZ;
                        GameObject tree = Instantiate(treePrefab, new Vector3(treeX, treeY, treeZ), Quaternion.identity, gameObject.transform);
                        tree.GetComponent<LookAtPlayer>().player = player;
                    }
                }
            }
        }
    }

    void Start()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        MakeVertices();
        MakeTriangles();
        MakeColors();
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.RecalculateNormals();
        gameObject.AddComponent(typeof(MeshCollider));
        MakeTrees();
    }

    void Update()
    {
    }
}
