using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "RANGER/Creature")]
public class CreatureData : ScriptableObject
{
	public Sprite sprite;

	public string desc;

	[FoldoutGroup("Health")]
	public float healthMaxAlly;
	[FoldoutGroup("Health")]
	public float healthMaxEnemy;

	[Space]

	[Tooltip("For when guy is an enemy")]
	public List<ShapeData> enemyShapePool = new List<ShapeData>();

	[Tooltip("For when guy is an fren")]
	public List<ShapeData> allyShapePool = new List<ShapeData>();

	[ReadOnly]
	[Tooltip("What attacks they have left")]
	public List<ShapeData> currentShapePool = new List<ShapeData>();

	public void FillPool(bool isEnemy)
	{
		if (isEnemy)
		{

		}
	}

	public void RemoveFromPool(ShapeData shape)
	{
		var toRemove = currentShapePool.FirstOrDefault(x => x.uniqueID == shape.uniqueID);

		if(toRemove)
			currentShapePool.Remove(toRemove);

		if(currentShapePool.Count == 0)
		{
			//Fill up the pool again
		}
	}
}
