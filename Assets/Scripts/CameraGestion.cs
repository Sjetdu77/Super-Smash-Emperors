using System.Linq;
using UnityEngine;

public class CameraGestion : MonoBehaviour
{
    public float DeltaScale = 1f;
    public float DefaultScale = 25f;

    private float minX, minY, maxX, maxY;

    static public CameraGestion instance;

    private void Awake()
    {
        if (instance != null) return;

        instance = this;
    }

    void Update()
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            var Scale = Input.GetAxis("Mouse ScrollWheel") * DeltaScale;
            if (Camera.main.orthographicSize - Scale > 10)
                Camera.main.orthographicSize -= Scale;
        }
        if (Input.GetMouseButton(0) || Input.GetMouseButton(2))
        {
            transform.Translate(4 * Camera.main.orthographicSize * Time.deltaTime * new Vector2(-Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y")));
        }

        Vector3 newPosition = transform.position;
        if (newPosition.x > maxX) newPosition.x = maxX;
        if (newPosition.y > maxY) newPosition.y = maxY;
        if (newPosition.x < minX) newPosition.x = minX;
        if (newPosition.y < minY) newPosition.y = minY;
        transform.position = newPosition;
    }

    public void MoveCamera(float x, float y) => transform.position = new Vector3(x, y, -10f);

    public void SetExtremes(float[] coordX, float[] coordY)
    {
        minX = coordX[0]; minY = coordY[0]; maxX = coordX[1]; maxY = coordY[1];
        MoveCamera(coordX.Average(), coordY.Average());
        Camera.main.orthographicSize = DefaultScale;
    }
}
