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
    public List<Fighter> AllFighters;
    public List<Fighter> FallenFighters;
    public List<Fighter> FightersInQuest;

    readonly Dictionary<Fighter, int[]> FighterLands = new();
    readonly Dictionary<Fighter, List<Fighter>> Possessions = new();

    public FighterGrid() { }

    public FighterGrid(ExportGrid Grid, int Battle)
    {
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

        foreach (var Fighter in FighterList.instance.list.Values)
        {
            Possessions.Add(Fighter, new());
            AllFighters.Add(Fighter);
            FallenFighters.Add(Fighter);
        }

        foreach (var XGrid in this)
            foreach (var YGrid in XGrid.Value)
            {
                int[] Coords = new int[] { XGrid.Key, YGrid.Key };
                var Land = YGrid.Value[0];
                var Belongings = YGrid.Value[1];
                FighterLands.Add(Land, Coords);
                Possessions[Belongings].Add(Land);
                FallenFighters.Remove(Belongings);

                if (!FightersInQuest.Contains(Belongings)) FightersInQuest.Add(Belongings);
            }
    }

    public Fighter[] AttackSearch()
    {
        Fighter Attacker = null;
        Fighter AttackerLand = null;
        Fighter Defender = null;
        Fighter DefenderLand = null;

        do
        {
            int index = UnityEngine.Random.Range(0, FighterLands.Count);
            var AttackArray = FighterLands.ToArray();

            var AttackCurrent = AttackArray[index];
            if (AttackCurrent.Key != FighterList.instance.list[0])
            {
                AttackerLand = AttackCurrent.Key;
                var Place = AttackCurrent.Value;

                Attacker = this[Place[0]][Place[1]][1];

                List<Fighter[]> Possibilities = new();

                foreach (var A in GridGestion.instance.Frontiers.GetFrontiers(AttackerLand))
                {
                    var Land = FighterLands[A];
                    if (this[Land[0]][Land[1]][1] != Attacker) Possibilities.Add(this[Land[0]][Land[1]]);
                }

                if (Possibilities.Count > 0)
                {
                    var PossibilitiesArray = Possibilities;
                    int indexP = UnityEngine.Random.Range(0, PossibilitiesArray.Count);
                    DefenderLand = Possibilities[indexP][0];
                    Defender = Possibilities[indexP][1];
                }
            }
        } while (Defender == null);

        var DefenderPlace = FighterLands[DefenderLand];
        var DeltaGrid = GridGestion.instance.DeltaGrid;
        var O = GridGestion.instance.O;

        CameraGestion.instance.MoveCamera(O.x + DefenderPlace[0] * DeltaGrid, O.y - DefenderPlace[1] * DeltaGrid);

        return new Fighter[] { Attacker, AttackerLand, Defender, DefenderLand };
    }

    public void Conquest(Fighter Conqueror, Fighter Defeater, List<Fighter> Lands)
    {
        var ConquerorPossessions = Possessions[Conqueror];
        var DefeaterPossessions = Possessions[Defeater];

        if (Lands.Contains(Defeater))
        {
            FallenFighters.Add(Defeater);
            FightersInQuest.Remove(Defeater);
            foreach (var Possession in DefeaterPossessions)
            {
                Possession.ChangePossessor(Conqueror);
                var NewPlace = GetLand(Possession);
                this[NewPlace[0]][NewPlace[1]][1] = Conqueror;
            }
            ConquerorPossessions.AddRange(DefeaterPossessions);
            DefeaterPossessions.Clear();
        }
        else
            foreach (var Fighter in Lands)
            {
                var Place = GetLand(Fighter);

                Fighter.ChangePossessor(Conqueror);
                DefeaterPossessions.Remove(Fighter);
                ConquerorPossessions.Add(Fighter);
                Possessions[Fighter].Clear();

                this[Place[0]][Place[1]][1] = Conqueror;
            }

        if (FightersInQuest.Count == 1) EmperorsManager.instance.SetVictorious(FightersInQuest[0]);
    }

    public void Conquest(Fighter Conqueror, List<Fighter> Lands)
    {
        foreach (var Land in Lands) Conquest(Conqueror, GetOwner(Land), new() { Land });
    }

    public List<Fighter> GetPossessions(Fighter Fighter) => Possessions[Fighter];

    public int[] GetLand(Fighter Fighter) => FighterLands[Fighter];

    public Fighter GetOwner(Fighter Fighter) => GetDatas(Fighter)[1];

    public Fighter[] GetDatas(Fighter Fighter)
    {
        var Land = GetLand(Fighter);
        return this[Land[0]][Land[1]];
    }
}