using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public int score;
    public int lives = 3;
    public TMP_Text scoreText;
    public TMP_Text timerText;
    private float gameTime;
    private bool gameRunning;
    public GameObject[] lifeIcons;
    public GameObject gameOverText;
    public GameObject blocker;
    public float scaredTimer;
    public TMP_Text countdownText;

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        if (gameRunning)
        {
            gameTime += Time.deltaTime;
            UpdateTimer();
        }

        if (scaredTimer > 0)
        {
            scaredTimer -= Time.deltaTime;
        }
    }

    void Start()
    {
        StopGame();
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
        }

        PacStudentController pacStudent = FindAnyObjectByType<PacStudentController>();
        if (pacStudent != null)
        {
            pacStudent.enabled = false;
        }

        GameObject[] ghosts = GameObject.FindGameObjectsWithTag("Ghost");
        foreach (var ghost in ghosts)
        {
            GhostController ghostController = ghost.GetComponent<GhostController>();
            if (ghostController != null)
            {
                ghostController.enabled = false;
            }
        }

        StartCoroutine(CountdownRoutine(pacStudent, ghosts));
    }

    IEnumerator CountdownRoutine(PacStudentController pacStudent, GameObject[] ghosts)
    {
        string[] countdown = { "3", "2", "1", "GO!" };
        foreach (string number in countdown)
        {
            if (countdownText != null)
            {
                countdownText.text = number;
            }

            yield return new WaitForSeconds(1f);
        }

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }

        if (pacStudent != null)
        {
            pacStudent.lastInput = PacStudentController.Direction.Right;
            pacStudent.currentInput = PacStudentController.Direction.Right;
            pacStudent.enabled = true;
        }

        foreach (var ghost in ghosts)
        {
            GhostController ghostController = ghost.GetComponent<GhostController>();
            if (ghostController != null)
            {
                ghostController.enabled = true;
            }
        }
        StartGame();
    }

    public void AddScore(int points)
    {
        score += points;
        scoreText.text = score.ToString("D6");
    }

    void UpdateTimer()
    {
        int minutes = (int)(gameTime / 60);
        int seconds = (int)(gameTime % 60);
        int milliseconds = (int)((gameTime * 100) % 100);

        timerText.text =
            minutes.ToString("00") + ":" +
            seconds.ToString("00") + ":" +
            milliseconds.ToString("00");
    }

    public void StartGame()
    {
        gameRunning = true;
    }

    public void StopGame()
    {
        gameRunning = false;
    }

    public void PacStudentHitGhost()
    {
        StartCoroutine(DeathSequenceRoutine());
    }

    IEnumerator DeathSequenceRoutine()
    {
        StopGame();
        PacStudentController pacStudent = FindAnyObjectByType<PacStudentController>();
        GameObject[] ghosts = GameObject.FindGameObjectsWithTag("Ghost");

        if (pacStudent != null)
        {
            pacStudent.enabled = false;
            pacStudent.PlayDeathFX();
        }

        foreach (var ghost in ghosts)
        {
            GhostController ghostController = ghost.GetComponent<GhostController>();
            if (ghostController != null)
            {
                ghostController.enabled = false;
            }
        }

        yield return new WaitForSeconds(2f);
        lives--;
        if (lives >= 0 && lives < lifeIcons.Length && lifeIcons[lives] != null)
        {
            lifeIcons[lives].SetActive(false);
        }

        if (lives <= 0)
        {
            GameOver();
        }
        else
        {
            if (pacStudent != null)
            {
                pacStudent.Respawn();
            }

            foreach (var ghost in ghosts)
            {
                GhostController ghostController = ghost.GetComponent<GhostController>();
                if (ghostController != null)
                {
                    ghostController.ResetToInitialState();
                    ghostController.enabled = true;
                }
            }

            yield return StartCoroutine(WaitForPlayerInput(pacStudent));
        }
    }

    IEnumerator WaitForPlayerInput(PacStudentController pacStudent)
    {
        if (pacStudent != null)
        {
            pacStudent.enabled = false;
        }

        while (true)
        {
            PacStudentController.Direction direction = PacStudentController.Direction.None;

            if (Input.GetKeyDown(KeyCode.W))
            {
                direction = PacStudentController.Direction.Up;
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                direction = PacStudentController.Direction.Down;
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                direction = PacStudentController.Direction.Left;
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                direction = PacStudentController.Direction.Right;
            }

            if (direction != PacStudentController.Direction.None)
            {
                if (pacStudent != null)
                {
                    pacStudent.lastInput = direction;
                    pacStudent.currentInput = direction;
                    PacStudentController.isWaitingForInput = false;
                    pacStudent.enabled = true;
                }
                StartGame();
                yield break;
            }
            yield return null;
        }
    }

    public void LoseLife()
    {
        PacStudentHitGhost();
    }

    public void GameOver()
    {
        StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine()
    {
        StopGame();
        PacStudentController pacStudent = FindAnyObjectByType<PacStudentController>();
        if (pacStudent != null)
        {
            pacStudent.enabled = false;
        }

        GameObject[] ghosts = GameObject.FindGameObjectsWithTag("Ghost");
        foreach (var ghost in ghosts)
        {
            GhostController ghostController = ghost.GetComponent<GhostController>();
            if (ghostController != null)
            {
                ghostController.enabled = false;
            }
        }
        blocker.SetActive(true);
        gameOverText.SetActive(true);
        yield return new WaitForSeconds(3);
        SaveHighScore();
        SceneManager.LoadScene("StartScene");
    }

    void SaveHighScore()
    {
        int highScore =
            PlayerPrefs.GetInt("HighScore", 0);

        if (score > highScore)
        {
            PlayerPrefs.SetInt("HighScore", score);
            PlayerPrefs.SetFloat("BestTime", gameTime);
            PlayerPrefs.Save();
        }
    }
}