using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class SelectableShape : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	[SerializeField] Image mainImg;
	[SerializeField] Image borderImg;

	[SerializeField] ColorDict baseColors = new ColorDict();

	[System.Serializable]
	class ColorDict : SerializableDictionary<AttackRarity, ColorSettings> { }

	//[SerializeField] ColorSettings mainColor;
	[SerializeField] ColorSettings greyedColor;

	[System.Serializable]
	public class ColorSettings
	{
		public Color mainColor;
		public Color borderColor;
	}

	[Space]

	[SerializeField] Transform shapeHolder;

	[SerializeField] CellFX cellPrefab;

	private List<CellFX> spawnedLines = new List<CellFX>();

	private Vector3 originPosition;
	private Vector3 originalHolderScale;
	private bool isDragging = false;
	private Canvas canvas;
	private RectTransform rectTransform;
	private Transform originalParent;
	private Vector2 originalPivot;
	public ShapeData currentShape { get; private set; }
	private string creatureUniqueID;

	private bool isEnabled = true;
	public bool Enabled
	{
		get => isEnabled;
		set
		{
			isEnabled = value;
		}
	}

	public void SetNotInteractable()
	{
		Enabled = false;

		GetComponent<CanvasGroup>().interactable = false;
	}

	System.Action OnSelected;

	[Button]
	public void Initialise(ShapeData shape, System.Action onSelected, string creatureID = "")
	{
		OnSelected = onSelected;

		Enabled = true;

		currentShape = shape;
		creatureUniqueID = creatureID;

		// Clear existing preview
		ClearPreview();

		if (shape == null || shape.GridWrapper == null || shape.GridWrapper.rows == null)
		{
			Debug.LogWarning("SelectableShape: Invalid shape data");
			return;
		}

		var gridWrapper = shape.GridWrapper;
		int gridSize = gridWrapper.rows.Count;

		// Get cell size from GridManager
		float cellSize = GridManager.instance.CellSize;

		// Calculate the size of the preview container at scale 1
		float totalWidth = gridSize * cellSize;
		float totalHeight = gridSize * cellSize;

		// Scale the shape holder to fit within parent
		SetInternalShapeScale();

		// Center offset so grid is centered at (0,0)
		Vector2 centerOffset = new Vector2(-(totalWidth / 2f), -(totalHeight / 2f)) + Vector2.one * (cellSize / 2f);

		// Spawn cells for the shape
		for (int y = 0; y < gridWrapper.rows.Count; y++)
		{
			for (int x = 0; x < gridWrapper.rows[y].colors.Count; x++)
			{
				Color cellColor = gridWrapper.rows[y].colors[x];

				// Only spawn cells that are not transparent
				if (cellColor.a > 0.5f)
				{
					Vector2 cellPos = new Vector2(x * cellSize, y * cellSize) + centerOffset;

					// Create the cell
					CellFX cell = Instantiate(cellPrefab, shapeHolder);
					cell.GetComponent<Button>().enabled = false;

					RectTransform rect = cell.rectTransform;
					rect.sizeDelta = new Vector2(cellSize, cellSize);
					rect.localPosition = cellPos;
					cell.name = $"Cell ({x},{y})";

					spawnedLines.Add(cell);
				}
			}
		}

		SetColors(true);
	}

	public void SetInternalShapeScale()
	{
		var gridWrapper = currentShape.GridWrapper;
		int gridSize = gridWrapper.rows.Count;

		// Get cell size from GridManager
		float cellSize = GridManager.instance.CellSize;

		float totalWidth = gridSize * cellSize;
		float totalHeight = gridSize * cellSize;

		// Get the parent RectTransform size
		RectTransform parentRect = shapeHolder.parent.GetComponent<RectTransform>();
		RectTransform holderRect = shapeHolder.GetComponent<RectTransform>();

		if (parentRect != null && holderRect != null)
		{
			float availableWidth = parentRect.rect.width;
			float availableHeight = parentRect.rect.height;

			// Calculate scale to fit within parent
			float scaleX = availableWidth / totalWidth;
			float scaleY = availableHeight / totalHeight;
			float finalScale = Mathf.Min(scaleX, scaleY);

			// Apply scale to shapeHolder
			shapeHolder.localScale = Vector3.one * finalScale;

			// Center the shapeHolder
			holderRect.anchoredPosition = Vector2.zero;
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

	private void Awake()
	{
		rectTransform = GetComponent<RectTransform>();
		canvas = GetComponentInParent<Canvas>();
	}
	void Start()
	{
		originPosition = rectTransform.anchoredPosition;
	}

	public static bool DraggingAny;

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (!Enabled)
			return;

		DraggingAny = true;

		isDragging = true;
		originPosition = rectTransform.anchoredPosition;
		originalHolderScale = shapeHolder.localScale;
		originalParent = rectTransform.parent;
		originalPivot = rectTransform.pivot;

		// Store current world position before changing parent
		Vector3 worldPos = rectTransform.position;

		// Reparent to canvas root to avoid parent transforms affecting drag
		rectTransform.SetParent(canvas.transform, true);

		// Center the pivot
		rectTransform.pivot = new Vector2(0.5f, 0.5f);

		// Restore world position after pivot change
		rectTransform.position = worldPos;

		shapeHolder.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutQuad);

		SoundManager.instance.PlaySoundEffect(AudioType.PickupTile);
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (!Enabled)
			return;

		if (isDragging && canvas != null)
		{
			// Convert screen position to world position, then set the RectTransform's world position directly
			Vector3 worldPoint;
			RectTransformUtility.ScreenPointToWorldPointInRectangle(
				canvas.transform as RectTransform,
				eventData.position,
				canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
				out worldPoint
			);

			rectTransform.position = worldPoint;
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		if (!Enabled)
			return;

		DraggingAny = false;

		isDragging = false;

		// Check if we're over the GridManager
		bool placedOnGrid = false;
		if (currentShape != null)
		{
			// Get the cell under the mouse
			CellFX cellUnderMouse = GridManager.instance.GetCellAtWorldPosition(rectTransform.position);

			if (cellUnderMouse != null)
			{
				// Get the center tile of the current shape
				Vector2Int shapeCenter = GetShapeCenter();

				// Calculate the base position (top-left corner of shape relative to where center should go)
				Vector2Int basePosition = cellUnderMouse.GridCoordinate - shapeCenter;

				Debug.Log($"Cell under mouse: {cellUnderMouse.GridCoordinate}, Shape center: {shapeCenter}, Base position: {basePosition}");

				// Place the shape on the grid
				GridManager.instance.AddShape(currentShape, basePosition, false, creatureUniqueID); // false = ally
				placedOnGrid = true;
			}
		}

		// Restore parent and pivot
		rectTransform.SetParent(originalParent, true);
		rectTransform.pivot = originalPivot;

		rectTransform.DOAnchorPos(originPosition, 0.3f).SetEase(Ease.OutBack);
		shapeHolder.DOScale(originalHolderScale, 0.3f).SetEase(Ease.OutBack);

		if (placedOnGrid)
		{
			OnSelected?.Invoke();
			Enabled = false;
		}
	}

	/// <summary>
	/// Get the center tile coordinate of the current shape
	/// Shapes are always odd-sized (3x3, 5x5, 7x7, etc.)
	/// </summary>
	private Vector2Int GetShapeCenter()
	{
		if (currentShape == null || currentShape.GridWrapper == null || currentShape.GridWrapper.rows == null)
		{
			Debug.LogWarning("SelectableShape: Cannot get center of invalid shape");
			return Vector2Int.zero;
		}

		int gridSize = currentShape.GridWrapper.rows.Count;

		// For odd-sized grids, the center is simply size/2 (integer division)
		// e.g., 3x3 -> center is (1,1), 5x5 -> center is (2,2), 7x7 -> center is (3,3)
		return new Vector2Int(gridSize / 2, gridSize / 2);
	}

	public void SetColors(bool normal)
	{
		ColorSettings settings = normal ? baseColors[currentShape.attackType] : greyedColor;

		mainImg.color = settings.mainColor;
		borderImg.color = settings.borderColor;
	}
}
