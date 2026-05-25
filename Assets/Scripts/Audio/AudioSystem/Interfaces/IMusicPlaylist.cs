using System.Collections.Generic;
using UnityEngine;

namespace AudioSystem
{
    public interface IMusicPlaylist
    {
        void Initialize(List<AudioClip> tracks);

        AudioClip GetNextTrack();

        void Shuffle();

        bool HasTracks { get; }
    }
}