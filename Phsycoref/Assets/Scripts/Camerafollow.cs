using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camerafollow : MonoBehaviour
{
    public Transform player;
    Vector3 offset;
    // Start is called before the first frame update
    void Start()
    {
        offset= transform.position- player.position;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 playerpos = player.position + offset;
        playerpos.x = 0f;
        transform.position = playerpos;
    }
}
