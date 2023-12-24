using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(IKSystem))]
[RequireComponent(typeof(AnimationSystem))]
[RequireComponent(typeof(AttackingScript))]
public class PlayerMovement4 : MonoBehaviour
{
    #region Instances
    AttackingScript attckInst;
    #endregion

    #region Interactibles in unity
    [SerializeField] private CharacterController charControl;
    [SerializeField] private CinemachineFreeLook freelookCam;
    [SerializeField] private Transform CameraAngle;
    [SerializeField] private Animator animator;
    [SerializeField] private LayerMask LayerMasks;
    #endregion

    #region Physics values
    [SerializeField] private float playerSpeed = 3.2f;
    [SerializeField] private float jumpForce;
    [SerializeField] private float changeJump = 0f;
    [SerializeField] private float addJumpForce = 0f;
    [SerializeField] private float Gravity = -9.81f;
    [SerializeField] private float minHeightDifference = 2.8f;
    [SerializeField] private float fallDelay = -3.2f;
    [SerializeField] private Vector3 Velocity;
    #endregion

    #region Camera Movement Smoothness
    private float turnSmoothing = 0.1f;
    private float smoothingVelocity;
    #endregion

    #region Jump Motion
    private float sprintingForce = 6.2f;
    private float runningForce = 4.6f;
    private float walkingForce = 2.6f;
    private float duration = 1.2f;
    private float jumpCooldown = 1.2f;
    private float jumpTimer = 0f;
    private bool canJump = true;
    #endregion

    #region Slope Detection
    private float maxRayDistance = 1.0f;
    private float slopeAngle;
    private float previousYPosition;
    #endregion

    #region
    [SerializeField] public string playerState;
    [SerializeField] public bool isMoving;
    // Sorts of Movements
    [SerializeField] public bool isJumping;
    [SerializeField] public bool isWalking;
    [SerializeField] public bool isRunning;
    [SerializeField] public bool isSprinting;
    [SerializeField] private bool isAttacking;
    [SerializeField] public bool currentlyMoving;

    // Falling or Grounded
    [SerializeField] public bool isFalling;
    [SerializeField] public bool isGrounded;
    [SerializeField] public bool onSlope;
    [SerializeField] private bool goingUp;
    [SerializeField] private bool goingDown;
    #endregion

