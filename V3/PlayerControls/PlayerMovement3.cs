using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.ProBuilder.Shapes;

public class PlayerMovement3 : MonoBehaviour
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
    [SerializeField] private float jumpForce = 6.5f;
    [SerializeField] private float Gravity = -9.81f;
    private Vector3 Velocity;
    #endregion

    #region Player Movements Variables
    private float horizontal;
    private float vertical;
    #endregion

    #region Booleans
    [SerializeField] private bool isMoving;
    [SerializeField] private bool isRunning;
    [SerializeField] private bool isSprinting;
    [SerializeField] private bool isWalking;
    [SerializeField] private bool isJumping;
    [SerializeField] private bool isGrounded;
    [SerializeField] private string lastMovement;

    #endregion

    #region Variables: Camera Motions
    // Character & Camera
    private float turnSmoothing = 0.1f;
    private float smoothingVelocity;
    #endregion

    #region Jump Motion
    public float forwardForce = 4.8f;
    public float duration = 1.2f;
    private float jumpStartTime;
    #endregion


    private Vector3 previousPosition;
    private float previousYPosition;
    private bool isMovingUp;
    private bool isMovingDown;

    public float maxRayDistance = 1.0f;
    float slopeAngle;

    public float minHeightDifference = 3.0f;

    private void Start()
    {
        previousYPosition = transform.position.y;
        charControl = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        isRunning = true;
        lastMovement = "isRunning";
        // Hides & lock the cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    
    private void Update()
    {
        horizontal = Input.GetKey(KeyCode.A) ? -1f : Input.GetKey(KeyCode.D) ? 1f : 0f;
        vertical = Input.GetKey(KeyCode.W) ? 1f : Input.GetKey(KeyCode.S) ? -1f : 0f;
        MovePlayer();
        MoveSpeedChanger();
        GravityPhysics();
        KeyPressHandler();
        GetSlopeAngle();
        HighandLowJump();
        ChangeJumpHeight();
        FallDistance();
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
                    print("isFalling");
                    Vector3 moveDirection = transform.forward * 1.2f + Vector3.up * Velocity.y;
                    // Execute the motion
                    charControl.Move(moveDirection * Time.deltaTime);
                    isGrounded = false;
                }
            }
        }
    }

    private void MovePlayer()
    {
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (isGrounded)
        {
            if (direction.magnitude >= 0.1f)
            {
                if (!isJumping)
                {
                    /*
                     * This Section will calculate the direction of the player, then smoothens it rotation based on the calulated direction
                     */
                    float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + CameraAngle.eulerAngles.y;
                    float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref smoothingVelocity, turnSmoothing);
                    transform.rotation = Quaternion.Euler(0f, angle, 0f);

                    // This moves the player based on its forward direction
                    Vector3 newDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                    charControl.Move(newDirection.normalized * playerSpeed * Time.deltaTime);

                    isMoving = true;
                }
            }
            else
            {
                isMoving = false;
            }
        }
    }

    private void GetSlopeAngle()
    {
        if (Physics.Raycast(transform.position, -Vector3.up, out RaycastHit hit, maxRayDistance, LayerMask))
        {
            Vector3 groundNormal = hit.normal;
            slopeAngle = Vector3.Angle(groundNormal, Vector3.up);
        }
    }

    private void MoveSpeedChanger()
    {
        if (isMoving)
        {
            if (lastMovement == "isWalking")
            {
                isWalking = true;            
                playerSpeed = 1.3f;
                animator.SetBool("isWalking", true);
                animator.SetBool("isRunning", false);
            }


            if (lastMovement == "isRunning")
            {
                print(slopeAngle);
                isRunning = true;

                float currentYPosition = transform.position.y;

                if (slopeAngle >= 35 && slopeAngle <= 45)
                {
                    if (currentYPosition > previousYPosition)
                    {
                        // Player is going up
                        playerSpeed = 2.5f;

                    }
                    else if (currentYPosition < previousYPosition)
                    {
                        // Going down
                        playerSpeed = 3.7f;
                    }
                    else
                    {
                        return;
                    }

                    previousYPosition = currentYPosition;
                }
                else if (slopeAngle >= 25 && slopeAngle <= 35)
                {
                    if (currentYPosition > previousYPosition)
                    {
                        // Player is going up
                        playerSpeed = 2.6f;

                    }
                    else if (currentYPosition < previousYPosition)
                    {
                        // Going down
                        playerSpeed = 3.5f;
                    }
                    else
                    {
                        return;
                    }
                    previousYPosition = currentYPosition;
                }
                else
                {
                    playerSpeed = 3.9f;
                }
                animator.SetBool("isWalking", false);
                animator.SetBool("isRunning", true);
            }
        }
        else if(!isMoving)
        {
            isWalking = false;
            isRunning = false;
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", false);
        }
    }

    private void KeyPressHandler()
    {
        if(Input.GetKeyDown(KeyCode.LeftControl) && !isJumping)   
        {
            if (lastMovement == "isWalking")
            {
                lastMovement = "isRunning";
                isRunning = true;
                isWalking = false;
                return;
            }

            if (lastMovement == "isRunning")
            {
                lastMovement = "isWalking";
                isRunning = false;
                isWalking = true;            
                return;
            }
        }
    }

    private void GravityPhysics()
    {
        if (charControl.isGrounded)
        {
            Velocity.y = -1f;
            animator.SetBool("isJumping", false);

            if (Input.GetButtonDown("Jump"))
            {
                isGrounded = false;
                isJumping = true;
                Velocity.y = jumpForce;
                jumpStartTime = Time.time;
            }
            else
            {
                isJumping = false;
                isGrounded = true;
            }
        }
        else
        {
            Velocity.y -= Gravity * -2f * Time.deltaTime;
        }

        charControl.Move(Velocity * Time.deltaTime);
    }

    private void HighandLowJump()
    {

        if (isJumping && isWalking || isJumping && !isMoving)
        {
            animator.SetBool("isJumping", true);
            float elapsedTime = Time.time - jumpStartTime;
            if (elapsedTime < duration)
            {
            }
        }
        else if(isJumping && isRunning || isJumping && isSprinting)
        {
            animator.SetBool("isJumping", true);
            // This gives the jumping motion a duration to make it smoother
            float elapsedTime = Time.time - jumpStartTime;
            if (elapsedTime < duration)
            {
                // Calculate the direction for the combined forward and upward motion
                Vector3 moveDirection = transform.forward * forwardForce + Vector3.up * Velocity.y;
                // Execute the motion
                charControl.Move(moveDirection * Time.deltaTime);
            }
        }
    }

    private void ChangeJumpHeight()
    {
        if (isRunning)
        {
            jumpForce = 4.4f;
        }
        else
        {
            jumpForce = 5.6f;
        }
    }
}
