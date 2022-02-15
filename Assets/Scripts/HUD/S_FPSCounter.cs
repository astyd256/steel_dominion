using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class S_FPSCounter : MonoBehaviour
{
    public float HudRefreshRate = 1f;
    public TMP_Text fpsText;
    void Start()
    {
        InvokeRepeating(nameof(fpsupdate), 1f, HudRefreshRate);
    }

    private void fpsupdate()
    {
            int fps = (int)(1f / Time.unscaledDeltaTime);
            fpsText.text = "FPS: " + fps;
    }
}
