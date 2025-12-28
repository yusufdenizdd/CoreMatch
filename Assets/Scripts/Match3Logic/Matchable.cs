using System.Collections;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UIElements;
[RequireComponent(typeof(SpriteRenderer))]
public class Matchable : Movable
{
    private MatchablePool _pool;
    private MatchableGrid _grid;
    private Cursor _cursor;
    private int _type;

    private MatchType _powerup = MatchType.invalid;
    public bool IsGem
    {
        get
        {
            return _powerup == MatchType.match5;
        }
    }
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
        _grid = (MatchableGrid)MatchableGrid.Instance;
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

        // if matchable is a powerup...
        if (_powerup != MatchType.invalid)
        {
            //resolve a match4 powerup
            if (_powerup == MatchType.match4)
            {
                _grid.MatchAllAdjacent(this);

            }

            //resolve a match5 powerup
            if (_powerup == MatchType.match5)
            {

            }


            //resolve a cross powerup
            if (_powerup == MatchType.cross)
            {
                _grid.MatchRowAndColumn(this);

            }


            _powerup = MatchType.invalid;
        }
        if (collectionPoint == null)
        {
            yield break;
        }


        //draw avove orthers in the grid
        _spriteRenderer.sortingOrder = 2;
        //move off the grid to a collection point
        yield return StartCoroutine(MoveToTransform(collectionPoint/* collection point*/));
        //reset
        _spriteRenderer.sortingOrder = 1;

        //return back to the pool
        _pool.ReturnObjectToPool(this);

    }

    //matchable'ın sprite'ını powerup sprite'ı yap
    public Matchable Upgrade(MatchType powerupType, Sprite powerupSprite)
    {
        // eğer zaten powerup ise resolve edelim, tekrar upgrade etmeye gerek yok
        if (_powerup != MatchType.invalid)
        {
            _idle = false;
            StartCoroutine(Resolve(null));
            _idle = true;
        }
        if (powerupType == MatchType.match5)
        {
            _type = -1;
            _spriteRenderer.color = Color.white;
        }
        _powerup = powerupType;
        _spriteRenderer.sprite = powerupSprite;
        return this;
    }

    public int SortingOrder
    {
        set
        {
            _spriteRenderer.sortingOrder = value;
        }
    }

    private void OnMouseDown()
    {
        // 1. Level oynanmıyorsa (Panel açıksa vs.) tıklamayı engelle
        if (LevelManager.Instance != null && !LevelManager.Instance.IsPlaying) return;

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

    // TEST FONKSİYONU: Mouse taşın üzerindeyken çalışır
    private void OnMouseOver()
    {
        // Sağ Tık: Tür/Renk Değiştir
        if (Input.GetMouseButtonDown(1))
        {
            if (_pool != null)
            {
                _pool.NextType(this);
            }
        }

        // P Tuşu: Powerup Döngüsü (Match4 <-> Match5)
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (_powerup == MatchType.match4)
            {
                // Roket ise -> Renk Bombası yap
                _pool.UpgradeMatchable(this, MatchType.match5);
            }
            else if (_powerup == MatchType.match5)
            {
                // Renk Bombası ise -> Roket yap

                // DİKKAT: Bomba renksiz olduğu için, rokete çevirmeden önce
                // ona rastgele bir renk/tür veriyoruz ki "beyaz roket" olmasın.
                _pool.RandomizeType(this);

                _pool.UpgradeMatchable(this, MatchType.match4);
            }
            else
            {
                // Hiçbiri değilse (Normal taşsa) -> Roket yaparak başlat
                _pool.UpgradeMatchable(this, MatchType.match4);
            }
        }
    }

}
