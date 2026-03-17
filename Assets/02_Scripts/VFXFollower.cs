using UnityEngine;

public class VFXFollower : MonoBehaviour
{
    private Transform target;       // 따라다닐 주인
    private Camera mainCam;
    private Camera vfxCam;
    private bool isUI;

    public void Setup(Transform target, Camera mainCam, Camera vfxCam)
    {
        this.target = target;
        this.mainCam = mainCam;
        this.vfxCam = vfxCam;
        
        this.isUI = target.GetComponent<RectTransform>() != null;
    }

    void LateUpdate()
    {        
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 finalPos;
        
        if (isUI)
        {
            Vector3 screenPos = target.position;
            finalPos = vfxCam.ViewportToWorldPoint(new Vector3(screenPos.x / Screen.width, screenPos.y / Screen.height, 10f));
        }
        else
        {
            Vector3 viewport = mainCam.WorldToViewportPoint(target.position);
            finalPos = vfxCam.ViewportToWorldPoint(new Vector3(viewport.x, viewport.y, 10f));
        }

        transform.position = finalPos;
    }
}