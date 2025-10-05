using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface ISelectableToTeam
{
	void TryToggleEquipped(SelectableAllyUI ui, CreatureData creature);
}

public class ExpiditionSelectScreen : MonoBehaviour, ISelectableToTeam
{
	[SerializeField] Button deployBtn;

	[SerializeField] Transform selectableAllyHolder;
	[SerializeField] SelectableAllyUI selectableAllyPrefab;

	private void Awake()
	{
		deployBtn.onClick.AddListener(TryDeploy);
	}

	private void OnEnable()
	{
		OwnedCreaturePage.instance.CreaturesInExpidition.Clear();

		selectableAllyHolder.DestroyAllChildren();
		foreach (var ally in OwnedCreaturePage.instance.OwnedCreaturesThisRun)
		{
			var inst = Instantiate(selectableAllyPrefab, selectableAllyHolder);
			inst.Setup(ally, this);
		}
	}

	void TryDeploy()
	{
		if (OwnedCreaturePage.instance.CreaturesInExpidition.Count == 0)
			return;

		//Go char select instead
		GameManager.instance.CharacterSelect();
	}

	public void TryToggleEquipped(SelectableAllyUI ui, CreatureData creature)
	{
		//If equipped, unequip
		if (OwnedCreaturePage.instance.CreaturesInExpidition.Contains(creature))
		{
			ui.SetEquipped(false);
			OwnedCreaturePage.instance.CreaturesInExpidition.Remove(creature);
			return;
		}

		//If full, can't equip
		if (OwnedCreaturePage.instance.CreaturesInExpidition.Count >= 6)
		{
			ui.SetEquipped(false);
			return;
		}

		OwnedCreaturePage.instance.CreaturesInExpidition.Add(creature);
		ui.SetEquipped(true);
	}
}
