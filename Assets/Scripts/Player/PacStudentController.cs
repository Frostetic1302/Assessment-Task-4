using UnityEngine;
using UnityEngine.Tilemaps;

public class PacStudentController : MonoBehaviour
{
    public enum Direction
    {
        None,
        Up,
        Down,
        Left,
        Right
    }

    public float moveSpeed = 5f;
    public Direction lastInput = Direction.Right;
    public Direction currentInput = Direction.Right;
    private bool isMoving;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float lerpValue;
    private Animator animator;
    public ParticleSystem dustParticles;
    public AudioSource audioSource;
    public AudioClip walkSound;
    public AudioClip pelletSound;
    public Tilemap[] allTilemaps;
    public PelletManager pelletManager;
    public Transform spriteTransform;
    public ParticleSystem wallParticles;
    public AudioSource wallAudio;
    bool hitWall;
    public AudioClip deathSound;
    public Vector3 respawnPosition = new Vector3(-3.5f, 2.5f, 0f);
    public ParticleSystem deathParticles;
    public static bool isWaitingForInput = false;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        lastInput = Direction.Right;
        currentInput = Direction.Right;
        UpdateAnimatorDirection(currentInput);
    }

    void Update()
    {
        GetInput();

        if (isMoving)
        {
            MoveLerp();
        }
        else
        {
            TryMove();
        }

        if (isMoving)
        {
            UpdateMovementAudio();
        }
        else
        {
            StopMovementAudio();
        }
    }

    void GetInput()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            lastInput = Direction.Up;
            if (currentInput == Direction.None) currentInput = Direction.Up;
            isWaitingForInput = false;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            lastInput = Direction.Down;
            if (currentInput == Direction.None) currentInput = Direction.Down;
            isWaitingForInput = false;
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            lastInput = Direction.Left;
            if (currentInput == Direction.None) currentInput = Direction.Left;
            isWaitingForInput = false;
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            lastInput = Direction.Right;
            if (currentInput == Direction.None) currentInput = Direction.Right;
            isWaitingForInput = false;
        }
    }

    void TryMove()
    {
        if (lastInput == Direction.None)
        {
            return;
        }
        Vector3 nextPosition = GetNextPosition(lastInput);

        if (!IsWalkable(nextPosition))
        {
            if (!hitWall)
            {
                wallParticles.Play();
                wallAudio.Play();
                hitWall = true;
            }
        }
        else
        {
            hitWall = false;
        }

        if (IsWalkable(nextPosition))
        {
            currentInput = lastInput;
            StartMove(nextPosition);
        }
        else
        {
            nextPosition = GetNextPosition(currentInput);

            if (IsWalkable(nextPosition))
            {
                StartMove(nextPosition);
            }
            else
            {
                StopMovementAudio();
            }
        }
    }

    void StartMove(Vector3 nextPosition)
    {
        startPosition = transform.position;
        targetPosition = nextPosition;
        lerpValue = 0f;
        isMoving = true;
        UpdateAnimatorDirection(currentInput);

        if (dustParticles != null)
        {
            Vector3 offset = Vector3.zero;
            switch (currentInput)
            {
                case Direction.Up:
                    offset = new Vector3(0f, -0.25f, 0f);
                    break;
                case Direction.Down:
                    offset = new Vector3(0f, 0.2f, 0f);
                    break;
                case Direction.Left:
                    offset = new Vector3(0.25f, -0.15f, 0f);
                    break;
                case Direction.Right:
                    offset = new Vector3(-0.25f, -0.15f, 0f);
                    break;
            }
            dustParticles.transform.localPosition = offset;

            if (!dustParticles.isPlaying)
            {
                dustParticles.Play();
            }
        }
    }

    public void PlayDeathFX()
    {
        StopMovementAudio();

        if (animator != null)
        {
            animator.SetInteger("Direction", -1);
            animator.SetTrigger("Death");
        }

        if (dustParticles != null)
        {
            dustParticles.transform.position = transform.position;
            dustParticles.Play();
        }

        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        if (deathParticles != null)
        {
            Instantiate(deathParticles, transform.position, Quaternion.identity);
        }
    }

    public void Respawn()
    {
        transform.position = respawnPosition;
        startPosition = respawnPosition;
        targetPosition = respawnPosition;
        isMoving = false;
        lastInput = Direction.None;
        currentInput = Direction.None;
        UpdateAnimatorDirection(currentInput);
        isWaitingForInput = true;

        GhostController[] ghosts = FindObjectsByType<GhostController>();
        foreach (GhostController ghost in ghosts)
        {
            ghost.ResetToInitialState();
        }
    }

    void UpdateMovementAudio()
    {
        bool hasPellet = false;

        foreach (Tilemap map in allTilemaps)
        {
            if (map == null)
                continue;

            Vector3 playerPosition = animator.transform.position;
            Vector3Int cell = map.WorldToCell(playerPosition);
            TileBase tile = map.GetTile(cell);

            if (tile != null && tile.name.Contains("Pellet"))
            {
                hasPellet = true;
                break;
            }
        }

        AudioClip clip = walkSound;

        if (hasPellet)
        {
            clip = pelletSound;
        }

        if (audioSource == null || clip == null)
            return;

        if (audioSource.clip != clip)
        {
            audioSource.Stop();
            audioSource.clip = clip;
        }

        audioSource.loop = true;

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    void StopMovementAudio()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    void MoveLerp()
    {
        lerpValue += moveSpeed * Time.deltaTime;
        transform.position = Vector3.Lerp(startPosition, targetPosition, lerpValue);

        if (lerpValue >= 1f)
        {
            transform.position = targetPosition;
            isMoving = false;
            dustParticles.Stop();
            pelletManager.EatPellet(spriteTransform.position);
        }
    }

    Vector3 GetNextPosition(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return transform.position + Vector3.up;
            case Direction.Down:
                return transform.position + Vector3.down;
            case Direction.Left:
                return transform.position + Vector3.left;
            case Direction.Right:
                return transform.position + Vector3.right;
            default:
                return transform.position;
        }
    }

    bool IsWalkable(Vector3 position)
{
    if (WallManager.instance.IsWall(position))
    {
        return false;
    }

    if (WallManager.instance.IsGhostWall(position))
    {
        return false;
    }

    return true;
}

    void UpdateAnimatorDirection(Direction direction)
    {
        int directionValue = 3;
        switch (direction)
        {
            case Direction.Up:
                directionValue = 0;
                break;
            case Direction.Down:
                directionValue = 1;
                break;
            case Direction.Left:
                directionValue = 2;
                break;
            case Direction.Right:
                directionValue = 3;
                break;
        }
        animator.SetInteger("Direction", directionValue);
    }

    public void ContinueTeleport(Transform destination)
    {
        Vector3 offset = destination.position - transform.position;
        transform.position += offset;
        startPosition += offset;
        targetPosition += offset;
    }
}