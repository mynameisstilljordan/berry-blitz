using System.Collections;
using System.Collections.Generic;
using System.Text;
using CartoonFX;
using DamageNumbersPro;
using DG.Tweening;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using Image = UnityEngine.UI.Image;

//this somehow became the ingame handler class...
public class FruitPlacer : MonoBehaviour {
    public enum FruitType {BlueBerry, Cherry, Kiwi, Plum, Orange, Apple, Peach, Pear, Coconut, Pineapple, WaterMelon } //the fruit types
    public enum GameState{Playing, Ended, Paused}

    private GameState _currentGameState;
    private CFXR_Effect _fxController;
    private Vector3 _fruitPlacerPos;
    private GameObject _board;
    private Scaler _s;
    private float _xClamp, _placerScale, _fruitScale; //the x clamp of the fruit placer
    private FruitType _currentFruitType; //the current fruit type
    private FruitBrain _currentFruitBrain;
    private GameObject _currentFruitObject;
    private GameObject _fruitSpawnPoint;
    public Queue<FruitType> _fruitQueue; //the queue of fruit
    [SerializeField] private GameObject _collisionExplosion;
    private float _highestY; //the y value of the tap registration line
    [SerializeField] private Sprite[] _fruitSprites;
    [SerializeField] private DamageNumber _dN;
    [SerializeField] private TMP_FontAsset[] _neonFonts;
    private int _currentNeonFont; //current font
    private int _currentCombo; //the current fruit merge combo
    private int _score; //the score 
    private int _coins; //the coins count
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private TMP_Text _coinText;
    private IEnumerator _comboCoroutine; //to reset the combo coroutine if needed
    private bool _isComboActive;
    [SerializeField] private Image _nextFruitImage;
    private bool _isFruitAllowedToDrop = true;
    [SerializeField] private GameObject _endLine;
    private SpriteRenderer _endLineSR;
    private string _enabledFaces;
    private int _faceCount;
    private Ingame _ingame;
    [SerializeField] private GameObject _mainCanvas;
    [SerializeField] private GameObject _endgamePanel;
    [SerializeField] private TMP_Text _endgameScore;
    [SerializeField] private TMP_Text _endgameBest;
    [SerializeField] private GameObject _ingameScoreCanvas;
    [SerializeField] private GameObject _endgameScoreCanvas;
    [SerializeField] private Button _continueButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _trophyButton;
    
    private Fruit[] _allFruits = new Fruit[] {
        new Fruit(FruitType.BlueBerry, 0.05f, 0),
        new Fruit(FruitType.Cherry, 0.055f, 1),
        new Fruit(FruitType.Kiwi, 0.065f, 2),
        new Fruit(FruitType.Plum, 0.07f, 3),
        new Fruit(FruitType.Orange, 0.08f, 4),
        new Fruit(FruitType.Apple, 0.1125f, 5),
        new Fruit(FruitType.Peach, 0.1375f, 6),
        new Fruit(FruitType.Pear, 0.1625f, 7),
        new Fruit(FruitType.Coconut, 0.2f, 8),
        new Fruit(FruitType.Pineapple, 0.225f, 9),
        new Fruit(FruitType.WaterMelon, 0.25f, 10)
    };
    
    [SerializeField] private Sprite[] _allFaces;
    private Sprite[] _selectedFaces;

    //this method returns the fruit of the given fruit type
    public Fruit FruitTypeToFruit(FruitType type) {
        return _allFruits[(int)type]; //return the fruit
    }

    public Sprite[] GetAllSprites() {
        return _fruitSprites;
    }

    // Start is called before the first frame update
    void Start() {
        _ingame = GameObject.FindGameObjectWithTag("IngameHandler").GetComponent<Ingame>();
        _enabledFaces = PlayerPrefs.GetString("enabledFaces", "10000000000");
        _selectedFaces = new Sprite[GetEnabledFaceCount()]; //array for the selected faces
        LoadSelectedFaces(); //load the selected faces from the string to sprite array
        
        _fxController = _collisionExplosion.GetComponent<CFXR_Effect>();

        if (PlayerPrefs.GetInt("shake", 1) == 0) _fxController.cameraShake.enabled = false; //disable camera shake if playerpref is off

        _continueButton.onClick.AddListener(OnContinueButtonPressed); //add listener for continue button
        _settingsButton.onClick.AddListener(OnSettingsButtonPressed); //add listener for settings button
        _trophyButton.onClick.AddListener(OnTrophyButtonPressed); //add listener for trophy button
        
        _currentGameState = GameState.Playing;
        _endgamePanel.SetActive(false); //deactivate endgame panel
        
        _endgameScoreCanvas.SetActive(false); //deactivate endgame score canvas
        _dN.PrewarmPool(); //pre warm the object pool for the damage numbers
        _comboCoroutine = ResetCombo(); //set the coroutine 
        
        _fruitSpawnPoint = transform.GetChild(0).gameObject; //get the fruit spawn point
        _highestY = _fruitSpawnPoint.transform.position.y; //set the highest y value
        _fruitQueue = new Queue<FruitType>(); //initialize queue
        InitializeQueue(); //add the initial elements to the queues
        _s = GetComponentInParent<Scaler>(); //get the scaler component
        _board = transform.parent.gameObject; //get the board reference
        _placerScale = transform.localScale.x; //set the local scale
        SpawnFruit(); //spawn the first fruit
        UpdateXClamp(); //update the x clamp
        _collisionExplosion.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
        _endLineSR = _endLine.GetComponent<SpriteRenderer>(); //get the sprite renderer from the end line
        //StartEndLineAnimation(); //start the endline flash animation
    }

