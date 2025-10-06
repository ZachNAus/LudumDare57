using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI foundGuys;

	private void OnEnable()
	{
		foundGuys.SetText("");

		StartCoroutine(Wait());
	}

	IEnumerator Wait()
	{
		for(int i = 0; i < 2; i++)
		{
			yield return null;
		}

		foundGuys.SetText($"Discovered: {OwnedCreaturePage.instance.GetFoundCreatureCount()}/{OwnedCreaturePage.instance.allCreatures.Count}");
	}
}
