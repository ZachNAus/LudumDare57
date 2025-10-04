using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RANGER/Shape")]
public class ShapeData : ScriptableObject
{
	[Header("IMPORTANT")]
	public string uniqueID;


	[Header("Less important")]
	[OnValueChanged(nameof(ResizeGrid))]
	public int gridSize = 8;

	public Color currentColor = Color.white;

	[SerializeField, HideInInspector]
	private ColorGridWrapper gridWrapper = new ColorGridWrapper();

	[ShowInInspector, OnInspectorGUI("DrawColorGrid")]
	private bool showGrid = true;

	private void OnEnable()
	{
		if (gridWrapper.rows == null || gridWrapper.rows.Count == 0)
		{
			InitializeGrid();
		}
	}

	private void InitializeGrid()
	{
		gridWrapper.rows = new List<ColorRow>();
		for (int y = 0; y < gridSize; y++)
		{
			ColorRow row = new ColorRow();
			row.colors = new List<Color>();
			for (int x = 0; x < gridSize; x++)
			{
				row.colors.Add(Color.clear);
			}
			gridWrapper.rows.Add(row);
		}
	}

	private void ResizeGrid()
	{
		if (gridWrapper.rows == null)
		{
			InitializeGrid();
			return;
		}

		// Adjust height
		while (gridWrapper.rows.Count < gridSize)
		{
			ColorRow row = new ColorRow();
			row.colors = new List<Color>();
			for (int x = 0; x < gridSize; x++)
			{
				row.colors.Add(Color.clear);
			}
			gridWrapper.rows.Add(row);
		}
		while (gridWrapper.rows.Count > gridSize)
		{
			gridWrapper.rows.RemoveAt(gridWrapper.rows.Count - 1);
		}

		// Adjust width
		foreach (var row in gridWrapper.rows)
		{
			while (row.colors.Count < gridSize)
			{
				row.colors.Add(Color.clear);
			}
			while (row.colors.Count > gridSize)
			{
				row.colors.RemoveAt(row.colors.Count - 1);
			}
		}
	}

#if UNITY_EDITOR
	private void DrawColorGrid()
	{
		if (gridWrapper.rows == null || gridWrapper.rows.Count == 0)
		{
			InitializeGrid();
		}

		float cellSize = 30f;

		for (int y = 0; y < gridWrapper.rows.Count; y++)
		{
			UnityEditor.EditorGUILayout.BeginHorizontal();
			for (int x = 0; x < gridWrapper.rows[y].colors.Count; x++)
			{
				bool isLockedCell = x % 2 == 1 && y % 2 == 1;

				if (isLockedCell)
				{
					gridWrapper.rows[y].colors[x] = Color.green;
				}

				Rect rect = GUILayoutUtility.GetRect(cellSize, cellSize);

				UnityEditor.EditorGUI.DrawRect(rect, gridWrapper.rows[y].colors[x]);
				UnityEditor.EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1), Color.black);
				UnityEditor.EditorGUI.DrawRect(new Rect(rect.x, rect.y, 1, rect.height), Color.black);

				if (!isLockedCell && Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
				{
					if (Event.current.button == 0) // Left click
					{
						gridWrapper.rows[y].colors[x] = currentColor;
						UnityEditor.EditorUtility.SetDirty(this);
						Event.current.Use();
					}
					else if (Event.current.button == 1) // Right click
					{
						gridWrapper.rows[y].colors[x] = Color.clear;
						UnityEditor.EditorUtility.SetDirty(this);
						Event.current.Use();
					}
				}
			}
			UnityEditor.EditorGUILayout.EndHorizontal();
		}
	}
#endif

	[Serializable]
	public class ColorGridWrapper
	{
		public List<ColorRow> rows = new List<ColorRow>();
	}

	[Serializable]
	public class ColorRow
	{
		public List<Color> colors = new List<Color>();
	}
}
