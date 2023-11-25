using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapPlayerIcon : MonoBehaviour
{
    public Transform player; // Reference to the player's transform

    void Update()
    {
        // Get the player's Z-axis rotation
        float playerZRotation = player.eulerAngles.y;

        // Apply the player's Z-axis rotation to the icon's Z-axis rotation
        Vector3 iconRotation = transform.eulerAngles;
        iconRotation.z = -playerZRotation; // Negate the rotation if needed
        transform.eulerAngles = iconRotation;

    }
}
