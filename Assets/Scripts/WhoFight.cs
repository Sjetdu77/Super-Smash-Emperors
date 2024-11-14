using UnityEngine;
using UnityEngine.UI;

public class WhoFight : MonoBehaviour
{
    public RawImage Image;

    public void SetImage(Fighter Fighter)
    {
        var Color = Image.color;
        Color.a = Fighter == null ? 0 : 1;
        Image.color = Color;
        if (Fighter != null) Image.texture = Fighter.GetShowSprite().texture;
    }
}
