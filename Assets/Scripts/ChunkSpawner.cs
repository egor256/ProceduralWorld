using System.Collections.Generic;
using UnityEngine;

public class ChunkSpawner : MonoBehaviour
{
    public GameObject chunkPrefab;

    private static int loadDistance = 3;
    private List<int[]> loadedChunks = new List<int[]>();

    private int prevChunkX;
    private int prevChunkZ;
    private int currentChunkX;
    private int currentChunkZ;

    void Start()
    {
        prevChunkX = currentChunkX = (int) Mathf.Floor(transform.position.x / (Chunk.sizeX * Chunk.scaleX));
        prevChunkZ = currentChunkZ = (int) Mathf.Floor(transform.position.z / (Chunk.sizeZ * Chunk.scaleZ));

        for (int i = -loadDistance; i <= loadDistance; i++)
        {
            for (int j = -loadDistance; j <= loadDistance; j++)
            {
                LoadChunk(currentChunkX + i, currentChunkZ + j);
            }
        }
    }

    public static string GetChunkName(int chunkX, int chunkZ)
    {
        return "Chunk(" + chunkX + "," + chunkZ + ")";
    }

    private bool IsChunkLoaded(int chunkX, int chunkZ)
    {
        return loadedChunks.Exists(c => c[0] == chunkX && c[1] == chunkZ);
    }

    private void LoadChunk(int chunkX, int chunkZ)
    {
        chunkPrefab.GetComponent<Chunk>().chunkX = chunkX;
        chunkPrefab.GetComponent<Chunk>().chunkZ = chunkZ;
        float posX = (chunkX * Chunk.sizeX) * Chunk.scaleX;
        float posZ = (chunkZ * Chunk.sizeZ) * Chunk.scaleZ;
        GameObject chunk = Instantiate(chunkPrefab, new Vector3(posX, 0, posZ), Quaternion.identity);
        chunk.name = GetChunkName(chunkX, chunkZ);
        loadedChunks.Add(new int[]{ chunkX, chunkZ });
    }

    private void UnloadChunk(int chunkX, int chunkZ)
    {
        GameObject chunk = GameObject.Find(GetChunkName(chunkX, chunkZ));
        Destroy(chunk);
        loadedChunks.RemoveAll(c => c[0] == chunkX && c[1] == chunkZ);
    }

    void Update()
    {
        int currentChunkX = (int) Mathf.Floor(transform.position.x / (Chunk.sizeX * Chunk.scaleX));
        int currentChunkZ = (int) Mathf.Floor(transform.position.z / (Chunk.sizeZ * Chunk.scaleZ));

        if (currentChunkX != prevChunkX || currentChunkZ != prevChunkZ)
        {
            GameObject[] chunks = GameObject.FindGameObjectsWithTag("Chunk");
            for (int i = 0; i < chunks.Length; i++)
            {
                if (System.Math.Abs(currentChunkX - chunks[i].GetComponent<Chunk>().chunkX) > loadDistance ||
                    System.Math.Abs(currentChunkZ - chunks[i].GetComponent<Chunk>().chunkZ) > loadDistance)
                {
                    UnloadChunk(chunks[i].GetComponent<Chunk>().chunkX, chunks[i].GetComponent<Chunk>().chunkZ);
                }
            }
        }

        prevChunkX = currentChunkX;
        prevChunkZ = currentChunkZ;

        for (int i = -loadDistance; i <= loadDistance; i++)
        {
            for (int j = -loadDistance; j <= loadDistance; j++)
            {
                if (!IsChunkLoaded(currentChunkX + i, currentChunkZ + j))
                {
                    LoadChunk(currentChunkX + i, currentChunkZ + j);
                    return; // Load only one chunk per update
                }
            }
        }
    }
}
