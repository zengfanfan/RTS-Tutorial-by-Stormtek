using UnityEngine;
using System.Collections.Generic;

public class AudioElement {

    private GameObject element;
    private Dictionary<AudioClip, GameObject> soundObjects = new Dictionary<AudioClip, GameObject>();

    public AudioElement(List<AudioClip> sounds, List<float> volumes, string id, Transform parentTransform) {
        if (sounds == null || sounds.Count == 0 || volumes == null || volumes.Count == 0 || sounds.Count != volumes.Count) return;
        element = new GameObject("AudioElement_" + id);
        if (parentTransform) element.transform.parent = parentTransform;
        else {
            //attach it to the game object list (since we know there should be one present)
            //do so to keep the inspector cleaner - this saves making a sounds object
            GameObjectList list = Object.FindObjectOfType(typeof(GameObjectList)) as GameObjectList;
            if (list) element.transform.parent = list.transform;
        }
        Add(sounds, volumes);
    }

    public void Add(List<AudioClip> sounds, List<float> volumes) {
        for (int i = 0; i < sounds.Count; i++) {
            AudioClip sound = sounds[i];
            if (!sound) continue;
            GameObject temp = new(sound.name);
            var audio = temp.AddComponent<AudioSource>();
            audio.clip = sound;
            audio.volume = volumes[i];
            temp.transform.parent = element.transform;
            soundObjects.Add(sound, temp);
        }
    }

    AudioSource GetAudioSource(AudioClip sound) {
        if (sound != null && soundObjects.TryGetValue(sound, out GameObject go)) {
            return go.GetComponent<AudioSource>();
        }
        return null;
    }
    bool GetAudioSource(AudioClip sound, out AudioSource audio) {
        audio = GetAudioSource(sound);
        return audio != null;
    }

    public void Play(AudioClip sound) {
        if (GetAudioSource(sound, out var audio) && !audio.isPlaying) audio.Play();
    }

    public void Pause(AudioClip sound) {
        if (GetAudioSource(sound, out var audio)) audio.Pause();
    }

    public void Stop(AudioClip sound) {
        if (GetAudioSource(sound, out var audio)) audio.Stop();
    }

    public bool IsPlaying(AudioClip sound) {
        if (GetAudioSource(sound, out var audio)) return audio.isPlaying;
        return false;
    }

}
