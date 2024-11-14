using System.Collections.Generic;
using UnityEngine;

public enum Mode
{
    None,
    Continue,
    FixedDefault,
    FixedEcho,
    FixedClassic,
    RandomDefault,
    RandomEcho
}

public class OrganizationTable : MonoBehaviour
{

    static public OrganizationTable instance;

    private readonly int[][] DefaultGrid = {
        new int[] { 1, 14, 27, 42, 58, 71 },
        new int[] { 2, 15, 28, 43, 59, 72 },
        new int[] { 3, 16, 29, 44, 60, 73 },
        new int[] { 4, 17, 30, 45, 61, 74 },
        new int[] { 5, 18, 31, 46, 62, 75 },
        new int[] { 6, 19, 32, 47, 63, 76 },
        new int[] { 7, 20, 33, 48, 64, 77 },
        new int[] { 8, 21, 36, 49, 65, 78 },
        new int[] { 9, 22, 37, 50, 66, 79 },
        new int[] { 10, 23, 38, 54, 67, 81 },
        new int[] { 11, 24, 39, 55, 68, 82 },
        new int[] { 12, 25, 40, 56, 69, 51 },
        new int[] { 13, 26, 41, 57, 70, 0 },
    };

    private readonly int[][] DefaultEchoGrid =
    {
        new int[] { 1, 13, 24, 37, 50, 65 },
        new int[] { 2, 313, 25, 38, 54, 66 },
        new int[] { 3, 14, 325, 39, 55, 366, 77 },
        new int[] { 4, 15, 26, 40, 56, 67, 78 },
        new int[] { 304, 16, 27, 41, 57, 68, 79 },
        new int[] { 5, 17, 28, 42, 58, 69, 81 },
        new int[] { 6, 18, 328, 43, 59, 70, 82 },
        new int[] { 7, 19, 29, 44, 60, 71, 511 },
        new int[] { 8, 20, 30, 45, 360, 72, 512 },
        new int[] { 9, 21, 31, 46, 61, 73, 513 },
        new int[] { 10, 321, 32, 47, 62, 74, 0 },
        new int[] { 11, 22, 33, 48, 63, 75 },
        new int[] { 12, 23, 36, 49, 64, 76 },
    };