    //required for array allocation
    private int GetEnabledFaceCount() {
        StringBuilder sb = new StringBuilder(_enabledFaces);
        int count = 0;
        for (int i = 0; i < 11; i++) //for all faces
            if (sb[i] == '1') //if face enabled
                count ++; //increment count
        _faceCount = count;
        return count; //return count
    }
    
    private void LoadSelectedFaces() {
        StringBuilder sb = new StringBuilder(_enabledFaces);
        for (int i = 0; i < 11; i++) //for all faces
            if (sb[i] == '1') //if face enabled
                AddSpriteToSelectedFaces(_allFaces[i]);
    }
    
    private void AddSpriteToSelectedFaces(Sprite sprite) {
        for (int i = 0; i < _selectedFaces.Length; i++) {
            if (_selectedFaces[i] == null) {
                _selectedFaces[i] = sprite;
                break;
            }
        }
    }

    private Sprite GetRandomFace() {
        return _selectedFaces[Random.Range(0, _selectedFaces.Length)];
    }
    
    public float GetMaxHeight() {
        return _endLine.transform.position.y;
    }
    
    void AddRandomFruitToQueue() {
        int choice = 0;
        float randomNumber = Random.Range(0f,1f);

        if (randomNumber >= 0 && randomNumber < 0.2f) choice = 0; //0-20 (20%) blueberry
        else if (randomNumber >= 0.2f && randomNumber < 0.4f) choice = 1; //20-40 (20%) cherry
        else if (randomNumber >= 0.4f && randomNumber < 0.6f) choice = 2; //40-60 (20%) kiwi
        else if (randomNumber >= 0.6f && randomNumber < 0.8f) choice = 3; //60-80 (20%) plum
        else choice = 4; //80-100 (20%) orange

        _fruitQueue.Enqueue((FruitType) choice); //add the fruit type to queue

        //if there is more than 1 fruit in queue
        if (_fruitQueue.Count > 1) {
            var nextFruit = _fruitQueue.ToArray(); //convert to array
            _nextFruitImage.sprite = _fruitSprites[(int)nextFruit[1]]; //update the next image for the fruit
            if (_nextFruitImage.sprite == _fruitSprites[1]) _nextFruitImage.rectTransform.sizeDelta = new Vector2(50, 60); //if cherry, resize
            else _nextFruitImage.rectTransform.sizeDelta = new Vector2(50, 50); //otherwise
        }
    }

    //this method sets up the queue
    void InitializeQueue() {
        //add 3 items to the queue
        for (int i = 0; i < 3; i++) AddRandomFruitToQueue(); //add a random fruit from 0-3 (cherry - kiwi)
    }

    void SpawnFruit() {
        _currentFruitType = _fruitQueue.Peek(); //set the current fruit type to the last in queue
        _currentFruitObject = ObjectPool.Instance.GetFruit(); //spawn a fruit from the object pool
        
        _currentFruitBrain = _currentFruitObject.GetComponent<FruitBrain>();
        _currentFruitBrain.InitializeFruit(_allFruits[(int)_currentFruitType]); //initialize the fruit with the current fruit as a parameter
        _currentFruitBrain.GetSpriteRenderer().sprite = _fruitSprites[(int)_currentFruitType]; //set the sprite to the current fruit sprite
        _currentFruitBrain.SuspendPhysics(); //suspend the physics of the fruit
        _currentFruitBrain.ResetRotation(); //reset the rotation of the fruit
        if (_faceCount > 0) _currentFruitBrain.SetFaceSprite(GetRandomFace()); else _currentFruitBrain.DisableFace(); //set random face is a face is equipped, otherwise remove face
        _currentFruitObject.SetActive(true); //enable the object
        
        UpdateXClamp(); //update the x clamp of the fruit placer
        UpdatePlacerPositionAccordingToClamp(); //adjusts the fruit placer if a fruit would be over the boundary
    }

