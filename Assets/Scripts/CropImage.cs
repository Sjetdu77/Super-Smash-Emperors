using UnityEngine;
using UnityEngine.UI;

public class CropImage : MonoBehaviour
{
    public RawImage Image;
    public RawImage Background;

    public void ChangeImage(Fighter Fighter)
    {
        if (Fighter != null)
        {
            Image.texture = Fighter.GetAnnouncements().texture;
            Background.color = Fighter.Color;
        }
    }
}
