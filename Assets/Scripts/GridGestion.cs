using ProjectTools;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridGestion : MonoBehaviour
{
    public GameObject objectToCreate;
    public float DeltaGrid;
    public FighterGrid ActualGrid;
    public Frontiers Frontiers;
    public Vector2 O = new(0f, 0f);

    public static GridGestion instance;

    private void Awake()
    {
        if (instance != null) return;
        instance = this;
    }

    public void CreateGrid(FighterGrid FighterGrid)
    {
        var GoArray = FindObjectsOfType<GameObject>();
        foreach (var go in GoArray) if (objectToCreate.CompareTag(go.tag)) Destroy(go);

        ActualGrid = FighterGrid;
        float[] coordX = { O.x, O.x };
        float[] coordY = { O.y, O.y };

        foreach (var xItem in ActualGrid)
        {
            var X = xItem.Key;
            var placeX = O.x + X * DeltaGrid;
            var allY = xItem.Value;

            if (placeX < coordX[0]) coordX[0] = placeX;
            if (placeX > coordX[1]) coordX[1] = placeX;

            foreach (var yItem in allY)
            {
                var Y = yItem.Key;
                var placeY = O.y - Y * DeltaGrid;
                var Land = yItem.Value[0];
                var Belonging = yItem.Value[1];

                if (placeY < coordY[0]) coordY[0] = placeY;
                if (placeY > coordY[1]) coordY[1] = placeY;

                var NewObject = Instantiate(objectToCreate, transform);
                NewObject.transform.localPosition = new Vector2(placeX, placeY);
                NewObject.name = Land.Name;

                var Territory = NewObject.GetComponent<TerritoryBehaviour>();
                Territory.SetTerritoryFighter(Land);
                if (!Land.Equals(Territory)) { Territory.ChangeSprites(Belonging); }

                Land.SetCore(Territory);
            }
        }

        CameraGestion.instance.SetExtremes(coordX, coordY);
        ActualGrid.Launch();
        Frontiers = new(ActualGrid);
        ActualGrid.Frontiers = Frontiers;
    }

    public void DestroyGrid()
    {
        var allObjects = GameObject.FindGameObjectsWithTag("Territory");
        foreach (var obj in allObjects) Destroy(obj);
        ActualGrid = null;
    }
}

[Serializable]
public class FighterYGrid : SerializableDictionary<int, Fighter[]> { }

[Serializable]
public class FighterGrid : SerializableDictionary<int, FighterYGrid> {
    public int Battle = 1;
    public Roster Roster = Roster.UltimateAll;
    public List<Fighter> AllFighters;
    public List<Fighter> FallenFighters;
    public List<Fighter> FightersInQuest;

    private int MaxLands = 4;
    private Fighter Random;
    public Frontiers Frontiers { set; private get; }

    readonly Dictionary<Fighter, int[]> FighterLands = new();
    readonly Dictionary<Fighter, List<Fighter>> Possessions = new();

    public FighterGrid() { }

    public FighterGrid(FixedGrid Grid, Roster Roster)
    {
        this.Roster = Roster;
        var FList = FighterList.instance.list;
        for (int i = 0; i < Grid[0].Count; i++) Add(i, new FighterYGrid());

        foreach (var XGrid in Grid)
        {
            foreach (var YGrid in XGrid.Value)
            {
                if (YGrid.Value > -1)
                {
                    var Fighter = FList[YGrid.Value];
                    this[YGrid.Key].Add(XGrid.Key, new Fighter[] { Fighter, Fighter });
                }
            }
        }
    }

    public FighterGrid(ExportGrid Grid, int Battle)
    {
        Roster = (Roster)Roster.Parse(typeof(Roster), Grid.Roster);
        var FList = FighterList.instance.list;
        this.Battle = Battle;

        foreach (var XGrid in Grid)
        {
            FighterYGrid Y = new();
            foreach (var YGrid in XGrid.Value)
            {
                var Ids = YGrid.Value;
                var A = FList[Ids[0]];
                var B = FList[Ids[1]];
                Y.Add(YGrid.Key, new Fighter[] { A, B });
            }
            Add(XGrid.Key, Y);
        }
    }

    public void Launch()
    {
        AllFighters = new();
        FallenFighters = new();
        FightersInQuest = new();

        Random = FighterList.instance.list[0];

        foreach (var Fighter in FighterList.instance.list.Values)
            Possessions.Add(Fighter, new());

        foreach (var XGrid in this)
            foreach (var YGrid in XGrid.Value)
            {
                int[] Coords = new int[] { XGrid.Key, YGrid.Key };
                var Land = YGrid.Value[0];
                var Belongings = YGrid.Value[1];
                FighterLands.Add(Land, Coords);
                AllFighters.Add(Land);
                Possessions[Belongings].Add(Land);
                if (!FightersInQuest.Contains(Belongings) && Belongings != Random)
                    FightersInQuest.Add(Belongings);
            }

        FallenFighters.AddRange(AllFighters);
        FallenFighters.RemoveAll(x => FightersInQuest.Contains(x));
        FallenFighters.Remove(Random);

        if (Roster == Roster.Smash64 || Roster == Roster.Melee || Roster == Roster.Brawl) MaxLands = 2;
    }

