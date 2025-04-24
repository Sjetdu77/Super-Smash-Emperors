using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum Roster
{
    Smash64,
    Melee,
    Brawl,
    Smash4,
    Smash4DLC,
    UltimateClassic,
    UltimateEcho,
    UltimateDLC,
    UltimateAll
}

public class OrganizationTable : MonoBehaviour
{
    static public OrganizationTable instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("OrganizationTable already exists.");
            return;
        }
        instance = this;
    }

    private void LoadScene()
    {
        SceneManager.LoadScene("World");
        gameObject.SetActive(true);
    }

    public void OnClickContinueGrid()
    {
        SaveLoadData.instance.LoadGridFromJson();
        LoadScene();
    }

    public void OnClickDefaultGrid(Roster Roster)
    {
        SaveLoadData.instance.LoadWorldFromJson(Roster);
        LoadScene();
    }

    public void OnClickRandomGrid(Roster Roster)
    {
        var RandomFighter = FighterList.instance.list[0];
        var FighterGrid = new FighterGrid() {
            { 0, new FighterYGrid() {
                { 0, new Fighter[] { RandomFighter, RandomFighter } }
            } }
        };

        var idList = new List<int>();

        foreach (var Couple in FighterList.instance.list)
        {
            var Fighter = Couple.Value;
            bool IsIn = Roster switch
            {
                Roster.Smash64          => Fighter.in64,
                Roster.Melee            => Fighter.inMelee,
                Roster.Brawl            => Fighter.inBrawl,
                Roster.Smash4           => Fighter.inSmash4 && !Fighter.isDLCSmash4,
                Roster.Smash4DLC        => Fighter.inSmash4,
                Roster.UltimateClassic  => Fighter.Id < 300 && !Fighter.isDLCUltimate,
                Roster.UltimateEcho     => Fighter.Id != 51 && !Fighter.isDLCUltimate,
                Roster.UltimateDLC      => Fighter.Id < 300,
                Roster.UltimateAll      => Fighter.Id != 51,
                _                       => false
            };

            if (IsIn && Fighter != RandomFighter) idList.Add(Couple.Key);
        }

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
        LoadScene();
    }
}
