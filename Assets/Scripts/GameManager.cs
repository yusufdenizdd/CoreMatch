using System.Collections;
using TMPro;

//using System.Numerics;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    private MatchablePool _pool;
    private MatchableGrid _grid;
    [SerializeField] private Vector2Int dimensions;
    [SerializeField] private TMP_Text gridOutput;
    private void Start()
    {
        _pool = (MatchablePool)MatchablePool.Instance;
        _grid = (MatchableGrid)MatchableGrid.Instance;
        _pool.PoolObjects(10);

        // create the grid
        _grid.InitializeGrid(dimensions);

        StartCoroutine(Demo());
    }
    private IEnumerator Demo()
    {
        /*
       Matchable m = _pool.GetPooledObject();
       m.gameObject.SetActive(true);

       Vector3 randomPosition;

       for (int i = 0; i < 7; i++)
       {
           randomPosition = new Vector3(Random.Range(-6f, 6f), Random.Range(-4f, 4f));
           yield return StartCoroutine(m.MoveToPosition(randomPosition));
       }
       */


        //display the grid
        gridOutput.text = _grid.ToString();
        Debug.Log("1");
        yield return new WaitForSeconds(2);

        // take matchables from the pool
        Matchable m1 = _pool.GetPooledObject();
        m1.gameObject.SetActive(true);
        m1.gameObject.name = "a";

        Matchable m2 = _pool.GetPooledObject();
        m2.gameObject.SetActive(true);
        m2.gameObject.name = "b";

        // put them on the grid
        _grid.PutItemAt(m1, 0, 1);
        _grid.PutItemAt(m2, 2, 3);

        //display the grid
        gridOutput.text = _grid.ToString();
        Debug.Log("2");
        yield return new WaitForSeconds(2);


        // swap the matchables
        _grid.SwapItemsAt(0, 1, 2, 3);
        gridOutput.text = _grid.ToString();
        Debug.Log("3");
        yield return new WaitForSeconds(2);

        // remove the matchables from the grid
        _grid.RemoveItemAt(0, 1);
        _grid.RemoveItemAt(2, 3);
        gridOutput.text = _grid.ToString();
        Debug.Log("4");
        yield return new WaitForSeconds(2);

        // return the matchables to the pool
        _pool.ReturnObjectToPool(m1);
        _pool.ReturnObjectToPool(m2);
        Debug.Log("5");


        yield return null;
    }

}
