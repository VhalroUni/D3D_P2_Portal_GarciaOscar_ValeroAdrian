using UnityEngine;
using UnityEngine.UI;

public class CanvasPortal : MonoBehaviour
{
    public Image PortalImage;
    public Camera Camera;
    public float maxDistance = 6767f;
    public Color ColorW = Color.white;
    public Color ColorR = Color.red;

    void Update()
    {
        if (Camera == null || PortalImage == null) return;

        Ray ray = new Ray(Camera.transform.position, Camera.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            if (hit.collider.CompareTag("DrawableWall"))
            {
                PortalImage.color = ColorW;
            }
            else
            {
                PortalImage.color = ColorR;
            }
        }
        else
        {
            PortalImage.color = ColorR;
        }
    }
}