    private void Start()
    {
        charControl = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        attckInst = GetComponent<AttackingScript>();
        playerState = "Idle";
        isRunning = true;

        // Hides & lock the cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        float newJump = changeJump + addJumpForce;
        changeJump = newJump;

        ChangeState();
        GetKeyPress();
        PhysicsApplication();
        FallDistance();
        GetSlopeAngle();
        JumpAndSpeedChange();

        float horizontal = Input.GetKey(KeyCode.A) ? -1f : Input.GetKey(KeyCode.D) ? 1f : 0f;
        float vertical = Input.GetKey(KeyCode.W) ? 1f : Input.GetKey(KeyCode.S) ? -1f : 0f;

        if (Mathf.Abs(horizontal) > 0.2f || Mathf.Abs(vertical) > 0.2f)
        {
            currentlyMoving = true;
        }
        else
        {
            currentlyMoving = false;
        }

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
        if (direction.magnitude >= 0.1f)
        {
            if (!isAttacking)
            {
                if (!isJumping || isFalling)
                {
                    // This Section will calculate the direction of the player, then smoothens it rotation based on the calulated direction
                    float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + CameraAngle.eulerAngles.y;
                    float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref smoothingVelocity, turnSmoothing);
                    transform.rotation = Quaternion.Euler(0f, angle, 0f);


                    // This moves the player based on its forward direction
                    Vector3 newDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                    charControl.Move(newDirection.normalized * playerSpeed * Time.deltaTime);
                    isMoving = true;

                }
                else
                {
                    if (isJumping && isRunning && !isSprinting)
                    {
                        Vector3 moveDirection = transform.forward * runningForce + Vector3.up * Velocity.y;
                        charControl.Move(moveDirection * Time.deltaTime);
                        isMoving = false;
                    }
                    else if (isJumping && isSprinting)
                    {
                        Vector3 moveDirection = transform.forward * sprintingForce + Vector3.up * Velocity.y;
                        charControl.Move(moveDirection * Time.deltaTime);
                        isMoving = false;
                    }
                    else if (isJumping && isWalking && !isSprinting)
                    {
                        Vector3 moveDirection = transform.forward * walkingForce + Vector3.up * Velocity.y;
                        charControl.Move(moveDirection * Time.deltaTime);
                        isMoving = false;
                    }
                    else
                    {

                    }
                }
            }
        }
        else
        {
            isMoving = false;
        }
    }

    private void GetSlopeAngle()
    {
        if (Physics.Raycast(transform.position, -Vector3.up, out RaycastHit hit, maxRayDistance, LayerMasks))
        {
            Vector3 groundNormal = hit.normal;
            slopeAngle = Vector3.Angle(groundNormal, Vector3.up);
            float currentYPosition = transform.position.y;
            if (slopeAngle >= 1)
            {
                onSlope = true;
                if (isMoving)
                {
                    if (currentYPosition > previousYPosition)
                    {
                        goingUp = true;
                        goingDown = false;
                    }
                    else if (currentYPosition < previousYPosition)
                    {
                        goingUp = false;
                        goingDown = true;
                    }
                    else
                    {
                        goingDown = false;
                        goingUp = false;
                        return;
                    }
                }
                else
                {
                    goingDown = false;
                    goingUp = false;
                }
                previousYPosition = currentYPosition;
            }
            else 
            {
                goingDown = false;
                goingUp = false;
                onSlope = false;
            }
        }
    }

    private void GetKeyPress()
    {
        // Toggle between Walk and Sprint Section
        if (Input.GetKeyDown(KeyCode.LeftControl) && !isJumping)
        {
            if (isRunning)
            {
                isRunning = false;
                isWalking = true;
            }
            else
            {
                isRunning = true;
                isWalking = false;
            }
        }

        // Sprinting Section
        if (Input.GetKey(KeyCode.LeftShift))
        {
            isSprinting = true;
        }
        else
        {
            isSprinting = false;
        }

        jumpTimer += Time.deltaTime;
        if (!canJump)
        {
            if (jumpTimer >= jumpCooldown)
            {
                jumpTimer = 0;
                canJump = true;
            }
        }

        if (canJump && Input.GetKeyDown(KeyCode.Space) && !isJumping)
        {
            isJumping = true;
            jumpForce = changeJump;
        }
        else if (!goingDown && charControl.isGrounded)
        {
            jumpForce = 0f;
            isJumping = false;
        }
    }

    private void FallDistance()
    {
        if (!charControl.isGrounded)
        {
            RaycastHit hit;
            Ray ray = new Ray(transform.position, Vector3.down);

            // Cast a ray downward to detect the ground
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMasks))
            {
                float currentHeight = hit.distance;
                // Check if the character fell from a certain height
                if (currentHeight >= minHeightDifference)
                {
                    Vector3 moveDirection = transform.forward * 1.2f + Vector3.up * Velocity.y;
                    charControl.Move(moveDirection * Time.deltaTime);
                    if (Velocity.y <= fallDelay)
                    {
                        isFalling = true;
                    }
                }
            }
        }
    }

    /*
     * Change the player state based on the movement
     */
    private void ChangeState()
    {
        if (!isFalling)
        {
            if (isMoving)
            {
                if (!isJumping)
                {
                    if (isRunning)
                    {
                        playerState = "Running";
                    }

                    if (isWalking)
                    {
                        playerState = "Walking";
                    }

                    if (isSprinting)
                    {
                        playerState = "Sprinting";
                    }
                }
            }
            else
            {
                if (!isJumping)
                {
                    playerState = "Idle";
                }

                if (currentlyMoving)
                {
                    if (isJumping && isWalking && !isSprinting)
                    {
                        playerState = "WalkingJump";
                    }

                    if (isJumping && isRunning && !isSprinting)
                    {
                        playerState = "RunningJump";
                    }

                    if (isJumping && isSprinting)
                    {
                        playerState = "SprintingJump";
                    }
                }
                else
                {
                    if (isJumping)
                    {
                        playerState = "Jumped";
                    }
                }
            }
        }
        else
        {
            playerState = "Falling";
        }
    }

    /*
     * This Section is for Physics only
     */
    private void PhysicsApplication()
    {
        if (Velocity.y <= -7.5f)
        {
            Velocity.y = -7.5f;
        }

        if (charControl.isGrounded)
        {
            isGrounded = true;
            Velocity.y = -1f;
            Velocity.y = jumpForce;
            isFalling = false;
        }
        else
        {
            isGrounded = false;
            Velocity.y -= Gravity * -2f * Time.deltaTime;
        }
        charControl.Move(Velocity * Time.deltaTime);
    }

    private void JumpAndSpeedChange()
    {
        if (currentlyMoving)
        {
            if (isRunning || isSprinting)
            {
                changeJump = 4.8f;
            }
            else if (isWalking)
            {
                changeJump = 4.6f;
            }
        }
        else
        {
            changeJump = 6.2f;
        }

        if (isFalling)
        {
            playerSpeed = 2.2f;
        }

        if (isMoving && !isFalling)
        {
            if (playerState == "Running" && !isSprinting)
            {
                playerSpeed = 3.9f;
            }

            if (playerState == "Walking" && !isSprinting)
            {
                playerSpeed = 1.4f;
            }

            if (playerState == "Sprinting")
            {
                playerSpeed = 7.2f;
            }
        }
    }
}