    public Dictionary<string, Fighter> AttackSearch()
    {
        Fighter Attacker = null,
                AttackerLand = null,
                Defender = null,
                DefenderLand = null,
                OutFighter = null;

        do
        {
            int index = UnityEngine.Random.Range(0, AllFighters.Count);
            var AttackCurrent = AllFighters[index];
            if (AttackCurrent != Random)
            {
                AttackerLand = AttackCurrent;
                Attacker = GetOwner(AttackCurrent);

                List<Fighter[]> Possibilities = new();

                foreach (var A in GridGestion.instance.Frontiers.GetFrontiers(AttackerLand))
                {
                    var Frontier = GetDatas(A);
                    if (Frontier[1] != Attacker) Possibilities.Add(Frontier);
                }

                if (Possibilities.Count > 0)
                {
                    int indexP = UnityEngine.Random.Range(0, Possibilities.Count);
                    if (Possibilities[indexP][1] != Random
                        || (Possibilities[indexP][1] == Random && FallenFighters.Count > 3))
                    {
                        DefenderLand = Possibilities[indexP][0];
                        Defender = Possibilities[indexP][1];
                    }
                }
            }
        } while (Defender == null);

        var DefenderPlace = FighterLands[DefenderLand];
        var DeltaGrid = GridGestion.instance.DeltaGrid;
        var O = GridGestion.instance.O;

        CameraGestion.instance.MoveCamera(O.x + DefenderPlace[0] * DeltaGrid, O.y - DefenderPlace[1] * DeltaGrid);

        if (Defender == Random)
        {
            int index = UnityEngine.Random.Range(0, FallenFighters.Count);
            OutFighter = FallenFighters[index];
            FallenFighters.Remove(OutFighter);
            FightersInQuest.Add(OutFighter);

            List<Fighter> Reconquered = new() { OutFighter, Random };

            Reconquered.AddRange(Frontiers.GetFrontiers(OutFighter));
            Conquest(OutFighter, Reconquered);

            if (Reconquered.Contains(Attacker) || Reconquered.Contains(AttackerLand)) return null;
        }

        return new()
        {
            { "Attacker", Attacker },
            { "AttackerLand", AttackerLand },
            { "Defender", Defender },
            { "DefenderLand", DefenderLand },
            { "OutFighter", OutFighter }
        };
    }

    public void Conquest(Fighter Conqueror, Fighter Defeater, List<Fighter> Lands)
    {
        var ConquerorPossessions = Possessions[Conqueror];
        var DefeaterPossessions = Possessions[Defeater];
        if (Defeater == Random)
        {
            ConquerorPossessions.Add(Random);
            ChangePossessor(Random, Conqueror);
            DefeaterPossessions.Clear();
        }
        else if (Lands.Contains(Defeater))
        {
            FallenFighters.Add(Defeater);
            FightersInQuest.Remove(Defeater);
            foreach (var Possession in DefeaterPossessions) ChangePossessor(Possession, Conqueror);
            ConquerorPossessions.AddRange(DefeaterPossessions);
            DefeaterPossessions.Clear();
        }
        else
            foreach (var Land in Lands)
            {
                ConquerorPossessions.Add(Land);

                if (!Land.Equals(Conqueror))
                {
                    DefeaterPossessions.Remove(Land);
                    Possessions[Land].Clear();
                }
                ChangePossessor(Land, Conqueror);
            }

        for (int i = 0; i < ConquerorPossessions.Count; i++)
        {
            int j = i + 1;
            while (j < ConquerorPossessions.Count)
            {
                if (ConquerorPossessions[i].Equals(ConquerorPossessions[j]))
                    ConquerorPossessions.RemoveAt(j);
                else j++;
            }
        }

        if (FightersInQuest.Count == 1 && Possessions[Random].Count == 0)
            EmperorsManager.instance.SetVictorious(FightersInQuest[0]);
    }

    public void Conquest(Fighter Conqueror, List<Fighter> Lands)
    {
        foreach (var Land in Lands) Conquest(Conqueror, GetOwner(Land), new() { Land });
    }

