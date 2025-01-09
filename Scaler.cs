using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scaler : MonoBehaviour{
    private float _screenWidth;
    [SerializeField] private GameObject _fruitPlacer;

    void Awake() {
        _screenWidth = GetScreenToWorldWidth(); //set screen width
        transform.localScale = new Vector3(_screenWidth  * 0.9f, _screenWidth * 0.9f, 1); //scale the board to screen width
        _fruitPlacer.transform.localScale = new Vector3(_screenWidth * 0.0075f, _screenWidth * 0.9f); //scale the fruit placer 
        _fruitPlacer.transform.SetParent(this.gameObject.transform); //adopt fruit placer
        _fruitPlacer.transform.localPosition = new Vector3(0, 0, -0.1f); //move fruit placer to foreground
    }

    float GetScreenToWorldHeight() {
        Vector2 topRightCorner = new Vector2(1, 1);
        Vector2 edgeVector = Camera.main.ViewportToWorldPoint(topRightCorner);
        var height = edgeVector.y * 2;
        return height;
    }

    public float GetScreenToWorldWidth() {
        Vector2 topRightCorner = new Vector2(1, 1);
        Vector2 edgeVector = Camera.main.ViewportToWorldPoint(topRightCorner);
        var width = edgeVector.x * 2;
        return width;
    }
}
