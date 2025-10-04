using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public enum LineState
	{
        empty,
        ally,
        enemy,
        clash
	}

    [ReadOnly][SerializeField]
    AppliedShapeDict appliedShapes = new AppliedShapeDict();
    [System.Serializable]
    class AppliedShapeDict : SerializableDictionary<ShapeData, AppliedShapeData> { }

    struct AppliedShapeData
	{
        public Vector2Int basePosition;
        public bool allyTile;
	}

    public void AddShape(ShapeData shape, Vector2Int baseLocation, bool enemyTile)
	{

	}

    /// <summary>
    /// Looks at the current state of the grid and returns which color the current line is
    /// </summary>
    /// <param name="coords"></param>
    /// <returns></returns>
    public LineState GetLineState(Vector2Int coords)
	{
        return LineState.empty;
	}

    /// <summary>
    /// Remove all tiles the allies have placed
    /// </summary>
    public void ClearAllyTiles()
	{

	}

    /// <summary>
    /// Remove all tiles the enemies have placed
    /// </summary>
    public void ClearAllTiles()
	{

	}

    /// <summary>
    /// Update the UI to match what the current grid should look like
    /// </summary>
    public void UpdateDrawGrid()
	{

	}
}
