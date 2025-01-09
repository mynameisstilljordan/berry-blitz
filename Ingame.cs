using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Ingame : MonoBehaviour{
    
    [SerializeField] private Canvas _pauseCanvas;
    [SerializeField] private Button _settingsButton, _continueButton, _quitButton;
    enum GameState {ingame, paused}

    private GameState _currentGameState;

    // Start is called before the first frame update
    void Start() {
        GameManager.Instance._hasGameBeenPlayedInThisSession = true;
        PlayerPrefs.SetInt("gamesPlayed",PlayerPrefs.GetInt("gamesPlayed",0)+1);
        Debug.Log(PlayerPrefs.GetInt("gamesPlayed",0));
        _pauseCanvas.enabled = false;
        _currentGameState = GameState.ingame;
        _continueButton.onClick.AddListener(OnContinueButtonPressed);
        _quitButton.onClick.AddListener(OnQuitButtonPressed);
        _settingsButton.onClick.AddListener(OnSettingsButtonPressed);
    }

    void OnContinueButtonPressed() {
        _currentGameState = GameState.ingame;
        _pauseCanvas.enabled = false;
        Time.timeScale = 1f;
        SoundManager.PlaySound("click");
    }
    
    void OnSettingsButtonPressed() {
        _currentGameState = GameState.paused;
        _pauseCanvas.enabled = true;
        Time.timeScale = 0f;
        SoundManager.PlaySound("click");
    }

    void OnQuitButtonPressed() {
        Time.timeScale = 1f;
        SceneManager.LoadScene("menu");
        SoundManager.PlaySound("click");
    }

    public bool IsGamePaused() {
        if (_currentGameState == GameState.paused) return true;
        return false;
    }
}
