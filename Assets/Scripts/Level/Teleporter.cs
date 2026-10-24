using UnityEngine;

public class Teleporter : MonoBehaviour
{
    public Transform destination;
    private static bool canTeleport = true;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && canTeleport)
        {
            collision.transform.position = destination.position;
            canTeleport = false;
            Invoke(nameof(ResetTeleport), 0.2f);
        }
    }

    void ResetTeleport()
    {
        canTeleport = true;
    }
}