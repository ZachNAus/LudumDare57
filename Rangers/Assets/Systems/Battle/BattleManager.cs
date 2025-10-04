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
    public List<CreatureData> SelectedAllies;

    [Space]
    
    [ReadOnly]
    public float CurrentAllyHealth;
    [ReadOnly]
    public float CurrentEnemyHealth;

    [Header("UI")]
    [SerializeField] Image allyHealthBar;
    [SerializeField] Image enemyHealthBar;
    
    [Space]

    [SerializeField] GameObject creatureStuff;

	public void Init(CreatureData enemy, List<CreatureData> allies)
	{
        CurrentEnemy = enemy;
        CurrentEnemyHealth = enemy.healthMaxEnemy;
        CurrentEnemy.FillPool(true);


        SelectedAllies = new List<CreatureData>();
        foreach(var ally in allies)
		{
            SelectedAllies.Add(ally);
            ally.FillPool(false);
        }
        CurrentAllyHealth = SelectedAllies.Sum(x => x.healthMaxAlly);


        EnemyTurn();
    }

    public void EnemyTurn()
	{
        //Spawn a random shape on the grid

        GridManager.instance.ClearAllTiles();

        var shape = CurrentEnemy.currentShapePool.GetRandom();

        CurrentEnemy.currentShapePool.Remove(shape);
        if(CurrentEnemy.currentShapePool.Count == 0)
		{
            CurrentEnemy.FillPool(true);
		}

        Vector2Int pos = new Vector2Int(GridManager.instance.GridSize, GridManager.instance.GridSize);
        GridManager.instance.AddShape(shape, pos, true);

        PlayerTurnInit();
    }

    public void PlayerTurnInit()
	{
        //Create options for shapes

        foreach(var ally in SelectedAllies)
		{
            //Pick 2 attacks from their pool and spawn them
            for(int i = 0; i < 2; i++)
			{
                var atk = ally.currentShapePool.GetRandom();

                ally.currentShapePool.Remove(atk);

                if (ally.currentShapePool.Count == 0)
                    ally.FillPool(false);

                //Create card for the attack
			}
		}
	}

    public void Go()
	{
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

    void UpdateHealthBars()
	{
        //Consider a tween?

        allyHealthBar.fillAmount = CurrentAllyHealth / SelectedAllies.Sum(x => x.healthMaxAlly);

        enemyHealthBar.fillAmount = CurrentEnemyHealth / CurrentEnemy.healthMaxEnemy;
	}
}
