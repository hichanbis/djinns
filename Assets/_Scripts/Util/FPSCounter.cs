using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
    float timeleft;

    private float fps;
    private int frames; // Frames drawn over the interval
    private Text textContainer;

    void Start(){
        textContainer = GetComponent<Text>();
    }


    void Update()
    {
        timeleft -= Time.deltaTime;
        ++frames;

        if (timeleft <= 0.0) {
            fps = frames;
            timeleft = 1;
            frames = 0;
        }
        textContainer.text = "FPS: " + fps.ToString();

    }
}