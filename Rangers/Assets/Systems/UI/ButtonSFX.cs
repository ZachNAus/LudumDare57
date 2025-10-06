using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSFX : MonoBehaviour
{
	private void Awake()
	{
		GetComponent<Button>().onClick.AddListener(() => SoundManager.instance.PlaySoundEffect(AudioType.ButtonPress));
	}
}
