using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    #region Value: Camera Scroll zomm & Camera zoom smoothness
    // Reference of the freelook camera that the player / Character is using
    public CinemachineFreeLook freeLookCamera;
    [SerializeField]
    public float zoomSpeed = 7.0f;
    [SerializeField]
    public float minRadius = 1.5f;
    [SerializeField]
    public float maxRadius = 10.0f;
    [SerializeField]
    public float zoomSmooth = 5.0f;
    public float currentRadius = 5.6f;
    public float targetRadius = 5.6f;
    #endregion

    void Update()
    {
        // Get the mouse Scroll wheel input
        float zoomValue = Input.GetAxis("Mouse ScrollWheel");

        // Change the zoom based on mouse scroll input
        if (Mathf.Abs(zoomValue) > 0.01f)
        {
            // This updates the radius which is based on the scoll input
            targetRadius -= zoomValue * zoomSpeed;
            targetRadius = Mathf.Clamp(targetRadius, minRadius, maxRadius);
        }

        // This will smoothen the camera zoom
        currentRadius = Mathf.Lerp(currentRadius, targetRadius, Time.deltaTime * zoomSmooth);

        // list all the orbits with a smoothened value
        List<CinemachineFreeLook.Orbit> modifiedOrbits = new List<CinemachineFreeLook.Orbit>();

        foreach (var orbit in freeLookCamera.m_Orbits)
        {
            CinemachineFreeLook.Orbit modifiedOrbit = new CinemachineFreeLook.Orbit();
            modifiedOrbit = orbit; // Copy the original values
            modifiedOrbit.m_Radius = currentRadius;
            
            modifiedOrbits.Add(modifiedOrbit);
        }

        freeLookCamera.m_Orbits = modifiedOrbits.ToArray();
    }
}
