using System.Collections;
using System.Collections.Generic;
//using System.Numerics;
using Unity.VisualScripting;
//using System.Numerics;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class MatchableGrid : GridSystem<Matchable>
{
    [SerializeField] private Vector3 offscreenOffset;
    private List<Matchable> possibleMoves;
    private MatchablePool _pool;
    private ScoreManager _score;

    private Hint _hint;
    private AudioMixer _audioMixer;

    private void Start()
    {
        _pool = (MatchablePool)MatchablePool.Instance;
        _score = ScoreManager.Instance;
        _hint = Hint.Instance;
        _audioMixer = AudioMixer.Instance;
    }
    public IEnumerator PopulateGrid(bool allowMatches = false, bool initialPopulation = false)
    {
        List<Matchable> newMatchables = new List<Matchable>();
        Matchable newMatchable;
        Vector3 onscreenPosition;

        for (int y = 0; y < Dimensions.y; y++)
        {
            for (int x = 0; x < Dimensions.x; x++)
                if (IsEmpty(x, y))
                {
                    // get a matchable from the pool
                    newMatchable = _pool.GetRandomMatchable();

                    //position the matchable on screen

                    newMatchable.transform.position = transform.position + new Vector3(x, y) + offscreenOffset;

                    //activate the matchable
                    newMatchable.gameObject.SetActive(true);

                    newMatchable.position = new Vector2Int(x, y);

                    //place the matchable in the grid
                    PutItemAt(newMatchable, x, y);

                    //add the new matchable to the list
                    newMatchables.Add(newMatchable);

                    int initialType = newMatchable.Type;

                    while (!allowMatches && IsPartOfAMatch(newMatchable))
                    {

                        //change the matchable's type until it isn't a match anymore
                        if (_pool.NextType(newMatchable) == initialType)
                        {
                            Debug.LogWarning("Failed to find a matchable type that didn't match at (" + x + ", " + y + ")");
                            Debug.Break();
                            yield return null;
                            break;
                        }
                    }


                }


        }
        for (int i = 0; i < newMatchables.Count; i++)
        {
            onscreenPosition = transform.position + new Vector3(newMatchables[i].position.x, newMatchables[i].position.y);

            _audioMixer.PlayDelayedSound(SoundEffects.land, 1f / newMatchables[i].Speed);

            if (i == newMatchables.Count - 1)
            {
                yield return StartCoroutine(newMatchables[i].MoveToPosition(onscreenPosition));

            }
            else
            {
                StartCoroutine(newMatchables[i].MoveToPosition(onscreenPosition));

            }

            if (initialPopulation)
            {
                yield return new WaitForSeconds(0.1f);

            }

        }
    }

    // Check if the matchable being populated is part of a match or not
    private bool IsPartOfAMatch(Matchable toMatch)
    {
        int horizontalMatches = 0;
        int verticalMatches = 0;

        //first look to the left
        horizontalMatches += CountMatchesInDirection(toMatch, Vector2Int.left);

        //then look to the right
        horizontalMatches += CountMatchesInDirection(toMatch, Vector2Int.right);

        if (horizontalMatches > 1)
        {
            return true;
        }

        //look up
        verticalMatches += CountMatchesInDirection(toMatch, Vector2Int.up);

        //look down
        verticalMatches += CountMatchesInDirection(toMatch, Vector2Int.down);

        if (verticalMatches > 1)
        {
            return true;
        }


        return false;
    }

    //Count the number of matches on the grid starting from the matchable to match moving in the direction indicated

    private int CountMatchesInDirection(Matchable toMatch, Vector2Int direction)
    {
        int matches = 0;
        Vector2Int position = toMatch.position + direction;

        while (CheckBounds(position) && !IsEmpty(position) && GetItemAt(position).Type == toMatch.Type)
        {
            matches++;
            position += direction;
        }
        return matches;
    }
    // zaten swap olan iki taşı o swap işlemi bitmeden bidaha swap edemememiz lazım
    public IEnumerator TrySwap(Matchable[] toBeSwapped)
    {
        Matchable[] copies = new Matchable[2];
        copies[0] = toBeSwapped[0];
        copies[1] = toBeSwapped[1];

        //ipucunu gizle
        _hint.CancelHint();

        // yield until matchables animate swapping
        yield return StartCoroutine(Swap(copies));

        //match5 için özel kurallar (match5'i eşleştirdiğimz taşın rengindeki her şey yok olsun)
        if (copies[0].IsGem && copies[1].IsGem)
        {
            //ikisi de match5 ise hayat biticek
            yield return MatchEverything();
            yield break;

        }
        if (copies[0].IsGem)
        {
            yield return MatchEverythingByType(copies[0], copies[1].Type);
            yield break;

        }
        else if (copies[1].IsGem)
        {
            yield return MatchEverythingByType(copies[1], copies[0].Type);
            yield break;

        }

        //check for a valid match
        Match[] matches = new Match[2];

        matches[0] = GetMatch(copies[0]);
        matches[1] = GetMatch(copies[1]);


        if (matches[0] != null)
        {
            //resolve match
            StartCoroutine(_score.ResolveMatch(matches[0]));

        }
        if (matches[1] != null)
        {
            //resolve match
            StartCoroutine(_score.ResolveMatch(matches[1]));

        }
        //if there's no match, swap them back
        if (matches[0] == null && matches[1] == null)
        {
            yield return StartCoroutine(Swap(copies));

            if (ScanForMatches())
            {
                StartCoroutine(FillAndScanGrid());
            }
            else
            {
                CheckPossibleMoves();
            }
        }
        else
        {
            StartCoroutine(FillAndScanGrid());

        }


    }

    private IEnumerator FillAndScanGrid()
    {
        CollapseGrid();
        yield return StartCoroutine(PopulateGrid(true));

        // scan grid for chain reactions
        if (ScanForMatches())
        {
            //scan again (collapsegrid, populategrid, scanformatches tekrar çalışcak)
            StartCoroutine(FillAndScanGrid());
        }
        else //chain reaction yoksa ve grid idle ise, mümkün hamle var mı kontrol et 
        {
            CheckPossibleMoves();
        }

    }

    public void CheckPossibleMoves()
    {
        if (ScanForMoves() == 0)
        {
            //hamle kalmadı
            GameManager.Instance.NoMoreMoves();

        }
        else
        {
            //hamle var, öneri yap 
            _hint.EnableHintButton();

            _hint.StartAutoHint(possibleMoves[Random.Range(0, possibleMoves.Count)].transform);

        }
    }

    // gridin hepsine bakıp non-empty ve idle matchables arıyor ve patlatıyor
    private bool ScanForMatches()
    {
        bool madeAMatch = false;
        Matchable toMatch;
        Match match;
        for (int y = 0; y < Dimensions.y; y++)
        {
            for (int x = 0; x < Dimensions.x; x++)
            {
                if (!IsEmpty(x, y))
                {
                    toMatch = GetItemAt(x, y);
                    if (!toMatch.Idle)
                    {
                        continue;
                    }

                    match = GetMatch(toMatch);
                    if (match != null)
                    {
                        madeAMatch = true;
                        StartCoroutine(_score.ResolveMatch(match));
                    }

                }
            }
        }
        return madeAMatch;
    }

    //komşuları patlat (match4)
    public void MatchAllAdjacent(Matchable powerup)
    {
        Match allAdjacent = new Match();

        for (int y = powerup.position.y - 1; y < powerup.position.y + 2; y++)
        {
            for (int x = powerup.position.x - 1; x < powerup.position.x + 2; x++)
            {
                if (CheckBounds(x, y) && !IsEmpty(x, y) && GetItemAt(x, y).Idle)
                {
                    allAdjacent.AddMatchable(GetItemAt(x, y));
                }

            }
        }
        StartCoroutine(_score.ResolveMatch(allAdjacent, MatchType.match4));

        _audioMixer.PlaySound(SoundEffects.powerup);
    }

    //+ şeklinde yatay ve dikeydeki her şeyi patlat (cross)
    public void MatchRowAndColumn(Matchable powerup)
    {
        Match rowAndColumn = new Match();
        for (int y = 0; y < Dimensions.y; y++)
        {
            if (CheckBounds(powerup.position.x, y) && !IsEmpty(powerup.position.x, y) && GetItemAt(powerup.position.x, y).Idle)
            {
                rowAndColumn.AddMatchable(GetItemAt(powerup.position.x, y));
            }

        }

        for (int x = 0; x < Dimensions.x; x++)
        {
            if (CheckBounds(x, powerup.position.y) && !IsEmpty(x, powerup.position.y) && GetItemAt(x, powerup.position.y).Idle)
            {
                rowAndColumn.AddMatchable(GetItemAt(x, powerup.position.y));
            }

        }


        StartCoroutine(_score.ResolveMatch(rowAndColumn, MatchType.cross));

        _audioMixer.PlaySound(SoundEffects.powerup);
    }
    private void CollapseGrid()
    {
        // soldan sağa her sütunu aşağıdan yukarıya tarıyor, boş yer bulunca yukarıya doğru boş olmayan yer bulana kadar taramaya devam ediyor
        // sonra yukarıdaki matchable'ı aşağıdaki boş yere taşıyor
        for (int x = 0; x < Dimensions.x; x++)
        {
            for (int yEmpty = 0; yEmpty < Dimensions.y - 1; yEmpty++)
            {
                if (IsEmpty(x, yEmpty))
                {
                    for (int yNotEmpty = yEmpty + 1; yNotEmpty < Dimensions.y; yNotEmpty++)
                    {
                        if (!IsEmpty(x, yNotEmpty) && GetItemAt(x, yNotEmpty).Idle)
                        {
                            // move the matchable from notempty to empty
                            MoveMatchableToPosition(GetItemAt(x, yNotEmpty), x, yEmpty);
                            break;
                        }
                    }
                }
            }
        }
    }

    private void MoveMatchableToPosition(Matchable toMove, int x, int y)
    {
        // remove the matchable from its original grid position
        // RemoveItemAt(toMove.position);

        // place it the matchable at its new position
        // PutItemAt(toMove, x, y);

        // üstteki ikisinin yerine yazdım
        // move the matchable to its new position in the grid
        MoveItemTo(toMove.position, new Vector2Int(x, y));

        // update the matchable's internal grid position
        toMove.position = new Vector2Int(x, y);

        // start the animation
        StartCoroutine(toMove.MoveToPosition(transform.position + new Vector3(x, y)));

        _audioMixer.PlayDelayedSound(SoundEffects.land, 1f / toMove.Speed);


    }


    private Match GetMatch(Matchable toMatch)
    {
        Match match = new Match(toMatch);
        Match horizontalMatch;
        Match verticalMatch;

        horizontalMatch = GetMatchesInDirection(match, toMatch, Vector2Int.left);
        horizontalMatch.Merge(GetMatchesInDirection(match, toMatch, Vector2Int.right));

        horizontalMatch.orientation = Orientation.horizontal;

        if (horizontalMatch.Count > 1)
        {
            match.Merge(horizontalMatch);
            //then scan for vertical branches
            GetBranches(match, horizontalMatch, Orientation.vertical);

        }
        verticalMatch = GetMatchesInDirection(match, toMatch, Vector2Int.up);
        verticalMatch.Merge(GetMatchesInDirection(match, toMatch, Vector2Int.down));

        verticalMatch.orientation = Orientation.vertical;

        if (verticalMatch.Count > 1)
        {
            match.Merge(verticalMatch);
            //then scan for horizontal branches
            GetBranches(match, verticalMatch, Orientation.horizontal);

        }

        if (match.Count == 1)
        {
            return null;
        }

        return match;
    }

    // add each matching matchable in the direction to a match and return it
    private Match GetMatchesInDirection(Match tree, Matchable toMatch, Vector2Int direction)
    {
        Match match = new Match();
        Vector2Int position = toMatch.position + direction;
        Matchable next;

        while (CheckBounds(position) && !IsEmpty(position))
        {
            next = GetItemAt(position);
            if (next.Type == toMatch.Type && next.Idle)
            {
                if (!tree.Contains(next))
                {
                    match.AddMatchable(next);
                }
                else
                {
                    match.AddUnlisted();
                }
                position += direction;
            }
            else { break; }

        }
        return match;
    }

    private void GetBranches(Match tree, Match branchToSearch, Orientation prependicular)
    {
        Match branch;

        foreach (Matchable matchable in branchToSearch.Matchables)
        {
            branch = GetMatchesInDirection(tree, matchable, prependicular == Orientation.horizontal ? Vector2Int.left : Vector2Int.down);
            branch.Merge(GetMatchesInDirection(tree, matchable, prependicular == Orientation.horizontal ? Vector2Int.right : Vector2Int.up));

            branch.orientation = prependicular;

            if (branch.Count > 1)
            {
                tree.Merge(branch);
                GetBranches(tree, branch, prependicular == Orientation.horizontal ? Orientation.vertical : Orientation.horizontal);
            }

        }
    }
    private IEnumerator Swap(Matchable[] toBeSwapped)
    {
        //swap them in the grid data structure
        SwapItemsAt(toBeSwapped[0].position, toBeSwapped[1].position);

        // tell the matchables their new positions
        Vector2Int temp = toBeSwapped[0].position;
        toBeSwapped[0].position = toBeSwapped[1].position;
        toBeSwapped[1].position = temp;


        // get the world positions of both
        Vector3[] worldPosition = new Vector3[2];
        worldPosition[0] = toBeSwapped[0].transform.position;
        worldPosition[1] = toBeSwapped[1].transform.position;

        _audioMixer.PlaySound(SoundEffects.swap);


        //move them to their new positions on screen
        StartCoroutine(toBeSwapped[0].MoveToPosition(worldPosition[1]));
        yield return StartCoroutine(toBeSwapped[1].MoveToPosition(worldPosition[0]));
    }

    //hayatı bitir
    public IEnumerator MatchEverything()
    {
        Match everything = new Match();
        for (int y = 0; y < Dimensions.y; y++)
        {
            for (int x = 0; x < Dimensions.x; x++)
            {
                if (CheckBounds(x, y) && !IsEmpty(x, y) && GetItemAt(x, y).Idle)
                {
                    everything.AddMatchable(GetItemAt(x, y));
                }

            }

        }

        yield return StartCoroutine(_score.ResolveMatch(everything, MatchType.match5));
        StartCoroutine(FillAndScanGrid());

        _audioMixer.PlaySound(SoundEffects.powerup);

    }

    //tek renkteki her şeyi bitir
    public IEnumerator MatchEverythingByType(Matchable gem, int type)
    {
        Match everythingByType = new Match(gem);
        for (int y = 0; y < Dimensions.y; y++)
        {
            for (int x = 0; x < Dimensions.x; x++)
            {
                if (CheckBounds(x, y) && !IsEmpty(x, y) && GetItemAt(x, y).Idle && GetItemAt(x, y).Type == type)
                {
                    everythingByType.AddMatchable(GetItemAt(x, y));
                }

            }

        }

        yield return StartCoroutine(_score.ResolveMatch(everythingByType, MatchType.match5));
        StartCoroutine(FillAndScanGrid());

        _audioMixer.PlaySound(SoundEffects.powerup);

    }

    //hamle yapacak kombinasyon kaldı mı diye tara
    private int ScanForMoves()
    {
        possibleMoves = new List<Matchable>();

        //tüm gridi tara
        for (int y = 0; y < Dimensions.y; y++)
        {
            for (int x = 0; x < Dimensions.x; x++)
            {
                if (CheckBounds(x, y) && !IsEmpty(x, y) && CanMove(GetItemAt(x, y)))
                {
                    possibleMoves.Add(GetItemAt(x, y));

                }

            }

        }

        //hamle yapacak matchable'lar varsa listeye ekle

        return possibleMoves.Count;
    }
    private bool CanMove(Matchable toCheck)
    {
        //her 4 yönde de 4 farklı durumu kontrol ediyor yani toplamda 16 durumu
        if (CanMove(toCheck, Vector2Int.up) || CanMove(toCheck, Vector2Int.right) || CanMove(toCheck, Vector2Int.down) || CanMove(toCheck, Vector2Int.left))
        {
            return true;
        }

        //17. durum kaldı, gem powerup olduysa o yani. gem ise her türlü eşleşir. yani true
        if (toCheck.IsGem)
        {
            return true;
        }
        return false;
    }

    private bool CanMove(Matchable toCheck, Vector2Int direction)
    {

        // taşın 2 ve 3 adım önündeki taşlara bak dümdüz yani (sağ/sol/yukarı/aşağı 2. ve 3. adım önündeki taşlara)
        Vector2Int position1 = toCheck.position + direction * 2;
        Vector2Int position2 = toCheck.position + direction * 3;

        if (IsAPotantialMatch(toCheck, position1, position2))
        {
            return true;
        }

        Vector2Int cw = new Vector2Int(direction.y, -direction.x);
        Vector2Int ccw = new Vector2Int(-direction.y, direction.x);
        // çaprazındaki 2. ve 3. taşa bak bak (saat yönünde)
        position1 = toCheck.position + direction + cw;
        position2 = toCheck.position + direction + cw * 2;

        if (IsAPotantialMatch(toCheck, position1, position2))
        {
            return true;
        }

        //iki yöndeki çapraza da bak (birer adım ilerisindeki)
        position2 = toCheck.position + direction + ccw;

        if (IsAPotantialMatch(toCheck, position1, position2))
        {
            return true;
        }

        // çaprazındaki 2. ve 3. taşa bak (saat yönünün tersinde)
        position1 = toCheck.position + direction + ccw * 2;

        if (IsAPotantialMatch(toCheck, position1, position2))
        {
            return true;
        }


        return false;
    }

    private bool IsAPotantialMatch(Matchable toCompare, Vector2Int position1, Vector2Int position2)
    {
        if (CheckBounds(position1) && CheckBounds(position2) && !IsEmpty(position1) && !IsEmpty(position2) && GetItemAt(position1).Idle && GetItemAt(position2).Idle && GetItemAt(position1).Type == toCompare.Type && GetItemAt(position2).Type == toCompare.Type)
        {
            return true;
        }
        return false;
    }

    //hint göster
    public void ShowHint()
    {
        _hint.PointHint(possibleMoves[Random.Range(0, possibleMoves.Count)].transform);
    }

    // Muhsina ekledi
    public IEnumerator ResetGrid(bool allowMatchesOnStart = false)
    {
        // Devam eden swap/fill/scan coroutineleri varsa çakışmasın:
        StopAllCoroutines();

        // 1) Griddeki tüm taşları pool’a iade et
        for (int y = 0; y < Dimensions.y; y++)
        {
            for (int x = 0; x < Dimensions.x; x++)
            {
                if (!IsEmpty(x, y))
                {
                    Matchable m = GetItemAt(x, y);

                    // grid datasından kaldır
                    RemoveItemAt(x, y);

                    // havuza gönder (animasyonsuz hızlı reset)
                    _pool.ReturnObjectToPool(m);
                }
            }
        }

        // 2) Grid datasını tamamen temizle
        Clear();

        // 3) Yeniden doldur
        yield return StartCoroutine(PopulateGrid(allowMatchesOnStart, true));
    }

    //Muhsina ekledi
}

