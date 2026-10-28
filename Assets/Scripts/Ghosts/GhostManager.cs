using TMPro;
using UnityEngine;

public class GhostManager : MonoBehaviour
{
    public static GhostManager instance;
    public Animator[] ghostAnimators;
    public TMP_Text ghostTimerText;
    public AudioSource backgroundMusic;
    public AudioClip normalMusic;
    public AudioClip scaredMusic;
    public AudioClip deadMusic;
    private float scaredTimer;
    private bool scaredMode;
    private bool recoveringTriggered;
    public TMP_Text adrenalineLevelLabel;
    public TMP_Text adrenalineNumberText;
    public int maxAdrenalineLevel = 4;
    public float adrenalinePitchIncrease = 0.08f;
    private int adrenalineLevel;
    private float normalMusicPitch = 1f;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (ghostTimerText != null)
        {
            ghostTimerText.gameObject.SetActive(false);
        }

        if (backgroundMusic != null)
        {
            normalMusicPitch = backgroundMusic.pitch;
        }

        bool adrenalineMode = GameManager.instance != null && GameManager.instance.levelNumber == 2;

        if (adrenalineLevelLabel != null)
        {
            adrenalineLevelLabel.gameObject.SetActive(adrenalineMode);
        }

        if (adrenalineNumberText != null)
        {
            adrenalineNumberText.gameObject.SetActive(adrenalineMode);
        }
        UpdateAdrenalineText();
    }

    void Update()
    {
        if (!scaredMode)
        {
            return;
        }

        scaredTimer -= Time.deltaTime;

        ghostTimerText.text =
            Mathf.CeilToInt(scaredTimer).ToString();

        if (scaredTimer <= 3f && scaredTimer > 0f)
        {
            if (!recoveringTriggered)
            {
                recoveringTriggered = true;
                SetRecoveringState();
            }
        }

        if (scaredTimer <= 0f)
        {
            EndScaredMode();
        }
    }

    public void StartScaredMode()
    {
        IncreaseAdrenaline();
        scaredMode = true;
        scaredTimer = 10f;
        recoveringTriggered = false;
        ghostTimerText.gameObject.SetActive(true);

        foreach (Animator animator in ghostAnimators)
        {
            GhostController ghost = animator.GetComponentInParent<GhostController>();
            animator.ResetTrigger("Normal");
            animator.ResetTrigger("Recovering");
            animator.ResetTrigger("Dead");
            animator.SetTrigger("Scared");
        }
        backgroundMusic.clip = scaredMusic;
        backgroundMusic.Play();
    }

    void IncreaseAdrenaline()
    {
        if (GameManager.instance == null ||
            GameManager.instance.levelNumber != 2)
        {
            return;
        }

        if (adrenalineLevel >= maxAdrenalineLevel)
        {
            return;
        }
        adrenalineLevel++;

        PacStudentController player = FindAnyObjectByType<PacStudentController>();

        if (player != null)
        {
            player.SetAdrenalineLevel(adrenalineLevel);
        }

        if (ghostAnimators != null)
        {
            foreach (Animator animator in ghostAnimators)
            {
                if (animator == null)
                {
                    continue;
                }
                GhostController ghost = animator.GetComponentInParent<GhostController>();

                if (ghost != null)
                {
                    ghost.SetAdrenalineLevel(adrenalineLevel);
                }
            }
        }

        if (backgroundMusic != null)
        {
            backgroundMusic.pitch = normalMusicPitch + (adrenalineLevel * adrenalinePitchIncrease);
        }
        UpdateAdrenalineText();
    }

    void UpdateAdrenalineText()
    {
        if (adrenalineNumberText != null)
        {
            adrenalineNumberText.text = adrenalineLevel.ToString();
        }
    }

    void SetRecoveringState()
    {
        foreach (Animator animator in ghostAnimators)
        {
            if (animator == null)
            {
                continue;
            }
            GhostController ghost = animator.GetComponentInParent<GhostController>();

            if (ghost != null && ghost.IsEaten())
            {
                continue;
            }
            animator.ResetTrigger("Scared");
            animator.ResetTrigger("Normal");
            animator.ResetTrigger("Dead");
            animator.SetTrigger("Recovering");
        }
    }


    void EndScaredMode()
    {
        scaredMode = false;
        scaredTimer = 0f;
        recoveringTriggered = false;
        ghostTimerText.gameObject.SetActive(false);

        foreach (Animator animator in ghostAnimators)
        {
            if (animator == null)
                continue;

            GhostController ghost = animator.GetComponentInParent<GhostController>();
            if (ghost != null && ghost.IsEaten())
                continue;

            animator.ResetTrigger("Scared");
            animator.ResetTrigger("Recovering");
            animator.SetTrigger("Normal");
        }
        backgroundMusic.clip = normalMusic;
        backgroundMusic.Play();
    }


    public bool GhostsAreScared()
    {
        return scaredTimer > 3f;
    }

    public bool GhostsAreRecovering()
    {
        return scaredTimer > 0f && scaredTimer <= 3f;
    }

    public float GetScaredTimer()
    {
        return scaredTimer;
    }

    public void PlayDeadMusic()
    {
        if (backgroundMusic.isPlaying && backgroundMusic.clip == deadMusic)
        {
            return;
        }
        backgroundMusic.clip = deadMusic;
        backgroundMusic.Play();
    }
}  