using System.Collections.Generic;
using UnityEngine;


public enum Orientation
{
    none,
    horizontal,
    vertical,
    both

}

public enum MatchType
{
    invalid,
    match3,
    match4,
    match5,
    cross
}

public class Match //component olarak kullanmayacağımız için monobehaviour olmasına gerek yok
{

    private int _unlisted = 0;
    public Orientation orientation = Orientation.none;
    private Matchable _toBeUpgraded;

    private List<Matchable> _matchables;
    public List<Matchable> Matchables
    {
        get
        {
            return _matchables;
        }
    }

    public int Count
    {
        get
        {
            return _matchables.Count + _unlisted;
        }
    }

    public MatchType GetMatchType
    {
        get
        {
            if (orientation == Orientation.both)
            {
                return MatchType.cross;
            }
            else if (_matchables.Count == 3)
            {
                return MatchType.match3;
            }
            else if (_matchables.Count == 4)
            {
                return MatchType.match4;
            }
            else if (_matchables.Count > 4)
            {
                return MatchType.match5;
            }
            else
            {
                return MatchType.invalid;
            }
        }
    }

    public bool Contains(Matchable toCompare)
    {
        return _matchables.Contains(toCompare);
    }

    //constructor
    public Match()
    {
        _matchables = new List<Matchable>(5);
    }
    //overloaded constructor
    public Match(Matchable original) : this()
    {
        AddMatchable(original);
        _toBeUpgraded = original;
    }

    public Matchable ToBeUpgraded
    {
        get
        {
            if (_toBeUpgraded != null)
            {
                return _toBeUpgraded;
            }
            return _matchables[Random.Range(0, _matchables.Count)];
        }
    }
    public void AddMatchable(Matchable toAdd)
    {
        _matchables.Add(toAdd);

    }

    public void AddUnlisted()
    {
        ++_unlisted;
    }

    public void RemoveMatchable(Matchable toBeRemoved)
    {
        _matchables.Remove(toBeRemoved);

    }

    public void Merge(Match toMerge)
    {
        _matchables.AddRange(toMerge._matchables);

        if (orientation == Orientation.both || toMerge.orientation == Orientation.both || (orientation == Orientation.horizontal && toMerge.orientation == Orientation.vertical) || (orientation == Orientation.vertical && toMerge.orientation == Orientation.horizontal))
        {
            orientation = Orientation.both;
        }
        else if (toMerge.orientation == Orientation.horizontal)
        {
            orientation = Orientation.horizontal;
        }
        else if (toMerge.orientation == Orientation.vertical)
        {
            orientation = Orientation.vertical;
        }
    }
    public override string ToString()
    {
        string s = "Match of type " + _matchables[0].Type + " : \n";

        foreach (Matchable m in _matchables)
        {
            s += "(" + m.position.x + ", " + m.position.y + ") ";
        }

        return s;
    }
}
