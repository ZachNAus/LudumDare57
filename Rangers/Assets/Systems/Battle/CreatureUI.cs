using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreatureUI : MonoBehaviour
{
	[SerializeField] Button btn;

	public CreatureData CurrentCreature { get; private set; }

	[SerializeField] Image creatureSprite;
	[SerializeField] TextMeshProUGUI creatureName;

	[SerializeField] HorizontalLayoutGroup attackHolder;
	[SerializeField] SelectableShape attackPrefab;

	[SerializeField] Animator activeAnimator;

	List<SelectableShape> currentShapes = new List<SelectableShape>();

	private void Awake()
	{
		btn.onClick.AddListener(RemoveShapeFromThisCreature);
	}
	

	public void Initialise(CreatureData creatureInst)
	{
		CurrentCreature = creatureInst;

		creatureSprite.sprite = CurrentCreature.sprite;
		creatureName.SetText(CurrentCreature.creatureName);

		GridManager.OnShapeRemoved += OnShapeRemoved;

		activeAnimator.speed = 0;
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

			inst.Initialise(atk, SelectedAttacks, CurrentCreature.uniqueId);

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

		activeAnimator.speed = 1;
	}

	void OnShapeRemoved(ShapeData shape, string creatureUniqueID)
	{
		// Only deselect attacks if the removed shape was placed by this creature
		if (CurrentCreature.uniqueId == creatureUniqueID)
		{
			DeselectedAttacks();
		}
	}

	public void DeselectedAttacks()
	{
		foreach (var s in currentShapes)
			s.Enabled = true;

		HasSelectedAttacks = false;

		activeAnimator.speed = 0;
	}

	void RemoveShapeFromThisCreature()
	{
		// Remove all shapes placed by this creature on the grid
		var gridManager = GridManager.instance;
		if (gridManager != null)
		{
			gridManager.RemoveShapesByCreature(CurrentCreature.uniqueId);
		}

		DeselectedAttacks();
	}
}
