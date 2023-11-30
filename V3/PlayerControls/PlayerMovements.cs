using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

public class PlayerMovements : MonoBehaviour
{
    #region Variables: Character Control & Camera
    [SerializeField]
    public CharacterController charControl;
    public CinemachineFreeLook freelookCam;
    public Transform MainCamera;
    private bool cameraEnabled = true;
    #endregion

    #region Character & rotation Speed & Gravity
    [SerializeField]
    private PlayerState currentState;
    [SerializeField]
    private float charSpeed = 1f;
    [SerializeField]
    public float jumpSpeed = 5.4f;
    [SerializeField]
    public float gravity = 20.0f;
    [SerializeField]
    private float verticalSpeed = 0.0f;
    [SerializeField]
    private bool isMoving;
    [SerializeField]
    private bool isWalking;
    [SerializeField]
    private bool isRunning;
    [SerializeField]
    private bool isSprinting;
    [SerializeField]
    private bool isJumping;
    [SerializeField]
    private bool isHopping;
    #endregion

    #region Value: Ground Distance & Mask
    [SerializeField]
    public float groundDistance = 0.1f;
    public LayerMask groundMask;
    #endregion

    #region Variables: Camera Motions
    // Character & Camera
    public float turnSmoothing = 0.1f; // Player direction smoothing
    public Transform CameraAngle; // Default camera angle
    float smoothingVelocity;
    #endregion

    private Animator animator;

    private void Start()
    {
        charControl = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        isRunning = true;
        // Hides the cursor
        Cursor.visible = false;
        // Lock the cursor to the center of the screen
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        GravityPhysics();
        ChangeState();
        CheckKeyPressed();
        float horizontal = Input.GetKey(KeyCode.A) ? -1f : Input.GetKey(KeyCode.D) ? 1f : 0f;
        float vertical = Input.GetKey(KeyCode.W) ? 1f : Input.GetKey(KeyCode.S) ? -1f : 0f;

        // Get the direction based on the movement, always normalize movement
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        // Direction Movement
        if (direction.magnitude >= 0.1f)
        {
            if (!isHopping)
            {
                // Calculate the direction of the player
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + MainCamera.eulerAngles.y;
                // This will smoothen the rotation of the angle (More math)
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref smoothingVelocity, turnSmoothing);
                // Rotates the player based on the direction
                transform.rotation = Quaternion.Euler(0f, angle, 0f);

                // Direction base on the camera angle
                Vector3 newDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                // This will move the character
                charControl.Move(newDirection.normalized * charSpeed * Time.deltaTime);
            }
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }
    }

    private void CheckKeyPressed()
    {
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
        if (Input.GetButtonDown("Jump") && !isJumping) // Player can now jump even without moving
        {
            isJumping = true;
            verticalSpeed = jumpSpeed;
            if (isWalking)
            {
                isHopping = true;
            }
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
                currentState = PlayerState.Running;
                charSpeed = 3.2f;
            }
            else
            {
            }

            if (isWalking)
            {
                currentState = PlayerState.Walking;
                charSpeed = 2.4f;
            }
            else
            {
            }

            if (isSprinting)
            {
                currentState = PlayerState.Sprinting;
                charSpeed = 6.7f;
            }
            else
            {
            }

            if (isJumping)
            {
                currentState = PlayerState.Jumping;
            }
        }
        else 
        {
            currentState = PlayerState.Idle;
        }
    }

    private void GravityPhysics()
    {
        if (charControl.isGrounded && verticalSpeed < 0.0f)
        {
            verticalSpeed = (-1.0f);
        }
        else
        {
            verticalSpeed -= gravity * Time.deltaTime;
        }

        Vector3 moveDirection = new Vector3(0, verticalSpeed, 0);
        charControl.Move(moveDirection * Time.deltaTime);
    }

    void FixedUpdate()
    {
        bool runJumped = animator.GetBool("isJumping");
        bool runSprint = animator.GetBool("isSprinting");
        bool walkJumped = animator.GetBool("isJumping");
        bool walkSprint = animator.GetBool("isSprinting");

        // Use the current state to perform actions or behaviors
        switch (currentState)
        {
            case PlayerState.Idle:
                if (isRunning && !isMoving)
                {
                    animator.SetBool("isRunning", false);
                }

                if (isWalking && !isMoving)
                {
                    animator.SetBool("isWalking", false);
                }

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
                if(isWalking && isMoving)
                {
                    animator.SetBool("isRunning", false);
                    animator.SetBool("isWalking", true);
                }
                break;
            case PlayerState.Running:
                if (isRunning && isMoving)
                {
                    animator.SetBool("isWalking", false);
                    animator.SetBool("isRunning", true);
                }
                break;
            case PlayerState.Sprinting:
                
                break;
            case PlayerState.Jumping:
                if (isWalking && isJumping)
                {
                    animator.SetBool("isHopping", true);
                }
                break;
            default:
                break;
        }
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