    void DropFruit() {
        _currentFruitObject.transform.position = _fruitSpawnPoint.transform.position; //move the z before releasing
        _currentFruitBrain.EnablePhysics(); //enable the physics of the fruit
        _fruitQueue.Dequeue(); //remove the fruit from the queue
        AddRandomFruitToQueue(); //add a new fruit to the queue
        SpawnFruit(); //spawn the next fruit in queue
    }

    //this method updates the x clamp boundaries depending on the scale of the fruit placer, the scale of the board, and the scale of the current fruit
    void UpdateXClamp() {
        _fruitScale = _allFruits[(int)_currentFruitType].GetScale(); //get the scale of the current fruit type
        //_xClamp = Mathf.Abs((_board.transform.localScale.x / 2) - (_board.transform.localScale.x*(_placerScale/2))); //set the x clamp
        _xClamp = Mathf.Abs((_board.transform.localScale.x / 2) - (_board.transform.localScale.x*(_fruitScale / 2)));
    }

    // Update is called once per frame
    void Update() {
        if (_currentGameState == GameState.Playing && !_ingame.IsGamePaused()) {
            if (Input.touchCount > 0) { //if there's at least one touch on the screen 
                Touch touch = Input.touches[0]; //get the first touch
                //if touch started, moved, or stayed still
                if (IsTouchValid(touch) && (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)) {
                    _fruitPlacerPos = new Vector3(touch.position.x, 0, 0); //set the fruit placer pos to the x of the touch position
                    transform.position = (Camera.main.ScreenToWorldPoint(_fruitPlacerPos)); //set the position to the 
                    UpdatePlacerPositionAccordingToClamp();
                }
                //if the phase ended 
                else if (IsTouchValid(touch) && touch.phase == TouchPhase.Ended && _isFruitAllowedToDrop) {
                    DropFruit(); //drop the current fruit being held
                    SoundManager.PlaySound("drop"); //play drop sound
                }
            }

            _currentFruitObject.transform.position = new Vector3(_fruitSpawnPoint.transform.position.x, _fruitSpawnPoint.transform.position.y, -2f); //move the current fruit to the position of the fruit placer
        }
    }

    //this method returns true if the given touch is valid (below the board)
    private bool IsTouchValid(Touch touch) {
        if (Camera.main.ScreenToWorldPoint(touch.position).y < _highestY) return true; //if touch is below the line, return true
        return false; //otherwise return false
    }

    void UpdatePlacerPositionAccordingToClamp() {
        var pos = transform.position; //the position for clamping
        pos.x = Mathf.Clamp(transform.position.x, -_xClamp, _xClamp); //x clamp
        pos.y = Mathf.Clamp(transform.position.y, 0f, 0f); //y clamp
        pos.z = Mathf.Clamp(transform.position.z, -0.1f, 0.1f); //z clamp
        transform.position = pos; //set the position (including the clamps)
    }

    //play the collision explosion
    public void PlayCollisionExplosion(Vector3 location) {
        Instantiate(_collisionExplosion, new Vector3(location.x, location.y, -2), quaternion.identity); //create the explosion at location
        SoundManager.PlaySound("pop");
    }

    //this method updates the current score
    public void UpdateScore(FruitType type) {
        switch (type) {
            case FruitType.BlueBerry:
                _score++; //add 1 score
                _coins++;
                break;
            case FruitType.Cherry:
                _score += 3; //add 3 score
                _coins += 2;
                break;
            case FruitType.Kiwi:
                _score += 6; //add 6 score
                _coins += 3;
                break;
            case FruitType.Plum:
                _score += 10; //add 10 score
                _coins += 4;
                break;
            case FruitType.Orange:
                _score += 15; //add 15 score
                _coins += 5;
                break;
            case FruitType.Apple:
                _score += 21; //add 21 score
                _coins += 6;
                break;
            case FruitType.Peach:
                _score += 28; //add 28 score
                _coins += 7;
                break;
            case FruitType.Pear:
                _score += 36; //add 45 score
                _coins += 8;
                break;
            case FruitType.Coconut:
                _score += 45; //add 36 score
                _coins += 9;
                break;
            case FruitType.Pineapple:
                _score += 55; //add 55 score
                _coins += 10;
                break;
            case FruitType.WaterMelon:
                _score += 66; //add 66 score
                _coins += 11;
                break;
        }

        _scoreText.text = _score.ToString(); //update the score text
        _coinText.text = _coins.ToString(); //update the coin counter
    }
    
