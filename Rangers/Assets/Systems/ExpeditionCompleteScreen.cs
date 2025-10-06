using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpeditionCompleteScreen : MonoBehaviour
{
    [SerializeField] Transform capturedGuysHolder;
    [SerializeField] CharacterMinorUI capturedGuyPrefab;

	private void OnEnable()
	{
		capturedGuysHolder.DestroyAllChildren();

		foreach(var c in OwnedCreaturePage.instance.CreaturedFoundThisExpedition)
		{
			var inst = Instantiate(capturedGuyPrefab, capturedGuysHolder);
			inst.Setup(c, null);
		}
	}
}
