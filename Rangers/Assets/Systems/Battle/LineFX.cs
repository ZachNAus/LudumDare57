using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LineFX : MonoBehaviour
{
    [SerializeField] Button btn;

    [SerializeField] Image img;

    [Space]
    [SerializeField] float popScale = 1.1f;
    [SerializeField] float popTime = 1;
    [SerializeField] int popvibrato = 10;
    [SerializeField] float popelasticity = 1;

    public RectTransform rectTransform => transform as RectTransform;

    public void SetColor(Color col)
	{
        img.color = col;
    }

    public void Initialise()
	{
        btn.onClick.AddListener(OnClick);
	}

    private void OnClick()
	{
        if (GridManager.instance != null)
		{
            GridManager.instance.OnLineClicked(this);
		}
	}

    [Button("Pop")]
    public void PopLine()
	{
        transform.DOPunchScale(Vector3.one * popScale, popTime, popvibrato, popelasticity);
	}
}
