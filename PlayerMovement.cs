using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    // Serizalize field allows you to change the value on unity

    #region Variables: Character Speed & Stamina
    [SerializeField] 
    private float charSpeed = 1f;
    [SerializeField]
    public float maxStamina = 100f;
    [SerializeField]
    public Slider staminaSlider;

    #endregion

    #region Variables: Camera Motions
    // Character & Camera
    public float turnSmoothing = 0.1f; // Player direction smoothing
    public Transform CameraAngle; // Default camera angle
    float smoothingVelocity;
    #endregion

    #region Variables: Character Control & Camera
    [SerializeField]
    public CharacterController charControl;
    public CinemachineFreeLook freelookCam;
    private bool cameraEnabled = true;
    #endregion

    #region Variables: Walking, Running, Sprinting, Jumping & Idle
    [SerializeField]
    public bool isSprinting;
    [SerializeField]
    public bool isJumping;
    [SerializeField]
    private bool isWalking;
    [SerializeField]
    private bool isRunning = true;
    private string moveStatus = "";
    #endregion

    #region Variables: Stamina Depletion & Regeneration
    public float currentStamina;
    public float runningstamina = 7f;
    public float walkingstamina = 12f;
    public float staminaDeplete = 15f;
    public bool staminaReachedZero;
    #endregion

    #region Variables: Gravity Physics
    [SerializeField]
    public float jumpSpeed = 5.0f;
    [SerializeField]
    public float gravity = 20.0f;
    [SerializeField]
    private float verticalSpeed = 0.0f;
    #endregion

    #region Value: Ground Distance & Mask
    [SerializeField]
    public float groundDistance = 0.1f;
    public LayerMask groundMask;
    #endregion

    #region Variables: Character Dashing
    [SerializeField]
    public float dashDuration = 0.2f; // Duration of the dash
    [SerializeField]
    public float dashCooldown = 1.2f; // Cooldown between dashes
    [SerializeField]
    public float dashSpeed = 8.2f; // Dash Speed
    private float cooldownTimer = 0.0f;
    #endregion

    #region Variables: Animation
    private Animator animator;
    private bool isMovingPlayer;
    #endregion

    #region Character Hitting
    private bool isHitting;
    #endregion

    private void Start()
    {
        animator = GetComponent<Animator>();
        currentStamina = maxStamina;
        // Hides the cursor
        Cursor.visible = false;
        // Lock the cursor to the center of the screen
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        GravityAndJump();
        SprintingStatus();
        RunningToggle();

        /*
         * Keyboard inputs (wasd or arrows)
         */
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Get the direction based on the movement, always normalize movement
        Vector3 direction = new Vector3 (horizontal, 0f, vertical).normalized;

        // Direction Movement
        if (direction.magnitude >= 0.1f)
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
            isMovingPlayer = true;
        }
        else
        {
            isMovingPlayer = false;
        }
        PlayerMovements();
        UnhideCursor();

    }

    void GravityAndJump()
    {
        if (charControl.isGrounded && verticalSpeed < 0.0f)
        {
            verticalSpeed = -1.0f;
        }
        else
        {
            verticalSpeed -= gravity * Time.deltaTime;
        }

        // Make the Player Jump
        if (Input.GetButtonDown("Jump") && !isJumping) // Player can now jump even without moving
        {
            isJumping = true;
            verticalSpeed = jumpSpeed;
        }

        Vector3 moveDirection = new Vector3(0, verticalSpeed, 0);
        charControl.Move(moveDirection * Time.deltaTime);

        // Reset the jump flag after the character is grounded again
        if (charControl.isGrounded)
        {
            isJumping = false;
        }
    }


    // Player Animation 
    void PlayerMovements()
    {
        // This section is the walking
        if (isWalking == true && isMovingPlayer == true)
        {
            // Walk to true
            animator.SetBool("isWalking", true);
        }
        else
        {
            // Walk to false
            animator.SetBool("isWalking", false);
        }

        // Running Section
        if (isRunning == true && isMovingPlayer == true)
        {
            // Running to true
            animator.SetBool("isRunning", true);
        }
        else
        {
            // Running to true
            animator.SetBool("isRunning", false);
        }

        // Walk to Sprint
        if(isSprinting == true && isMovingPlayer == true)
        {
            animator.SetBool("isSprinting", true);
        }
        else
        {
            animator.SetBool("isSprinting", false);
        }
    }
    void UnhideCursor()
    {
        // Show or Hide cursor on Keypress and KeyRelease
        // Stops the camera movement when the key is pressed
        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            // This Disables the camera
            cameraEnabled = false;
            // Unhides the cursor
            Cursor.visible = true;
            // Unlocks the cursor to the center of the screen
            Cursor.lockState = CursorLockMode.None;
        }
        else if(Input.GetKeyUp(KeyCode.LeftAlt))
        {
            // This Enables the Camera
            cameraEnabled = true;
            // Hides the cursor
            Cursor.visible = true;
            // Lock the cursor to the center of the screen
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (freelookCam != null)
        {
            CinemachineCore.GetInputAxis = GetInputAxis;
            // Disable or Enable the Axis Input for the camera
            freelookCam.m_XAxis.m_InputAxisName = cameraEnabled ? "Mouse X" : "";
            freelookCam.m_YAxis.m_InputAxisName = cameraEnabled ? "Mouse Y" : "";
        }
    }

    private float GetInputAxis(string axisName)
    {
        return cameraEnabled ? Input.GetAxis(axisName) : 0f;
    }

    void RunningToggle()
    {
        // Running and Walking key toggle
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (isRunning == false)
            {
                isRunning = true;
                isWalking = false;
                return;
            }
            else if (isRunning == true)
            {
                isRunning = false;
                isWalking = true; 
                return;
            }
        }
    }

    public Color fullStaminaColor = Color.green; // Color for high stamina
    public Color lowStaminaColor = Color.red; // Color for low stamina

    /*
     * Sprinting & Stamina Physics
     */

    void SprintingStatus()
    {
        /*
         * Stamina Regeneration
         * Regeneration are different base on if the player is running or walking
         * if the player is walking stamina regenerates faster
         * if the player is running stamina regenerates a bit slower
         */

        if (!isSprinting && isWalking && currentStamina < maxStamina)
        {
            currentStamina = Mathf.Min(maxStamina, currentStamina + walkingstamina * Time.deltaTime);
        }
        else if (!isSprinting && isRunning && currentStamina < maxStamina)
        {
            currentStamina = Mathf.Min(maxStamina, currentStamina + runningstamina * Time.deltaTime);
        }

        // Set the value of the slider to the current stamina
        staminaSlider.value = currentStamina;

        // Smooth color change from green to red
        float normalizedStamina = currentStamina / maxStamina;
        Color lerpedColor = Color.Lerp(lowStaminaColor, fullStaminaColor, normalizedStamina);

        // Apply the color to the slider's fill area
        staminaSlider.fillRect.GetComponent<Image>().color = lerpedColor;

        /*
         * Statement 1
         * If the player is holding the sprint while moving, set the value of isSprinting to true
         * if the player is holding the sprint while not moving, reset it to false
         */

        // Get the Keyup for the lshift

        if (Input.GetKeyUp(KeyCode.LeftShift) || currentStamina <= 0)
        {
            isSprinting = false;
            staminaReachedZero = true;
            Task.Delay(TimeSpan.FromSeconds(0.30));
            if (moveStatus == "wasWalking")
            {
                isWalking = true;
                return;
            }
            else if (moveStatus == "wasRunning")
            {
                isRunning = true;
                return;
            }
        }

        if (Input.GetKey(KeyCode.LeftShift) && !staminaReachedZero)
        {
            if (!isMovingPlayer)
            {
                isSprinting = false;
                Task.Delay(TimeSpan.FromSeconds(0.30));
                if (moveStatus == "wasWalking")
                {
                    isWalking = true;
                    return;
                }
                else if (moveStatus == "wasRunning")
                {
                    isRunning = true;
                    return;
                }
            }
            else if (isMovingPlayer && currentStamina > 0)
            {
                isSprinting = true;
                charSpeed = 6.2f;
                if (isWalking == true)
                {
                    moveStatus = "wasWalking";
                }
                else if (isRunning == true)
                {
                    moveStatus = "wasRunning";
                }
                Task.Delay(TimeSpan.FromSeconds(0.30));
                isWalking = false;
                isRunning = false;
            }
        }

        /*
         * Change Character Speed Section
         */

        if (isRunning == true)
        {
            charSpeed = 4.2f;
        }
        else if (isWalking == true)
        {
            charSpeed = 1f;
        }

        if (currentStamina > 30)
        {
            staminaReachedZero = false;
        }

        // This Section Depletes the player's Stamina
        if (isSprinting == true && isMovingPlayer == true)
        {
            currentStamina -= staminaDeplete * Time.deltaTime;
        }
    }
}
