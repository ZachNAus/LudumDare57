using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    public static GridManager instance;

    public enum LineState
	{
        empty,
        ally,
        enemy,
        clash
	}

    [Title("Grid Configuration")]
    [SerializeField] private Vector2Int gridSize = new Vector2Int(15, 15);
    public int GridSize => gridSize.x;
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

    // Grid state tracking - separate for horizontal and vertical lines
    private Dictionary<Vector2Int, HashSet<bool>> horizontalLineOccupancy = new Dictionary<Vector2Int, HashSet<bool>>();
    private Dictionary<Vector2Int, HashSet<bool>> verticalLineOccupancy = new Dictionary<Vector2Int, HashSet<bool>>();

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
        instance = this;

        StartCoroutine(InitializeGridDelayed());
    }

    /// <summary>
    /// Initialize grid after Canvas has been properly laid out
    /// </summary>
    private IEnumerator InitializeGridDelayed()
    {
        // Wait for end of frame to ensure Canvas layout is complete
        yield return new WaitForEndOfFrame();
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
        horizontalLineOccupancy.Clear();
        verticalLineOccupancy.Clear();

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
            // Horizontal line - extend by half thickness on each end
            rect.sizeDelta = new Vector2(cellSize + lineThickness, lineThickness);
            float posX = (x * cellSize) - (gridWidth / 2f) + (cellSize / 2f);
            float posY = (y * cellSize) - (gridHeight / 2f);
            rect.anchoredPosition = new Vector2(posX, posY);
            line.name = $"H_Line ({x},{y})";
        }
        else
        {
            // Vertical line - extend by half thickness on each end
            rect.sizeDelta = new Vector2(lineThickness, cellSize + lineThickness);
            float posX = (x * cellSize) - (gridWidth / 2f);
            float posY = (y * cellSize) - (gridHeight / 2f) + (cellSize / 2f);
            rect.anchoredPosition = new Vector2(posX, posY);
            line.name = $"V_Line ({x},{y})";
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

        // Read the gridWrapper from the shape and mark white tiles as occupied
        var gridWrapper = inst.GridWrapper;
        if (gridWrapper != null && gridWrapper.rows != null)
        {
            for (int y = 0; y < gridWrapper.rows.Count; y++)
            {
                for (int x = 0; x < gridWrapper.rows[y].colors.Count; x++)
                {
                    // Skip green cells (odd x AND odd y only)
                    if ((x % 2 == 1 && y % 2 == 1) || (x % 2 == 0 && y % 2 == 0))
                        continue;

                    Color cellColor = gridWrapper.rows[y].colors[x];

                    // Check if the cell is white (or approximately white)
                    if (IsWhite(cellColor))
                    {
                        // Map shape grid position to horizontal/vertical line coordinates
                        bool isHorizontal;
                        Vector2Int lineCoord;

                        if (x % 2 == 0 && y % 2 == 0)
                        {
                            // Even x, Even y → Vertical line
                            isHorizontal = false;
                            lineCoord = new Vector2Int(x / 2, y / 2);
                        }
                        else if (x % 2 == 1 && y % 2 == 0)
                        {
                            // Odd x, Even y → Horizontal line (top edge of cell)
                            isHorizontal = true;
                            lineCoord = new Vector2Int((x - 1) / 2, y / 2);
                        }
                        else // (x % 2 == 0 && y % 2 == 1)
                        {
                            // Even x, Odd y → Vertical line (between columns)
                            isHorizontal = false;
                            lineCoord = new Vector2Int(x / 2, (y - 1) / 2);
                        }

                        // Apply offset from base location
                        Vector2Int finalCoord = baseLocation + lineCoord;

                        // Mark line as occupied in the correct dictionary
                        Dictionary<Vector2Int, HashSet<bool>> targetOccupancy = isHorizontal ? horizontalLineOccupancy : verticalLineOccupancy;
                        if (!targetOccupancy.ContainsKey(finalCoord))
                        {
                            targetOccupancy[finalCoord] = new HashSet<bool>();
                        }
                        targetOccupancy[finalCoord].Add(!enemyTile); // true for ally, false for enemy
                    }
                }
            }
        }

        UpdateDrawGrid();
	}

    /// <summary>
    /// Check if a color is white or approximately white
    /// </summary>
    private bool IsWhite(Color color)
    {
        // Check if RGB values are close to 1 and alpha is not transparent
        float threshold = 0.9f;
        return color.r >= threshold && color.g >= threshold && color.b >= threshold && color.a > 0.5f;
    }

    /// <summary>
    /// Looks at the current state of the grid and returns which color the current line is
    /// </summary>
    /// <param name="coords">The coordinate of the line to check</param>
    /// <param name="isHorizontal">Whether this is a horizontal line</param>
    /// <returns>The state of the line based on occupancy</returns>
    public LineState GetLineState(Vector2Int coords, bool isHorizontal)
	{
        Dictionary<Vector2Int, HashSet<bool>> targetOccupancy = isHorizontal ? horizontalLineOccupancy : verticalLineOccupancy;

        if (!targetOccupancy.ContainsKey(coords) || targetOccupancy[coords].Count == 0)
        {
            return LineState.empty;
        }

        HashSet<bool> occupants = targetOccupancy[coords];

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

        // Remove ally occupancy from horizontal lines
        var hCellsToUpdate = horizontalLineOccupancy.Keys.ToList();
        foreach (var cell in hCellsToUpdate)
        {
            horizontalLineOccupancy[cell].Remove(true); // Remove allies (true)
            if (horizontalLineOccupancy[cell].Count == 0)
            {
                horizontalLineOccupancy.Remove(cell);
            }
        }

        // Remove ally occupancy from vertical lines
        var vCellsToUpdate = verticalLineOccupancy.Keys.ToList();
        foreach (var cell in vCellsToUpdate)
        {
            verticalLineOccupancy[cell].Remove(true); // Remove allies (true)
            if (verticalLineOccupancy[cell].Count == 0)
            {
                verticalLineOccupancy.Remove(cell);
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
        horizontalLineOccupancy.Clear();
        verticalLineOccupancy.Clear();
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
                LineState state = GetLineState(coord, true);
                line.color = GetColorForState(state);

                // Bring colored lines to front of hierarchy
                if (state != LineState.empty)
                {
                    line.transform.SetAsLastSibling();
                }
            }
        }

        // Update vertical lines
        foreach (var kvp in verticalLines)
        {
            Vector2Int coord = kvp.Key;
            Image line = kvp.Value;

            if (line != null)
            {
                LineState state = GetLineState(coord, false);
                line.color = GetColorForState(state);

                // Bring colored lines to front of hierarchy
                if (state != LineState.empty)
                {
                    line.transform.SetAsLastSibling();
                }
            }
        }
	}
}
