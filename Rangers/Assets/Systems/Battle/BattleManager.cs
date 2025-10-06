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
	TextMeshProUGUI goTxt;

	[Space]

	[SerializeField] TextMeshProUGUI allyDamagePrediction;
	[SerializeField] TextMeshProUGUI enemyDamagePrediction;

	[SerializeField] TextMeshProUGUI allyCurrentHealth;
	[SerializeField] TextMeshProUGUI enemyCurrentHealth;

	[Space]

	[SerializeField] Button dmgNumbersToggleBtn;

	[Header("Tween settings")]
	[SerializeField] float healthTxtSizeTween;
	[SerializeField] float healthTxtDuration;
	[SerializeField] int healthTxtVibrato;
	[SerializeField] float healthTxtElasticity;

	[Header("Extra Combat settings")]
	[SerializeField] bool adjacentAlliesReduceDmg;

	[ReadOnly]
	public bool showDamageNumbers = false;

	private void Awake()
	{
		goBtn.onClick.AddListener(Go);
		goTxt = goBtn.GetComponentInChildren<TextMeshProUGUI>();

		dmgNumbersToggleBtn.onClick.AddListener(ToggleDamageNumbers);

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
			ally.allyUI.DeselectedAttacks();
		}
	}

	public void Go()
	{
		if (GameLoading)
			return;

		//If the players have selected attakcs
		foreach (var ally in SelectedAllies)
		{
			if (ally.allyUI.HasSelectedAttacks == false)
				return;
		}

		StartCoroutine(Co_RunSim());
	}
	IEnumerator Co_RunSim()
	{
		GameLoading = true;


		//Foreach enemy tile, punch
		var enemyTiles = GridManager.instance.GetAllCellsInState(GridManager.CellState.enemy);

		float longestTweenTime = 0.5f;
		foreach (var tile in enemyTiles)
		{
			//var cellDmg = GetSingleTileDamage(tile);
			//tile.PunchMultiple(Mathf.RoundToInt(cellDmg));
			tile.Punch();

			//longestTweenTime = Mathf.Max(longestTweenTime, cellDmg * tile.popTime);
		}

		yield return new WaitForSeconds(longestTweenTime);


		CurrentAllyHealth -= CalculateDamageTaken();

		allyCurrentHealth.SetText(CurrentAllyHealth.ToString());

		yield return allyHealthBar.DOFillAmount(CurrentAllyHealth / SelectedAllies.Sum(x => x.allyData.healthMaxAlly), 0.5f).WaitForCompletion();
		

		if (CurrentAllyHealth > 0)
		{
			//Foreach ally tile, punch
			var allyTiles = GridManager.instance.GetAllCellsInState(GridManager.CellState.ally);
			foreach (var tile in allyTiles)
			{
				tile.Punch();
			}

			yield return new WaitForSeconds(0.5f);

			//Do damage calculation
			CurrentEnemyHealth -= GetDamageDealt();
			enemyCurrentHealth.SetText(CurrentEnemyHealth.ToString());

			yield return enemyHealthBar.DOFillAmount(CurrentEnemyHealth / CurrentEnemy.healthMaxEnemy, 0.5f).WaitForCompletion();
		}


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

		GameLoading = false;
	}


	public static bool GameLoading;

	float GetDamageDealt()
	{
		return GridManager.instance.GetAllCellsInState(GridManager.CellState.ally).Count;
	}

	public float GetSingleTileDamage(CellFX cell)
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

		int alliesAdjacent = 0;
		foreach (var dir in directions)
		{
			Vector2Int adjacentPos = cell.GridCoordinate + dir;

			// Check if the adjacent cell is also an enemy cell
			if (GridManager.instance.GetCellState(adjacentPos) == GridManager.CellState.enemy)
			{
				adjacentCount++;
			}

			if (adjacentAlliesReduceDmg && GridManager.instance.GetCellState(adjacentPos) == GridManager.CellState.ally)
			{
				alliesAdjacent++;
			}
		}

		// Deal 1 damage base, plus 1 for each adjacent cell
		return Mathf.Max(1 + adjacentCount - alliesAdjacent, 1);
	}

	float CalculateDamageTaken()
	{
		List<CellFX> cells = GridManager.instance.GetAllCellsInState(GridManager.CellState.enemy);

		float totalDamage = 0;

		foreach (var cell in cells)
		{
			totalDamage += GetSingleTileDamage(cell);
		}

		return totalDamage;
	}

	void UpdateHealthBars(bool instant = false, System.Action onComplete = null)
	{
		if (instant)
		{
			allyHealthBar.fillAmount = CurrentAllyHealth / SelectedAllies.Sum(x => x.allyData.healthMaxAlly);

			enemyHealthBar.fillAmount = CurrentEnemyHealth / CurrentEnemy.healthMaxEnemy;
			onComplete?.Invoke();
		}
		else
		{
			allyHealthBar.DOFillAmount(CurrentAllyHealth / SelectedAllies.Sum(x => x.allyData.healthMaxAlly), 0.5f);

			enemyHealthBar.DOFillAmount(CurrentEnemyHealth / CurrentEnemy.healthMaxEnemy, 0.5f).OnComplete(() => onComplete?.Invoke());
		}

		allyCurrentHealth.SetText(CurrentAllyHealth.ToString());
		enemyCurrentHealth.SetText(CurrentEnemyHealth.ToString());
	}

	public void SetActive(bool value)
	{
		container.SetActive(value);
	}

	public void OnBoardUpdated()
	{
		allyDamagePrediction.SetText(GetDamageDealt().ToString());
		allyDamagePrediction.DOKill();
		allyDamagePrediction.transform.localScale = Vector3.one;
		allyDamagePrediction.transform.DOPunchScale(Vector3.one * healthTxtElasticity, healthTxtDuration, healthTxtVibrato, healthTxtElasticity);

		enemyDamagePrediction.SetText(CalculateDamageTaken().ToString());
		enemyDamagePrediction.DOKill();
		enemyDamagePrediction.transform.localScale = Vector3.one;
		enemyDamagePrediction.transform.DOPunchScale(Vector3.one * healthTxtElasticity, healthTxtDuration, healthTxtVibrato, healthTxtElasticity);

		StartCoroutine(Co_waitTxt());

		UpdateDamageNumbersOnGrid();
	}

	IEnumerator Co_waitTxt(int framesToWait = 1)
	{
		for (int i = 0; i < framesToWait; i++)
			yield return null;

		var allyCount = SelectedAllies.Count;
		var attackingAllies = SelectedAllies.Where(x => x.allyUI.HasSelectedAttacks).Count();

		goTxt.SetText($"Go! ({attackingAllies}/{allyCount})");
	}


	void ToggleDamageNumbers()
	{
		showDamageNumbers = !showDamageNumbers;
		UpdateDamageNumbersOnGrid();
	}

	void UpdateDamageNumbersOnGrid()
	{
		// First, hide damage numbers on all non-enemy cells to prevent stale text
		List<CellFX> allyCells = GridManager.instance.GetAllCellsInState(GridManager.CellState.ally);
		List<CellFX> clashCells = GridManager.instance.GetAllCellsInState(GridManager.CellState.clash);
		List<CellFX> emptyCells = GridManager.instance.GetAllCellsInState(GridManager.CellState.empty);

		foreach (var cell in allyCells)
			cell.SetDmgShowActive(false);
		foreach (var cell in clashCells)
			cell.SetDmgShowActive(false);
		foreach (var cell in emptyCells)
			cell.SetDmgShowActive(false);

		List<CellFX> enemyCells = GridManager.instance.GetAllCellsInState(GridManager.CellState.enemy);

		if (!showDamageNumbers)
		{
			// Hide all damage numbers on enemy cells too
			foreach (var cell in enemyCells)
			{
				cell.SetDmgShowActive(false);
			}
			return;
		}

		// Calculate and display damage for each enemy cell
		foreach (var cell in enemyCells)
		{
			int damage = (int)GetSingleTileDamage(cell);

			if (damage > 0)
			{
				cell.SetDmgShowActive(true);
				cell.PreviewDmg(damage);
			}
			else
			{
				cell.SetDmgShowActive(false);
			}
		}
	}

	private void Update()
	{
		enemySprite.color = new Color(1, 1, 1, SelectableShape.DraggingAny ? 0.3f : 1);
	}
}