    //this method updates the current fruit combo
    public void UpdateCombo() {
        _currentCombo++; //increment combo

        //if combo is greater than 1
        if (_currentCombo > 1) {
            var newFont = _currentNeonFont; //temp new font
            _currentNeonFont = Random.Range(0, _neonFonts.Length); //pick random font
            //if the new thought picked was the same as the last font, pick again
            while (newFont == _currentNeonFont) _currentNeonFont = Random.Range(0, _neonFonts.Length); //pick random font
            _dN.Spawn(_board.transform.position, _currentCombo).SetFontMaterial(_neonFonts[_currentNeonFont]); //spawn the damage number
            if (_currentCombo == 2) SoundManager.PlaySound("combo1");
            else if (_currentCombo == 3) SoundManager.PlaySound("combo2");
            else SoundManager.PlaySound("combo3");
        }
        
        if (_isComboActive) { //if coroutine is active
            StopCoroutine(_comboCoroutine); //stop the current one
            _comboCoroutine = ResetCombo(); 
            StartCoroutine(_comboCoroutine); //start it again
        }
        else{
            _comboCoroutine = ResetCombo(); 
            StartCoroutine(_comboCoroutine); //otherwise, start it
        }
    }

    //this method resets the fruit merge combo
    private IEnumerator ResetCombo() {
        _isComboActive = true; //mark combo as active
        yield return new WaitForSeconds(2f); //timer for combo reset
        _currentCombo = 0; //reset comobo
        _isComboActive = false; //unmark combo as active
    }

    public void EndGame() {
        //if game is still running
        if (_currentGameState != GameState.Ended) {
            _currentGameState = GameState.Ended; //end game

            var screenWidth = _board.GetComponent<Scaler>().GetScreenToWorldWidth();//save screen width 
            _board.transform.localScale = new Vector3(screenWidth*0.7f, screenWidth*0.7f, 1); //shrink board
            _mainCanvas.SetActive(false); //deactivate main canvas
            _endgamePanel.SetActive(true); //enable the endgame panel
            _ingameScoreCanvas.SetActive(false); //disable the ingame score canvas
            _endgameScoreCanvas.SetActive(true); //enable the endgame score canvas
            _endgameScore.text = "SCORE: " + _score; //update end score
            UpdateBestScore(); //check and update the best score
            UpdateTotalCoins();
            _endgameBest.text = "BEST: " + PlayerPrefs.GetInt("highScore", 0); //load highscore

            var allFruits = GameObject.FindGameObjectsWithTag("Fruit"); //find all fruits
            foreach (GameObject fruit in allFruits) { //for all fruits
                var brain = fruit.GetComponent<FruitBrain>(); //get the brain instance
                brain.SuspendPhysics(); //suspend physics for fruit
                brain.GetRigidbody().simulated = false; //disable rigidbody simulation
                _currentFruitBrain.GetSpriteRenderer().enabled = false; //hide the current fruit
            }
        }
    }

    //this method checks the highscore and updates if the current score is higher
    private void UpdateBestScore() {
        var bestScore = PlayerPrefs.GetInt("highScore", 0); //load highscore
        if (_score > bestScore) PlayerPrefs.SetInt("highScore", _score); //check and replace highscore
    }

    private void UpdateTotalCoins() {
        var currentCoins = PlayerPrefs.GetInt("coins", 0);
        PlayerPrefs.SetInt("coins", currentCoins + _coins);
    }

    private void StartEndLineAnimation() {
        Sequence endLineSequence = DOTween.Sequence(); //create new sequence
        float animationSpeed = 0.5f;
        endLineSequence.Append(_endLineSR.DOColor(Color.red, animationSpeed)) //append
            .AppendInterval(3f)
            .Append(_endLineSR.DOColor(Color.white, animationSpeed)) //append
            .Append(_endLineSR.DOColor(Color.red, animationSpeed*3f));
        endLineSequence.SetLoops(-1); //start
        endLineSequence.Play(); //play the sequence
    }

    //this method is called when the continue button is pressed
    private void OnContinueButtonPressed() {
        SoundManager.PlaySound("click"); //play click sound

        SceneManager.LoadScene("menu");
    }
    
    //this method is called when the settings button is pressed
    private void OnSettingsButtonPressed() {
        SoundManager.PlaySound("click"); //play click sound
    }
    
    //this method is called when the trophies button is pressed
    private void OnTrophyButtonPressed() {
        SoundManager.PlaySound("click"); //play click sound
    }

    //this method returns the current gamestate
    public GameState GetGameState() {
        return _currentGameState;
    }
}
