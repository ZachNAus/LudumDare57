using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using TMPro;

public class CharacterMinorUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField] Image charSprite;

	[SerializeField] Button btn;

	[SerializeField] bool canHover;

	[SerializeField] Transform toSpin;
	public bool Spin;
	[SerializeField] float spinSpeed;

	[Header("Optional")]
	[SerializeField] TextMeshProUGUI enemyNameTxt;

	public CreatureData Creature { get; private set; }

	public static event System.Action<CreatureData> OnHoverAny;

	private void Awake()
	{
		btn.onClick.AddListener(OnPress);
	}

	System.Action<CreatureData, CharacterMinorUI> onPress;
	public void Setup(CreatureData creature, System.Action<CreatureData, CharacterMinorUI> onPress)
	{
		this.onPress = onPress;

		SetCreature(creature);
	}

	public void SetCreature(CreatureData creature)
	{
		Creature = creature;

		UpdateUI();
	}

	public void UpdateUI()
	{
		charSprite.gameObject.SetActive(Creature != null);

		if (Creature != null)
		{
			charSprite.sprite = Creature.sprite;
		}

		if (enemyNameTxt != null)
		{
			if(Creature != null)
			{
				enemyNameTxt.SetText(OwnedCreaturePage.instance.HasFoundCreature(Creature) ? Creature.creatureName : "???");
			}
			else
			{
				enemyNameTxt.SetText("???");
			}
		}
	}


	void OnPress()
	{
		onPress?.Invoke(Creature, this);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (canHover)
			OnHoverAny?.Invoke(Creature);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (canHover)
			OnHoverAny?.Invoke(null);
	}

	public void Punch()
	{
		transform.DOKill();
		transform.localScale = Vector3.one;
		transform.DOPunchScale(Vector3.one * 0.5f, 0.5f, 5, 3);
	}

	private void Update()
	{
		if (Spin)
		{
			toSpin.Rotate(new Vector3(0,0,spinSpeed * Time.deltaTime));
		}
	}
}
