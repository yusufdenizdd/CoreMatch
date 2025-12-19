using System.Collections.Generic;
using UnityEngine;

public class Match //component olarak kullanmayacağımız için monobehaviour olmasına gerek yok
{
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
            return _matchables.Count;
        }
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
