using Unity.VisualScripting;
using UnityEngine;

public class MatchablePool : ObjectPool<Matchable>
{
    [SerializeField] private int howManyTypes;
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private Color[] colors;
    [SerializeField] private Sprite match4Powerup;
    [SerializeField] private Sprite match5Powerup;

    [SerializeField] private Sprite crossPowerup;

    public void RandomizeType(Matchable toRandomize)
    {
        int random = Random.Range(0, howManyTypes);

        toRandomize.SetType(random, sprites[random], colors[random]);
    }
    public Matchable GetRandomMatchable()
    {
        Matchable randomMatchable = GetPooledObject();

        RandomizeType(randomMatchable);

        return randomMatchable;
    }
    public int NextType(Matchable matchable)
    {
        int nextType = (matchable.Type + 1) % howManyTypes;
        matchable.SetType(nextType, sprites[nextType], colors[nextType]);
        return nextType;
    }

    public Matchable UpgradeMatchable(Matchable toBeUpgraded, MatchType matchType)
    {
        if (matchType == MatchType.cross)
        {
            return toBeUpgraded.Upgrade(MatchType.cross, crossPowerup);
        }
        if (matchType == MatchType.match4)
        {
            return toBeUpgraded.Upgrade(MatchType.match4, match4Powerup);

        }
        else if (matchType == MatchType.match5)
        {
            return toBeUpgraded.Upgrade(MatchType.match5, match5Powerup);
        }

        Debug.LogWarning("Tried to upgrade a matchable with an invalid match type");
        return toBeUpgraded;
    }

}
