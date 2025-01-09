using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Lofelt.NiceVibrations;

public class FruitBrain : MonoBehaviour{

    public int _mergeID; //the id that's used to prevent merge duplications
    private FruitPlacer.FruitType _fruitType; //the type of the fruit
    private SpriteRenderer _sR;
    private Rigidbody2D _rB;
    private CapsuleCollider2D _c;
    private FruitPlacer _fP;
    private bool _hasFruitMadeContact;
    private bool _canFruitMerge;
    private bool _canFruitEndGame;
    private bool _canPulse = true; 
    private float _maxHeight;
    private int _endGameCounter;
    private bool _vibrationsEnabled;
    private GameObject _face;
    private Image _faceImage;
    private SpriteRenderer _faceSpriteRenderer;
    private float _faceDisplacementY;

    void Start() {
        _sR = GetComponent<SpriteRenderer>();
        _rB = GetComponent<Rigidbody2D>();
        _c = GetComponent<CapsuleCollider2D>();
        RandomizeMergeID();
        if (PlayerPrefs.GetInt("vibration", 0) == 1) _vibrationsEnabled = true;
    }
    
    //this method randomizes the merge id 
    public void RandomizeMergeID() { _mergeID = Random.Range(int.MinValue, int.MaxValue); } //randomize the merge ID

    //this method returns the merge id of the fruit
    public int GetMergeID() { return _mergeID; }
    
    //this method returns the fruit type of the fruit
    public FruitPlacer.FruitType GetFruitType() { return _fruitType; }
    
    //this method returns the rigidbody
    public Rigidbody2D GetRigidbody() {
        return _rB;
    }

    //this method sets the scale of the fruit object
    private void SetFruitScale(float scale) {
        var board = _fP.transform.parent;
        transform.SetParent(board); 
        transform.localScale = new Vector3(scale, scale, 1);
    }

    private void SetSprite(Sprite sprite) { _sR.sprite = sprite; }

    //this method passes all parameters from fruit object to fruit brain
    public void InitializeFruit(Fruit fruit) {
        if (_face == null) _face = transform.GetChild(0).transform.gameObject; //set face
        if (_faceSpriteRenderer == null) _faceSpriteRenderer = _face.GetComponent<SpriteRenderer>();
        _hasFruitMadeContact = false;
        _canFruitEndGame = false;
        if (!_sR) _sR = GetComponent<SpriteRenderer>();
        if (!_c) _c = GetComponent<CapsuleCollider2D>();
        if (!_rB) _rB = GetComponent<Rigidbody2D>();
        if (!_fP) _fP = GameObject.FindGameObjectWithTag("FruitPlacer").GetComponent<FruitPlacer>(); //initialize fruit placer if not initialized
        if (_maxHeight == 0) _maxHeight = _fP.GetMaxHeight();
        _fruitType = fruit.GetFruitType();
        SetFruitScale(fruit.GetScale());
        _canFruitMerge = true; //let fruit merge again
        AdjustCollider(); //adjust the collider of the fruit
    }

    public SpriteRenderer GetSpriteRenderer() { return _sR; }

    public void SuspendPhysics() {
        _rB.gravityScale = 0f; //turn off gravity
        _c.enabled = false; //disable collisions
    }

    public void EnablePhysics() {
        _rB.gravityScale = 1f; //turn on gravity
        _c.enabled = true; //enable the collider
    }

    //when the fruit collides with another fruit
    private void OnCollisionEnter2D(Collision2D other) {
        //if collided with a fruit
        if (other.transform.CompareTag("Fruit")) {
            var otherBrain = other.gameObject.GetComponent<FruitBrain>(); //save the other fruit's brain
            if (otherBrain.IsThisFruitAbleToEndTheGame() && !_canFruitEndGame) _canFruitEndGame = true; //if this fruit makes contact with another fruit that's able to end the game, let this fruit do the same
            //if the fruit is the same fruit (and neither are watermelons)
            if (GetFruitType() == otherBrain.GetFruitType() && GetFruitType() != FruitPlacer.FruitType.WaterMelon) {
                //if this merge ID is greater
                if (GetMergeID() > otherBrain.GetMergeID() && _canFruitMerge) {
                    _canFruitMerge = false; //make fruit unable to merge
                    MergeFruits(other.gameObject); //merge the fruits
                }
                else if (GetMergeID() == otherBrain.GetMergeID()) RandomizeMergeID(); //in the rare case where merge ID's match, randomize them again
            }
            //if the type isn't the same 
            else if (!_hasFruitMadeContact) {
                SoundManager.PlaySound("step"); //step sound on contact with floor
            }
        }
        else if (other.transform.CompareTag("Floor")) {
            if (!_hasFruitMadeContact) SoundManager.PlaySound("step"); //step sound on contact with floor
            if (!_canFruitEndGame) _canFruitEndGame = true; //mark fruit as being able to end the game
        }
        _hasFruitMadeContact = true; //mark fruit as having made contact with an object
    }

    //this is so fruits will register collisions even if they've been active for a while
    private void OnCollisionStay2D(Collision2D other) {
        //if collided with a fruit
        if (other.transform.CompareTag("Fruit")) {
            var otherBrain = other.gameObject.GetComponent<FruitBrain>(); //save the other fruit's brain
            if (otherBrain.IsThisFruitAbleToEndTheGame() && !_canFruitEndGame) _canFruitEndGame = true; //if this fruit makes contact with another fruit that's able to end the game, let this fruit do the same
            //if the fruit is the same fruit (and neither are watermelons)
            if (GetFruitType() == otherBrain.GetFruitType() && GetFruitType() != FruitPlacer.FruitType.WaterMelon) {
                //if this merge ID is greater
                if (GetMergeID() > otherBrain.GetMergeID() && _canFruitMerge) {
                    _canFruitMerge = false; //make fruit unable to merge
                    MergeFruits(other.gameObject); //merge the fruits
                }
                else if (GetMergeID() == otherBrain.GetMergeID()) RandomizeMergeID(); //in the rare case where merge ID's match, randomize them again
            }
        }
        else if (other.transform.CompareTag("Floor") && !_canFruitEndGame) _canFruitEndGame = true; 
    }

