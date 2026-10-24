using TMPro;
using UnityEngine;

public class StartSceneHighScore : MonoBehaviour
{
    public TMP_Text highScoreText;
    public TMP_Text timeText;

    void Start()
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        float bestTime = PlayerPrefs.GetFloat("BestTime", 0f);

        int minutes = (int)(bestTime / 60);
        int seconds = (int)(bestTime % 60);
        int milliseconds = (int)((bestTime * 100) % 100);

        highScoreText.text = "Level 1 High Score: " + highScore.ToString("D6");
        timeText.text = "Level 1 Time: " + minutes.ToString("00") + ":" + seconds.ToString("00") + ":" + milliseconds.ToString("00");
    }
}