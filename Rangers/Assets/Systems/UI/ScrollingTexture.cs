using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Scrolls the UVRect of a UI RawImage component over time.
/// </summary>
[RequireComponent(typeof(RawImage))]
public class ScrollingTexture : MonoBehaviour
{
    [Header("Scroll Settings")]
    [Tooltip("Horizontal scroll speed (units per second)")]
    public float scrollSpeedX = 0.5f;

    [Tooltip("Vertical scroll speed (units per second)")]
    public float scrollSpeedY = 0f;

    [Header("Optional")]
    [Tooltip("Use unscaled time (ignores Time.timeScale)")]
    public bool useUnscaledTime = false;

    private RawImage rawImage;
    private Vector2 offset;

    private void Awake()
    {
        rawImage = GetComponent<RawImage>();
    }

    private void Update()
    {
        if (rawImage == null) return;

        float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        // Update offset
        offset.x += scrollSpeedX * deltaTime;
        offset.y += scrollSpeedY * deltaTime;

        // Wrap offset to prevent floating point precision issues
        offset.x %= 1f;
        offset.y %= 1f;

        // Apply to UVRect
        Rect uvRect = rawImage.uvRect;
        uvRect.x = offset.x;
        uvRect.y = offset.y;
        rawImage.uvRect = uvRect;
    }
}
