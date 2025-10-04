using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public CreatureData CurrentEnemy;

    public List<CreatureData> SelectedAllies;

    [Space]
    
    public float CurrentAllyHealth;
    public float CurrentEnemyHealth;

	public void Init(CreatureData enemy, List<CreatureData> allies)
	{
        CurrentEnemy = enemy;
        CurrentEnemyHealth = enemy.healthMaxEnemy;

        SelectedAllies = new List<CreatureData>();
        SelectedAllies.AddRange(allies);

        CurrentAllyHealth = SelectedAllies.Sum(x => x.healthMaxAlly);
    }

    public void EnemyTurn()
	{
        //Spawn a random shape on the grid
	}
}
