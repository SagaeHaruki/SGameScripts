using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.Playables;

public class PlayerMovements : MonoBehaviour
{
    #region Variables: Character Control & Camera
    [SerializeField]
    public CharacterController charControl;
    public CinemachineFreeLook freelookCam;
    public Transform CameraAngle;
    private bool cameraEnabled = true;
    #endregion

    #region Character & rotation Speed & Gravity
    [SerializeField] private PlayerState currentState;
    [SerializeField] private float charSpeed = 1f;
    [SerializeField] public float jumpSpeed = 8f;
    [SerializeField] public float hopSpeed = 6.2f;
    [SerializeField] public float gravity = 20.0f;
    [SerializeField] private float verticalSpeed = 0.0f;
    [SerializeField] private bool isMoving;
    [SerializeField] private bool isWalking;
    [SerializeField] private bool isRunning;
    [SerializeField] public bool isWalkingMoving;
    [SerializeField] private bool isRunningMoving;
    [SerializeField] private bool isSprinting;
    [SerializeField] private bool isJumping;
    [SerializeField] public bool isHopping;
    [SerializeField] private bool ignoreGravity;
    [SerializeField] private bool canMove;
    #endregion

    #region Jump Motion
    private float jumpTimer = 0f;
    public float jumpCooldown = 1.4f;
    public float forwardForce = 4.8f; // Best forward motion
    public float duration = 1f; // Best Duration for the smoothest motion
    private float jumpStartTime;
    #endregion

    #region Value: Ground Distance & Mask
    [SerializeField] public float groundDistance = 0.1f;
    [Range(-20, 10)][SerializeField] public float fallingThreshold = -10f;
    public LayerMask groundMask;
    #endregion

    #region Variables: Camera Motions
    // Character & Camera
    public float turnSmoothing = 0.1f; // Player direction smoothing
    float smoothingVelocity;
    #endregion

    private Vector3 previousPosition;
    private bool isMovingUp;
    private bool isMovingDown;

    protected Animator animator;


