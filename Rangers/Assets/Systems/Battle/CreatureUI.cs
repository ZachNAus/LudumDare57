using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreatureUI : MonoBehaviour
{
	public CreatureData CurrentCreature { get; private set; }

	[SerializeField] Image creatureSprite;
	[SerializeField] TextMeshProUGUI creatureName;

	[SerializeField] HorizontalLayoutGroup attackHolder;
	[SerializeField] SelectableShape attackPrefab;

	List<SelectableShape> currentShapes = new List<SelectableShape>();

	public void Initialise(CreatureData creatureInst)
	{
		CurrentCreature = creatureInst;

		creatureSprite.sprite = CurrentCreature.sprite;
		creatureName.SetText(CurrentCreature.creatureName);

		GridManager.OnShapeRemoved += OnShapeRemoved;
	}

	public void SetAttacks(List<ShapeData> attacks)
	{
		currentShapes.Clear();
		attackHolder.transform.DestroyAllChildren();

		attackHolder.enabled = true;

		for (int i = 0; i < attacks.Count; i++)
		{
			var atk = attacks[i];

			var inst = Instantiate(attackPrefab, attackHolder.transform);

			inst.Initialise(atk, SelectedAttacks);

			currentShapes.Add(inst);
		}

		StartCoroutine(Co_Do());
	}

	IEnumerator Co_Do()
	{
		yield return new WaitForEndOfFrame();

		attackHolder.enabled = false;
	}

	public bool HasSelectedAttacks { get; private set; }

	public void SelectedAttacks()
	{
		//Disable any other existing shapes

		foreach (var s in currentShapes)
			s.Enabled = false;

		HasSelectedAttacks = true;
	}

	void OnShapeRemoved(ShapeData shape)
	{
		foreach (var s in currentShapes)
		{
			if (s.currentShape.uniqueID == shape.uniqueID)
			{
				DeselectedAttacks();

				return;
			}
		}
	}

	public void DeselectedAttacks()
	{
		foreach (var s in currentShapes)
			s.Enabled = true;

		HasSelectedAttacks = false;
	}
}
