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

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        ghostTimerText.gameObject.SetActive(false);
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