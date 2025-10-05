using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
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

	private void Awake()
	{
        goBtn.onClick.AddListener(Go);
    }

    [Header("Testing")]
    [SerializeField]
    CreatureData testEnemy;
    [SerializeField]
    List<CreatureData> testAllies;
    [Button]
    public void InitWithCurrent()
	{
        Init(testEnemy, testAllies);
	}

	public void Init(CreatureData enemy, List<CreatureData> allies)
	{
        if (CurrentEnemy)
            Destroy(CurrentEnemy);

        foreach(var ally in SelectedAllies)
		{
            if (ally != null)
                Destroy(ally.allyData);
		}

        CurrentEnemy = Instantiate(enemy);
        CurrentEnemyHealth = enemy.healthMaxEnemy;

        enemySprite.sprite = enemy.sprite;

        allyHolder.DestroyAllChildren();

        SelectedAllies = new List<CreatureInfo>();
        foreach(var ally in allies)
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


        Vector2Int pos = new Vector2Int(0,0/*GridManager.instance.GridSize/4, GridManager.instance.GridSize/4*/);
        GridManager.instance.AddShape(CurrentEnemy.GetRandomAttack(true), pos, true);

        PlayerTurnInit();
    }

    public void PlayerTurnInit()
	{
        //Create options for shapes

        foreach(var ally in SelectedAllies)
		{
            var atks = new List<ShapeData>();

            //Pick 2 attacks from their pool and spawn them
            for(int i = 0; i < 2; i++)
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
        foreach(var ally in SelectedAllies)
		{
            if (ally.allyUI.HasSelectedAttacks == false)
                return;
		}

        //Do damage calculation
        UpdateHealthBars();

        //Resolve the game

        //Hop back to enemy turn


        if (CurrentAllyHealth > 0 && CurrentEnemyHealth > 0)
            EnemyTurn();
		else
		{
            if(CurrentAllyHealth <= 0)
			{
                //YOU LOSE
                Application.Quit();
			}
			else
			{
                //YOU WIN
                Debug.Log("Good shit bithc");
			}
		}
	}

    void UpdateHealthBars(bool instant = false)
	{
        //Consider a tween?

        allyHealthBar.fillAmount = CurrentAllyHealth / SelectedAllies.Sum(x => x.allyData.healthMaxAlly);

        enemyHealthBar.fillAmount = CurrentEnemyHealth / CurrentEnemy.healthMaxEnemy;
	}
}
