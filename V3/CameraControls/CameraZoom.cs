using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    #region Value: Camera Scroll zomm & Camera zoom smoothness

    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private CinemachineFramingTransposer transposer;
    [SerializeField] private float defaultDistance = 6.0f;
    [SerializeField] private float minDistance = 1.0f;
    [SerializeField] private float maxDistance = 6.0f;

    [SerializeField] private float smoothing = 4f;
    [SerializeField] private float zoomSensitivity = 1f;

    private float currentTargetDistance;

    #endregion

    private void Start()
    {
        transposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        currentTargetDistance = defaultDistance;
    }

    private void Update()
    {
        float zoomInpt = Input.GetAxis("Mouse ScrollWheel");

        float zoomValue = -zoomInpt * zoomSensitivity;

        currentTargetDistance = Mathf.Clamp(currentTargetDistance + zoomValue, minDistance, maxDistance);

        float currentDistance = transposer.m_CameraDistance;

        if (currentDistance == currentTargetDistance)
        {
            return;
        }

        float lerpedZoomValue = Mathf.Lerp(currentDistance, currentTargetDistance, smoothing * Time.deltaTime);

        transposer.m_CameraDistance = lerpedZoomValue;
    }
}
