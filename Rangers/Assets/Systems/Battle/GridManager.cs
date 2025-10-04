using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    public enum LineState
	{
        empty,
        ally,
        enemy,
        clash
	}

    [Title("Grid Configuration")]
    [SerializeField] private Vector2Int gridSize = new Vector2Int(15, 15);
    [SerializeField] private float lineThickness = 3f;
    [SerializeField] private float padding = 10f;

    [ReadOnly][SerializeField] private float cellSize;

    [Title("UI References")]
    [SerializeField] private RectTransform gridContainer;
    [SerializeField] private Image linePrefab;

    [Title("Line Colors")]
    [SerializeField] private Color emptyColor = Color.gray;
    [SerializeField] private Color allyColor = Color.blue;
    [SerializeField] private Color enemyColor = Color.red;
    [SerializeField] private Color clashColor = Color.yellow;

    [Title("Runtime Data")]
    [ReadOnly][SerializeField]
    AppliedShapeDict appliedShapes = new AppliedShapeDict();
    [System.Serializable]
    class AppliedShapeDict : SerializableDictionary<ShapeData, AppliedShapeData> { }

    // Grid state tracking
    private Dictionary<Vector2Int, HashSet<bool>> cellOccupancy = new Dictionary<Vector2Int, HashSet<bool>>();

    // UI line references
    private Dictionary<Vector2Int, Image> horizontalLines = new Dictionary<Vector2Int, Image>();
    private Dictionary<Vector2Int, Image> verticalLines = new Dictionary<Vector2Int, Image>();

    struct AppliedShapeData
	{
        public Vector2Int basePosition;
        public bool allyTile;
	}

    private void Awake()
    {
        InitializeGrid();
    }

    /// <summary>
    /// Initialize the grid and spawn all UI lines
    /// </summary>
    [Button("Initialize Grid")]
    private void InitializeGrid()
    {
        if (gridContainer == null || linePrefab == null)
        {
            Debug.LogError("GridManager: Missing grid container or line prefab!");
            return;
        }

        // Clear existing lines
        gridContainer.DestroyAllChildren();
        horizontalLines.Clear();
        verticalLines.Clear();
        cellOccupancy.Clear();

        // Get grid container dimensions with padding
        float availableWidth = gridContainer.rect.width - (padding * 2);
        float availableHeight = gridContainer.rect.height - (padding * 2);

        // Calculate cell size to fit the grid within the container
        cellSize = Mathf.Min(availableWidth / gridSize.x, availableHeight / gridSize.y);

        float gridWidth = gridSize.x * cellSize;
        float gridHeight = gridSize.y * cellSize;

        // Create horizontal lines (rows)
        for (int y = 0; y <= gridSize.y; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                Vector2Int coord = new Vector2Int(x, y);
                Image line = CreateLine(true, x, y, gridWidth, gridHeight);
                horizontalLines[coord] = line;
            }
        }

        // Create vertical lines (columns)
        for (int x = 0; x <= gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector2Int coord = new Vector2Int(x, y);
                Image line = CreateLine(false, x, y, gridWidth, gridHeight);
                verticalLines[coord] = line;
            }
        }

        UpdateDrawGrid();
    }

    /// <summary>
    /// Create a single line UI element
    /// </summary>
    private Image CreateLine(bool isHorizontal, int x, int y, float gridWidth, float gridHeight)
    {
        Image line = Instantiate(linePrefab, gridContainer);
        RectTransform rect = line.rectTransform;

        if (isHorizontal)
        {
            // Horizontal line
            rect.sizeDelta = new Vector2(cellSize, lineThickness);
            float posX = (x * cellSize) - (gridWidth / 2f) + (cellSize / 2f);
            float posY = (y * cellSize) - (gridHeight / 2f);
            rect.anchoredPosition = new Vector2(posX, posY);
        }
        else
        {
            // Vertical line
            rect.sizeDelta = new Vector2(lineThickness, cellSize);
            float posX = (x * cellSize) - (gridWidth / 2f);
            float posY = (y * cellSize) - (gridHeight / 2f) + (cellSize / 2f);
            rect.anchoredPosition = new Vector2(posX, posY);
        }

        line.color = emptyColor;
        return line;
    }

    [Button("Add shape")]
    public void AddShape(ShapeData shape, Vector2Int baseLocation, bool enemyTile)
	{
        var inst = Instantiate(shape);

        if (inst == null)
        {
            Debug.LogWarning("GridManager: Attempted to add null shape");
            return;
        }

        // Store the shape data
        appliedShapes[inst] = new AppliedShapeData
        {
            basePosition = baseLocation,
            allyTile = !enemyTile
        };

        // Mark cells as occupied
        // Note: This assumes shapes occupy a single cell at baseLocation
        // Expand this logic when ShapeData contains actual shape patterns
        if (!cellOccupancy.ContainsKey(baseLocation))
        {
            cellOccupancy[baseLocation] = new HashSet<bool>();
        }
        cellOccupancy[baseLocation].Add(!enemyTile); // true for ally, false for enemy

        UpdateDrawGrid();
	}

    /// <summary>
    /// Looks at the current state of the grid and returns which color the current line is
    /// </summary>
    /// <param name="coords">The coordinate of the line to check</param>
    /// <returns>The state of the line based on occupancy</returns>
    public LineState GetLineState(Vector2Int coords)
	{
        if (!cellOccupancy.ContainsKey(coords) || cellOccupancy[coords].Count == 0)
        {
            return LineState.empty;
        }

        HashSet<bool> occupants = cellOccupancy[coords];

        // Check if both allies and enemies occupy this cell
        if (occupants.Contains(true) && occupants.Contains(false))
        {
            return LineState.clash;
        }
        // Only allies
        else if (occupants.Contains(true))
        {
            return LineState.ally;
        }
        // Only enemies
        else if (occupants.Contains(false))
        {
            return LineState.enemy;
        }

        return LineState.empty;
	}

    /// <summary>
    /// Get the color for a given line state
    /// </summary>
    private Color GetColorForState(LineState state)
    {
        switch (state)
        {
            case LineState.ally:
                return allyColor;
            case LineState.enemy:
                return enemyColor;
            case LineState.clash:
                return clashColor;
            case LineState.empty:
            default:
                return emptyColor;
        }
    }

    /// <summary>
    /// Remove all tiles the allies have placed
    /// </summary>
    [Button("Clear Ally Tiles")]
    public void ClearAllyTiles()
	{
        // Remove ally shapes from appliedShapes
        var allyShapes = appliedShapes.Where(kvp => kvp.Value.allyTile).Select(kvp => kvp.Key).ToList();
        foreach (var shape in allyShapes)
        {
            Destroy(shape);
            appliedShapes.Remove(shape);
        }

        // Remove ally occupancy from cells
        var cellsToUpdate = cellOccupancy.Keys.ToList();
        foreach (var cell in cellsToUpdate)
        {
            cellOccupancy[cell].Remove(true); // Remove allies (true)

            // Clean up empty cells
            if (cellOccupancy[cell].Count == 0)
            {
                cellOccupancy.Remove(cell);
            }
        }

        UpdateDrawGrid();
	}

    /// <summary>
    /// Remove all tiles (both allies and enemies)
    /// </summary>
    [Button("Clear All Tiles")]
    public void ClearAllTiles()
	{
        foreach (var shape in appliedShapes)
        {
            Destroy(shape.Key);
        }

        appliedShapes.Clear();
        cellOccupancy.Clear();
        UpdateDrawGrid();
	}

    /// <summary>
    /// Update the UI to match what the current grid should look like
    /// </summary>
    [Button("Update Grid Display")]
    public void UpdateDrawGrid()
	{
        // Update horizontal lines
        foreach (var kvp in horizontalLines)
        {
            Vector2Int coord = kvp.Key;
            Image line = kvp.Value;

            if (line != null)
            {
                LineState state = GetLineState(coord);
                line.color = GetColorForState(state);
            }
        }

        // Update vertical lines
        foreach (var kvp in verticalLines)
        {
            Vector2Int coord = kvp.Key;
            Image line = kvp.Value;

            if (line != null)
            {
                LineState state = GetLineState(coord);
                line.color = GetColorForState(state);
            }
        }
	}
}
