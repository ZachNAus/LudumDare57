using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class SelectableShape : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	[SerializeField] Transform shapeHolder;

	[SerializeField] Image linePrefab;

	[SerializeField] private float cellSize = 20f;
	[SerializeField] private float lineThickness = 2f;
	[SerializeField] private Color previewColor = Color.white;

	private List<Image> spawnedLines = new List<Image>();

	private Vector3 originPosition;
	private Vector3 originalHolderScale;
	private bool isDragging = false;
	private Canvas canvas;
	private RectTransform rectTransform;
	private ShapeData currentShape;

	[SerializeField] ShapeData startingShapeData;

	[Button]
    public void Initialise(ShapeData shape)
	{
		currentShape = shape;

		// Clear existing preview
		ClearPreview();

		if (shape == null || shape.GridWrapper == null || shape.GridWrapper.rows == null)
		{
			Debug.LogWarning("SelectableShape: Invalid shape data");
			return;
		}

		var gridWrapper = shape.GridWrapper;
		int gridSize = gridWrapper.rows.Count;

		// Calculate the size of the preview container at scale 1
		float totalWidth = gridSize * cellSize;
		float totalHeight = gridSize * cellSize;

		// Get the parent RectTransform size
		RectTransform parentRect = shapeHolder.parent.GetComponent<RectTransform>();
		RectTransform holderRect = shapeHolder.GetComponent<RectTransform>();

		float availableWidth = 0f;
		float availableHeight = 0f;

		if (parentRect != null && holderRect != null)
		{
			availableWidth = parentRect.rect.width;
			availableHeight = parentRect.rect.height;

			// Calculate scale to fit within parent
			float scaleX = availableWidth / totalWidth;
			float scaleY = availableHeight / totalHeight;
			float finalScale = Mathf.Min(scaleX, scaleY);

			// Apply scale to shapeHolder
			shapeHolder.localScale = Vector3.one * finalScale * 2;

			// Center the shapeHolder
			holderRect.anchoredPosition = Vector2.zero;
		}

		Vector2 centerOffset = new Vector2(-(totalWidth / 4f), -(totalHeight / 4f)) + Vector2.one * (cellSize/4);

		// Spawn lines following the grid logic
		for (int y = 0; y < gridWrapper.rows.Count; y++)
		{
			for (int x = 0; x < gridWrapper.rows[y].colors.Count; x++)
			{
				// Skip green cells (odd x AND odd y)
				if ((x % 2 == 1 && y % 2 == 1) || (x % 2 == 0 && y % 2 == 0))
					continue;

				Color cellColor = gridWrapper.rows[y].colors[x];

				// Only spawn lines for white (non-empty) cells
				if (IsWhite(cellColor))
				{
					bool isHorizontal;
					Vector2 linePos;
					Vector2 lineSize;

					if (x % 2 == 0 && y % 2 == 0)
					{
						// Even x, Even y → Vertical line
						isHorizontal = false;
						lineSize = new Vector2(lineThickness, cellSize + lineThickness);
						linePos = new Vector2((x / 2) * cellSize,
						                      (y / 2) * cellSize + (cellSize / 2f));
					}
					else if (x % 2 == 1 && y % 2 == 0)
					{
						// Odd x, Even y → Horizontal line
						isHorizontal = true;
						lineSize = new Vector2(cellSize + lineThickness, lineThickness);
						linePos = new Vector2(((x - 1) / 2) * cellSize + (cellSize / 2f),
						                      (y / 2) * cellSize);
					}
					else // (x % 2 == 0 && y % 2 == 1)
					{
						// Even x, Odd y → Vertical line
						isHorizontal = false;
						lineSize = new Vector2(lineThickness, cellSize + lineThickness);
						linePos = new Vector2((x / 2) * cellSize,
						                      ((y - 1) / 2) * cellSize + (cellSize / 2f));
					}

					// Apply center offset so grid is centered at (0,0)
					linePos += centerOffset;

					// Create the line
					Image line = Instantiate(linePrefab, shapeHolder);
					RectTransform rect = line.rectTransform;
					rect.sizeDelta = lineSize;
					rect.localPosition = linePos;
					line.color = previewColor;
					line.name = $"{(isHorizontal ? "H" : "V")}_Line ({x},{y})";

					spawnedLines.Add(line);
				}
			}
		}
	}

	private void ClearPreview()
	{
		foreach (var line in spawnedLines)
		{
			if (line != null)
				Destroy(line.gameObject);
		}
		spawnedLines.Clear();
	}

	private bool IsWhite(Color color)
	{
		float threshold = 0.9f;
		return color.r >= threshold && color.g >= threshold && color.b >= threshold && color.a > 0.5f;
	}

	private void Awake()
	{
		rectTransform = GetComponent<RectTransform>();
		canvas = GetComponentInParent<Canvas>();
		originPosition = rectTransform.anchoredPosition;

		Initialise(startingShapeData);
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		isDragging = true;
		originPosition = rectTransform.anchoredPosition;
		originalHolderScale = shapeHolder.localScale;
		shapeHolder.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutQuad);
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (isDragging && canvas != null)
		{
			Vector2 localPoint;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(
				rectTransform.parent as RectTransform,
				eventData.position,
				canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
				out localPoint
			);

			rectTransform.anchoredPosition = localPoint;
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		isDragging = false;

		// Check if we're over the GridManager
		bool placedOnGrid = false;
		if (currentShape != null)
		{
			RectTransform gridRect = GridManager.instance.GetComponent<RectTransform>();
			if (gridRect != null && RectTransformUtility.RectangleContainsScreenPoint(gridRect, eventData.position, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera))
			{
				// Convert screen point to local point in grid
				Vector2 localPoint;
				RectTransformUtility.ScreenPointToLocalPointInRectangle(
					gridRect,
					eventData.position,
					canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
					out localPoint
				);

				// Find the closest grid position
				Vector2Int gridPosition = GetClosestGridPosition(localPoint, gridRect);

				// Place the shape on the grid
				GridManager.instance.AddShape(currentShape, gridPosition, false); // false = ally
				placedOnGrid = true;
			}
		}

		if (!placedOnGrid)
		{
			rectTransform.DOAnchorPos(originPosition, 0.3f).SetEase(Ease.OutBack);
			shapeHolder.DOScale(originalHolderScale, 0.3f).SetEase(Ease.OutBack);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	private Vector2Int GetClosestGridPosition(Vector2 localPoint, RectTransform gridRect)
	{
		// Get grid dimensions from GridManager
		int gridSizeValue = GridManager.instance.GridSize;
		Vector2Int gridSize = new Vector2Int(gridSizeValue, gridSizeValue);

		// The GridManager has padding and calculates actual grid dimensions
		// We need to match that calculation
		float padding = 10f; // GridManager default padding
		float availableWidth = gridRect.rect.width - (padding * 2);
		float availableHeight = gridRect.rect.height - (padding * 2);

		float cellSize = Mathf.Min(availableWidth / gridSize.x, availableHeight / gridSize.y);
		float actualGridWidth = gridSize.x * cellSize;
		float actualGridHeight = gridSize.y * cellSize;

		// localPoint is relative to the gridRect's center
		// Offset to get coordinates relative to top-left of actual grid
		float offsetX = localPoint.x + (actualGridWidth / 2f);
		float offsetY = localPoint.y + (actualGridHeight / 2f);

		// Calculate grid position
		// The shape's visual is centered using this offset in Initialize:
		// centerOffset = new Vector2(-(totalWidth / 4f), -(totalHeight / 4f)) + Vector2.one * (cellSize/4);
		// This means the shape's grid coordinate system is offset from its visual center

		int shapeGridSize = currentShape.gridSize;

		// The visual center is at (shapeGridSize / 2) in shape grid coordinates
		// We need to subtract this to get the position of grid coordinate (0,0)
		float shapeCenterInTiles = shapeGridSize / 4.0f; // Matches the /4 in centerOffset calculation

		float gridPosX = (offsetX / cellSize) - shapeCenterInTiles;
		float gridPosY = (offsetY / cellSize) - shapeCenterInTiles;

		int gridX = Mathf.RoundToInt(gridPosX);
		int gridY = Mathf.RoundToInt(gridPosY);

		// Clamp to grid bounds
		gridX = Mathf.Clamp(gridX, 0, gridSize.x - 1);
		gridY = Mathf.Clamp(gridY, 0, gridSize.y - 1);

		Debug.Log($"Shape gridSize: {currentShape.gridSize}");
		Debug.Log($"Cell Size: {cellSize}, Actual Grid: {actualGridWidth}x{actualGridHeight}");
		Debug.Log($"Offset: ({offsetX}, {offsetY}) -> GridPos: ({gridPosX}, {gridPosY}) -> Final: ({gridX}, {gridY})");

		return new Vector2Int(gridX, gridY);
	}
}
