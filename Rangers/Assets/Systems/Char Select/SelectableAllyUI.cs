using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectableAllyUI : MonoBehaviour
{
	[SerializeField] Image allySprite;
	[SerializeField] TextMeshProUGUI allyName;
	
	[Space]

	[SerializeField] Button onPress;
	[SerializeField] GameObject selectedIcon;

	[Space]

	[SerializeField] Transform atkHolder;
	[SerializeField] SelectableShape shapePrefab;

	private void Awake()
	{
		onPress.onClick.AddListener(TryEquip);
	}

	CreatureData c;
	CharSelectScreen screen;
	public void Setup(CreatureData creature, CharSelectScreen screenRef)
	{
		screen = screenRef;
		c = creature;

		allyName.SetText(creature.creatureName);
		allySprite.sprite = creature.sprite;

		foreach(var atk in creature.allyShapePool)
		{
			var inst = Instantiate(shapePrefab, atkHolder);

			inst.Initialise(atk, null);
			inst.SetNotInteractable();
		}
	}

	void TryEquip()
	{
		screen.TryToggleEquipped(this, c);
	}

	public void SetEquipped(bool val)
	{
		selectedIcon.SetActive(val);
	}
}
