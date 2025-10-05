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

	private void OnEnable()
	{
		CharacterMinorUI.OnHoverAny -= OnHover;
		CharacterMinorUI.OnHoverAny += OnHover;

		OnHover(OwnedCreaturePage.instance.OwnedCreaturesThisRun[0]);
	}
	private void OnDisable()
	{
		CharacterMinorUI.OnHoverAny -= OnHover;
	}

	void OnHover(CreatureData creature)
	{
		if(creature != null)
		{
			Hide();

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
		}
	}

	void Hide()
	{
		sprite.SetCreature(null);

		attackHolder.DestroyAllChildren();
	}
}
