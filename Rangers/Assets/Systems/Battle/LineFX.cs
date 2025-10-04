using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LineFX : MonoBehaviour
{
    [SerializeField] Button btn;

    [SerializeField] Image img;

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
}
