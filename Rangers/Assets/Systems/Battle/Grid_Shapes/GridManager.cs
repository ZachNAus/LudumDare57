using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    public static GridManager instance;

    public enum CellState
	{
        empty,
        ally,
        enemy,
        clash
	}

    [Title("Grid Configuration")]
    [SerializeField] private Vector2Int gridSize = new Vector2Int(15, 15);
    public int GridSize => gridSize.x;
    [SerializeField] private float padding = 10f;

    [ReadOnly][SerializeField] private float cellSize;
    public float CellSize => cellSize;

    [Title("UI References")]
    [SerializeField] private RectTransform gridContainer;
    [SerializeField] private CellFX cellPrefab;

    [Title("Cell Colors")]
    [SerializeField] private Color emptyColor = Color.gray;
    [SerializeField] private Color allyColor = Color.blue;
    [SerializeField] private Color enemyColor = Color.red;
    [SerializeField] private Color clashColor = Color.yellow;

    [Title("Runtime Data")]
    [ReadOnly][SerializeField]
    AppliedShapeDict appliedShapes = new AppliedShapeDict();
    [System.Serializable]
    class AppliedShapeDict : SerializableDictionary<ShapeData, AppliedShapeData> { }

    // Grid state tracking - cell occupancy
    // Dictionary maps: coordinate -> (isAlly -> count)
    private Dictionary<Vector2Int, Dictionary<bool, int>> cellOccupancy = new Dictionary<Vector2Int, Dictionary<bool, int>>();

    // UI cell references
    private Dictionary<Vector2Int, CellFX> cells = new Dictionary<Vector2Int, CellFX>();

    /// <summary>
    /// Get the cell at a world position (returns null if not over any cell)
    /// </summary>
    public CellFX GetCellAtWorldPosition(Vector3 worldPosition)
	{
        foreach (var kvp in cells)
		{
            CellFX cell = kvp.Value;
            if (cell != null && RectTransformUtility.RectangleContainsScreenPoint(
                cell.rectTransform,
                worldPosition,
                GetComponentInParent<Canvas>().renderMode == RenderMode.ScreenSpaceOverlay ? null : GetComponentInParent<Canvas>().worldCamera))
			{
                return cell;
			}
		}
        return null;
	}

    struct AppliedShapeData
	{
        public Vector2Int basePosition;
        public bool allyTile;
        public string creatureUniqueID;
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
    /// Initialize the grid and spawn all UI cells
    /// </summary>
    [Button("Initialize Grid")]
    private void InitializeGrid()
    {
        if (gridContainer == null || cellPrefab == null)
        {
            Debug.LogError("GridManager: Missing grid container or cell prefab!");
            return;
        }

        // Clear existing cells
        gridContainer.DestroyAllChildren();
        cells.Clear();
        cellOccupancy.Clear();

        // Get grid container dimensions with padding
        float availableWidth = gridContainer.rect.width - (padding * 2);
        float availableHeight = gridContainer.rect.height - (padding * 2);

        // Calculate cell size to fit the grid within the container
        cellSize = Mathf.Min(availableWidth / gridSize.x, availableHeight / gridSize.y);

        float gridWidth = gridSize.x * cellSize;
        float gridHeight = gridSize.y * cellSize;

        // Create cells
        for (int y = 0; y < gridSize.y; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                Vector2Int coord = new Vector2Int(x, y);
                CellFX cell = CreateCell(x, y, gridWidth, gridHeight);
                cells[coord] = cell;
            }
        }

        UpdateDrawGrid();
    }

    /// <summary>
    /// Create a single cell UI element
    /// </summary>
    private CellFX CreateCell(int x, int y, float gridWidth, float gridHeight)
    {
        CellFX cell = Instantiate(cellPrefab, gridContainer);
        RectTransform rect = cell.rectTransform;

        // Set cell size
        rect.sizeDelta = new Vector2(cellSize, cellSize);

        // Position cell (centered grid)
        float posX = (x * cellSize) - (gridWidth / 2f) + (cellSize / 2f);
        float posY = (y * cellSize) - (gridHeight / 2f) + (cellSize / 2f);
        rect.anchoredPosition = new Vector2(posX, posY);

        cell.name = $"Cell ({x},{y})";
        cell.SetColor(emptyColor);
        cell.Initialise(new Vector2Int(x, y));
        return cell;
    }

    public List<CellFX> GetAllCellsInState(CellState state)
	{
        List<CellFX> result = new List<CellFX>();

        foreach (var kvp in cells)
        {
            Vector2Int coord = kvp.Key;
            CellFX cell = kvp.Value;

            if (GetCellState(coord) == state)
            {
                result.Add(cell);
            }
        }

        return result;
	}

    [Button("Add shape")]
    public void AddShape(ShapeData shape, Vector2Int baseLocation, bool enemyTile, string creatureUniqueID = "")
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
            allyTile = !enemyTile,
            creatureUniqueID = creatureUniqueID
        };

        // Track updated cells for pop effect
        HashSet<Vector2Int> updatedCells = new HashSet<Vector2Int>();

        // Read the gridWrapper from the shape and mark non-transparent cells as occupied
        var gridWrapper = inst.GridWrapper;
        if (gridWrapper != null && gridWrapper.rows != null)
        {
            for (int y = 0; y < gridWrapper.rows.Count; y++)
            {
                for (int x = 0; x < gridWrapper.rows[y].colors.Count; x++)
                {
                    Color cellColor = gridWrapper.rows[y].colors[x];

                    // Only process cells that are not transparent
                    if (cellColor.a > 0.5f)
                    {
                        Vector2Int cellCoord = new Vector2Int(x, y);
                        Vector2Int finalCoord = baseLocation + cellCoord;

                        // Mark cell as occupied
                        if (!cellOccupancy.ContainsKey(finalCoord))
                        {
                            cellOccupancy[finalCoord] = new Dictionary<bool, int>();
                        }
                        bool isAllyShape = !enemyTile;
                        if (!cellOccupancy[finalCoord].ContainsKey(isAllyShape))
                        {
                            cellOccupancy[finalCoord][isAllyShape] = 0;
                        }
                        cellOccupancy[finalCoord][isAllyShape]++;

                        // Track this cell for pop effect
                        updatedCells.Add(finalCoord);
                    }
                }
            }
        }

        UpdateDrawGrid();

        // Pop all updated cells
        foreach (var coord in updatedCells)
        {
            if (cells.TryGetValue(coord, out CellFX cell) && cell != null)
            {
                cell.PopLine();
            }
        }
	}

    /// <summary>
    /// Looks at the current state of the grid and returns which color the current cell is
    /// </summary>
    /// <param name="coords">The coordinate of the cell to check</param>
    /// <returns>The state of the cell based on occupancy</returns>
    public CellState GetCellState(Vector2Int coords)
	{
        if (!cellOccupancy.ContainsKey(coords) || cellOccupancy[coords].Count == 0)
        {
            return CellState.empty;
        }

        Dictionary<bool, int> occupants = cellOccupancy[coords];

        bool hasAllies = occupants.ContainsKey(true) && occupants[true] > 0;
        bool hasEnemies = occupants.ContainsKey(false) && occupants[false] > 0;

        // Check if both allies and enemies occupy this cell
        if (hasAllies && hasEnemies)
        {
            return CellState.clash;
        }
        // Only allies
        else if (hasAllies)
        {
            return CellState.ally;
        }
        // Only enemies
        else if (hasEnemies)
        {
            return CellState.enemy;
        }

        return CellState.empty;
	}

    /// <summary>
    /// Get the color for a given cell state
    /// </summary>
    private Color GetColorForState(CellState state)
    {
        switch (state)
        {
            case CellState.ally:
                return allyColor;
            case CellState.enemy:
                return enemyColor;
            case CellState.clash:
                return clashColor;
            case CellState.empty:
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
            if (cellOccupancy[cell].ContainsKey(true))
            {
                cellOccupancy[cell].Remove(true); // Remove allies (true)
            }
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
    /// Called when a LineFX cell is clicked
    /// </summary>
    public void OnCellClicked(CellFX clickedCell)
	{
        // Find the coordinate of the clicked cell
        Vector2Int? foundCoord = null;

        foreach (var kvp in cells)
		{
            if (kvp.Value == clickedCell)
			{
                foundCoord = kvp.Key;
                break;
			}
		}

        if (!foundCoord.HasValue)
		{
            Debug.LogWarning("GridManager: Clicked cell not found in grid!");
            return;
		}

        Vector2Int cellCoord = foundCoord.Value;

        // Check if this cell has any ally occupancy
        if (!cellOccupancy.ContainsKey(cellCoord) ||
            !cellOccupancy[cellCoord].ContainsKey(true) ||
            cellOccupancy[cellCoord][true] <= 0)
		{
            Debug.Log("GridManager: Clicked cell has no ally shapes.");
            return;
		}

        // Find the first ally shape that occupies this cell
        ShapeData shapeToRemove = null;
        foreach (var kvp in appliedShapes)
		{
            if (!kvp.Value.allyTile)
                continue; // Skip enemy shapes

            ShapeData shape = kvp.Key;
            Vector2Int basePosition = kvp.Value.basePosition;

            // Check if this shape occupies the clicked cell
            var gridWrapper = shape.GridWrapper;
            if (gridWrapper != null && gridWrapper.rows != null)
			{
                for (int y = 0; y < gridWrapper.rows.Count; y++)
				{
                    for (int x = 0; x < gridWrapper.rows[y].colors.Count; x++)
					{
                        Color cellColor = gridWrapper.rows[y].colors[x];

                        if (cellColor.a > 0.5f)
						{
                            Vector2Int cellCoordInShape = new Vector2Int(x, y);
                            Vector2Int finalCoord = basePosition + cellCoordInShape;

                            // Check if this matches our clicked cell
                            if (finalCoord == cellCoord)
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

    public static event System.Action<ShapeData, string> OnShapeRemoved;

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

        // Track updated cells for pop effect
        HashSet<Vector2Int> updatedCells = new HashSet<Vector2Int>();

        // Remove occupancy data for this shape
        var gridWrapper = shape.GridWrapper;
        if (gridWrapper != null && gridWrapper.rows != null)
		{
            for (int y = 0; y < gridWrapper.rows.Count; y++)
			{
                for (int x = 0; x < gridWrapper.rows[y].colors.Count; x++)
				{
                    Color cellColor = gridWrapper.rows[y].colors[x];

                    if (cellColor.a > 0.5f)
					{
                        Vector2Int cellCoord = new Vector2Int(x, y);
                        Vector2Int finalCoord = basePosition + cellCoord;

                        // Remove from occupancy
                        if (cellOccupancy.ContainsKey(finalCoord) && cellOccupancy[finalCoord].ContainsKey(isAlly))
						{
                            cellOccupancy[finalCoord][isAlly]--;
                            if (cellOccupancy[finalCoord][isAlly] <= 0)
							{
                                cellOccupancy[finalCoord].Remove(isAlly);
							}
                            if (cellOccupancy[finalCoord].Count == 0)
							{
                                cellOccupancy.Remove(finalCoord);
							}

                            // Track this cell for pop effect
                            updatedCells.Add(finalCoord);
						}
					}
				}
			}
		}

        // Remove from appliedShapes and destroy the object
        string creatureID = shapeData.creatureUniqueID;
        appliedShapes.Remove(shape);

        //Send an event out
        OnShapeRemoved?.Invoke(shape, creatureID);

        Destroy(shape);

        UpdateDrawGrid();

        // Pop all updated cells
        foreach (var coord in updatedCells)
        {
            if (cells.TryGetValue(coord, out CellFX cell) && cell != null)
            {
                cell.PopLine();
            }
        }
	}

    /// <summary>
    /// Update the UI to match what the current grid should look like
    /// </summary>
    [Button("Update Grid Display")]
    public void UpdateDrawGrid()
	{
        // Update cells
        foreach (var kvp in cells)
        {
            Vector2Int coord = kvp.Key;
            CellFX cell = kvp.Value;

            if (cell != null)
            {
                CellState state = GetCellState(coord);
                cell.SetColor(GetColorForState(state));

                // Bring colored cells to front of hierarchy
                if (state != CellState.empty)
                {
                    cell.transform.SetAsLastSibling();
                }
            }
        }
	}
}
