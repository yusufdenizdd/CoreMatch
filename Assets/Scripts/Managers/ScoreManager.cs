using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class ScoreManager : Singleton<ScoreManager>
{
    private TMP_Text scoreText;
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

    }

    public void AddScore(int amount)
    {
        _score += amount;
        scoreText.text = "Score: " + _score;
    }

    public IEnumerator ResolveMatch(Match toResolve)
    {
        Matchable matchable;
        for (int i = 0; i < toResolve.Count; i++)
        {
            matchable = toResolve.Matchables[i];
            // remove the matchables from the grid
            _grid.RemoveItemAt(matchable.position);

            //move them off to the side of the screen
            if (i == toResolve.Count - 1)
            {
                yield return StartCoroutine(matchable.Resolve(collectionPoint));
            }
            else
            {
                StartCoroutine(matchable.Resolve(collectionPoint));
            }



        }

        //update the player's score
        AddScore(toResolve.Count * toResolve.Count);



        yield return null;
    }


}

