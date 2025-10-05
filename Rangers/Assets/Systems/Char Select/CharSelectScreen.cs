using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharSelectScreen : MonoBehaviour, ISelectableToTeam
{
	[SerializeField] Button deployBtn;

	[SerializeField] Image enemyPreview;

	List<CreatureData> selectedAllies = new List<CreatureData>();

	[SerializeField] Transform selectableAllyHolder;
	[SerializeField] SelectableAllyUI selectableAllyPrefab;

	private void Awake()
	{
		deployBtn.onClick.AddListener(TryStartCombat);
	}

	private void OnEnable()
	{
		selectedAllies.Clear();
		enemyPreview.sprite = GameManager.instance.CurrentEnemy.sprite;

		selectableAllyHolder.DestroyAllChildren();
		foreach (var ally in OwnedCreaturePage.instance.CreaturesInExpidition)
		{
			var inst = Instantiate(selectableAllyPrefab, selectableAllyHolder);
			inst.Setup(ally, this);
		}
	}

	void TryStartCombat()
	{
		if (selectedAllies.Count == 0)
			return;

		GameManager.instance.BeginBattle(selectedAllies);
	}

	public void TryToggleEquipped(SelectableAllyUI ui, CreatureData creature)
	{
		//If equipped, unequip
		if (selectedAllies.Contains(creature))
		{
			ui.SetEquipped(false);
			selectedAllies.Remove(creature);
			return;
		}

		//If full, can't equip
		if (selectedAllies.Count >= 3)
		{
			ui.SetEquipped(false);
			return;
		}

		selectedAllies.Add(creature);
		ui.SetEquipped(true);
	}
}
