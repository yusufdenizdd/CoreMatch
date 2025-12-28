using System.Collections;
using TMPro;

//using System.Numerics;
using UnityEngine;

// This class will set up the scene and initialize objects
// This class inherits from Singleton so any other script can access it easily through GameMaganer.Instance
public class GameManager : Singleton<GameManager>
{
    private MatchablePool _pool;
    private MatchableGrid _grid;

    // the dimensions of the matchable grid, set in the inspector
    [SerializeField] private Vector2Int dimensions = Vector2Int.one;

    // a UI Text object for displaying the contents of the grid data
    // for testing and debugging purposes only
    [SerializeField] private TMP_Text gridOutput;
    private void Start()
    {
        //music
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayGameplayMusic();

        // get references to other important game objects
        _pool = (MatchablePool)MatchablePool.Instance;
        _grid = (MatchableGrid)MatchableGrid.Instance;


        // set up the scene
        StartCoroutine(Setup());
    }
    private IEnumerator Setup()
    {
        //It's a good idea to put a loading screen here

        //pool the matchables
        _pool.PoolObjects(dimensions.x * dimensions.y * 2);

        //create the grid
        _grid.InitializeGrid(dimensions);

        yield return null;

        StartCoroutine(_grid.PopulateGrid(false, true));

        //then remove the loading screen down here
    }

}
