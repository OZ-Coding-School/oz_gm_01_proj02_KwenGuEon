using UnityEngine;
using UnityEngine.UI;

public class PanelDimmer : MonoBehaviour
{
    public Image panelImage;
    public float targetAlpha = 0.7f;

    private void Start()
    {
        panelImage.gameObject.SetActive(false);
    }
    public void ShowDimmer()
    {
        panelImage.gameObject.SetActive(true);

        Color panelColor = panelImage.color;
        panelColor.a = targetAlpha;
        panelImage.color = panelColor;
    }
    public void HideDimmer()
    {
        Color panelColor = panelImage.color;
        panelColor.a = 0;
        panelImage.color = panelColor;

        panelImage.gameObject.SetActive(false);
    }
}
