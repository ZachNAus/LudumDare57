using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OwnedCreaturePage : MonoBehaviour
{
	public List<CreatureData> allCreatures = new List<CreatureData>();

    public static OwnedCreaturePage instance;


	private void Awake()
	{
		instance = this;
	}

	public bool HasFoundCreature(CreatureData creatureName)
	{
		return PlayerPrefs.GetInt(creatureName.creatureName, 0) > 0;
	}

	public int CreaturesFound(CreatureData creatureName)
	{
		return PlayerPrefs.GetInt(creatureName.creatureName, 0);
	}

	public void OnFoundCreature(CreatureData creatureName)
	{
		PlayerPrefs.SetInt(creatureName.creatureName, PlayerPrefs.GetInt(creatureName.creatureName, 0) + 1);
	}

	////////////////////////////////////////////////
	[ReadOnly]
	public List<CreatureData> OwnedCreaturesThisRun = new List<CreatureData>();

	[ReadOnly]
	public List<CreatureData> CreaturesInExpidition = new List<CreatureData>();

	[ReadOnly]
	public List<CreatureData> CreaturedFoundThisExpedition = new List<CreatureData>();

	public void ClearOwnedCreatures()
	{
		OwnedCreaturesThisRun.Clear();
		CreaturesInExpidition.Clear();
		CreaturedFoundThisExpedition.Clear();
	}

	public void AddCreature(CreatureData capturedCreature, bool instantiate, bool addToLog)
	{
		var inst = instantiate ? Instantiate(capturedCreature) : capturedCreature;
		
		if(instantiate)
			inst.FirstTimeSetup(!addToLog);

		inst.EmptyPool();

		OwnedCreaturesThisRun.Add(inst);

		if (CreaturesInExpidition.Count < 6)
			CreaturesInExpidition.Add(inst);

		if (addToLog)
		{
			OnFoundCreature(capturedCreature);

			CreaturedFoundThisExpedition.Add(capturedCreature);
		}
	}

	/////////////////////////////////////////////////////

	public List<ShapeData> globalShapePool; 
}
