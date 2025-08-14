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
    public float jumpForce = 10f; // How high the player can jump
   
    public Rigidbody rb;
    public CapsuleCollider playerCollider; // CapsuleCollider attached to the player
    public Animator animator; // Animator for playing animations (attach your Animator component here)
    public RuntimeAnimatorController animatorController; // NEW: Public field to assign Animator Controller
    private float currentLanePosition = 0;
    private float swipeLerpTime;
    private bool isJumping = false; // Track if currently jumping
    private bool canJump = true; // Ensure a jump can only happen when allowed
    private bool isSliding = false; // Track if currently sliding
    private Vector3 originalColliderCenter;
    private float originalColliderHeight;

    void Start()
    {
        // Get essential components
        rb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<CapsuleCollider>();

        // Check if Rigidbody exists
        if (rb == null)
        {
            Debug.LogError("Rigidbody component is missing! Please add it to this game object.");
            return;
        }

        // Check if CapsuleCollider exists
        if (playerCollider == null)
        {
            Debug.LogError("CapsuleCollider component is missing! Please add it to this game object.");
            return;
        }

        // Align CapsuleCollider for proper grounding
        float bottomColliderOffset = playerCollider.height / 2f - playerCollider.radius;
        playerCollider.center = new Vector3(playerCollider.center.x, bottomColliderOffset, playerCollider.center.z);

        // Debugging logs for CapsuleCollider
        Debug.Log($"Adjusted CapsuleCollider Center: {playerCollider.center}, Height: {playerCollider.height}");

        // Configure Rigidbody for stability
        rb.constraints = RigidbodyConstraints.FreezeRotation; // Freeze rotation to avoid tipping
        rb.useGravity = true; // Ensure gravity is enabled

        // Ensure Animator is assigned
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError("Animator component is missing! It must be assigned or added to this game object.");
                return;
            }
        }

        // Assign Animator Controller if not already assigned
        if (animator.runtimeAnimatorController == null)
        {
            if (animatorController != null)
            {
                animator.runtimeAnimatorController = animatorController;
                Debug.Log($"Assigned Animator Controller: {animatorController.name}");
            }
            else
            {
                Debug.LogError("Animator Controller is not assigned to the Animator. Please assign one in the Inspector.");
            }
        }

        // Disable root motion to prevent unexpected position or rotation changes
        animator.applyRootMotion = false;
        Debug.Log("Animator root motion has been disabled.");

        // Ensure the player is facing forward at the start
        transform.rotation = Quaternion.LookRotation(Vector3.forward);

        // Save original Collider info for sliding logic
        originalColliderHeight = playerCollider.height;
        originalColliderCenter = playerCollider.center;

        Debug.Log("Initialization complete. Rigidbody, CapsuleCollider, and Animator have been set up.");
    }
    private bool IsGrounded()
    {
        // Create a raycast slightly below the player to check for the ground
        float groundCheckDistance = 0.1f; // Small allowance for checking
        Ray ray = new Ray(transform.position, Vector3.down);

        // Check if we hit a collider below the player
        return Physics.Raycast(ray, groundCheckDistance + playerCollider.radius);
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
        // Jump input
        if (Input.GetKeyDown(KeyCode.UpArrow) && IsGrounded() && canJump && !isJumping)
        {
            Jump();
        }
    }
    private void Jump()
    {
        if (isJumping) return; // Prevent multiple jumps
        isJumping = true;
        canJump = false; // Disable jumping until grounded

        // Trigger jump animation
        animator.ResetTrigger("Jump");
        animator.SetTrigger("Jump");

        // Apply upward force for the jump
        rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);

        // Start cooldown to reset jumping
        StartCoroutine(JumpCooldown());
    }

    private IEnumerator JumpCooldown()
    {
        // Wait for the player to return to the ground
        while (!IsGrounded())
        {
            yield return null; // Wait until the next frame
        }

        yield return new WaitForSeconds(0.1f); // Small delay to ensure grounded

        // Reset jumping status
        isJumping = false;
        canJump = true; // Allow jumping again
        Debug.Log("Jump completed. Player is grounded.");
    }
    private System.Collections.IEnumerator Slide()
    {
        if (isSliding) yield break; // Prevent sliding if already sliding
        isSliding = true;

        // Trigger the Slide animation
        animator.ResetTrigger("Slide");
        animator.SetTrigger("Slide");

        // Lock the horizontal (lane) position based on the current lane
        float lockedXPosition = targetLane * laneDistance;

        // Temporarily adjust the capsule collider for sliding
        float reducedHeight = originalColliderHeight / 2f;
        float centerAdjustment = (originalColliderHeight - reducedHeight) / 2f;
        playerCollider.height = reducedHeight;
        playerCollider.center = new Vector3(originalColliderCenter.x, originalColliderCenter.y - centerAdjustment, originalColliderCenter.z);

        // Calculate slide duration
        float slideEndTime = Time.time + slideDuration;

        // Maintain proper forward motion and lane-lock throughout the slide
        while (Time.time < slideEndTime)
        {
            // Ensure player locks to the current lane (horizontal X-axis)
            rb.velocity = new Vector3(
                (lockedXPosition - rb.position.x) / Time.deltaTime, // Lock lateral X movement
                rb.velocity.y, // Retain vertical (gravity) velocity
                speed           // Constant forward velocity
            );

            // Force the player to face forward
            transform.rotation = Quaternion.LookRotation(Vector3.forward);

            yield return null; // Wait until the next frame
        }

        // Restore the capsule collider's original dimensions
        playerCollider.height = originalColliderHeight;
        playerCollider.center = originalColliderCenter;

        Debug.Log("Slide completed!");
        isSliding = false;
    }
    private void FixedUpdate()
    {
        // Check if the player is on the ground
        if (!IsGrounded())
        {
            Debug.LogWarning("The player is not grounded!");
        }

        // Compute the player's target X position based on the target lane
        float targetXPosition = targetLane * laneDistance;

        // Smoothly move toward the target X position when not sliding
        if (!isSliding)
        {
            swipeLerpTime += Time.fixedDeltaTime / swipeDuration;
            currentLanePosition = Mathf.Lerp(currentLanePosition, targetXPosition, swipeLerpTime);
        }
        else
        {
            // Lock the X position during sliding
            currentLanePosition = targetXPosition;
        }

        // Set the rigidbody velocity (forward + lateral locking)
        rb.velocity = new Vector3(
            (currentLanePosition - rb.position.x) / Time.fixedDeltaTime, // Smooth X movement
            rb.velocity.y,                                               // Gravity handling
            speed                                                        // Steady forward motion
        );

        // Keep the player always forward-facing
        transform.rotation = Quaternion.LookRotation(Vector3.forward);

        Debug.Log($"Lane: {targetLane}, CurrentLanePosition: {currentLanePosition}, Speed: {speed}");
    }

    private void ChangeLane(int direction)
    {
        // Do not allow lane changes during a slide
        if (isSliding) return;

        // Update the target lane within the valid range (-1, 0, 1)
        targetLane += direction;
        targetLane = Mathf.Clamp(targetLane, -1, 1);

        // Reset the lerp progress for smooth transition
        swipeLerpTime = 0;

        Debug.Log($"Changed to Target Lane: {targetLane}");
    }

  
}
