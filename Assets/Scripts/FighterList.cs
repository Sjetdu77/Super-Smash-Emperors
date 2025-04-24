using ProjectTools;
using System;
using System.Collections.Generic;
using UnityEngine;

public class FighterList : MonoBehaviour
{
    public IdFighter list;

    static public FighterList instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("FighterList instance already existed.");
            return;
        }
        instance = this;
    }

    private void Start()
    {
        foreach (var FighterTuple in list)
        {
            var Fighter = FighterTuple.Value;
            Fighter.Id = FighterTuple.Key;
            Fighter.AddSprites();
        }
    }
}

[Serializable]
public class IdFighter : SerializableDictionary<int, Fighter> {}

[Serializable]
public class Fighter
{
    public int Id;
    public string Name;
    public Color Color;

    public bool in64;
    public bool inMelee;
    public bool inBrawl;
    public bool inSmash4;

    public bool isDLCSmash4;
    public bool isDLCUltimate;

    private Sprite Announcements = null;
    private Sprite Belongings = null;
    private Sprite ShowSprite = null;

    private TerritoryBehaviour Core = null;

    readonly static Dictionary<Fighter, Dictionary<Fighter, Fighter[]>> Assemblages = new();

    static public Fighter[] Assemble(Fighter A, Fighter B)
    {
        Fighter[] ResA;
        if (!Assemblages.ContainsKey(A)) Assemblages.Add(A, new());
        if (!Assemblages.ContainsKey(B)) Assemblages.Add(B, new());

        if (Assemblages[A].ContainsKey(B)) ResA = Assemblages[A][B];
        else
        {
            ResA = new Fighter[] { A, B };
            Assemblages[A].Add(B, ResA);
        }

        if (!Assemblages[B].ContainsKey(A))
            Assemblages[B].Add(A, new Fighter[] { B, A });

        return ResA;
    }

    public override string ToString() { return Name; }

    public void AddSprites()
    {
        var Path = $"Fighters/{Name}";
        Announcements = Resources.Load<Sprite>($"{Path}/Announcement");
        Belongings = Resources.Load<Sprite>($"{Path}/Belongings");
        ShowSprite = Resources.Load<Sprite>($"{Path}/ShowSprite");
    }

    public void SetCore(TerritoryBehaviour Behaviour) { Core = Behaviour; }

    public TerritoryBehaviour GetCore() => Core;

    public void ChangePossessor(Fighter Conqueror) => Core.ChangeSprites(Conqueror);

    public Sprite GetAnnouncements() => Announcements;
    public Sprite GetBelongings() => Belongings;
    public Sprite GetShowSprite() => ShowSprite;
}