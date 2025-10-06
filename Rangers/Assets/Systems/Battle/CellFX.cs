using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CellFX : MonoBehaviour
{
    [SerializeField] Button btn;

    [SerializeField] Image img;
    [SerializeField] Image edgeImg;

    [Space]
    [SerializeField] float popScale = 1.1f;
    [SerializeField] float popScaleBig = 1.1f;
    public float popTime = 1;
    [SerializeField] int popvibrato = 10;
    [SerializeField] float popelasticity = 1;

    [Space]
    [SerializeField] TextMeshProUGUI dmgNotifier;

    public RectTransform rectTransform => transform as RectTransform;

    [ReadOnly][SerializeField] private Vector2Int gridCoordinate;
    public Vector2Int GridCoordinate => gridCoordinate;

    public void SetColor(Color col)
	{
        edgeImg.color = col;
        Color fillCol = new Color(col.r, col.g, col.b, 0.3f);
        img.color = fillCol;
        
    }

    public void Initialise(Vector2Int coordinate)
	{
        gridCoordinate = coordinate;
        btn.onClick.AddListener(OnClick);
	}

    private void OnClick()
	{
        if (GridManager.instance != null)
		{
            GridManager.instance.OnCellClicked(this);
		}
	}

    [Button("Pop")]
    public void Punch(bool big)
	{
        transform.DOKill();
        transform.localScale = Vector3.one;
        transform.DOPunchScale(Vector3.one * (big ? popScaleBig : popScale), popTime, popvibrato, popelasticity);
	}

    public void PunchMultiple(int times)
	{
        transform.DOKill();
        transform.localScale = Vector3.one;
        transform.DOPunchScale(Vector3.one * popScale, popTime, popvibrato, popelasticity).SetLoops(times, LoopType.Restart);
    }

    public void SetDmgShowActive(bool val)
	{
        dmgNotifier.gameObject.SetActive(val);
    }

    public void PreviewDmg(int dmg)
	{
        dmgNotifier.SetText(dmg.ToString());
    }
}
