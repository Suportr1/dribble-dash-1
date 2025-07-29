using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Playercontrols : MonoBehaviour
{

    public float speed = 5;
    public Rigidbody rb;
    float Horizontalmovement;
   public float movementmultiplier = 2f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Horizontalmovement = Input.GetAxis("Horizontal"); 
    }
    private void FixedUpdate()
    {
        Vector3 forwardmove = transform.forward * speed * Time.fixedDeltaTime;
        Vector3 Horizontalmove = transform.right * Horizontalmovement * Time.fixedDeltaTime* movementmultiplier;
        rb.MovePosition(rb.position + forwardmove+ Horizontalmove);
    }

}
