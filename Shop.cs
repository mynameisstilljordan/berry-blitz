using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoreMountains.FeedbacksForThirdParty;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour{
    [SerializeField] private Image _faceDisplay;
    [SerializeField] private Sprite[] _allFaces;
    [SerializeField] private Button _purchaseButton, _leftButton, _rightButton, _enableButton, _disableButton;
    [SerializeField] private TMP_Text _coinsText, _priceText;
    private int _faceIndex, _maxUnlockedFace, _currentPrice, _currentCoins;
    [SerializeField] string _enabledFaces;

    private int[] _facePrices = new int[] {
        0, 500, 750, 1250, 2000, 3000, 4250, 5750, 7500, 10000, 15000
    };

    // Start is called before the first frame update
    void Start() {
        _maxUnlockedFace = PlayerPrefs.GetInt("unlockedFace", 1);
        if (_maxUnlockedFace < 11) _currentPrice = _facePrices[_maxUnlockedFace];
        _faceIndex = 0;
        
        _purchaseButton.onClick.AddListener(PurchaseFace);
        _leftButton.onClick.AddListener(OnLeftButtonPressed);
        _rightButton.onClick.AddListener(OnRightButtonPressed);
        _enableButton.onClick.AddListener(EnableButtonPressed);
        _disableButton.onClick.AddListener(DisableButtonPressed);

        _enabledFaces = PlayerPrefs.GetString("enabledFaces", "10000000000");
    }

    void PurchaseFace() {
        _currentCoins -= _currentPrice;
        PlayerPrefs.SetInt("coins", _currentCoins);
        _maxUnlockedFace++;
        PlayerPrefs.SetInt("unlockedFace", _maxUnlockedFace);
        UpdateCoinsDisplay();
        UpdateUI();
        //so face is enabled when purchased
        EnableFace(_faceIndex);
        UpdatePurchaseButtonAction(1);
        if (_maxUnlockedFace < 11) _currentPrice = _facePrices[_maxUnlockedFace];
        SoundManager.PlaySound("click"); //play click sound
    }

    void OnLeftButtonPressed() {
        if (_faceIndex == 0) _faceIndex = _allFaces.Length - 1;
        else _faceIndex--;
        UpdateFace();
        UpdatePriceText();
        UpdateUI();
        SoundManager.PlaySound("click"); //play click sound
    }

    void OnRightButtonPressed() {
        if (_faceIndex == _allFaces.Length - 1) _faceIndex = 0;
        else _faceIndex++;
        UpdateFace();
        UpdatePriceText();
        UpdateUI();
        SoundManager.PlaySound("click"); //play click sound
    }

    void UpdateFace() {
        _faceDisplay.sprite = _allFaces[_faceIndex]; //update the face
    }

    public void SetFaceDisplay(int index) {
        _faceIndex = index;
        _faceDisplay.sprite = _allFaces[_faceIndex];
        _currentCoins = PlayerPrefs.GetInt("coins", 0);
        _coinsText.text = "Balance: $" + _currentCoins;
        UpdatePriceText();
        UpdateUI();
    }

    private void UpdateCoinsDisplay() {
        _coinsText.text = "Balance: $" + _currentCoins;
    }

    private void UpdatePriceText() {
        _priceText.text = "$" + _facePrices[_faceIndex];
    }

    void EnableButtonPressed() {
        DisableFace(_faceIndex);
        UpdatePurchaseButtonAction(1);
        SoundManager.PlaySound("click"); //play click sound
    }

    void DisableButtonPressed() {
        EnableFace(_faceIndex);
        UpdatePurchaseButtonAction(1);
        SoundManager.PlaySound("click"); //play click sound
    }

    private void EnableFace(int face) {
        StringBuilder sb = new StringBuilder(_enabledFaces);
        sb[face] = '1';
        _enabledFaces = sb.ToString();
        PlayerPrefs.SetString("enabledFaces", _enabledFaces);
    }

    private void DisableFace(int face) {
        StringBuilder sb = new StringBuilder(_enabledFaces);
        sb[face] = '0';
        _enabledFaces = sb.ToString();
        PlayerPrefs.SetString("enabledFaces", _enabledFaces);
    }

    private bool IsFaceEnabled(int face) {
        if (_enabledFaces.ToCharArray()[face] == '1') {
            return true;
        }
        return false;
        
    }

    //type 0 = puchase button, type 1 = enable/disable button
    private void UpdatePurchaseButtonAction(int type) {
        if (type == 0) {
            _purchaseButton.gameObject.SetActive(true);
            _enableButton.gameObject.SetActive(false);
            _disableButton.gameObject.SetActive(false);
            _priceText.text = "$" + _facePrices[_faceIndex];
        }
        else {
            //if face is enabled
            if (IsFaceEnabled(_faceIndex)) {
                _enableButton.gameObject.SetActive(true);
                _disableButton.gameObject.SetActive(false);
                _purchaseButton.gameObject.SetActive(false);
            }

            //if face is disabled
            else {
                _enableButton.gameObject.SetActive(false);
                _disableButton.gameObject.SetActive(true);
                _purchaseButton.gameObject.SetActive(false);
            }
            _priceText.text = "---";
        }
    }

    void UpdateUI() {
        //if broke boi
        if (_facePrices[_faceIndex] > _currentCoins) {
            UpdatePurchaseButtonAction(0);
            _purchaseButton.interactable = false;
        }
        
        //if breadman
        else if (_facePrices[_faceIndex] <= _currentCoins){
            UpdatePurchaseButtonAction(0);
            _purchaseButton.interactable = true;
        }
        
        if (_maxUnlockedFace > _faceIndex) {
            UpdatePurchaseButtonAction(1);
        }
        
        if (_faceIndex == _maxUnlockedFace || _faceIndex == 10) _rightButton.interactable = false;
        else _rightButton.interactable = true;
        
        if (_faceIndex == 0) _leftButton.interactable = false;
        else _leftButton.interactable = true;
    }
}
