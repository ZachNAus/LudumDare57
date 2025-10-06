using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreatureLogsUI : MonoBehaviour
{
    [SerializeField] CreatureData creatureToLog;

    [SerializeField] TextMeshProUGUI creatureName;


    [SerializeField] TextMeshProUGUI hpTxt;

    [SerializeField] Image regularSprite;
    [SerializeField] Image shinySprite;


    [SerializeField] TextMeshProUGUI allLogs;

	[SerializeField] Transform attackHolder;
	[SerializeField] SelectableShape attackPrefab;

	private void OnEnable()
	{
		var foundCount = OwnedCreaturePage.instance.CreaturesFound(creatureToLog);

		regularSprite.sprite = creatureToLog.sprite_base;
		shinySprite.sprite = creatureToLog.sprite_shiny;

		attackHolder.DestroyAllChildren();

		if (foundCount == 0)
		{
			creatureName.SetText("???");
			allLogs.SetText("");
			hpTxt.SetText("");

			regularSprite.color = Color.black;
			shinySprite.transform.parent.gameObject.SetActive(false);

			return;
		}

		creatureName.SetText(creatureToLog.creatureName);


		regularSprite.color = Color.white;


		bool foundShiny = OwnedCreaturePage.instance.HasFoundShiny(creatureToLog);
		shinySprite.transform.parent.gameObject.SetActive(foundShiny);

		hpTxt.SetText($"HP: {creatureToLog.healthRange.x} - {creatureToLog.healthRange.y}");

		allLogs.SetText(creatureToLog.GetCurrentLog());

		foreach(var atk in creatureToLog.allyShapePool)
		{
			var inst = Instantiate(attackPrefab, attackHolder);
			inst.Initialise(atk, null);
			inst.SetNotInteractable();

			StartCoroutine(Co_Wait(() => inst.SetInternalShapeScale()));
		}
	}

	IEnumerator Co_Wait(System.Action Oncomplete)
	{
		yield return null;

		Oncomplete?.Invoke();
	}
}
