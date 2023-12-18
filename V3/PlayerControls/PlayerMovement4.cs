using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaverMovement4 : MonoBehaviour
{
    #region Interactibles in unity
    [SerializeField] private CharacterController charControl;
    [SerializeField] private CinemachineFreeLook freelookCam;
    [SerializeField] private Transform CameraAngle;
    [SerializeField] private Animator animator;
    [SerializeField] private LayerMask LayerMask;
    #endregion

    #region Physics values
    [SerializeField] private float playerSpeed = 3.2f;
    [SerializeField] private float jumpForce;
    [SerializeField] private float Gravity = -9.81f;
    [SerializeField] private float minHeightDifference = 2.8f;
    [SerializeField] private Vector3 Velocity;
    #endregion

    #region Camera Movement Smoothness
    private float turnSmoothing = 0.1f;
    private float smoothingVelocity;
    #endregion

    #region Jump Motion
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
    [SerializeField] private string playerState;
    [SerializeField] private bool isMoving;
    // Sorts of Movements
    [SerializeField] private bool isJumping;
    [SerializeField] private bool isWalking;
    [SerializeField] private bool isRunning;
    [SerializeField] private bool isSprinting;

    // Falling or Grounded
    [SerializeField] private bool isFalling;
    [SerializeField] private bool isGrounded;
    [SerializeField] private bool onSlope;
    [SerializeField] private bool goingUp;
    [SerializeField] private bool goingDown;
    #endregion


    private void Start()
    {
        charControl = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        playerState = "Idle";
        isRunning = true;

        // Hides & lock the cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        ChangeState();
        GetKeyPress();
        PhysicsApplication();
        FallDistance();
        GetSlopeAngle();
        float horizontal = Input.GetKey(KeyCode.A) ? -1f : Input.GetKey(KeyCode.D) ? 1f : 0f;
        float vertical = Input.GetKey(KeyCode.W) ? 1f : Input.GetKey(KeyCode.S) ? -1f : 0f;

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
        if (direction.magnitude >= 0.1f)
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
                if (isJumping && isRunning || isJumping && isSprinting)
                {
                    Vector3 moveDirection = transform.forward * runningForce + Vector3.up * Velocity.y;
                    charControl.Move(moveDirection * Time.deltaTime);
                    isMoving = false;
                }
                else if (isJumping && isWalking)
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
        else
        {
            isMoving = false;
        }

    }

    private void GetSlopeAngle()
    {
        if (Physics.Raycast(transform.position, -Vector3.up, out RaycastHit hit, maxRayDistance, LayerMask))
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
            if (isMoving)
            {
                isSprinting = true;
            }
            else
            {
                isSprinting = false;
            }
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
            jumpForce = 6.5f;
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
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask))
            {
                float currentHeight = hit.distance;
                // Check if the character fell from a certain height
                if (currentHeight >= minHeightDifference)
                {
                    Vector3 moveDirection = transform.forward * 1.2f + Vector3.up * Velocity.y;
                    charControl.Move(moveDirection * Time.deltaTime);
                    isFalling = true;
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
                else
                {
                    if (isRunning || isSprinting)
                    {
                        playerState = "FastJump";
                    }

                    if (isWalking)
                    {
                        playerState = "SlowJump";
                    }
                }
            }
            else
            {
                if (!isJumping)
                {
                    playerState = "Idle";
                }
                if (isJumping)
                {
                    playerState = "SmallJump";
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
        if(Velocity.y <= -6f)
        {
            Velocity.y = -7.5f;
        }

        if (charControl.isGrounded)
        {
            isGrounded = true;
            isFalling = false;
            Velocity.y = -1f;
            Velocity.y = jumpForce;
        }
        else
        {
            Velocity.y -= Gravity * -2f * Time.deltaTime;
        }
        charControl.Move(Velocity * Time.deltaTime);
    }
}