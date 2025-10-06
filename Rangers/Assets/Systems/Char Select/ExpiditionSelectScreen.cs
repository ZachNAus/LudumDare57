using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface ISelectableToTeam
{
	void TryToggleEquipped(SelectableAllyUI ui, CreatureData creature);
}

public class ExpiditionSelectScreen : MonoBehaviour
{
	[SerializeField] Button deployBtn;
	TextMeshProUGUI deployTxt;

	[Header("Owned")]
	[SerializeField] Transform selectableAllyHolder;
	[SerializeField] CharacterMinorUI selectableAllyPrefab;
	List<CharacterMinorUI> currentSelectableAllies = new List<CharacterMinorUI>();
	//[SerializeField] SelectableAllyUI selectableAllyPrefab;
	
	[Header("Equipped")]
	[SerializeField] Transform equippedAllyHolder;
	[SerializeField] CharacterMinorUI equippedAllyPrefab;
	List<CharacterMinorUI> equippedAllyObjects = new List<CharacterMinorUI>(); 

	private void Awake()
	{
		deployBtn.onClick.AddListener(TryDeploy);

		deployTxt = deployBtn.GetComponentInChildren<TextMeshProUGUI>();
	}

	const int ExpeditionTeam = 6;

	private void OnEnable()
	{
		OwnedCreaturePage.instance.CreaturesInExpidition.Clear();
		OwnedCreaturePage.instance.CreaturedFoundThisExpedition.Clear();
		UpdateDeployTxt();

		equippedAllyObjects.Clear();

		selectableAllyHolder.DestroyAllChildren();
		currentSelectableAllies.Clear();
		foreach (var ally in OwnedCreaturePage.instance.OwnedCreaturesThisRun)
		{
			var inst = Instantiate(selectableAllyPrefab, selectableAllyHolder);
			inst.Setup(ally, TryEquip);
			currentSelectableAllies.Add(inst);
		}

		equippedAllyHolder.DestroyAllChildren();
		for (int i = 0; i < ExpeditionTeam; i++)
		{
			var inst = Instantiate(equippedAllyPrefab, equippedAllyHolder);
			inst.Setup(null, TryUnequip);

			equippedAllyObjects.Add(inst);
		}
	}

	void TryDeploy()
	{
		if (OwnedCreaturePage.instance.CreaturesInExpidition.Count == 0)
			return;

		GameManager.instance.CharacterSelect();
	}

	public void TryEquip(CreatureData creature, CharacterMinorUI ui)
	{
		if (creature == null)
			return;

		//If full, can't equip
		if (OwnedCreaturePage.instance.CreaturesInExpidition.Count >= ExpeditionTeam)
		{
			return;
		}

		if (OwnedCreaturePage.instance.CreaturesInExpidition.Any(x => x.uniqueId == creature.uniqueId))
		{
			return;
		}

		OwnedCreaturePage.instance.CreaturesInExpidition.Add(creature);

		ui.Punch();
		ui.Spin = true;

		var toEdit = equippedAllyObjects.First(x => x.Creature == null);

		toEdit.SetCreature(creature);
		toEdit.Punch();
		toEdit.Spin = true;

		UpdateDeployTxt();

		//ui.SetEquipped(true);
	}

	public void TryUnequip(CreatureData creature, CharacterMinorUI ui)
	{
		if (creature == null)
			return;

		//If equipped, unequip
		if (OwnedCreaturePage.instance.CreaturesInExpidition.Contains(creature))
		{
			ui.SetCreature(null);
			ui.Punch();
			ui.Spin = false;
			OwnedCreaturePage.instance.CreaturesInExpidition.Remove(creature);
			UpdateDeployTxt();

			currentSelectableAllies.First(x => x.Creature.uniqueId == creature.uniqueId).Spin = false;
		}
	}

	void UpdateDeployTxt()
	{
		deployTxt.SetText($"Ready ({OwnedCreaturePage.instance.CreaturesInExpidition.Count}/{ExpeditionTeam})");
	}
}
