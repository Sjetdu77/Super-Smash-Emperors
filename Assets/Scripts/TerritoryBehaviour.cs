using UnityEngine;

public class TerritoryBehaviour : MonoBehaviour
{
    public BelongingGestion FighterGestion;
    public BelongingGestion BelongingGestion;
    public SpriteRenderer Background;
    public Animator Animator;

    public void SetTerritoryFighter(Fighter Fighter)
    {
        BelongingGestion.ChangeSprite(Fighter.GetBelongings());
        ChangeSprites(Fighter);
    }

    public void ChangeSprites(Fighter Fighter)
    {
        FighterGestion.ChangeSprite(Fighter.GetShowSprite());
        Background.color = Fighter.Color;
    }
}
