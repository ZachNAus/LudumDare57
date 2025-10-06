using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	[SerializeField] BattleManager battleManager;

	[SerializeField] GameObject mainMenu;

	[SerializeField] GameObject loseScreen;

	[SerializeField] GameObject winScreen;

	[SerializeField] GameObject expiditionSelectScreen;

	[SerializeField] GameObject expiditionCompleteScreen;

	[SerializeField] GameObject charSelectScreen;

	[Header("Gameplay")]
	[SerializeField] CreatureData startingCreature;

	[SerializeField] List<CreatureData> firstExpeditionCreatures;

	public static GameManager instance;

	private void Awake()
	{
		instance = this;

		battleManager.SetActive(false);

		mainMenu.SetActive(true);
		loseScreen.SetActive(false);

		charSelectScreen.SetActive(false);
		expiditionSelectScreen.SetActive(false);

		expiditionCompleteScreen.SetActive(false);
	}

	public void StartGame()
	{
		StartAgain();

		SelectExpidition();
	}

	public void SelectExpidition()
	{
		mainMenu.SetActive(false);
		winScreen.SetActive(false);
		charSelectScreen.SetActive(false);
		expiditionCompleteScreen.SetActive(false);

		expiditionSelectScreen.SetActive(true);

		CombatsDoneThisExpdition = 0;
	}

	public int CombatsDoneThisExpdition;

	public CreatureData CurrentEnemy { get; private set; }

	public void CharacterSelect()
	{
		var creature = OwnedCreaturePage.instance.allCreatures.GetRandom();

		if (OwnedCreaturePage.instance.OwnedCreaturesThisRun.Count <= firstExpeditionCreatures.Count)
		{
			creature = firstExpeditionCreatures[CombatsDoneThisExpdition];
		}

		CurrentEnemy = Instantiate(creature);
		CurrentEnemy.FirstTimeSetup();

		mainMenu.SetActive(false);
		winScreen.SetActive(false);
		expiditionSelectScreen.SetActive(false);

		charSelectScreen.SetActive(true);
	}

	public void BeginBattle(List<CreatureData> selectedAllies)
	{
		expiditionSelectScreen.SetActive(false);
		charSelectScreen.SetActive(false);

		battleManager.Init(CurrentEnemy, selectedAllies);
	}

	public void OnLoseBattle()
	{
		loseScreen.SetActive(true);
		battleManager.SetActive(false);
	}


	public void OnWinBattle()
	{
		winScreen.SetActive(true);
		battleManager.SetActive(false);

		//Add guy to team
		OwnedCreaturePage.instance.AddCreature(CurrentEnemy, false);
	}

	public void NextWave()
	{
		CombatsDoneThisExpdition++;

		if (OwnedCreaturePage.instance.OwnedCreaturesThisRun.Count <= 3)
		{
			if (CombatsDoneThisExpdition >= firstExpeditionCreatures.Count)
			{
				expiditionCompleteScreen.SetActive(true);
			}
			else
			{
				CharacterSelect();
			}
		}
		else
		{

			if (CombatsDoneThisExpdition >= 3)
			{
				//We have completed the expidition
				expiditionCompleteScreen.SetActive(true);
			}
			else
			{
				CharacterSelect();
			}
		}
	}

	public void StartAgain()
	{
		CurrentEnemy = null;

		OwnedCreaturePage.instance.ClearOwnedCreatures();
		OwnedCreaturePage.instance.AddCreature(startingCreature, true);
	}

	public void Quit()
	{
		Application.Quit();
	}

	public void MainMenu()
	{
		StartAgain();

		battleManager.SetActive(false);

		mainMenu.SetActive(true);
		loseScreen.SetActive(false);

		charSelectScreen.SetActive(false);
	}
}
