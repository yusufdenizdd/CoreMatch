using System.Collections;
using UnityEngine;
[RequireComponent(typeof(SpriteRenderer))]
public class Matchable : Movable
{
    private MatchablePool _pool;
    private Cursor _cursor;
    private int _type;
    public int Type
    {
        get
        {
            return _type;
        }
    }

    private SpriteRenderer _spriteRenderer;

    // where is this matchable in the grid?
    public Vector2Int position;

    private void Awake()
    {
        _cursor = Cursor.Instance;
        _pool = (MatchablePool)MatchablePool.Instance;
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetType(int type, Sprite sprite, Color color)
    {
        _type = type;
        _spriteRenderer.sprite = sprite;
        _spriteRenderer.color = color;
    }

    public IEnumerator Resolve(Transform collectionPoint)
    {
        //draw avove orthers in the grid
        _spriteRenderer.sortingOrder = 2;
        //move off the grid to a collection point
        yield return StartCoroutine(MoveToPosition(collectionPoint.position/* collection point*/));
        //reset
        _spriteRenderer.sortingOrder = 1;

        //return back to the pool
        _pool.ReturnObjectToPool(this);

    }

    private void OnMouseDown()
    {
        if (!Idle)
        {
            return;
        }
        else
        {
            _cursor.SelectFirst(this);
            print("onmousedown");

        }
    }

    private void OnMouseUp()
    {
        if (!Idle)
        {
            return;
        }
        else
        {
            _cursor.SelectFirst(null);
            print("onmouseup");

        }

    }

    private void OnMouseEnter()
    {
        if (!Idle)
        {
            return;
        }
        else
        {
            _cursor.SelectSecond(this);
            print("onmouseenter");

        }

    }
    public override string ToString()
    {
        return gameObject.name;
    }

}
