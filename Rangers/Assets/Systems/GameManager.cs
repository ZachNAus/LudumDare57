using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	[SerializeField] BattleManager battleManager;

	[SerializeField] GameObject mainMenu;

	[SerializeField] GameObject loseScreen;

	[SerializeField] GameObject winScreen;

	[SerializeField] GameObject charSelectScreen;

	[Header("Gameplay")]
	[SerializeField]
	List<WaveData> possibleEnemiesPerWave = new List<WaveData>();

	[System.Serializable]
	class WaveData
	{
		public List<CreatureData> possibleEnemies;
	}

	[SerializeField] CreatureData startingCreature;

	public static GameManager instance;

	private void Awake()
	{
		instance = this;

		battleManager.SetActive(false);

		mainMenu.SetActive(true);
		loseScreen.SetActive(false);

		charSelectScreen.SetActive(false);
	}

	public void StartGame()
	{
		StartAgain();

		NextWave();
	}

	public CreatureData CurrentEnemy { get; private set; }
	public int CurrentWave { get; private set; }
	public void NextWave()
	{
		CurrentEnemy = possibleEnemiesPerWave[CurrentWave].possibleEnemies.GetRandom();

		mainMenu.SetActive(false);
		charSelectScreen.SetActive(true);
	}
	
	public void BeginBattle(List<CreatureData> selectedAllies)
	{
		charSelectScreen.SetActive(false);

		battleManager.Init(CurrentEnemy, selectedAllies);
	}

	public void OnLoseBattle()
	{
		loseScreen.SetActive(true);
		battleManager.SetActive(false);
	}
	public void StartAgain()
	{
		CurrentEnemy = null;
		CurrentWave = 0;

		OwnedCreaturePage.instance.ClearOwnedCreatures();
		OwnedCreaturePage.instance.AddCreature(startingCreature);
	}

	public void OnWinBattle()
	{
		winScreen.SetActive(true);
		battleManager.SetActive(false);

		//Add guy to team
		OwnedCreaturePage.instance.AddCreature(CurrentEnemy);

		CurrentWave++;
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
