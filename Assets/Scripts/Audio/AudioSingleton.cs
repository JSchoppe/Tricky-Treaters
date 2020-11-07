using System.Collections.Generic;
using UnityEngine;

public enum BackgroundTrack : byte
{
    Waltz, Synth, Rock
}

public sealed class AudioSingleton : MonoBehaviour
{
    [SerializeField] private AudioSource backgroundAudioSource = null;

    [SerializeField] private AudioClip backgroundTrackWaltz = null;
    [SerializeField] private AudioClip backgroundTrackSynth = null;
    [SerializeField] private AudioClip backgroundTrackRock = null;

    private static AudioSource bgmAudioSource;
    private static Dictionary<BackgroundTrack, AudioClip> backgroundTracks;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        bgmAudioSource = backgroundAudioSource;
        backgroundTracks = new Dictionary<BackgroundTrack, AudioClip>();
        backgroundTracks.Add(BackgroundTrack.Waltz, backgroundTrackWaltz);
        backgroundTracks.Add(BackgroundTrack.Synth, backgroundTrackSynth);
        backgroundTracks.Add(BackgroundTrack.Rock, backgroundTrackRock);
    }

    public static void PlayBackgroundMusic(BackgroundTrack track)
    {
        StopBackgroundMusic();
        bgmAudioSource.clip = backgroundTracks[track];
        bgmAudioSource.Play();
    }
    public static void StopBackgroundMusic()
    {
        bgmAudioSource.Stop();
    }

}
