using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquippedPetInfo : MonoBehaviour
{
    [SerializeField] CharacterMinorUI minorUI;

    [SerializeField] TextMeshProUGUI charName;
    [SerializeField] TextMeshProUGUI charHealth;

    [SerializeField] Transform shapeHolder;

    [SerializeField] SelectableShape shapePrefab;

    [SerializeField] Button btn;

	private void Awake()
	{
        btn.onClick.AddListener(OnPress);
    }

	public CreatureData Creature { get; private set; }

    List<SelectableShape> ActiveShapeUI = new List<SelectableShape>();

    public void Setup(CreatureData creature)
	{
        Creature = creature;

        minorUI.SetCreature(Creature);

        charName.SetText(Creature != null ? Creature.creatureName : "");

        charHealth.SetText(Creature != null ? Creature.healthMaxAlly.ToString() : "");

        shapeHolder.DestroyAllChildren();

        ActiveShapeUI.Clear();

        if (Creature)
		{
            foreach(var shape in Creature.allyShapePool)
			{
                var inst = Instantiate(shapePrefab, shapeHolder);

                inst.Initialise(shape, null);
                inst.SetNotInteractable();

                ActiveShapeUI.Add(inst);

            }
		}

        StartCoroutine(Co_Wait());
    }

    IEnumerator Co_Wait()
	{
        yield return null;

        foreach(var item in ActiveShapeUI)
		{
            item.SetInternalShapeScale();
		}
	}

    public event System.Action<CreatureData, EquippedPetInfo> OnPressChar;

    void OnPress()
	{
        OnPressChar?.Invoke(Creature, this);
    }

    public void Punch()
	{
        minorUI.Punch();
	}
}
