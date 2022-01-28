using UnityEngine;
using UnityEngine.UI;

public class S_FPSCounter : MonoBehaviour
{
    public float HudRefreshRate = 1f;
    public Text fpsText;
    void Start()
    {
        Application.targetFrameRate = 60;
        InvokeRepeating(nameof(fpsupdate), 1f, HudRefreshRate);
    }

    private void fpsupdate()
    {
            int fps = (int)(1f / Time.unscaledDeltaTime);
            fpsText.text = "FPS: " + fps;
    }
}
