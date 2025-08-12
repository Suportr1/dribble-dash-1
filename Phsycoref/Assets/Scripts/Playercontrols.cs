using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public RuntimeAnimatorController animatorController; // NEW: Public field to assign Animator Controller
    private float currentLanePosition = 0;
    private float swipeLerpTime;

    private bool isSliding = false; // Track if currently sliding
    private Vector3 originalColliderCenter;
    private float originalColliderHeight;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<CapsuleCollider>();

        // Assign Animator programmatically, or validate runtime controller
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError("Animator component is missing! It must be assigned or added to this game object.");
                return;
            }
        }

        // NEW: Assign Animator Controller if not already assigned
        if (animator.runtimeAnimatorController == null)
        {
            if (animatorController != null)
            {
                animator.runtimeAnimatorController = animatorController;
                Debug.Log($"Assigned Animator Controller: {animatorController.name}");
            }
            else
            {
                Debug.LogError("Animator Controller (e.g., JamesController) is not assigned to the Animator. Please assign one in the Inspector.");
            }
        }

        // Save original collider info for sliding logic
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
            animator.ResetTrigger("Slide"); // Reset old trigger
            animator.SetTrigger("Slide");   // Fire slide trigger
            StartCoroutine(Slide());
        }
    }

    private System.Collections.IEnumerator Slide()
    {
        if (isSliding) yield break; // Prevent duplicate slides
        isSliding = true;

        if (animator != null) // Ensure Animator is assigned
        {
            Debug.Log($"Animator found. Active Controller: {(animator.runtimeAnimatorController != null ? animator.runtimeAnimatorController.name : "None")}");

            // Trigger the Slide animation
            animator.SetTrigger("Slide");
            Debug.Log("Slide trigger set.");

            // Wait for 1 frame to allow Animator to update
            yield return null;

            // Verify animation state
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("Slide"))
            {
                Debug.Log("Slide animation is now playing!");
            }
            else
            {
                Debug.LogError("Failed to transition to Slide state. Check Animator transitions and parameters.");
            }
        }
        else
        {
            Debug.LogError("Animator is null or not assigned.");
            isSliding = false;
            yield break;
        }

        // Adjust CapsuleCollider for sliding
        float reducedHeight = originalColliderHeight / 2f;
        float centerAdjustment = (originalColliderHeight - reducedHeight) / 2f;

        playerCollider.height = reducedHeight;
        playerCollider.center = new Vector3(originalColliderCenter.x, originalColliderCenter.y - centerAdjustment, originalColliderCenter.z);

        // Wait for the slide duration
        yield return new WaitForSeconds(slideDuration);

        // Reset CapsuleCollider to its original state
        playerCollider.height = originalColliderHeight;
        playerCollider.center = originalColliderCenter;

        Debug.Log("Slide animation completed.");
        isSliding = false;
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

  
}
