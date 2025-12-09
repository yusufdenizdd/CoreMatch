using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// This class will allow us to instantiate as many objects as we will need
// while the scene is being set up so that we dont slow down game by
// calling Instantiate during gameplay.
// 
// It is prefered, but optional, to call PoolObjects first with a specified amount

public abstract class ObjectPool<T> : Singleton<ObjectPool<T>> where T : MonoBehaviour
{
    [SerializeField] protected T prefab;

    private List<T> _pooledObjects;
    private int _amount;
    private bool _isReady;

    // create the pool, with a specified amount of objects
    public void PoolObjects(int amount = 0)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException("Amount to pool must be non-negative.");
        }
        _amount = amount;

        // initialize the list
        _pooledObjects = new List<T>(_amount);

        // instantiate a bunch of T's
        GameObject newObject;

        for (int i = 0; i < amount; i++)
        {
            newObject = Instantiate(prefab.gameObject, transform);
            newObject.SetActive(false);
            // add each T to the list
            _pooledObjects.Add(newObject.GetComponent<T>());
        }
        // flag the pool as ready
        _isReady = true;
    }

    // get an object from the pool
    public T GetPooledObject()
    {
        // check if pool is ready, if not make it ready
        if (!_isReady)
        {
            PoolObjects(1);
        }

        //search through list for something not in use and return it
        for (int i = 0; i < _amount; i++)
        {
            if (!_pooledObjects[i].isActiveAndEnabled)
            {
                return _pooledObjects[i];
            }
        }


        // if we didnt find anything, make a new one
        GameObject newObject = Instantiate(prefab.gameObject, transform);
        newObject.SetActive(false);
        _pooledObjects.Add(newObject.GetComponent<T>());
        _amount++;
        return newObject.GetComponent<T>();
    }

    // return an object back to pool
    public void ReturnObjectToPool(T toBeReturned)
    {
        //verify the argument
        if (toBeReturned == null)
        {
            return;
        }

        //make sure that the pool is ready, if not, make it ready
        if (!_isReady)
        {
            PoolObjects();
            _pooledObjects.Add(toBeReturned);
        }

        //deactivate the game object
        toBeReturned.gameObject.SetActive(false);
    }
}
