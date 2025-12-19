using System.Collections;
using System.Runtime.CompilerServices;

//using System.Numerics;
using UnityEngine;

// This script will allow any game object to be moved smoothly
// from its current position to a new target position at a speed
// set in the inspector using a coroutine.
// 
// The speed must be a positive number
// 
// You can see if the objects is currently moving using Idle
// 
// There is an Easing function to alter the speed of the animation over time
// 


public class Movable : MonoBehaviour
{
    private Vector3 from;
    private Vector3 to;
    private float howfar;
    private bool _idle = true;
    public bool Idle
    {
        get
        {
            return _idle;
        }
    }
    [SerializeField] private float speed = 1;

    // coroutine move from current position to new position
    public IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        if (speed <= 0)
        {
            Debug.LogWarning("Speed must be a positive number.");
        }
        from = transform.position;
        to = targetPosition;
        howfar = 0;
        _idle = false;



        do
        {
            howfar += speed * Time.deltaTime;
            if (howfar > 1)
            {
                howfar = 1;
            }
            transform.position = Vector3.LerpUnclamped(from, to, Easing(howfar));
            yield return null;
        } while (howfar != 1);
        _idle = true;



    }
    private float Easing(float t)
    {
        return t * t;
    }
}
