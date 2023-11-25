using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControls : MonoBehaviour
{
    // Serizalize field allows you to change the value on unity

    #region Variables: Character Speed & Stamina
    [SerializeField]
    private float charSpeed = 1f;
    [SerializeField]
    public float maxStamina = 100f;
    [SerializeField]
    //public Slider staminaSlider;

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
    private bool isDashing;
    #endregion

    #region Variables: Animation
    private Animator animator;
    public Animation characterAnimation;
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
    public void AdjustCameraDamping()
    {
        CinemachineOrbitalTransposer[] transposers = freelookCam.GetComponentsInChildren<CinemachineOrbitalTransposer>();

        // Loop through each transposer (representing each rig: TopRig, MiddleRig, BottomRig)
        foreach (CinemachineOrbitalTransposer transposer in transposers)
        {
            // Adjust damping for each axis
            transposer.m_XDamping = 5.0f; // Adjust X-axis damping
            transposer.m_YDamping = 5.0f; // Adjust Y-axis damping
        }
    }

    /*
     * Main Movement Section
     */
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
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

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

    /*
     * Gravity & Jump Physics Section
     */

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
        if (Input.GetButtonDown("Jump")) // Player can now jump even without moving
        {
            isJumping = true;
            animator.SetBool("isJumping", true);
            verticalSpeed = jumpSpeed;
        }

        Vector3 moveDirection = new Vector3(0, verticalSpeed, 0);
        charControl.Move(moveDirection * Time.deltaTime);

        // Reset the jump flag after the character is grounded again
        if (charControl.isGrounded)
        {
            isJumping = false;
            animator.SetBool("isJumping", false);
        }
    }

    /*
     * Toggle Cursor Section
     */
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
        else if (Input.GetKeyUp(KeyCode.LeftAlt))
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

    /*
     * Running Toggle Section
     */

    void RunningToggle()
    {
        // Running and Walking key toggle
        if (Input.GetKeyDown(KeyCode.LeftControl) && !isJumping)
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
     * Sprinting & Stamina Physics Section
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

        //// Set the value of the slider to the current stamina
        //staminaSlider.value = currentStamina;

        //// Smooth color change from green to red
        //float normalizedStamina = currentStamina / maxStamina;
        //Color lerpedColor = Color.Lerp(lowStaminaColor, fullStaminaColor, normalizedStamina);

        //// Apply the color to the slider's fill area
        //staminaSlider.fillRect.GetComponent<Image>().color = lerpedColor;

        if (Input.GetKeyUp(KeyCode.LeftShift) || currentStamina <= 0)
        {
            isSprinting = false;
            staminaReachedZero = true;
        }

        if (Input.GetKey(KeyCode.LeftShift) && !staminaReachedZero)
        {
            if (!isMovingPlayer)
            {
                isSprinting = false;
            }
            else if (isMovingPlayer && (currentStamina > 0 && !isJumping))
            {
                isSprinting = true;
                charSpeed = 5f;
            }
        }

        /*
         * Change Character Speed Section
         */

        if (isRunning == true && !isSprinting)
        {
            charSpeed = 4f;
        }
        else if (isWalking == true && !isSprinting)
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


    /*
     * Player Animation Section
     */
    void PlayerMovements()
    {
        bool runJumped = animator.GetBool("isJumping");
        bool runSprint = animator.GetBool("isSprinting");
        bool walkJumped = animator.GetBool("isJumping");
        bool walkSprint = animator.GetBool("isSprinting");

        /*
         * Running Animation
         */
        if (isRunning && isMovingPlayer)
        {
            animator.SetBool("isRunning", true);
        }
        else
        {
            animator.SetBool("isRunning", false);
        }

        /*
         * Walking Animation
         */

        if (isWalking && isMovingPlayer)
        {
            animator.SetBool("isWalking", true);
        }
        else
        {
            animator.SetBool("isWalking", false);
        }
        /*
         * All Possibilities when jumping
         */
        if (!runJumped && (isMovingPlayer && isJumping))
        {
            animator.SetBool("isJumping", true);
        }
        else if (runJumped && (!isMovingPlayer || !isJumping))
        {
            animator.SetBool("isJumping", false);
        }

        if (!runSprint && (isMovingPlayer && isSprinting))
        {
            animator.SetBool("isSprinting", true);
        }
        else if (runSprint && (!isMovingPlayer || !isSprinting))
        {
            animator.SetBool("isSprinting", false);
        }

        if (!walkJumped && (isMovingPlayer && isJumping))
        {
            animator.SetBool("isJumping", true);
        }
        else if (walkJumped && (!isMovingPlayer || !isJumping))
        {
            animator.SetBool("isJumping", false);
        }

        if (!walkSprint && (isMovingPlayer && isSprinting))
        {
            animator.SetBool("isSprinting", true);
        }
        else if (walkSprint && (!isMovingPlayer || !isSprinting))
        {
            animator.SetBool("isSprinting", false);
        }
    }
}
