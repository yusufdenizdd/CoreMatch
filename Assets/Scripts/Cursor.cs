using Unity.Multiplayer.Center.Common;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Cursor : Singleton<Cursor>
{
    private SpriteRenderer _spriteRenderer;

    private Matchable[] _selected;

    protected override void Init()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.enabled = false;
        _selected = new Matchable[2];
    }
    public void SelectFirst(Matchable toSelect)
    {
        _selected[0] = toSelect;

        if (!enabled || _selected[0] == null)
        {
            return;
        }

        transform.position = toSelect.transform.position;

        _spriteRenderer.size = new Vector2(0.3f, 0.3f);

        _spriteRenderer.enabled = true;
    }

    public void SelectSecond(Matchable toSelect)
    {
        _selected[1] = toSelect;
        if (!enabled || _selected[0] == null || _selected[1] == null || !_selected[1].Idle || _selected[0] == _selected[1])
        {
            return;
        }
        if (SelectedAreAdjacent())
        {
            print("Swapping matchables at positions: (" + _selected[0].position.x + ", " + _selected[0].position.y + ") and (" + _selected[1].position.x + ", " + _selected[1].position.y + ")");
        }
        SelectFirst(null);
    }

    private bool SelectedAreAdjacent()
    {
        if (_selected[0].position.x == _selected[1].position.x)
        {
            if (_selected[0].position.y == _selected[1].position.y + 1)
            {
                _spriteRenderer.size = new Vector2(1 * 0.3f, 2 * 0.3f);
                transform.position += Vector3.down / 2;
                return true;

            }
            else if (_selected[0].position.y == _selected[1].position.y - 1)
            {

                _spriteRenderer.size = new Vector2(1 * 0.3f, 2 * 0.3f);
                transform.position += Vector3.up / 2;
                return true;

            }

        }
        else if (_selected[0].position.y == _selected[1].position.y)
        {
            if (_selected[0].position.x == _selected[1].position.x + 1)
            {


                _spriteRenderer.size = new Vector2(2 * 0.3f, 1 * 0.3f);

                transform.position += Vector3.left / 2;
                return true;

            }
            else if (_selected[0].position.x == _selected[1].position.x - 1)
            {

                _spriteRenderer.size = new Vector2(2 * 0.3f, 1 * 0.3f);

                transform.position += Vector3.right / 2;
                return true;

            }

        }

        return false;

    }

}