    //this method merges fruits with each other
    private void MergeFruits(GameObject other) {
        other.gameObject.SetActive(false); //deactivate other
        transform.localPosition = (transform.localPosition + other.transform.localPosition) / 2; //set position to average position of both objects
        _fP.UpdateScore(_fruitType); //update the score
        InitializeFruit(_fP.FruitTypeToFruit(1 + GetFruitType())); //reinitialize the fruit
        UpdateSprite((int)_fruitType); //update the sprite
        transform.localRotation = Quaternion.identity; //reset rotation
        _rB.linearVelocity = Vector3.zero; //reset velocity
        _fP.PlayCollisionExplosion(transform.position); //play the explosion sound
        _fP.UpdateCombo(); //update the current combo
        if (_vibrationsEnabled) HapticPatterns.PlayPreset(HapticPatterns.PresetType.LightImpact);
    }

    private void UpdateSprite(int fruit) {
        _sR.sprite = _fP.GetAllSprites()[fruit];
    }

    //this method resets the rotation to its default state
    public void ResetRotation() {
        transform.rotation = Quaternion.identity;
    }

    public bool IsThisFruitAbleToEndTheGame() {
        return _canFruitEndGame;
    }

    private void FixedUpdate() {
        //if fruit can end game and the current height is above the line
        if (_canFruitEndGame && transform.position.y > _maxHeight) {
            if (_canPulse) PulseAnimation();
            _endGameCounter++; //increment the counter 
            if (_endGameCounter > 90) _fP.EndGame(); //when counter hits 90 (3 seconds) end game
        }
        else if (transform.position.y < _maxHeight && _endGameCounter > 0) _endGameCounter = 0; //reset counter if fruit falls below line 
    }

    private void PulseAnimation() {
        //if the game hasn't ended
        if (_fP.GetGameState() != FruitPlacer.GameState.Ended) {
            _canPulse = false; //dont allow to pulse again
            _sR.DOColor(Color.red, 0.5f).OnComplete(() => { _sR.DOColor(Color.white, 0.5f); });
            _canPulse = true; //allow pulse
        }
    }

    private void AdjustCollider() {
        _faceDisplacementY = 0;
        _c.direction = CapsuleDirection2D.Vertical;
        _c.size = new Vector2(1f, 0.65f);
        switch (_fruitType) {
            case FruitPlacer.FruitType.BlueBerry:
                _c.size = new Vector2(1.2f, 1f);
                _c.offset = new Vector2(0, -0.03f);
                break;
                
            case FruitPlacer.FruitType.Cherry:
                _c.size = new Vector2(1.2f, 1f);
                _c.offset = new Vector2(0, -0.22f);
                _faceDisplacementY = -0.25f;
                break;

            case FruitPlacer.FruitType.Kiwi:
                _c.size = new Vector2(1.2f, 1f);
                _c.offset = new Vector2(0, -0.005f);
                break;
            
            case FruitPlacer.FruitType.Orange:
                _c.size = new Vector2(1.3f, 1f);
                _c.offset = new Vector2(0.02f, -0.07f);
                _faceDisplacementY = -0.03f;
                break;
            
            case FruitPlacer.FruitType.Plum:
                _c.offset = new Vector2(0, -0.06f);
                _c.size = new Vector2(1.3f, 1f);
                _faceDisplacementY = -0.1f;
                break;
            
            case FruitPlacer.FruitType.Apple:
                _c.size = new Vector2(1.25f, 1.13f);
                _c.offset = new Vector2(0, -0.13f);
                _c.direction = CapsuleDirection2D.Horizontal;
                _faceDisplacementY = -0.1f;
                break;
                
            case FruitPlacer.FruitType.Peach:
                _c.size = new Vector2(1.37f, 0.7f);
                _c.offset = new Vector2(0f, 0.01f);
                break;
                
            case FruitPlacer.FruitType.Pear:
                _c.size = new Vector2(1.3f, 1.45f);
                _c.offset = new Vector2(0, -0.12f);
                break;
            
            case FruitPlacer.FruitType.Coconut:
                _c.size = new Vector2(1.38f, 0.7f);
                _c.offset = new Vector2(0.004f, 0.015f);
                _faceDisplacementY = 0.03f;
                break;
                
            case FruitPlacer.FruitType.Pineapple:
                _c.size = new Vector2(1.45f, 1f);
                _c.offset = new Vector2(0, -0.35f);
                transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + 0.01f); //accounting for stem
                _faceDisplacementY = -0.3f;
                break;
                
            case FruitPlacer.FruitType.WaterMelon:
                _c.size = new Vector2(1.92f, 1f);
                _c.offset = new Vector2(0f, 0f);
                break;
        }
        _c.offset = new Vector2(_c.offset.x + Random.Range(-0.001f, 0.001f), _c.offset.y); //give the fruit collider a random x offset to make it feel more natural 
        _face.transform.localPosition = new Vector3(_face.transform.localPosition.x, _faceDisplacementY, _face.transform.localPosition.z);
    }

    //this method sets the face sprite to a given sprite
    public void SetFaceSprite(Sprite sprite) {
        _faceSpriteRenderer.sprite = sprite;
    }

    public void DisableFace() {
        _face.SetActive(false);
    }
}