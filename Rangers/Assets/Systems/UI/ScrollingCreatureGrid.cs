using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Creates a scrolling grid of random creature sprites in the background.
/// Handles wrapping to prevent infinite offscreen movement.
/// </summary>
public class ScrollingCreatureGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    [Tooltip("Size of each grid cell")]
    public float cellSize = 100f;

    [Tooltip("Horizontal scroll speed (units per second)")]
    public float scrollSpeedX = 20f;

    [Tooltip("Vertical scroll speed (units per second)")]
    public float scrollSpeedY = 10f;

    [Tooltip("Starting Y offset - spawns cells this much higher")]
    public float startingYOffset = 0f;

    [Header("Sprite Settings")]
    [Tooltip("Prefab for grid cell (should have Image component)")]
    public CharacterMinorUI cellPrefab;

    [Tooltip("Alpha transparency for creature sprites")]
    [Range(0f, 1f)]
    public float spriteAlpha = 0.2f;

    private RectTransform rectTransform;
    private List<CharacterMinorUI> gridCells = new List<CharacterMinorUI>();
    private List<Vector2> cellWrapOffsets = new List<Vector2>();
    private Vector2 scrollOffset;
    private int gridWidth;
    private int gridHeight;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        GenerateGrid();
    }

    private void GenerateGrid()
    {
        if (cellPrefab == null)
        {
            Debug.LogError("ScrollingCreatureGrid: cellPrefab is not assigned!");
            return;
        }

        if (OwnedCreaturePage.instance == null || OwnedCreaturePage.instance.allCreatures.Count == 0)
        {
            Debug.LogWarning("ScrollingCreatureGrid: No creatures available!");
            return;
        }

        // Calculate how many cells we need to fill the screen (plus extra for wrapping)
        Rect rect = rectTransform.rect;
        // Width: cover full width plus buffer cells on each side
        gridWidth = Mathf.CeilToInt(rect.width / cellSize) + 2;
        // Height: cover full height plus starting offset plus buffer cells for wrapping
        float totalHeight = rect.height + Mathf.Abs(startingYOffset);
        gridHeight = Mathf.CeilToInt(totalHeight / cellSize) + 2;

        // Create grid cells starting from the top
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                CreateCell(x, y);
            }
        }
    }

    private void CreateCell(int x, int y)
    {
        CharacterMinorUI cell = Instantiate(cellPrefab, transform);
        RectTransform cellRect = cell.GetComponent<RectTransform>();

        // Position cell - start from top of rect and go down, plus starting offset
        Rect rect = rectTransform.rect;
        float topY = rect.height / 2f + startingYOffset; // Top edge of the rect plus offset

        cellRect.sizeDelta = new Vector2(cellSize, cellSize);
        cellRect.anchoredPosition = new Vector2(x * cellSize, topY - (y * cellSize));

        // Get random creature sprite
        if (OwnedCreaturePage.instance.allCreatures.Count > 0)
        {
            CreatureData randomCreature = OwnedCreaturePage.instance.allCreatures.GetRandom();
            cell.Setup(randomCreature, null);
            cell.UpdateSpriteColor();
        }

        gridCells.Add(cell);
        cellWrapOffsets.Add(Vector2.zero);
    }

    private void Update()
    {
        if (gridCells.Count == 0) return;

        // Update scroll offset (scrolling down means decreasing Y)
        scrollOffset.x += scrollSpeedX * Time.deltaTime;
        scrollOffset.y += scrollSpeedY * Time.deltaTime;

        Rect rect = rectTransform.rect;
        float topY = rect.height / 2f + startingYOffset;
        float bottomY = -rect.height / 2f;

        // Apply offset to all cells and wrap them
        for (int i = 0; i < gridCells.Count; i++)
        {
            if (gridCells[i] == null) continue;

            RectTransform cellRect = gridCells[i].rectTransform;
            int x = i % gridWidth;
            int y = i / gridWidth;

            // Calculate base position from top (with offset and wrap offset)
            Vector2 basePos = new Vector2(x * cellSize, topY - (y * cellSize));
            cellRect.anchoredPosition = basePos + cellWrapOffsets[i] - scrollOffset;

            Vector2 pos = cellRect.anchoredPosition;

            // Wrap horizontally
            if (pos.x < -cellSize)
                cellWrapOffsets[i] += new Vector2(gridWidth * cellSize, 0);
            else if (pos.x > rect.width + cellSize)
                cellWrapOffsets[i] += new Vector2(-gridWidth * cellSize, 0);

            // Wrap vertically - when cell goes below bottom, teleport to top
            if (pos.y < bottomY - cellSize)
                cellWrapOffsets[i] += new Vector2(0, gridHeight * cellSize);
            else if (pos.y > topY + cellSize)
                cellWrapOffsets[i] += new Vector2(0, -gridHeight * cellSize);
        }
    }
}
