using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour {
    /// <summary>
    /// HOW TO IMPLEMENT:
    /// 
    /// When requesting to "spawn" a pooled gameobject:
    /// Gameobject "name" = ObjectPool.Instance.GetObject(parameter);
    /// 
    /// if ("name" != null){
    ///     "name".transform.position = XYZ;
    ///     "name".SetActive(true);
    /// }
    /// 
    /// HOW TO SEND BACK TO POOL:
    /// 
    /// Instead of destroying the gameobject, simply disable it: 
    /// "name".SetActive(false);
    /// </summary>

    public static ObjectPool Instance; //the instance (for singleton pattern)

    int _amountToPool = 50; //the number of each object to pool

    private List<GameObject> _fruit = new List<GameObject>(); //list for the fruits

    [SerializeField] private GameObject _fruitPrefab; //prefab of fruit

    private void Awake() {
        if (Instance == null) Instance = this; //set instance to this (if null)
    }

    // Start is called before the first frame update
    void Start() {
        var board = GameObject.FindGameObjectWithTag("Board");
        //ground
        for (int i = 0; i < _amountToPool; i++) { //for the number of objects to pool
            GameObject obj = Instantiate(_fruitPrefab); //instantiate the object
            obj.SetActive(false); //disable the object
            //obj.transform.SetParent(board.transform); //set the board as its parent
            _fruit.Add(obj); //add it to the queue
        }
    }

    //this method returns the requested pooled object
    public GameObject GetFruit() {
        for (int i = 0; i < _fruit.Count; i++) { //for all the gameobjects in the pool
            if (!_fruit[i].activeInHierarchy) return _fruit[i]; //if an inactive gameobject is found, return it
        }
        return Instantiate(_fruitPrefab, transform.position, Quaternion.identity) ; //if there is no inactive gameobject of this type, return a new one
    }
    
    //update parents
    private void UpdateParents() {
        
    }
}