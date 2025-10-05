using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VictoryScreen : MonoBehaviour
{
	[SerializeField] Image charSprite;
	[SerializeField] TextMeshProUGUI charName;
	[SerializeField] TextMeshProUGUI charDesc;

	[Space]

	[SerializeField] Transform attackHolder;
	[SerializeField] SelectableShape shapePrefab;

	private void OnEnable()
	{
		var c = GameManager.instance.CurrentEnemy;

		charSprite.sprite = c.sprite;

		charName.SetText(c.creatureName);
		charDesc.SetText(c.desc);

		attackHolder.DestroyAllChildren();

		foreach(var atk in c.allyShapePool)
		{
			var inst = Instantiate(shapePrefab, attackHolder);
			inst.Initialise(atk, null);
			inst.SetNotInteractable();
		}
	}
}
