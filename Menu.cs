using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Lofelt.NiceVibrations;

public class Menu : MonoBehaviour{
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _optionsButton;
    [SerializeField] private Button _tutorialButton;
    [SerializeField] private Button _musicOnButton, _musicOffButton, _sfxOnButton, _sfxOffButton, _backButton, _vibrationOnButton, _vibrationOffButton, _screenshakeOnButton, _screenshakeOffButton;
    [SerializeField] private Button _shopButton, _confirmShopButton, _creditsButton, _creditsCloseButton, _privacyButton;
    [SerializeField] private Canvas _menuCanvas, _optionscanvas, _shopCanvas, _creditsCanvas;
    [SerializeField] private Button _donateButton, _noAdsButton;
    [SerializeField] private Button _discordButton;
    private Shop _shop; 
    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.GetInt("noAds", 0) == 1) {
            _donateButton.gameObject.SetActive(true);
            _noAdsButton.gameObject.SetActive(false);
        }
        else {
            _donateButton.gameObject.SetActive(false);
            _noAdsButton.gameObject.SetActive(true);
        }

        _shop = GetComponent<Shop>();
        
        CloseOptionsCanvas(); //close the options canvas by default
        _shopCanvas.enabled = false;
        _menuCanvas.enabled = true;
        _creditsCanvas.enabled = false;
        _startButton.onClick.AddListener(StartButtonPressed); //add listener
        _optionsButton.onClick.AddListener(OptionButtonPressed); //add listener
        _tutorialButton.onClick.AddListener(TutorialButtonPressed); //add listener
        _backButton.onClick.AddListener(BackButtonPressed);
        _sfxOnButton.onClick.AddListener(EnableSound);
        _sfxOffButton.onClick.AddListener(DisableSound);
        _musicOnButton.onClick.AddListener(EnableMusic);
        _musicOffButton.onClick.AddListener(DisableMusic);
        _vibrationOnButton.onClick.AddListener(EnableVibration);
        _vibrationOffButton.onClick.AddListener(DisableVibration);
        _screenshakeOnButton.onClick.AddListener(EnableScreenShake);
        _screenshakeOffButton.onClick.AddListener(DisableScreenShake);
        _creditsButton.onClick.AddListener(CreditsCanvasButtonPressed);
        _creditsCloseButton.onClick.AddListener(CreditsCanvasCloseButtonPressed);
        _privacyButton.onClick.AddListener(PrivacyButtonPressed);
        _discordButton.onClick.AddListener(DiscordButtonPressed);
        _shopButton.onClick.AddListener(OpenShopCanvas);
        _confirmShopButton.onClick.AddListener(CloseShopCanvas);

        CheckAndUpdateOptionsButtons();
    }

    private void StartButtonPressed() {
        SoundManager.PlaySound("click"); //play click sound
        SceneManager.LoadScene("ingame"); //load the ingame scene
    }

    private void OptionButtonPressed() {
        OpenOptionsCanvas();
        SoundManager.PlaySound("click"); //play click sound
    }

    private void EnableSound() {
        PlayerPrefs.SetInt("sound", 1);
        SoundManager.PlaySound("click"); //play click sound
        CheckAndUpdateOptionsButtons();
    }

    private void DisableSound() {
        PlayerPrefs.SetInt("sound", 0);
        CheckAndUpdateOptionsButtons();
    }

    private void EnableMusic() {
        PlayerPrefs.SetInt("music",1);
        SoundManager.PlaySound("click"); //play click sound
        CheckAndUpdateOptionsButtons();
        SoundManager.UpdateMusicPreference();
    }

    private void DisableMusic() {
        PlayerPrefs.SetInt("music", 0);
        SoundManager.PlaySound("click"); //play click sound
        CheckAndUpdateOptionsButtons();
        SoundManager.UpdateMusicPreference();
    }

    private void EnableVibration() {
        SoundManager.PlaySound("click"); //play click sound
        PlayerPrefs.SetInt("vibration",1);
        CheckAndUpdateOptionsButtons();
    }

    private void DisableVibration() {
        SoundManager.PlaySound("click"); //play click sound
        PlayerPrefs.SetInt("vibration",0);
        CheckAndUpdateOptionsButtons();
    }

    private void EnableScreenShake() {
        SoundManager.PlaySound("click"); //play click sound
        PlayerPrefs.SetInt("shake",1);
        CheckAndUpdateOptionsButtons();
    }

    private void DisableScreenShake() {
        SoundManager.PlaySound("click"); //play click sound
        PlayerPrefs.SetInt("shake",0);
        CheckAndUpdateOptionsButtons();
    }

    private void BackButtonPressed() {
        CloseOptionsCanvas();
        SoundManager.PlaySound("click"); //play click sound
    }
    
    private void TutorialButtonPressed() {
        SoundManager.PlaySound("click"); //play click sound
        Application.Quit();
    }

    private void CreditsCanvasButtonPressed() {
        _creditsCanvas.enabled = true;
        SoundManager.PlaySound("click"); //play click sound
    }

    private void CreditsCanvasCloseButtonPressed() {
        _creditsCanvas.enabled = false;
        SoundManager.PlaySound("click"); //play click sound
    }

    private void CheckAndUpdateOptionsButtons() {
        if (PlayerPrefs.GetInt("sound", 1) == 1) {
            _sfxOnButton.interactable = false;
            _sfxOffButton.interactable = true;
        }
        else {
            _sfxOnButton.interactable = true;
            _sfxOffButton.interactable = false;
        }
        if (PlayerPrefs.GetInt("music", 1) == 1) {
            _musicOnButton.interactable = false;
            _musicOffButton.interactable = true;
        }
        else {
            _musicOnButton.interactable = true;
            _musicOffButton.interactable = false;
        }

        if (PlayerPrefs.GetInt("shake", 1) == 1) {
            _screenshakeOnButton.interactable = false;
            _screenshakeOffButton.interactable = true;
        }
        else {
            _screenshakeOnButton.interactable = true;
            _screenshakeOffButton.interactable = false;
        }

        if (PlayerPrefs.GetInt("vibration", 0) == 1) {
            _vibrationOnButton.interactable = false;
            _vibrationOffButton.interactable = true;
        }
        else {
            _vibrationOnButton.interactable = true;
            _vibrationOffButton.interactable = false;
        }
    }

    private void OpenOptionsCanvas() {
        _optionscanvas.enabled = true;
    }
    
    private void CloseOptionsCanvas() {
        _optionscanvas.enabled = false;
    }

    private void OpenShopCanvas() {
        _shop.SetFaceDisplay(0); //set the face display to 0
        _shopCanvas.enabled = true;
        _menuCanvas.enabled = false;
        SoundManager.PlaySound("click"); //play click sound
    }

    private void CloseShopCanvas() {
        _shopCanvas.enabled = false;
        _menuCanvas.enabled = true;
        SoundManager.PlaySound("click"); //play click sound
    }

    public void HideNoAdsButton() {
        _noAdsButton.gameObject.SetActive(false);
        _donateButton.gameObject.SetActive(true);
    }

    private void DiscordButtonPressed() {
        SoundManager.PlaySound("click"); //play click sound
        Application.OpenURL("https://discord.gg/ajvAVM8dS7");
    }

    private void PrivacyButtonPressed() {
        GameManager.Instance.ShowGDPRPopup();
    }
}
