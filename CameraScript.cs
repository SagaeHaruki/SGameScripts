using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    // Mouse Sensitivity
    [SerializeField]
    private float mouseSensi = 2.0f;
    // Mouse Smoothness Time
    [SerializeField]
    private float mouseSmooth = 0.1f;
    // Distance of camera to the player
    [SerializeField]
    private float TargetDistance = 3.5f;
    // Maximum rotation of the camera
    [SerializeField]
    private Vector2 CameraRotationMAX = new Vector2(-40, 40);
    // The Target player 
    [SerializeField]
    private Transform TargetPlayer;
    
    private float XRotation;
    private float YRotation;

    private Vector3 CurrentCamRot;
    private Vector3 Smoothness = Vector3.zero;

    void Start()
    {
        GameObject playerTarget = GameObject.FindGameObjectWithTag("Main Player");
        TargetPlayer = playerTarget.transform;
    }

    void Update()
    {
        float Xmouse = Input.GetAxis("Mouse X") * mouseSensi;
        float Ymouse = Input.GetAxis("Mouse Y") * mouseSensi;

        XRotation += Ymouse;
        YRotation += Xmouse;

        XRotation = Mathf.Clamp(XRotation, CameraRotationMAX.x, CameraRotationMAX.y);

        Vector3 nextRotation = new Vector3 (XRotation, YRotation);

        CurrentCamRot = Vector3.SmoothDamp(CurrentCamRot, nextRotation, ref Smoothness, mouseSmooth);
        transform.localEulerAngles = CurrentCamRot;

        transform.position = TargetPlayer.position - transform.forward * TargetDistance;
    }
}
