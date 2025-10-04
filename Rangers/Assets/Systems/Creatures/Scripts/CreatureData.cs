using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RANGER/Creature")]
public class CreatureData : ScriptableObject
{
	public Sprite sprite;

	public string desc;

	[Space]

	public List<ShapeData> shapePool = new List<ShapeData>();

	[ReadOnly]
	public List<ShapeData> currentShapePool = new List<ShapeData>();
}
