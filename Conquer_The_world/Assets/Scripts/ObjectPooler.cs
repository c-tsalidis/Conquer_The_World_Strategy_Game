using System;
using UnityEngine;
using System.Collections.Generic;

public class ObjectPooler : MonoBehaviour {
    
    public List<GameObject> pooledObjects;
    public GameObject objectToPool;
    public int amountToPool;
    

    private void Start() {
        pooledObjects = new List<GameObject>();
        for (int i = 0; i < amountToPool; i++) {
            GameObject obj = Instantiate(objectToPool, transform);
            obj.SetActive(false); 
            pooledObjects.Add(obj);
        }
    }
    
    public GameObject GetPooledObject() {
        for (int i = 0; i < pooledObjects.Count; i++) {
            if (!pooledObjects[i].activeInHierarchy) {
                return pooledObjects[i];
            }
        }
        return null;
    }

}