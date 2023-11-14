using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.UIElements;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Player Movement
    public CharacterController CController;
    public Transform groundCheckStart;
    public Transform groundCheckEnd;
    public float groundDistance = 0.1f;
    public float movementSpeed = 3f;
    public float jumpForce = 2.4f;
    public float gravity = -1f;
    public LayerMask groundMask;
    public bool isGrounded = true;

    private Vector3 velocity;
    private Rigidbody rigBody;

    // Camera Movement
    public float sensitivity = 2f;
    public bool invertMouse = false;


    void Start()
    {
        rigBody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        CameraMovement();
        float horizontalInput = Input.GetAxis("Horizontal") * Time.deltaTime * movementSpeed;
        float verticalInput = Input.GetAxis("Vertical") * Time.deltaTime * movementSpeed;

        transform.Translate(horizontalInput, 0, verticalInput);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rigBody.AddForce(new Vector3(0, 3f, 0), ForceMode.Impulse);
            isGrounded = false;
        }
    }

    private void CameraMovement() 
    {
        // Mouse Movement

        float MouseX = Input.GetAxis("Mouse X");
        float MouseY = Input.GetAxis("Mouse Y");

        MouseX *= sensitivity;
        MouseX *= sensitivity;

        if (invertMouse == true)
        {
            MouseY *= -1;
        }

        transform.Rotate(Vector3.up, MouseX);
        transform.Rotate(Vector3.left, MouseY);

        // This will prevent tilting of the Player
        float currentXRotation = transform.eulerAngles.x;
        if (currentXRotation > 180f)
        {
            currentXRotation -= 360f;
        }
        float clampedXRotation = Mathf.Clamp(currentXRotation, -80f, 80f);
        transform.rotation = Quaternion.Euler(clampedXRotation, transform.eulerAngles.y, 0f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Floor")
        {
            isGrounded = true;
            print("Landed");
        }
    }
}
