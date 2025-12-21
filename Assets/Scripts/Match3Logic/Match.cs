using System.Collections.Generic;
using UnityEngine;


public enum Orientation
{
    none,
    horizontal,
    vertical,
    both

}

public class Match //component olarak kullanmayacağımız için monobehaviour olmasına gerek yok
{

    private int _unlisted = 0;
    public Orientation orientation = Orientation.none;

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
    }

    public void AddMatchable(Matchable toAdd)
    {
        _matchables.Add(toAdd);

    }

    public void AddUnlisted()
    {
        ++_unlisted;
    }

    public void Merge(Match toMerge)
    {
        _matchables.AddRange(toMerge._matchables);
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
