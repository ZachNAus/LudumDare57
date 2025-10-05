using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharSelectScreen : MonoBehaviour
{
	[SerializeField] Button deployBtn;
	TextMeshProUGUI deployTxt;

	[SerializeField] CharacterMinorUI enemyPreview;

	[Header("ally stuff")]

	List<CreatureData> selectedAllies = new List<CreatureData>();
	[SerializeField] Transform selectableAllyHolder;
	[SerializeField] CharacterMinorUI selectableAllyPrefab;

	[Space]

	[SerializeField] TextMeshProUGUI healthTxt;

	[Space]

	[SerializeField] List<EquippedPetInfo> equippedInfoStuff;


	private void Awake()
	{
		deployBtn.onClick.AddListener(TryStartCombat);
		deployTxt = deployBtn.GetComponentInChildren<TextMeshProUGUI>();

		foreach(var e in equippedInfoStuff)
		{
			e.OnPressChar += TryUnequip;
		}
	}

	private void OnEnable()
	{
		selectedAllies.Clear();
		UpdateUI();

		enemyPreview.SetCreature(GameManager.instance.CurrentEnemy);

		selectableAllyHolder.DestroyAllChildren();

		for(int i = 0; i < 6; i++)
		{
			var inst = Instantiate(selectableAllyPrefab, selectableAllyHolder);

			if (OwnedCreaturePage.instance.CreaturesInExpidition.Count > i)
			{
				var ally = OwnedCreaturePage.instance.CreaturesInExpidition[i];

				inst.Setup(ally, TryEquip);
			}
			else
			{
				inst.Setup(null, null);
			}
		}
	}

	void TryStartCombat()
	{
		if (selectedAllies.Count == 0)
			return;

		GameManager.instance.BeginBattle(selectedAllies);
	}

	void TryEquip(CreatureData creature, CharacterMinorUI ui)
	{
		if (selectedAllies.Count >= 3)
			return;
		if (selectedAllies.Any(x => x.uniqueId == creature.uniqueId))
			return;

		ui.Punch();

		selectedAllies.Add(creature);

		UpdateUI();
	}

	void TryUnequip(CreatureData creature, EquippedPetInfo info)
	{
		if (creature == null)
			return;

		selectedAllies.Remove(creature);

		info.Punch();

		UpdateUI();
	}

	void UpdateUI()
	{
		UpdateHealth();
		UpdateText();

		for(int i = 0; i < equippedInfoStuff.Count; i++)
		{
			if(selectedAllies.Count <= i)
			{
				equippedInfoStuff[i].Setup(null);
			}
			else
			{
				equippedInfoStuff[i].Setup(selectedAllies[i]);
			}
		}
	}

	void UpdateText()
	{
		deployTxt.SetText($"Ready ({selectedAllies.Count}/3)");
	}

	void UpdateHealth()
	{
		healthTxt.SetText(selectedAllies.Sum(x => x.healthMaxAlly).ToString());
	}

	
}
