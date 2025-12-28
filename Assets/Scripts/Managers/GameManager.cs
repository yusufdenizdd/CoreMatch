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
    private Cursor _cursor;
    private AudioMixer _audioMixer;

    [SerializeField] private Fader loadingScreen;

    // the dimensions of the matchable grid, set in the inspector
    [SerializeField] private Vector2Int dimensions = Vector2Int.one;

    // a UI Text object for displaying the contents of the grid data
    // for testing and debugging purposes only
    [SerializeField] private TMP_Text gridOutput;
    private void Start()
    {
        // get references to other important game objects
        _pool = (MatchablePool)MatchablePool.Instance;
        _grid = (MatchableGrid)MatchableGrid.Instance;
        _cursor = Cursor.Instance;
        _audioMixer = AudioMixer.Instance;


        // set up the scene
        StartCoroutine(Setup());
    }
    private IEnumerator Setup()
    {
        //disable user input
        _cursor.enabled = false;

        //loading screen
        loadingScreen.Hide(false);

        //pool the matchables
        _pool.PoolObjects(dimensions.x * dimensions.y * 2);

        //create the grid
        _grid.InitializeGrid(dimensions);

        //loading screen'i kaldır
        //loadingScreen.Hide(true);
        StartCoroutine(loadingScreen.Fade(0));

        //background music başlat
        _audioMixer.PlayMusic();


        // Eğer LevelManager varsa, oyun akışını o yönetsin (StartLevel çağıracak)
        if (LevelManager.Instance != null)
        {
            // LevelManager grid'i kendisi populate edip başlatacak
            LevelManager.Instance.BeginGame();
            yield break;
        }

        yield return StartCoroutine(_grid.PopulateGrid(false, true));


        //grid ilk başlatıldığında mümkün hamle var mı yok mu kontrol et nolur nolmaz
        _grid.CheckPossibleMoves();

        //user input'u enable yap
        _cursor.enabled = true;
    }

    public void NoMoreMoves()
    {
        StartCoroutine(_grid.MatchEverything());

    }

}
