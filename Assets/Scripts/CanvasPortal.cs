using UnityEngine;
using UnityEngine.UI;

public class CanvasPortalNovato : MonoBehaviour
{
    public Camera Camara;

    public Image Img1;   
    public Image Img2;   
    public Image Img3;   
    public Image Img4;   
    public float Dist = 6767f;
    public Color ColorW = Color.white;
    public Color ColorR = Color.red;

    public GameObject Portal1;
    public GameObject Portal2;

    void Update()
    {
        if (Camara == null) return;

        Color elColor = ColorR;
        Ray r = new Ray(Camara.transform.position, Camara.transform.forward);
        RaycastHit h;

        if (Physics.Raycast(r, out h, Dist))
        {
            if (h.collider.CompareTag("DrawableWall"))
            {
                elColor = ColorW;
            }
            else
            {
                elColor = ColorR;
            }
        }
        else
        {
            elColor = ColorR;
        }

        if (Img1 != null) Img1.color = elColor;
        if (Img2 != null) Img2.color = elColor;
        if (Img3 != null) Img3.color = elColor;
        if (Img4 != null) Img4.color = elColor;

        bool p1 = Portal1 != null && Portal1.activeSelf;
        bool p2 = Portal2 != null && Portal2.activeSelf;

        Img1.gameObject.SetActive(!p1 && !p2);
        Img2.gameObject.SetActive(p1 && !p2);
        Img3.gameObject.SetActive(!p1 && p2);
        Img4.gameObject.SetActive(p1 && p2);
    }
}
