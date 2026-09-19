using UnityEngine;

//https://www.youtube.com/watch?v=28JTTXqMvOU
//https://www.youtube.com/watch?v=goo0lEKaLSQ

public class MiniMapFollow : MonoBehaviour
{
    public Transform Player;
    public Vector3 offset;

    public Camera MiniMap_Camera;

    public GameObject PlayerArrow;

    const int minChange = 1;

    private void Start()
    {
        arrowSize = 1.5f;
        PlayerArrow.transform.localScale = new Vector3(arrowSize * MiniMap_Camera.orthographicSize / 12.0f, arrowSize * MiniMap_Camera.orthographicSize / 12.0f, arrowSize * MiniMap_Camera.orthographicSize / 12.0f);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Player.position + offset;

    }

    public float arrowSize = 0.75f;
    public void ButtonClick_ZoomIn()
    {
        float change = MiniMap_Camera.orthographicSize * 0.10f;
        if (change < 1)
            change = 1;
        if (MiniMap_Camera.orthographicSize > 3)
            MiniMap_Camera.orthographicSize -= change;
        Global.ignoreRaycast = true;

        PlayerArrow.transform.localScale = new Vector3(arrowSize * MiniMap_Camera.orthographicSize / 12.0f, arrowSize * MiniMap_Camera.orthographicSize / 12.0f, arrowSize * MiniMap_Camera.orthographicSize / 12.0f);
    }


    public void ButtonClick_ZoomOut()
    {
        float change = MiniMap_Camera.orthographicSize * 0.10f;
        if (change < 1)
            change = 1;
        //if (MiniMap_Camera.orthographicSize < 150)
            MiniMap_Camera.orthographicSize += change;
        Global.ignoreRaycast = true;

        PlayerArrow.transform.localScale = new Vector3(arrowSize * MiniMap_Camera.orthographicSize / 12.0f, arrowSize * MiniMap_Camera.orthographicSize / 12.0f, arrowSize * MiniMap_Camera.orthographicSize / 12.0f);
    }


}