    private readonly int[][] DefaultClassicGrid =
    {
        new int[] { 1, 12, 22, 32, 46, 360, 71 },
        new int[] { 2, 13, 23, 33, 47, 61, 72 },
        new int[] { 3, 313, 24, 36, 48, 62, 73 },
        new int[] { 4, 14, 25, 37, 49, 63, 74 },
        new int[] { 304, 15, 325, 38, 50, 64, 75 },
        new int[] { 5, 16, 26, 39, 54, 65, 76 },
        new int[] { 6, 17, 27, 40, 55, 66, 77 },
        new int[] { 7, 18, 28, 41, 56, 366, 78 },
        new int[] { 8, 19, 328, 42, 57, 67, 79 },
        new int[] { 9, 20, 29, 43, 58, 68, 81 },
        new int[] { 10, 21, 30, 44, 59, 69, 82 },
        new int[] { 11, 321, 31, 45, 60, 70 },
    };

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("OrganizationTable already exists.");
            return;
        }
        instance = this;
    }

    public void CreateNewGrid(Mode Mode)
    {
        switch (Mode)
        {
            case Mode.Continue:
                SaveLoadData.instance.LoadFromJson();
                break;

            case Mode.FixedDefault:
                CreateDefaultGrid(DefaultGrid);
                break;

            case Mode.FixedEcho:
                CreateDefaultGrid(DefaultEchoGrid);
                break;

            case Mode.FixedClassic:
                CreateDefaultGrid(DefaultClassicGrid);
                break;

            case Mode.RandomDefault:
                CreateRandomGrid(false);
                break;

            case Mode.RandomEcho:
                CreateRandomGrid(true);
                break;

            default:
                Debug.LogError("No mode!");
                break;
        }
    }

    private void CreateDefaultGrid(int[][] SetGrid)
    {
        List<int> list = new();
        var FighterGrid = new FighterGrid();
        for (int i = 0; i < SetGrid.Length; i++)
        {
            var yGrid = new FighterYGrid();
            FighterGrid.Add(i, yGrid);

            for (int j = 0; j < SetGrid[i].Length; j++)
            {
                var Id = SetGrid[i][j];
                var fighter = FighterList.instance.list[Id];
                list.Add(Id);
                yGrid.Add(j, new Fighter[] { fighter, fighter });
            }
        }

        GridGestion.instance.CreateGrid(FighterGrid);
        SaveLoadData.instance.NewGame();
    }

    private void CreateRandomGrid(bool WithEcho)
    {
        var RandomFighter = FighterList.instance.list[0];
        var FighterGrid = new FighterGrid() {
            { 0, new FighterYGrid() {
                { 0, new Fighter[] { RandomFighter, RandomFighter } }
            } }
        };

        var idList = new List<int>();

        foreach (var id in FighterList.instance.list.Keys)
            if (id > 0 && (WithEcho || id < 300) && !(WithEcho && id == 51)) idList.Add(id);

        Dictionary<int, Dictionary<int, bool>>
            Possibilities = new()
            {
                { 
                    -1, new() 
                    {
                        { 0, true }
                    } 
                },
                { 
                    0, new()
                    {
                        { -1, true },
                        { 0, false },
                        { 1, true }
                    }
                },
                {
                    1, new()
                    {
                        { 0, true }
                    }
                }
            };

        while (idList.Count > 0)
        {
            bool placed = false;
            int X = 0, Y = 0, FighterId = 0;
            Fighter FighterChosen = null;

            while (!placed)
            {
                FighterId = idList[Random.Range(0, idList.Count)];
                FighterChosen = FighterList.instance.list[FighterId];
                var XKeys = Possibilities.Keys;
                int XMin = 0, XMax = 0;
                foreach (var x in XKeys)
                {
                    if (x > XMax) XMax = x;
                    if (x < XMin) XMin = x;
                }
                X = Random.Range(XMin, XMax + 1);

                var YGrid = Possibilities[X];
                var YKeys = YGrid.Keys;
                int YMin = 0, YMax = 0;
                foreach (var y in YKeys)
                {
                    if (y > YMax) YMax = y;
                    if (y < YMin) YMin = y;
                }
                Y = Random.Range(YMin, YMax + 1);

                if (Possibilities.ContainsKey(X) && Possibilities[X].ContainsKey(Y) && Possibilities[X][Y])
                {
                    Possibilities[X][Y] = false;
                    placed = true;
                }
            }

            if (!FighterGrid.ContainsKey(X))
                FighterGrid.Add(X, new() { { Y, new Fighter[] { FighterChosen, FighterChosen } } });
            else FighterGrid[X].Add(Y, new Fighter[] { FighterChosen, FighterChosen });

            if (!Possibilities[X].ContainsKey(Y - 1)) Possibilities[X].Add(Y - 1, true);
            if (!Possibilities[X].ContainsKey(Y + 1)) Possibilities[X].Add(Y + 1, true);

            if (!Possibilities.ContainsKey(X - 1)) Possibilities.Add(X - 1, new() { { Y, true } });
            else if (!Possibilities[X - 1].ContainsKey(Y)) Possibilities[X - 1].Add(Y, true);

            if (!Possibilities.ContainsKey(X + 1)) Possibilities.Add(X + 1, new() { { Y, true } });
            else if (!Possibilities[X + 1].ContainsKey(Y)) Possibilities[X + 1].Add(Y, true);

            idList.Remove(FighterId);
        }

        GridGestion.instance.CreateGrid(FighterGrid);
        SaveLoadData.instance.NewGame();
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

    public void Add(Fighter[] Frontier) {
        var A = Frontier[0];
        var B = Frontier[1];

        if (!ContainsKey(A)) Add(A, new());
        if (!ContainsKey(B)) Add(B, new());

        this[A].Add(B);
        this[B].Add(A);
    }

    public List<Fighter> GetFrontiers(Fighter Fighter) => this[Fighter];
}