    public void SetDistance(Fighter Land, Dictionary<Fighter, int> DistanceLands, int Distance)
    {
        DistanceLands[Land] = Distance;
        List<Fighter> LandFrontiers = Frontiers.GetFrontiers(Land);

        foreach (var Frontier in LandFrontiers)
            if (DistanceLands.ContainsKey(Frontier))
                if (DistanceLands[Frontier] > Distance && Distance < MaxLands)
                    SetDistance(Frontier, DistanceLands, Distance + 1);
    }

    public List<Fighter> SetCalled(Fighter Origin, Fighter Attacker, Fighter OutRandom = null)
    {
        if (OutRandom != null)
        {
            List<Fighter> RandLands = new() { OutRandom, Origin },
                          RandPossessions = new(GetPossessions(OutRandom));

            RandPossessions.RemoveAll(x => RandLands.Contains(x));

            while (RandLands.Count < MaxLands && RandPossessions.Count > 0)
            {
                var RandInt = UnityEngine.Random.Range(0, RandPossessions.Count);
                var RandLand = RandPossessions[RandInt];
                RandLands.Add(RandLand);
                RandPossessions.Remove(RandLand);
            }

            return RandLands;
        }

        List<Fighter> Lands = new() { Origin },
                      Possessions = new(GetPossessions(Attacker));

        Dictionary<Fighter, int> DistanceLands = new();
        foreach (var Possession in Possessions)
            if (!DistanceLands.ContainsKey(Possession)) DistanceLands.Add(Possession, 99);

        SetDistance(Origin, DistanceLands, 0);

        Dictionary<int, List<Fighter>> Priorities = new();
        foreach (var DistanceLand in DistanceLands)
        {
            var Land = DistanceLand.Key;
            var Priority = DistanceLand.Value;

            if (!Priorities.ContainsKey(Priority)) Priorities.Add(Priority, new());
            Priorities[Priority].Add(Land);
        }

        int i = 1;
        while (Priorities.ContainsKey(i) && Lands.Count < MaxLands)
        {
            var IndexPriority = Priorities[i];
            if (IndexPriority.Count + Lands.Count > MaxLands)
                do
                {
                    var RandInt = UnityEngine.Random.Range(0, IndexPriority.Count);
                    var Fighter = IndexPriority[RandInt];
                    Lands.Add(Fighter);
                    IndexPriority.Remove(Fighter);
                } while (Lands.Count < MaxLands);
            else Lands.AddRange(IndexPriority);
            i++;
        }

        return Lands;
    }

    public List<Fighter> GetPossessions(Fighter Fighter) => Possessions[Fighter];

    public int[] GetLand(Fighter Fighter) => FighterLands[Fighter];

    public Fighter GetOwner(Fighter Fighter) => GetDatas(Fighter)[1];

    public Fighter[] GetDatas(Fighter Fighter)
    {
        var Land = GetLand(Fighter);
        return this[Land[0]][Land[1]];
    }

    public void ChangePossessor(Fighter LandChanged, Fighter Conqueror)
    {
        LandChanged.ChangePossessor(Conqueror);
        var Land = GetLand(LandChanged);
        this[Land[0]][Land[1]][1] = Conqueror;
    }
}

public class Frontiers : Dictionary<Fighter, List<Fighter>>
{
    public Frontiers(FighterGrid ActualGrid)
    {
        foreach (var XGrid in ActualGrid)
        {
            var X = XGrid.Key;
            foreach (var YGrid in XGrid.Value)
            {
                var Y = YGrid.Key;
                var Land = YGrid.Value[0];

                Fighter[]
                    Top = ActualGrid[X].ContainsKey(Y - 1) ? ActualGrid[X][Y - 1] : null,
                    Bottom = ActualGrid[X].ContainsKey(Y + 1) ? ActualGrid[X][Y + 1] : null,
                    Left = null,
                    Right = null;

                if (ActualGrid.ContainsKey(X - 1))
                    Left = ActualGrid[X - 1].ContainsKey(Y) ? ActualGrid[X - 1][Y] : null;
                if (ActualGrid.ContainsKey(X + 1))
                    Right = ActualGrid[X + 1].ContainsKey(Y) ? ActualGrid[X + 1][Y] : null;

                if (Top != null) Add(Fighter.Assemble(Top[0], Land));
                if (Bottom != null) Add(Fighter.Assemble(Bottom[0], Land));
                if (Left != null) Add(Fighter.Assemble(Left[0], Land));
                if (Right != null) Add(Fighter.Assemble(Right[0], Land));
            }
        }
    }

    private void Add(Fighter[] Frontier)
    {
        var A = Frontier[0];
        var B = Frontier[1];

        if (!ContainsKey(A)) Add(A, new());
        if (!ContainsKey(B)) Add(B, new());

        this[A].Add(B);
        this[B].Add(A);
    }

    public List<Fighter> GetFrontiers(Fighter Fighter) => this[Fighter];
}