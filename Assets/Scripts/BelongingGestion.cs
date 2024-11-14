using UnityEngine;

public class BelongingGestion : MonoBehaviour
{
    public SpriteRenderer SpriteRenderer;

    public void ChangeSprite(Sprite Sprite)
    {
        var parent = (gameObject.transform as RectTransform);
        parent.localScale = parent.rect.size / Sprite.rect.size * Sprite.pixelsPerUnit;
        SpriteRenderer.sprite = Sprite;
    }
}
