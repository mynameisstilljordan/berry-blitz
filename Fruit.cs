using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fruit{
    private float _scale;
    private int _sprite;
    private FruitPlacer.FruitType _fruitType;

    public Fruit(FruitPlacer.FruitType fruitType, float scale, int sprite) {
        _scale = scale;
        _sprite = sprite;
        _fruitType = fruitType;
    }
    
    //this method returns the scale of the fruit
    public float GetScale() { return _scale; }

    //this method returns the sprite # of the fruit
    public int GetSprite() { return _sprite; }

    //this method returns the fruit type of the fruit
    public FruitPlacer.FruitType GetFruitType() { return _fruitType; }
}
