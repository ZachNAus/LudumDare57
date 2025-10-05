using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
	[SerializeField] GameObject container;

	[Space(50)]

	[ReadOnly]
	public CreatureData CurrentEnemy;

	[ReadOnly]
	public List<CreatureInfo> SelectedAllies;

	[System.Serializable]
	public class CreatureInfo
	{
		public CreatureData allyData;

		public CreatureUI allyUI;
	}

	[Space]

	[ReadOnly]
	public float CurrentAllyHealth;
	[ReadOnly]
	public float CurrentEnemyHealth;

	[Header("UI")]
	[SerializeField] Image allyHealthBar;
	[SerializeField] Image enemyHealthBar;

	[Space]

	[SerializeField] Image enemySprite;

	[SerializeField] Transform allyHolder;
	[SerializeField] CreatureUI allyPrefab;

	[Space]

	[SerializeField] Button goBtn;

	[Space]

	[SerializeField] TextMeshProUGUI allyDamagePrediction;
	[SerializeField] TextMeshProUGUI enemyDamagePrediction;

	private void Awake()
	{
		goBtn.onClick.AddListener(Go);

		SetActive(false);
	}

	private void Start()
	{
		GridManager.instance.OnGridUpdated += OnBoardUpdated;
	}

	public void Init(CreatureData enemy, List<CreatureData> allies)
	{
		SetActive(true);

		if (CurrentEnemy)
			Destroy(CurrentEnemy);

		foreach (var ally in SelectedAllies)
		{
			if (ally != null)
				Destroy(ally.allyData);
		}

		CurrentEnemy = Instantiate(enemy);
		CurrentEnemyHealth = enemy.healthMaxEnemy;

		enemySprite.sprite = enemy.sprite;

		allyHolder.DestroyAllChildren();

		SelectedAllies = new List<CreatureInfo>();
		foreach (var ally in allies)
		{
			var info = new CreatureInfo();

			info.allyData = Instantiate(ally);

			var inst = Instantiate(allyPrefab, allyHolder);
			inst.Initialise(ally);

			info.allyUI = inst;

			SelectedAllies.Add(info);
		}

		CurrentAllyHealth = SelectedAllies.Sum(x => x.allyData.healthMaxAlly);

		UpdateHealthBars(true);

		EnemyTurn();
	}

	public void EnemyTurn()
	{
		//Spawn a random shape on the grid

		GridManager.instance.ClearAllTiles();


		Vector2Int pos = new Vector2Int(0, 0/*GridManager.instance.GridSize/4, GridManager.instance.GridSize/4*/);
		GridManager.instance.AddShape(CurrentEnemy.GetRandomAttack(true), pos, true);

		PlayerTurnInit();
	}

	public void PlayerTurnInit()
	{
		//Create options for shapes

		foreach (var ally in SelectedAllies)
		{
			var atks = new List<ShapeData>();

			//Pick 2 attacks from their pool and spawn them
			for (int i = 0; i < 2; i++)
			{
				//Create card for the attack
				atks.Add(ally.allyData.GetRandomAttack(false));
			}

			ally.allyUI.SetAttacks(atks);
		}
	}

	public void Go()
	{
		//If the players have selected attakcs
		foreach (var ally in SelectedAllies)
		{
			if (ally.allyUI.HasSelectedAttacks == false)
				return;
		}

		//Do damage calculation
		CurrentEnemyHealth -= GetDamageDealt();

		CurrentAllyHealth -= CalculateDamageTaken();

		UpdateHealthBars();

		if (CurrentAllyHealth > 0 && CurrentEnemyHealth > 0)
			EnemyTurn();
		else
		{
			if (CurrentAllyHealth <= 0)
			{
				//YOU LOSE
				GameManager.instance.OnLoseBattle();
			}
			else
			{
				//YOU WIN
				GameManager.instance.OnWinBattle();
			}
		}
	}

	float GetDamageDealt()
	{
		return GridManager.instance.GetAllCellsInState(GridManager.CellState.ally).Count;
	}

	float CalculateDamageTaken()
	{
		List<CellFX> cells = GridManager.instance.GetAllCellsInState(GridManager.CellState.enemy);

		float totalDamage = 0;

		foreach (var cell in cells)
		{
			int adjacentCount = 0;

			// Check all 4 adjacent directions (up, down, left, right)
			Vector2Int[] directions = new Vector2Int[]
			{
				new Vector2Int(0, 1),   // up
                new Vector2Int(0, -1),  // down
                new Vector2Int(-1, 0),  // left
                new Vector2Int(1, 0)    // right
            };

			foreach (var dir in directions)
			{
				Vector2Int adjacentPos = cell.GridCoordinate + dir;

				// Check if the adjacent cell is also an enemy cell
				if (GridManager.instance.GetCellState(adjacentPos) == GridManager.CellState.enemy)
				{
					adjacentCount++;
				}
			}

			// Deal 1 damage base, plus 1 for each adjacent cell
			if (adjacentCount > 0)
			{
				totalDamage += 1 + adjacentCount;
			}
			else
			{
				totalDamage += 1;
			}
		}

		return totalDamage;
	}

	void UpdateHealthBars(bool instant = false)
	{
		if (instant)
		{
			allyHealthBar.fillAmount = CurrentAllyHealth / SelectedAllies.Sum(x => x.allyData.healthMaxAlly);

			enemyHealthBar.fillAmount = CurrentEnemyHealth / CurrentEnemy.healthMaxEnemy;
		}
		else
		{
			allyHealthBar.DOFillAmount(CurrentAllyHealth / SelectedAllies.Sum(x => x.allyData.healthMaxAlly), 0.5f);

			enemyHealthBar.DOFillAmount(CurrentEnemyHealth / CurrentEnemy.healthMaxEnemy, 0.5f);
		}
	}

	public void SetActive(bool value)
	{
		container.SetActive(value);
	}

	public void OnBoardUpdated()
	{
		allyDamagePrediction.SetText(GetDamageDealt().ToString());
		enemyDamagePrediction.SetText(CalculateDamageTaken().ToString());
	}
}
