using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class GhostController : MonoBehaviour
{
    private Animator animator;
    private Vector3 initialPosition;
    private bool isEaten;
    private Coroutine respawnCoroutine;

    private void Start()
    {
        animator = GetComponent<Animator>();
        initialPosition = transform.position;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player") || isEaten)
        {
            return;
        }

        if (GhostManager.instance.GhostsAreScared() ||GhostManager.instance.GhostsAreRecovering())
        {
            isEaten = true;
            if (animator != null)
            {
                animator.SetTrigger("Dead");
            }

            GameManager.instance.AddScore(300);
            GhostManager.instance.PlayDeadMusic();
            if (respawnCoroutine != null)
            {
                StopCoroutine(respawnCoroutine);
            }
            respawnCoroutine = StartCoroutine(RespawnGhost());
        }
        else
        {
            GameManager.instance.LoseLife();
        }
    }

    IEnumerator RespawnGhost()
    {
        yield return new WaitForSeconds(3f);
        isEaten = false;
        UpdateGhostAnimation();
        respawnCoroutine = null;
    }

    public void UpdateGhostAnimation()
    {
        if (animator == null || isEaten)
            return;
        animator.ResetTrigger("Normal");
        animator.ResetTrigger("Scared");
        animator.ResetTrigger("Recovering");
        animator.ResetTrigger("Dead");

        if (GhostManager.instance.GhostsAreScared())
        {
            animator.SetTrigger("Scared");
        }
        else if (GhostManager.instance.GhostsAreRecovering())
        {
            animator.SetTrigger("Recovering");
        }
        else
        {
            animator.SetTrigger("Normal");
        }
    }
    public bool IsEaten()
    {
        return isEaten;
    }


    public void ResetToInitialState()
    {
        if (respawnCoroutine != null)
        {
            StopCoroutine(respawnCoroutine);
            respawnCoroutine = null;
        }
        isEaten = false;
        transform.position = initialPosition;

        if (animator != null)
        {
            animator.ResetTrigger("Dead");
            animator.ResetTrigger("Scared");
            animator.ResetTrigger("Recovering");
            animator.ResetTrigger("Normal");
            animator.SetTrigger("Normal");
        }
    }

}