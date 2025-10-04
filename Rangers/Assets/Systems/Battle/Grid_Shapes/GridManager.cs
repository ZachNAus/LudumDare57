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
    [SerializeField] private LineFX linePrefab;

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
    // Dictionary maps: coordinate -> (isAlly -> count)
    private Dictionary<Vector2Int, Dictionary<bool, int>> horizontalLineOccupancy = new Dictionary<Vector2Int, Dictionary<bool, int>>();
    private Dictionary<Vector2Int, Dictionary<bool, int>> verticalLineOccupancy = new Dictionary<Vector2Int, Dictionary<bool, int>>();

    // UI line references
    private Dictionary<Vector2Int, LineFX> horizontalLines = new Dictionary<Vector2Int, LineFX>();
    private Dictionary<Vector2Int, LineFX> verticalLines = new Dictionary<Vector2Int, LineFX>();

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
                LineFX line = CreateLine(true, x, y, gridWidth, gridHeight);
                horizontalLines[coord] = line;
            }
        }

        // Create vertical lines (columns)
        for (int x = 0; x <= gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector2Int coord = new Vector2Int(x, y);
                LineFX line = CreateLine(false, x, y, gridWidth, gridHeight);
                verticalLines[coord] = line;
            }
        }

        UpdateDrawGrid();
    }

    /// <summary>
    /// Create a single line UI element
    /// </summary>
    private LineFX CreateLine(bool isHorizontal, int x, int y, float gridWidth, float gridHeight)
    {
        LineFX line = Instantiate(linePrefab, gridContainer);
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

        line.SetColor(emptyColor);
        line.Initialise();
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

        // Track updated lines for pop effect
        HashSet<(Vector2Int coord, bool isHorizontal)> updatedLines = new HashSet<(Vector2Int, bool)>();

        // Read the gridWrapper from the shape and mark white tiles as occupied
        var gridWrapper = inst.GridWrapper;
        if (gridWrapper != null && gridWrapper.rows != null)
        {
            for (int y = 0; y < gridWrapper.rows.Count; y++)
            {
                for (int x = 0; x < gridWrapper.rows[y].colors.Count; x++)
                {
                    if (ShouldSkipCell(x, y))
                        continue;

                    Color cellColor = gridWrapper.rows[y].colors[x];

                    if (IsWhite(cellColor))
                    {
                        bool isHorizontal;
                        Vector2Int lineCoord = MapShapePositionToLineCoord(x, y, out isHorizontal);
                        Vector2Int finalCoord = baseLocation + lineCoord;

                        // Mark line as occupied in the correct dictionary
                        Dictionary<Vector2Int, Dictionary<bool, int>> targetOccupancy = isHorizontal ? horizontalLineOccupancy : verticalLineOccupancy;
                        if (!targetOccupancy.ContainsKey(finalCoord))
                        {
                            targetOccupancy[finalCoord] = new Dictionary<bool, int>();
                        }
                        bool isAllyShape = !enemyTile;
                        if (!targetOccupancy[finalCoord].ContainsKey(isAllyShape))
                        {
                            targetOccupancy[finalCoord][isAllyShape] = 0;
                        }
                        targetOccupancy[finalCoord][isAllyShape]++;

                        // Track this line for pop effect
                        updatedLines.Add((finalCoord, isHorizontal));
                    }
                }
            }
        }

        UpdateDrawGrid();

        // Pop all updated lines
        foreach (var (coord, isHorizontal) in updatedLines)
        {
            Dictionary<Vector2Int, LineFX> targetLines = isHorizontal ? horizontalLines : verticalLines;
            if (targetLines.TryGetValue(coord, out LineFX line) && line != null)
            {
                line.PopLine();
            }
        }
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
    /// Map shape grid position (x, y) to line coordinates
    /// </summary>
    /// <param name="x">X position in shape grid</param>
    /// <param name="y">Y position in shape grid</param>
    /// <param name="isHorizontal">Output: whether this is a horizontal line</param>
    /// <returns>The line coordinate in the shape's local space</returns>
    private Vector2Int MapShapePositionToLineCoord(int x, int y, out bool isHorizontal)
	{
        if (x % 2 == 0 && y % 2 == 0)
		{
            // Even x, Even y → Vertical line
            isHorizontal = false;
            return new Vector2Int(x / 2, y / 2);
		}
        else if (x % 2 == 1 && y % 2 == 0)
		{
            // Odd x, Even y → Horizontal line (top edge of cell)
            isHorizontal = true;
            return new Vector2Int((x - 1) / 2, y / 2);
		}
        else // (x % 2 == 0 && y % 2 == 1)
		{
            // Even x, Odd y → Vertical line (between columns)
            isHorizontal = false;
            return new Vector2Int(x / 2, (y - 1) / 2);
		}
	}

    /// <summary>
    /// Check if a cell should be skipped (green cells)
    /// </summary>
    private bool ShouldSkipCell(int x, int y)
	{
        return (x % 2 == 1 && y % 2 == 1) || (x % 2 == 0 && y % 2 == 0);
	}

    /// <summary>
    /// Looks at the current state of the grid and returns which color the current line is
    /// </summary>
    /// <param name="coords">The coordinate of the line to check</param>
    /// <param name="isHorizontal">Whether this is a horizontal line</param>
    /// <returns>The state of the line based on occupancy</returns>
    public LineState GetLineState(Vector2Int coords, bool isHorizontal)
	{
        Dictionary<Vector2Int, Dictionary<bool, int>> targetOccupancy = isHorizontal ? horizontalLineOccupancy : verticalLineOccupancy;

        if (!targetOccupancy.ContainsKey(coords) || targetOccupancy[coords].Count == 0)
        {
            return LineState.empty;
        }

        Dictionary<bool, int> occupants = targetOccupancy[coords];

        bool hasAllies = occupants.ContainsKey(true) && occupants[true] > 0;
        bool hasEnemies = occupants.ContainsKey(false) && occupants[false] > 0;

        // Check if both allies and enemies occupy this line
        if (hasAllies && hasEnemies)
        {
            return LineState.clash;
        }
        // Only allies
        else if (hasAllies)
        {
            return LineState.ally;
        }
        // Only enemies
        else if (hasEnemies)
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
            if (horizontalLineOccupancy[cell].ContainsKey(true))
            {
                horizontalLineOccupancy[cell].Remove(true); // Remove allies (true)
            }
            if (horizontalLineOccupancy[cell].Count == 0)
            {
                horizontalLineOccupancy.Remove(cell);
            }
        }

        // Remove ally occupancy from vertical lines
        var vCellsToUpdate = verticalLineOccupancy.Keys.ToList();
        foreach (var cell in vCellsToUpdate)
        {
            if (verticalLineOccupancy[cell].ContainsKey(true))
            {
                verticalLineOccupancy[cell].Remove(true); // Remove allies (true)
            }
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
    /// Called when a LineFX object is clicked
    /// </summary>
    public void OnLineClicked(LineFX clickedLine)
	{
        // Find the coordinate and orientation of the clicked line
        Vector2Int? foundCoord = null;
        bool isHorizontal = false;

        // Check horizontal lines
        foreach (var kvp in horizontalLines)
		{
            if (kvp.Value == clickedLine)
			{
                foundCoord = kvp.Key;
                isHorizontal = true;
                break;
			}
		}

        // Check vertical lines if not found
        if (!foundCoord.HasValue)
		{
            foreach (var kvp in verticalLines)
			{
                if (kvp.Value == clickedLine)
				{
                    foundCoord = kvp.Key;
                    isHorizontal = false;
                    break;
				}
			}
		}

        if (!foundCoord.HasValue)
		{
            Debug.LogWarning("GridManager: Clicked line not found in grid!");
            return;
		}

        Vector2Int lineCoord = foundCoord.Value;

        // Get the occupancy dictionary for this line type
        Dictionary<Vector2Int, Dictionary<bool, int>> targetOccupancy = isHorizontal ? horizontalLineOccupancy : verticalLineOccupancy;

        // Check if this line has any ally occupancy
        if (!targetOccupancy.ContainsKey(lineCoord) ||
            !targetOccupancy[lineCoord].ContainsKey(true) ||
            targetOccupancy[lineCoord][true] <= 0)
		{
            Debug.Log("GridManager: Clicked line has no ally shapes.");
            return;
		}

        // Find the first ally shape that occupies this line
        ShapeData shapeToRemove = null;
        foreach (var kvp in appliedShapes)
		{
            if (!kvp.Value.allyTile)
                continue; // Skip enemy shapes

            ShapeData shape = kvp.Key;
            Vector2Int basePosition = kvp.Value.basePosition;

            // Check if this shape occupies the clicked line
            var gridWrapper = shape.GridWrapper;
            if (gridWrapper != null && gridWrapper.rows != null)
			{
                for (int y = 0; y < gridWrapper.rows.Count; y++)
				{
                    for (int x = 0; x < gridWrapper.rows[y].colors.Count; x++)
					{
                        if (ShouldSkipCell(x, y))
                            continue;

                        Color cellColor = gridWrapper.rows[y].colors[x];

                        if (IsWhite(cellColor))
						{
                            bool lineIsHorizontal;
                            Vector2Int lineCoordInShape = MapShapePositionToLineCoord(x, y, out lineIsHorizontal);
                            Vector2Int finalCoord = basePosition + lineCoordInShape;

                            // Check if this matches our clicked line
                            if (lineIsHorizontal == isHorizontal && finalCoord == lineCoord)
							{
                                shapeToRemove = shape;
                                break;
							}
						}
					}
                    if (shapeToRemove != null)
                        break;
				}
			}

            if (shapeToRemove != null)
                break;
		}

        // Remove the shape if found
        if (shapeToRemove != null)
		{
            RemoveSingleShape(shapeToRemove);
		}
	}

    /// <summary>
    /// Remove a single shape from the grid
    /// </summary>
    private void RemoveSingleShape(ShapeData shape)
	{
        if (!appliedShapes.ContainsKey(shape))
		{
            Debug.LogWarning("GridManager: Attempted to remove shape that isn't in appliedShapes!");
            return;
		}

        AppliedShapeData shapeData = appliedShapes[shape];
        Vector2Int basePosition = shapeData.basePosition;
        bool isAlly = shapeData.allyTile;

        // Track updated lines for pop effect
        HashSet<(Vector2Int coord, bool isHorizontal)> updatedLines = new HashSet<(Vector2Int, bool)>();

        // Remove occupancy data for this shape
        var gridWrapper = shape.GridWrapper;
        if (gridWrapper != null && gridWrapper.rows != null)
		{
            for (int y = 0; y < gridWrapper.rows.Count; y++)
			{
                for (int x = 0; x < gridWrapper.rows[y].colors.Count; x++)
				{
                    if (ShouldSkipCell(x, y))
                        continue;

                    Color cellColor = gridWrapper.rows[y].colors[x];

                    if (IsWhite(cellColor))
					{
                        bool isHorizontal;
                        Vector2Int lineCoord = MapShapePositionToLineCoord(x, y, out isHorizontal);
                        Vector2Int finalCoord = basePosition + lineCoord;

                        // Remove from occupancy
                        Dictionary<Vector2Int, Dictionary<bool, int>> targetOccupancy = isHorizontal ? horizontalLineOccupancy : verticalLineOccupancy;
                        if (targetOccupancy.ContainsKey(finalCoord) && targetOccupancy[finalCoord].ContainsKey(isAlly))
						{
                            targetOccupancy[finalCoord][isAlly]--;
                            if (targetOccupancy[finalCoord][isAlly] <= 0)
							{
                                targetOccupancy[finalCoord].Remove(isAlly);
							}
                            if (targetOccupancy[finalCoord].Count == 0)
							{
                                targetOccupancy.Remove(finalCoord);
							}

                            // Track this line for pop effect
                            updatedLines.Add((finalCoord, isHorizontal));
						}
					}
				}
			}
		}

        // Remove from appliedShapes and destroy the object
        appliedShapes.Remove(shape);
        Destroy(shape);

        UpdateDrawGrid();

        // Pop all updated lines
        foreach (var (coord, isHorizontal) in updatedLines)
        {
            Dictionary<Vector2Int, LineFX> targetLines = isHorizontal ? horizontalLines : verticalLines;
            if (targetLines.TryGetValue(coord, out LineFX line) && line != null)
            {
                line.PopLine();
            }
        }
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
            LineFX line = kvp.Value;

            if (line != null)
            {
                LineState state = GetLineState(coord, true);
                line.SetColor(GetColorForState(state));

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
            LineFX line = kvp.Value;

            if (line != null)
            {
                LineState state = GetLineState(coord, false);
                line.SetColor(GetColorForState(state));

                // Bring colored lines to front of hierarchy
                if (state != LineState.empty)
                {
                    line.transform.SetAsLastSibling();
                }
            }
        }
	}
}
