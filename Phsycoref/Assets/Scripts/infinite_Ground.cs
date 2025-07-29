using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class infinite_Ground : MonoBehaviour
{
    public GameObject groundPrefab; // Prefab for the ground chunk
    public int poolSize = 5;        // Number of chunks in the pool (determines the visible area)
    public float chunkLength = 10f; // Length of each ground chunk
    public Transform player;       // Reference to the player

    private Queue<GameObject> groundChunks = new Queue<GameObject>();
    private float recycleOffset = 20f; // Distance ahead of the player to keep spawning new chunks

    void Start()
    {
        // Initialize the pool
        for (int i = 0; i < poolSize; i++)
        {
            GameObject chunk = Instantiate(groundPrefab, new Vector3(0, 0, i * chunkLength), Quaternion.identity);
            groundChunks.Enqueue(chunk);
        }
    }

    void Update()
    {
        // Check when the player passes a chunk and recycle it
        GameObject firstChunk = groundChunks.Peek();

        if (player.position.z > firstChunk.transform.position.z + chunkLength + recycleOffset)
        {
            RecycleChunk();
        }
    }

    void RecycleChunk()
    {
        // Remove the first chunk (oldest)
        GameObject oldChunk = groundChunks.Dequeue();

        // Reposition it in front of the last chunk
        GameObject lastChunk = groundChunks.Peek();
        oldChunk.transform.position = lastChunk.transform.position + new Vector3(0, 0, chunkLength);

        // Add it back to the queue
        groundChunks.Enqueue(oldChunk);
    }
}
