using UnityEngine;

/// <summary>
/// Creates a parallax scrolling effect for background layers.
/// The layer moves proportionally to camera movement to simulate depth.
/// 
/// Parallax effect guideline:
/// - 0.0  = layer stays fixed (moves with camera, e.g., sky)
/// - 0.5  = moves half the camera speed (distant mountains)
/// - 1.0  = moves at camera speed (foreground elements)
/// </summary>
public class ParallaxLayer : MonoBehaviour
{
    [Header("Parallax Settings")]
    [Range(0f, 1f)]
    public float parallaxEffect = 0.5f;

    private Transform cameraTransform;
    private Vector3 lastCameraPosition;

    private void Start()
    {
        // Cache the main camera transform once for performance.
        // Note: Ensure your camera is tagged as "MainCamera".
        cameraTransform = Camera.main.transform;

        // Store initial camera position to measure movement deltas.
        lastCameraPosition = cameraTransform.position;
    }

    /// <summary>
    /// LateUpdate is used so the parallax layer updates after the camera has moved,
    /// preventing jitter and keeping movement smooth.
    /// </summary>
    private void LateUpdate()
    {
        // Calculate how far the camera moved since last frame.
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;

        // Move the background layer relative to camera movement (X axis only for 2D).
        transform.position += new Vector3(deltaMovement.x * parallaxEffect, 0, 0);

        // Update camera position for the next frame.
        lastCameraPosition = cameraTransform.position;
    }
}
