using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VictoryScreen : MonoBehaviour
{
	[SerializeField] CharacterMinorUI creature;
	[SerializeField] TextMeshProUGUI charhealth;
	[SerializeField] TextMeshProUGUI charDesc;

	[Space]

	[SerializeField] Transform attackHolder;
	[SerializeField] SelectableShape shapePrefab;

	private void OnEnable()
	{
		var c = GameManager.instance.CurrentEnemy;

		creature.Setup(c, null);
		charDesc.SetText(c.GetMostRecentLog());

		charhealth.SetText($"HP: {c.healthMaxAlly}");

		attackHolder.DestroyAllChildren();

		foreach(var atk in c.allyShapePool)
		{
			var inst = Instantiate(shapePrefab, attackHolder);
			inst.Initialise(atk, null);
			inst.SetNotInteractable();
		}
	}
}
