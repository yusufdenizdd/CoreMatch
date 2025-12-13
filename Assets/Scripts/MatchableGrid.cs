using System.Collections;
using UnityEngine;

public class MatchableGrid : GridSystem<Matchable>
{
    [SerializeField] private Vector3 offscreenOffset;
    private MatchablePool _pool;

    private void Start()
    {
        _pool = (MatchablePool)MatchablePool.Instance;
    }
    public IEnumerator PopulateGrid(bool allowMatches = false)
    {
        Matchable newMatchable;
        Vector3 onscreenPosition;

        for (int y = 0; y < Dimensions.y; y++)
        {
            for (int x = 0; x < Dimensions.x; x++)
            {
                // get a matchable from the pool
                newMatchable = _pool.GetRandomMatchable();

                //position the matchable on screen
                onscreenPosition = transform.position + new Vector3(x, y);
                newMatchable.transform.position = onscreenPosition + offscreenOffset;

                //activate the matchable
                newMatchable.gameObject.SetActive(true);

                newMatchable.position = new Vector2Int(x, y);

                //place the matchable in the grid
                PutItemAt(newMatchable, x, y);

                int type = newMatchable.Type;

                while (!allowMatches && IsPartOfAMatch(newMatchable))
                {
                    //change the matchable's type until it isn't a match anymore
                    if (_pool.NextType(newMatchable) == type)
                    {
                        Debug.LogWarning("Failed to find a matchable type that didn't match at (" + x + ", " + y + ")");
                        Debug.Break();
                        break;
                    }
                }

                //move the matchable to its on screen position
                StartCoroutine(newMatchable.MoveToPosition(onscreenPosition));

                //yield return new WaitForSeconds(0.1f);
            }
            yield return null;
        }
    }

    // TODO: Write this function!
    private bool IsPartOfAMatch(Matchable matchable)
    {
        return false;
    }
}