    private void Start()
    {
        previousPosition = transform.position;
        print(previousPosition);
        charControl = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        isRunning = true;
        canMove = true;
        // Hides the cursor
        Cursor.visible = false;
        // Lock the cursor to the center of the screen
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        getUpDown();
        GravityPhysics();
        ChangeState();
        CheckKeyPressed();
        // GetFalling();
        float horizontal = Input.GetKey(KeyCode.A) ? -1f : Input.GetKey(KeyCode.D) ? 1f : 0f;
        float vertical = Input.GetKey(KeyCode.W) ? 1f : Input.GetKey(KeyCode.S) ? -1f : 0f;

        // Get the direction based on the movement, always normalize movement
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;


        if (canMove)
        {
            if (isJumping && isRunning && !isHopping && !isWalking)
            {
                // This gives the jumping motion a duration to make it smoother
                float elapsedTime = Time.time - jumpStartTime;
                if (elapsedTime < duration)
                {
                    // Calculate the direction for the combined forward and upward motion
                    Vector3 moveDirection = transform.forward * forwardForce + Vector3.up * jumpSpeed;
                    // Execute the motion
                    charControl.Move(moveDirection * Time.deltaTime);
                }
            }

            // Direction Movement
            if (direction.magnitude >= 0.1f)
            {
                // Disables the movement during the jump
                // This is to prevent any changes of direction 
                // This is the cause of a bunny hop
                if (!isHopping && !isJumping)
                {
                    // Calculate the direction of the player
                    float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + CameraAngle.eulerAngles.y;
                    // This will smoothen the rotation of the angle (More math)
                    float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref smoothingVelocity, turnSmoothing);
                    // Rotates the player based on the direction
                    transform.rotation = Quaternion.Euler(0f, angle, 0f);

                    // Direction base on the camera angle
                    Vector3 newDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                    // This will move the character
                    charControl.Move(newDirection.normalized * charSpeed * Time.deltaTime);

                    isMoving = true;
                }
            }
            else
            {
                isMoving = false;
            }
        }
    }

    private void getUpDown()
    {
        Vector3 currentPosition = transform.position;

        RaycastHit hit;
        Vector3 downDirection = -transform.up;

        if (Physics.Raycast(currentPosition, downDirection, out hit))
        {
            Vector3 surfaceNormal = hit.normal;

            // Compare surface normals to detect movement direction
            float dotProduct = Vector3.Dot(surfaceNormal, Vector3.up);

            if (dotProduct > 0.8f) // Adjust this threshold as needed
            {
                isMovingUp = true;
                isMovingDown = false;
                Debug.Log("Moving Up");
                // Your code for handling upward movement
            }
            else if (dotProduct < -0.8f) // Adjust this threshold as needed
            {
                isMovingDown = true;
                isMovingUp = false;
                Debug.Log("Moving Down");
                // Your code for handling downward movement
            }
            else
            {
                isMovingUp = isMovingDown = false;
            }
        }

        previousPosition = currentPosition;
    }

    //private void GetFalling()
    //{
    //    if (charControl.isGrounded)
    //    {
    //        canMove = true;
    //    }
    //    else
    //    {
    //        if (charControl.velocity.y < fallingThreshold)
    //        {
    //            canMove = false;
    //        }
    //    }
    //}

    private void CheckKeyPressed()
    {
        if (jumpTimer > 0)
        {
            jumpTimer -= Time.deltaTime;
        }

        // Reset the jump flag after the character is grounded again
        if (charControl.isGrounded)
        {
            isHopping = false;
            isJumping = false;
            animator.SetBool("isHopping", false);
            animator.SetBool("isJumping", false);
            animator.SetBool("isRunningJump", false);
        }

        // Make the Player Jump
        if (Input.GetButtonDown("Jump") && !isJumping && jumpTimer <= 0)
        {
            if (isWalking || !isMoving)
            {
                verticalSpeed = hopSpeed;
                isHopping = true;
            }

            isJumping = true;
            jumpTimer = jumpCooldown;
            jumpStartTime = Time.time;
        }


        // Running Toggle
        if (Input.GetKeyDown(KeyCode.LeftControl) && !isJumping)
        {
            if (!isRunning)
            {
                isRunning = true;
                isWalking = false;
                return;
            }
            else if (isRunning)
            {
                isRunning = false;
                isWalking = true;
                return;
            }
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (!isMoving)
            {
                isSprinting = false;
            }
            else
            {
                isSprinting = true;
            }
        }

        // Sprinting
        if (Input.GetKeyUp(KeyCode.LeftShift) && isMoving)
        {
            isSprinting = false;
        }
    }

    private void ChangeState()
    {
        if (isMoving)
        {
            if (isRunning)
            {
                isRunningMoving = true;
                isWalkingMoving = false;
                currentState = PlayerState.Running;
                charSpeed = 3.2f;
            }
            
            if (isWalking)
            {
                isWalkingMoving = true;
                isRunningMoving = false;
                currentState = PlayerState.Walking;
                charSpeed = 2.4f;
            }

            if (isSprinting)
            {
                currentState = PlayerState.Sprinting;
                charSpeed = 6.7f;
            }

            if (isJumping)
            {
                currentState = PlayerState.Jumping;
            }
        }
        else 
        {
            currentState = PlayerState.Idle;
            isWalkingMoving = false;
            isRunningMoving = false;
            isSprinting = false;
        }
    }

    private void GravityPhysics()
    {
        if (ignoreGravity)
        {
            charControl.Move(Vector3.zero); // Reset any existing velocity
            charControl.detectCollisions = false;
        }
        else
        {
            if (charControl.isGrounded && verticalSpeed < 0.0f && isMoving || charControl.isGrounded && verticalSpeed < 0.0f && !isMoving)
            {
                verticalSpeed = -1.0f;
            }
            else
            {
                verticalSpeed -= gravity * Time.deltaTime;
            }

            //if (charControl.isGrounded && verticalSpeed < 0.0f && isMoving || charControl.isGrounded && verticalSpeed < 0.0f)
            //{
            //    verticalSpeed = -1.0f;
            //}
            //else if (isJumping)
            //{
            //    verticalSpeed -= gravity * Time.deltaTime;
            //}
            //else if (!isJumping)
            //{
            //    verticalSpeed -= gravity * Time.deltaTime;
            //}

            Vector3 moveDirection = new Vector3(0, verticalSpeed, 0);
            charControl.Move(moveDirection * Time.deltaTime);
        }
    }

    void FixedUpdate()
    {
        // Use the current state to perform actions or behaviors
        switch (currentState)
        {
            case PlayerState.Idle:
                if (!isRunningMoving)
                {
                    animator.SetBool("isRunning", false);
                    animator.SetBool("isRunningJump", false);
                    animator.SetBool("isSprinting", false);
                }

                if (!isWalkingMoving)
                {
                    animator.SetBool("isWalking", false);
                    animator.SetBool("isRunningJump", false);
                    animator.SetBool("isSprinting", false);
                }

                if (isSprinting && !isMoving)
                {
                    animator.SetBool("isSprinting", false);
                }
                // Hopping

                if (isJumping && isWalking && !isMoving || isJumping && isRunning && !isMoving)
                {
                    animator.SetBool("isHopping", true);
                }
                else
                {
                    animator.SetBool("isHopping", false);
                }
                break;
            case PlayerState.Walking:
                if(isWalkingMoving)
                {
                    animator.SetBool("isRunning", false);
                    animator.SetBool("isWalking", true);
                }

                if (isWalkingMoving && !isSprinting)
                {
                    animator.SetBool("isSprinting", false);
                }
                break;
            case PlayerState.Running:
                if (isRunningMoving)
                {
                    animator.SetBool("isWalking", false);
                    animator.SetBool("isRunning", true);
                }

                if (isRunningMoving && !isJumping)
                {
                    animator.SetBool("isJumping", false);
                }

                if (isRunningMoving && !isSprinting)
                {
                    animator.SetBool("isSprinting", false);
                }
                break;
            case PlayerState.Sprinting:
                if (isSprinting && isRunningMoving)
                {
                    animator.SetBool("isSprinting", true);
                    animator.SetBool("isRunning", false);
                }
                if (isSprinting && isWalkingMoving)
                {
                    animator.SetBool("isSprinting", true);
                    animator.SetBool("isRunning", false);
                }

                if (isSprinting && !isJumping)
                {
                    animator.SetBool("isRunningJump", false);
                }
                break;
            case PlayerState.Jumping:
                if (isWalkingMoving && isJumping)
                {
                    animator.SetBool("isHopping", true);
                }

                if (isRunningMoving && isJumping)
                {
                    animator.SetBool("isRunningJump", true);
                }

                if (isSprinting && isJumping)
                {
                    animator.SetBool("isRunningJump", true);
                }
                break;
            default:
                break;
        }
    }

    public float stepHeight = 0.2f; // Adjust this value for the height of each stair step
    public float stepDuration = 0.5f; // Adjust this value for the duration of the step movement

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Stairs")
        {
            // Get the normal of the collided stair surface
            Vector3 stairNormal = collision.contacts[0].normal;

            // Calculate the position adjustment based on the stair normal and step height
            Vector3 stepUp = stairNormal * stepHeight; // Adjust stepHeight as needed

            // Smoothly move the object upwards to match the stair height
            StartCoroutine(SmoothStepUp(stepUp, stepDuration)); // Implement a coroutine for smooth movement
        }
    }

    // Coroutine for smooth movement
    IEnumerator SmoothStepUp(Vector3 stepUp, float duration)
    {
        float elapsedTime = 0f;
        Vector3 initialPosition = transform.position;
        Vector3 targetPosition = initialPosition + stepUp;

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(initialPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition; // Ensure reaching the exact position
    }

}

public enum PlayerState
{
    Idle,
    Walking,
    Running,
    Jumping,
    Sprinting
}
