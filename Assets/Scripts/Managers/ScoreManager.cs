using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class ScoreManager : Singleton<ScoreManager>
{
    private TMP_Text scoreText;
    private MatchablePool _pool;
    private MatchableGrid _grid;
    [SerializeField] private Transform collectionPoint;
    private int _score;

    public int Score
    {
        get
        {
            return _score;
        }
    }

    protected override void Init()
    {
        scoreText = GetComponent<TMP_Text>();

    }

    private void Start()
    {
        _grid = (MatchableGrid)MatchableGrid.Instance;
        _pool = (MatchablePool)MatchablePool.Instance;

    }

    public void AddScore(int amount)
    {
        _score += amount;
        scoreText.text = "Score: " + _score;
    }

    public IEnumerator ResolveMatch(Match toResolve)
    {
        Matchable matchable;
        Matchable powerup = null;

        Transform target = collectionPoint;

        //powerup
        if (toResolve.Count > 3)
        {

            powerup = _pool.UpgradeMatchable(toResolve.ToBeUpgraded, toResolve.GetMatchType);
            toResolve.RemoveMatchable(powerup);

            target = powerup.transform;

            powerup.SortingOrder = 3;
        }


        for (int i = 0; i < toResolve.Count; i++)
        {
            matchable = toResolve.Matchables[i];
            // remove the matchables from the grid
            _grid.RemoveItemAt(matchable.position);

            //move them off to the side of the screen
            if (i == toResolve.Count - 1)
            {
                yield return StartCoroutine(matchable.Resolve(target));
            }
            else
            {
                StartCoroutine(matchable.Resolve(target));
            }



        }

        //update the player's score
        AddScore(toResolve.Count * toResolve.Count);

        if (powerup != null)
        {
            powerup.SortingOrder = 3;
        }



        yield return null;
    }


}

