using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName = "RANGER/Creature")]
public class CreatureData : ScriptableObject
{
	[ReadOnly]
	public string uniqueId;
	
	[Space]

	public string creatureName;

	public Sprite sprite;

	[TextArea(3,10)]
	public string desc;

	[SerializeField] Vector2 sizeRangeCM = new Vector2(24,56);
	public int CreatureSize { get; private set; }

	public bool IsBoy { get; private set; }

	[Space]

	[FoldoutGroup("Health")]
	public float healthMaxAlly;
	[FoldoutGroup("Health")]
	public float healthMaxEnemy;

	[Space]

	[Tooltip("For when guy is an enemy")]
	public List<ShapeData> enemyShapePool = new List<ShapeData>();

	[Tooltip("For when guy is an fren")]
	public List<ShapeData> allyShapePool = new List<ShapeData>();

	//[ReadOnly]
	[Tooltip("What attacks they have left")]
	public List<ShapeData> currentShapePool = new List<ShapeData>();

	public string GetFullDescription()
	{
		StringBuilder sb = new StringBuilder();

		sb.Append($"Size : {CreatureSize}cm\n");

		string gender = IsBoy ? "Male" : "Female";
		sb.Append($"Gender : {gender}\n");

		sb.Append($"Health : {healthMaxAlly}\n\n");

		sb.Append(desc);

		return sb.ToString();
	}

	public ShapeData GetRandomAttack(bool isEnemy)
	{
		if (currentShapePool.Count == 0)
			FillPool(isEnemy);

		var shape = currentShapePool.GetRandom();

		if (currentShapePool.Count == 0)
			FillPool(isEnemy);

		RemoveFromPool(shape);

		return shape;
	}

	private void FillPool(bool isEnemy)
	{
		currentShapePool.AddRange(isEnemy ? enemyShapePool : allyShapePool);
	}

	private void RemoveFromPool(ShapeData shape)
	{
		var toRemove = currentShapePool.FirstOrDefault(x => x.name == shape.name);

		if (toRemove)
			currentShapePool.Remove(toRemove);
	}

	public void EmptyPool()
	{
		currentShapePool.Clear();
	}

	static int IdsMade = 0;

	public void FirstTimeSetup()
	{
		this.uniqueId = $"Creature {IdsMade}";
		IdsMade++;

		CreatureSize = Mathf.RoundToInt(Random.Range(sizeRangeCM.x, sizeRangeCM.y));
		IsBoy = Random.value > 0.5f;
	}
}
