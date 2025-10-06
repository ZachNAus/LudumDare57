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
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource sfxSource;

	[Header("Audio")]
	[SerializeField] AudioClip[] nonBattleMusic;
	[SerializeField] AudioClip[] BattleMusic;
	[Space]

	[SerializeField] AudioDict soundEffects;

	[System.Serializable]
	class AudioDict : SerializableDictionary<AudioType, AudioList> { }

	[System.Serializable]
	class AudioList
	{
		public AudioClip[] clips;
	}

	private void Awake()
	{
		SwapMusic(false);
		PlayMusic();
	}

	public void SwapMusic(bool battleMusic)
	{
		musicSource.clip = battleMusic ? BattleMusic.GetRandom() : nonBattleMusic.GetRandom();
	}
    public void PauseMusic()
	{
		musicSource.DOFade(0, 0.5f);
	}
	public void PlayMusic()
	{
		musicSource.DOFade(1, 0.5f);
	}

	public void PlaySoundEffect(AudioType t)
	{
		var clip = soundEffects[t].clips.GetRandom();

		sfxSource.PlayOneShot(clip);
	}
}
