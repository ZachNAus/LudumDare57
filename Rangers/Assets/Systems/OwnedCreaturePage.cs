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
		return PlayerPrefs.GetInt(creatureName.creatureName, 0) == 1;
	}

	public void OnFoundCreature(CreatureData creatureName)
	{
		PlayerPrefs.SetInt(creatureName.creatureName, 1);
	}

	////////////////////////////////////////////////
	[ReadOnly]
	public List<CreatureData> OwnedCreaturesThisRun = new List<CreatureData>();

	[ReadOnly]
	public List<CreatureData> CreaturesInExpidition = new List<CreatureData>();

	public void ClearOwnedCreatures()
	{
		OwnedCreaturesThisRun.Clear();
		CreaturesInExpidition.Clear();
	}

	public void AddCreature(CreatureData capturedCreature, bool instantiate)
	{
		var inst = instantiate ? Instantiate(capturedCreature) : capturedCreature;

		inst.FirstTimeSetup();

		inst.EmptyPool();

		OwnedCreaturesThisRun.Add(inst);

		if (CreaturesInExpidition.Count < 6)
			CreaturesInExpidition.Add(inst);


		OnFoundCreature(capturedCreature);
	}

	/////////////////////////////////////////////////////

	public List<ShapeData> globalShapePool; 
}
