using UnityEngine;

namespace AudioSystem
{
    public interface IAudioPlayer
    {
        void Play(AudioClip audioClip);
        void PlayOneShot(AudioClip audioClip);
        void PlayOneShot(AudioClip audioClip, float volume);

        void Stop();
        void SetVolume(float volume);
        float GetVolume();

        bool IsPlaying { get; }
    }
}