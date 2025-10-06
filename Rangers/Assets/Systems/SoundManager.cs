using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AudioType
{
	ButtonPress,

	PlaceTile,
	RemoveTile,
	PickupTile,

	RoundStart,
	HitGo,

	TilesPunch,
	HealthBarHit,

	LoseEncounter,
	WinEncounter,


	AddToTeam,
	RemoveFromTeam
}

public class SoundManager : MonoBehaviour
{
	[SerializeField] AudioSource musicSourceBattle;
	[SerializeField] AudioSource musicSourceNonBattle;
	[SerializeField] AudioSource sfxSource;

	[Header("Audio")]
	[SerializeField] AudioClip[] nonBattleMusic;
	[SerializeField] AudioClip[] BattleMusic;
	[Space]

	[SerializeField] AudioDict soundEffects;

	[System.Serializable]
	class AudioDict : SerializableDictionary<AudioType, AudioList> { }

	public static SoundManager instance;

	[System.Serializable]
	class AudioList
	{
		public AudioClip[] clips;
	}

	private void Awake()
	{
		instance = this;
		musicSourceNonBattle.clip = nonBattleMusic.GetRandom();
		musicSourceNonBattle.Play();

		musicSourceBattle.Stop();
	}

	public void SwapMusic(bool battleMusic)
	{
		var fadeIn = battleMusic ? musicSourceBattle : musicSourceNonBattle;
		var fadeOut = !battleMusic ? musicSourceBattle : musicSourceNonBattle;

		fadeOut.DOKill();
		fadeOut.DOFade(0, 0.5f).OnComplete(() => fadeOut.Stop());

		fadeIn.DOKill();
		fadeIn.clip = battleMusic ? BattleMusic.GetRandom() : nonBattleMusic.GetRandom();
		fadeIn.Play();
		fadeIn.DOFade(1, 0.5f);
	}
	public void FadeOutBothMusic()
	{
		musicSourceBattle.DOKill();
		musicSourceBattle.DOFade(0, 0.5f);

		musicSourceNonBattle.DOKill();
		musicSourceNonBattle.DOFade(0, 0.5f);
	}
	public void FadeInMusic(bool battleMusic, float delay)
	{
		var fadeIn = battleMusic ? musicSourceBattle : musicSourceNonBattle;

		fadeIn.DOKill();
		fadeIn.clip = battleMusic ? BattleMusic.GetRandom() : nonBattleMusic.GetRandom();
		fadeIn.Play();
		fadeIn.DOFade(1, 0.5f).SetDelay(delay);
	}

	public void PauseMusic(float pauseDuration, System.Action midPause, System.Action endPaude)
	{
		StartCoroutine(Co_PauseMusic(pauseDuration, midPause, endPaude));
	}
	IEnumerator Co_PauseMusic(float pauseDuration, System.Action midPause, System.Action endPaude)
	{
		musicSourceBattle.DOFade(0, 0.5f);
		musicSourceNonBattle.DOFade(0, 0.5f);

		yield return new WaitForSeconds(0.5f);

		midPause?.Invoke();

		yield return new WaitForSeconds(pauseDuration);

		musicSourceBattle.DOFade(0, 0.5f);
		musicSourceNonBattle.DOFade(0, 0.5f);

		yield return new WaitForSeconds(0.5f);

		endPaude?.Invoke();
	}

	public void PlaySoundEffect(AudioType t)
	{
		if (soundEffects.ContainsKey(t) && soundEffects[t].clips.Length > 0)
		{
			var clip = soundEffects[t].clips.GetRandom();

			sfxSource.PlayOneShot(clip);
		}
		else
		{
			Debug.LogError("DGVFISBNJNKJDGREWSKLNDGSV NB POEFJW WE GOT NO AUDIO LKNSGED:JBNGFRS:NFES:O");
		}
	}
}
