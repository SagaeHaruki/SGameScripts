using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    /*
     * Player Movement And RigidBody
     */

    Rigidbody rigBody;
    [SerializeField] float movementSpeed = 3f;
    [SerializeField] float jumpForce = 4f;


    /*
     * Camera Movement
     */
    public float sensitivity = 2f;
    public bool invertMouse = false;


    void Start()
    {
        rigBody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        CameraMovement();
        PlayerMove();
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

    private void PlayerMove()
    {
        // Player Movement
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(horizontalInput * movementSpeed, rigBody.velocity.y, verticalInput * movementSpeed).normalized;

        // rigBody.velocity = new Vector3(horizontalInput * movementSpeed, rigBody.velocity.y, verticalInput * movementSpeed);

        transform.Translate(moveDirection * movementSpeed * Time.deltaTime, Space.Self);

        if (Input.GetKeyDown("space"))
        {
            rigBody.velocity = new Vector3(rigBody.velocity.x, 3f, rigBody.velocity.z);
        }
    }
}
