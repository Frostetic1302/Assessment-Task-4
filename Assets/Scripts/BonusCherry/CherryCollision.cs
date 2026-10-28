using UnityEngine;

public class CherryCollision : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GameManager.instance.AddScore(100);

            CherryController controller =
                FindAnyObjectByType<CherryController>();

            if (controller != null)
            {
                controller.OnCherryDestroyed();
            }

            Destroy(gameObject);
        }
    }
}