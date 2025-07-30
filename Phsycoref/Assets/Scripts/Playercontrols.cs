using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Playercontrols : MonoBehaviour
{

    public float laneDistance = 3f; // Distance between lanes
    public float speed = 5f; // Forward movement speed
    public float swipeDuration = 0.2f; // Duration for lane-switching
    public float slideDuration = 0.8f; // How long the slide lasts
    private int targetLane = 0; // Current target lane (-1, 0, 1 for left, center, right)

    public Rigidbody rb;
    public CapsuleCollider playerCollider; // CapsuleCollider attached to the player
    public Animator animator; // Animator for playing animations (attach your Animator component here)

    private float currentLanePosition = 0;
    private float swipeLerpTime;

    private bool isSliding = false; // Track if currently sliding
    private Vector3 originalColliderCenter;
    private float originalColliderHeight;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<CapsuleCollider>();
        animator = GetComponent<Animator>(); // Ensure the Animator component is attached to your character

        // Save original collider values for resetting after slide
        originalColliderHeight = playerCollider.height;
        originalColliderCenter = playerCollider.center;
    }

    void Update()
    {
        // Change lane
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ChangeLane(-1);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ChangeLane(1);
        }

        // Slide input (press Down Arrow for testing)
        if (Input.GetKeyDown(KeyCode.DownArrow) && !isSliding)
        {
            StartCoroutine(Slide());
        }
    }

    private void FixedUpdate()
    {
        // Forward constant movement
        Vector3 forwardMovement = transform.forward * speed * Time.fixedDeltaTime;

        // Smooth interpolation for lateral movement
        float lanePositionX = targetLane * laneDistance;
        swipeLerpTime += Time.fixedDeltaTime / swipeDuration;
        currentLanePosition = Mathf.Lerp(currentLanePosition, lanePositionX, swipeLerpTime);

        // Apply calculated movement
        Vector3 targetPosition = new Vector3(currentLanePosition, rb.position.y, rb.position.z) + forwardMovement;
        rb.MovePosition(targetPosition);
    }

    private void ChangeLane(int direction)
    {
        // Update the target lane (-1, 0, 1 for left, center, right)
        targetLane += direction;
        targetLane = Mathf.Clamp(targetLane, -1, 1);

        // Reset lerp progress for smooth lane switch
        swipeLerpTime = 0;
    }

    private System.Collections.IEnumerator Slide()
    {
        // Start sliding
        isSliding = true;

        // Trigger the slide animation
        if (animator != null)
        {
            animator.SetTrigger("Slide"); // Ensure "Slide" is the trigger name in your Animator Controller
        }

        // Reduce the collider height and lower the character
        playerCollider.height = originalColliderHeight / 2f; // Reduce height by half
        playerCollider.center = originalColliderCenter - new Vector3(0, originalColliderHeight / 4f, 0); // Lower collider center

        // Wait for slide duration
        yield return new WaitForSeconds(slideDuration);

        // Reset the collider to its original state
        playerCollider.height = originalColliderHeight;
        playerCollider.center = originalColliderCenter;

        // End sliding
        isSliding = false;
    }
}
