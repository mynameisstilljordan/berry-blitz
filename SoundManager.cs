using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {
    public static AudioClip _pop1, _pop2, _pop3, _step, _drop, _combo1, _combo2, _combo3, _click;
    static AudioSource _audioSrc, _musicSrc;

    // Start is called before the first frame update
    void Start() {
        _pop1 = Resources.Load<AudioClip>("pop1");
        _pop2 = Resources.Load<AudioClip>("pop2");
        _pop3 = Resources.Load<AudioClip>("pop3");
        _step = Resources.Load<AudioClip>("step");
        _drop = Resources.Load<AudioClip>("drop");
        _combo1 = Resources.Load<AudioClip>("combo1");
        _combo2 = Resources.Load<AudioClip>("combo2");
        _combo3 = Resources.Load<AudioClip>("combo3");
        _click = Resources.Load<AudioClip>("click");

        var audioSources = GetComponents<AudioSource>();
        
        _audioSrc = audioSources[0];
        _musicSrc = audioSources[1];
        
        UpdateMusicPreference();
    }

    //this method plays the inputted sound
    public static void PlaySound(string clip) {
        if (PlayerPrefs.GetInt("sound", 1) == 1) {
            switch (clip) {
                //pop sound
                case "pop":
                    switch (Random.Range(1, 4)) {
                        case 1: _audioSrc.PlayOneShot(_pop1);
                            break;
                        case 2: _audioSrc.PlayOneShot(_pop2);
                            break;
                        case 3: _audioSrc.PlayOneShot(_pop3);
                            break;
                    }
                    break;
                //step sound
                case "step":
                    _audioSrc.PlayOneShot(_step);
                    break;
                //drop sound
                case "drop":
                    _audioSrc.PlayOneShot(_drop);
                    break;
                //combo 1
                case "combo1":
                    _audioSrc.PlayOneShot(_combo1);
                    break;
                //combo 2
                case "combo2":
                    _audioSrc.PlayOneShot(_combo2);
                    break;
                //combo 3
                case "combo3":
                    _audioSrc.PlayOneShot(_combo3);
                    break;
                //click
                case "click":
                    _audioSrc.PlayOneShot(_click);
                    break;
            }
        }
    }

    public static void UpdateMusicPreference() {
        if (PlayerPrefs.GetInt("music", 1) == 0) _musicSrc.volume = 0f;
        else _musicSrc.volume = 0.2f;
    }
}