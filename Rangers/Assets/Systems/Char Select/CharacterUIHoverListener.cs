using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CharacterUIHoverListener : MonoBehaviour
{
    [SerializeField] CharacterMinorUI sprite;

    [SerializeField] TextMeshProUGUI charName;
    [SerializeField] TextMeshProUGUI charHealth;

    [SerializeField] Transform attackHolder;
    [SerializeField] SelectableShape shapePrefab;

	[Space]

	[SerializeField] GameObject[] hideWhenNoHover;

	private void OnEnable()
	{
		CharacterMinorUI.OnHoverAny -= OnHover;
		CharacterMinorUI.OnHoverAny += OnHover;

		OnHover(null);
	}
	private void OnDisable()
	{
		CharacterMinorUI.OnHoverAny -= OnHover;
	}

	void OnHover(CreatureData creature)
	{
		Hide();

		if (creature != null)
		{
			sprite.SetCreature(creature);

			charName.SetText(creature.creatureName);

			charHealth.SetText(creature.healthMaxAlly.ToString());

			foreach(var attack in creature.allyShapePool)
			{
				var inst = Instantiate(shapePrefab, attackHolder);

				inst.Initialise(attack, null);
				inst.Enabled = false;
				inst.SetNotInteractable();
			}

			foreach (var f in hideWhenNoHover)
				f.SetActive(true);
		}
	}

	void Hide()
	{
		sprite.SetCreature(null);

		charName.SetText("");
		charHealth.SetText("");

		attackHolder.DestroyAllChildren();

		foreach (var f in hideWhenNoHover)
			f.SetActive(false);
	}
}
