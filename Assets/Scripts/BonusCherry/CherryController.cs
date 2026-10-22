using UnityEngine;
using System.Collections;

public class CherryController : MonoBehaviour
{
    public GameObject cherryPrefab;
    public float speed = 4f;
    public float spawnOffset = 1f;
    public SpriteMask levelSpriteMask;
    private GameObject currentCherry;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float lerpValue;
    private float journeyLength;
    private bool isMoving = false;

    void Start()
    {
        StartCoroutine(CherryRoutine());
    }

    void Update()
    {
        if (!isMoving || currentCherry == null) return;

        if (journeyLength <= 0f)
        {
            Destroy(currentCherry);
            currentCherry = null;
            isMoving = false;
            return;
        }

        lerpValue += speed * Time.deltaTime / journeyLength;
        currentCherry.transform.position = Vector3.Lerp(startPosition, targetPosition, lerpValue);

        if (lerpValue >= 1f)
        {
            Destroy(currentCherry);
            currentCherry = null;
            isMoving = false;
        }
    }

    IEnumerator CherryRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);
            SpawnCherry();
            yield return new WaitUntil(() => currentCherry == null);
        }
    }

    void SpawnCherry()
    {
        Bounds localBounds = levelSpriteMask.sprite.bounds;
        Vector3 center = levelSpriteMask.transform.TransformPoint(localBounds.center);
        Vector3 lossyScale = levelSpriteMask.transform.lossyScale;

        float halfWidth = (localBounds.extents.x * lossyScale.x) + spawnOffset;
        float halfHeight = localBounds.extents.y * lossyScale.y;

        float minX = center.x - halfWidth;
        float maxX = center.x + halfWidth;
        float minY = center.y - halfHeight;
        float maxY = center.y + halfHeight;

        bool spawnOnLeft = Random.value < 0.5f;
        float startX = spawnOnLeft ? minX : maxX;
        float targetX = spawnOnLeft ? maxX : minX;
        float startY = Random.Range(minY, maxY);

        startPosition = new Vector3(startX, startY, 0f);
        targetPosition = center + (center - startPosition);
        targetPosition.z = 0f;
        journeyLength = Vector3.Distance(startPosition, targetPosition);
        lerpValue = 0f;

        currentCherry = Instantiate(cherryPrefab, startPosition, Quaternion.identity);
        isMoving = true;
    }
}