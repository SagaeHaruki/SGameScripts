using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterIK : MonoBehaviour
{
    public float minHeight = 1.0f; // Minimum character height
    public float maxHeight = 2.0f; // Maximum character height

    // Function to adjust the character's height
    public void AdjustHeight(float newHeight)
    {
        // Clamp the new height within the specified range
        float clampedHeight = Mathf.Clamp(newHeight, minHeight, maxHeight);

        // Get the current position of the character
        Vector3 currentPosition = transform.position;

        // Adjust the character's Y position to change its height
        currentPosition.y = clampedHeight;

        // Set the character's position to the new adjusted position
        transform.position = currentPosition;
    }

    // Example usage (you can call this wherever needed)
    void ExampleUsage()
    {
        // Example: If the character needs to crouch
        AdjustHeight(1.0f);

        // Example: If the character needs to stand tall
        AdjustHeight(2.0f);

        // You can adjust the height based on various conditions or game logic
    }
}
