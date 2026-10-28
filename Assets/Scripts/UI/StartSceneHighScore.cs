using TMPro;
using UnityEngine;

public class StartSceneHighScore : MonoBehaviour
{
    public TMP_Text highScoreText;
    public TMP_Text timeText;
    public TMP_Text level2HighScoreText;
    public TMP_Text level2TimeText;

    void Start()
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        float bestTime = PlayerPrefs.GetFloat("BestTime", 0f);
        int minutes = (int)(bestTime / 60);
        int seconds = (int)(bestTime % 60);
        int milliseconds = (int)((bestTime * 100) % 100);

        highScoreText.text = "Level 1 High Score: " + highScore.ToString("D6");
        timeText.text = "Level 1 Time: " + minutes.ToString("00") + ":" + seconds.ToString("00") + ":" + milliseconds.ToString("00");

        int level2HighScore = PlayerPrefs.GetInt("HighScoreLevel2", 0);
        float level2BestTime = PlayerPrefs.GetFloat("BestTimeLevel2", 0f);
        int level2Minutes = (int)(level2BestTime / 60);
        int level2Seconds = (int)(level2BestTime % 60);
        int level2Milliseconds = (int)((level2BestTime * 100) % 100);

        if (level2HighScoreText != null)
        {
            level2HighScoreText.text = "Level 2 High Score: " + level2HighScore.ToString("D6");
        }

        if (level2TimeText != null)
        {
            level2TimeText.text = "Level 2 Time: " + level2Minutes.ToString("00") + ":" + level2Seconds.ToString("00") + ":" + level2Milliseconds.ToString("00");
        }
    }
}
