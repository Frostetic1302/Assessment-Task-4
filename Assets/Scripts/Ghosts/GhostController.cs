using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostController : MonoBehaviour
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    public enum GhostType
    {
        Ghost1,
        Ghost2,
        Ghost3,
        Ghost4
    }

    public GhostType ghostType;
    public float moveSpeed = 4.5f;
    private bool isMoving;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private Vector3 previousPosition;
    private float lerpValue;
    private PacStudentController player;
    private Animator animator;
    private Vector3 initialPosition;
    private bool isEaten;
    private Coroutine respawnCoroutine;
    private Direction currentDirection = Direction.Right;
    public Vector3[] teleporterEntranceTiles;
    public Transform[] ghost4TurnPoints;
    public Direction[] ghost4TurnDirections;
    public float adrenalineSpeedIncrease = 0.25f;
    private float normalMoveSpeed;

    private void Start()
    {
        normalMoveSpeed = moveSpeed;
        animator = GetComponent<Animator>();
        initialPosition = transform.position;
        player = FindAnyObjectByType<PacStudentController>();
        previousPosition = transform.position;
    }

    private void Update()
    {
        if (PacStudentController.isWaitingForInput)
        {
            return;
        }
        float currentSpeed = GetCurrentGhostSpeed();

        if (isEaten)
        {
            MoveDeadGhost(currentSpeed);
            return;
        }

        if (isMoving)
        {
            lerpValue += currentSpeed * Time.deltaTime;
            transform.position = Vector3.Lerp(startPosition, targetPosition, lerpValue);

            if (lerpValue >= 1f)
            {
                transform.position = targetPosition;
                previousPosition = startPosition;
                isMoving = false;
            }
        }
        else
        {
            ChooseNextMove();
        }

        float GetCurrentGhostSpeed()
        {
            if (isEaten || GhostManager.instance.GhostsAreScared() || GhostManager.instance.GhostsAreRecovering())
            {
                return moveSpeed * 0.5f;
            }
            return moveSpeed;
        }
    }

    void StartMove(Vector3 nextPosition)
    {
        startPosition = transform.position;
        targetPosition = nextPosition;
        lerpValue = 0f;
        isMoving = true;
        Vector3 dir = nextPosition - startPosition;
        if (dir.y > 0.1f) currentDirection = Direction.Up;
        else if (dir.y < -0.1f) currentDirection = Direction.Down;
        else if (dir.x > 0.1f) currentDirection = Direction.Right;
        else if (dir.x < -0.1f) currentDirection = Direction.Left;
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
        }
        return transform.position;
    }

    void CheckDirection(Direction direction, List<Direction> directions)
    {
        Vector3 nextPosition = GetNextPosition(direction);
        if (IsTeleporterEntrance(nextPosition))
        {
            return;
        }

        if (WallManager.instance.IsWall(nextPosition))
        {
            return;
        }

        if (!InsideSpawnArea(transform.position) && InsideSpawnArea(nextPosition))
        {
            return;
        }

        if (!InsideSpawnArea() && WallManager.instance.IsGhostWall(nextPosition))
        {
            return;
        }

        if (!InsideSpawnArea() && nextPosition == previousPosition)
        {
            return;
        }
        directions.Add(direction);
    }

    bool InsideSpawnArea()
    {
        return InsideSpawnArea(transform.position);
    }

    bool InsideSpawnArea(Vector3 pos)
    {
        return pos.x > 6.5f && pos.x < 15.0f && pos.y < -9.0f && pos.y > -15.0f;
    }

    void ChooseNextMove()
    {
        List<Direction> possibleDirections = new List<Direction>();

        CheckDirection(Direction.Up, possibleDirections);
        CheckDirection(Direction.Down, possibleDirections);
        CheckDirection(Direction.Left, possibleDirections);
        CheckDirection(Direction.Right, possibleDirections);

        if (possibleDirections.Count == 0)
        {
            StartMove(previousPosition);
            return;
        }

        Direction chosenDirection;

        if (InsideSpawnArea())
        {
            if (ghostType == GhostType.Ghost1 || ghostType == GhostType.Ghost3)
            {
                if (possibleDirections.Contains(Direction.Up))
                {
                    chosenDirection = Direction.Up;
                }
                else if (possibleDirections.Contains(Direction.Right))
                {
                    chosenDirection = Direction.Right;
                }
                else if (possibleDirections.Contains(Direction.Left))
                {
                    chosenDirection = Direction.Left;
                }
                else
                {
                    chosenDirection = possibleDirections[Random.Range(0, possibleDirections.Count)];
                }
            }
            else
            {
                if (possibleDirections.Contains(Direction.Down))
                {
                    chosenDirection = Direction.Down;
                }
                else if (possibleDirections.Contains(Direction.Left))
                {
                    chosenDirection = Direction.Left;
                }
                else if (possibleDirections.Contains(Direction.Right))
                {
                    chosenDirection = Direction.Right;
                }
                else
                {
                    chosenDirection = possibleDirections[Random.Range(0, possibleDirections.Count)];
                }
            }
        }
        else
        {
            if (GhostManager.instance.GhostsAreScared() || GhostManager.instance.GhostsAreRecovering())
            {
                chosenDirection = ChooseGhost1Direction(possibleDirections);
            }
            else
            {
                switch (ghostType)
                {
                    case GhostType.Ghost1:
                        chosenDirection = ChooseGhost1Direction(possibleDirections);
                        break;
                    case GhostType.Ghost2:
                        chosenDirection = ChooseGhost2Direction(possibleDirections);
                        break;
                    case GhostType.Ghost3:
                        chosenDirection = ChooseGhost3Direction(possibleDirections);
                        break;
                    default:
                        chosenDirection = ChooseGhost4Direction(possibleDirections);
                        break;
                }
            }
        }
        Vector3 nextPosition = GetNextPosition(chosenDirection);
        StartMove(nextPosition);
    }

    void MoveDeadGhost(float currentSpeed)
    {
        transform.position = Vector3.MoveTowards(transform.position, initialPosition, currentSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, initialPosition) < 0.1f)
        {
            transform.position = initialPosition;
            isEaten = false;
            isMoving = false;
            previousPosition = initialPosition;
            startPosition = initialPosition;
            targetPosition = initialPosition;
            UpdateGhostAnimation();
        }
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
        }
        else
        {
            GameManager.instance.LoseLife();
        }
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

    public void SetAdrenalineLevel(int level)
    {
        moveSpeed = normalMoveSpeed + (adrenalineSpeedIncrease * level);
    }

    bool IsTeleporterEntrance(Vector3 position)
    {
        foreach (Vector3 entrance in teleporterEntranceTiles)
        {
            if (Vector3.Distance(position, entrance) < 0.1f)
            {
                return true;
            }
        }

        return false;
    }

    public void ResetToInitialState()
    {
        if (respawnCoroutine != null)
        {
            StopCoroutine(respawnCoroutine);
            respawnCoroutine = null;
        }
        isEaten = false;
        isMoving = false;
        transform.position = initialPosition;
        previousPosition = initialPosition;
        startPosition = initialPosition;
        targetPosition = initialPosition;

        if (animator != null)
        {
            animator.ResetTrigger("Dead");
            animator.ResetTrigger("Scared");
            animator.ResetTrigger("Recovering");
            animator.ResetTrigger("Normal");
            animator.SetTrigger("Normal");
        }
    }

    bool NextToOuterWall(Vector3 position)
    {
        return WallManager.instance.IsOuterWall(position + Vector3.up) || WallManager.instance.IsOuterWall(position + Vector3.down) || WallManager.instance.IsOuterWall(position + Vector3.left) || WallManager.instance.IsOuterWall(position + Vector3.right);
    }

    Direction TurnLeft(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return Direction.Left;

            case Direction.Left:
                return Direction.Down;

            case Direction.Down:
                return Direction.Right;

            default:
                return Direction.Up;
        }
    }

    Direction TurnRight(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return Direction.Right;

            case Direction.Right:
                return Direction.Down;

            case Direction.Down:
                return Direction.Left;

            default:
                return Direction.Up;
        }
    }

    Direction ChooseGhost1Direction(List<Direction> directions)
    {
        float currentDistance = Vector3.Distance(transform.position, player.transform.position);
        List<Direction> validDirections = new List<Direction>();

        foreach (Direction direction in directions)
        {
            Vector3 nextPosition = GetNextPosition(direction);
            float nextDistance = Vector3.Distance(nextPosition, player.transform.position);

            if (nextDistance >= currentDistance)
            {
                validDirections.Add(direction);
            }
        }

        if (validDirections.Count == 0)
        {
            return directions[Random.Range(0, directions.Count)];
        }

        return validDirections[Random.Range(0, validDirections.Count)];
    }

    Direction ChooseGhost2Direction(List<Direction> directions)
    {
        float currentDistance = Vector3.Distance(transform.position, player.transform.position);
        List<Direction> validDirections = new List<Direction>();

        foreach (Direction direction in directions)
        {
            Vector3 nextPosition = GetNextPosition(direction);
            float nextDistance = Vector3.Distance(nextPosition, player.transform.position);

            if (nextDistance <= currentDistance)
            {
                validDirections.Add(direction);
            }
        }

        if (validDirections.Count == 0)
        {
            return directions[Random.Range(0, directions.Count)];
        }
        return validDirections[Random.Range(0, validDirections.Count)];
    }

    Direction ChooseGhost3Direction(List<Direction> directions)
    {
        int randomIndex = Random.Range(0, directions.Count);
        return directions[randomIndex];
    }

    Direction ChooseGhost4Direction(List<Direction> directions)
    {
        for (int i = 0; i < ghost4TurnPoints.Length; i++)
        {
            if (ghost4TurnPoints[i] == null)
            {
                continue;
            }

            if (Vector3.Distance(transform.position,
                ghost4TurnPoints[i].position) < 0.2f)
            {
                Direction wantedDirection = ghost4TurnDirections[i];

                if (directions.Contains(wantedDirection))
                {
                    return wantedDirection;
                }
            }
        }
        List<Direction> wallDirections = new List<Direction>();

        foreach (Direction direction in directions)
        {
            Vector3 nextPosition = GetNextPosition(direction);

            if (NextToOuterWall(nextPosition))
            {
                wallDirections.Add(direction);
            }
        }

        if (wallDirections.Count > 0)
        {
            Direction rightDirection = TurnRight(currentDirection);

            if (wallDirections.Contains(rightDirection))
            {
                return rightDirection;
            }

            if (wallDirections.Contains(currentDirection))
            {
                return currentDirection;
            }

            Direction leftDirection = TurnLeft(currentDirection);

            if (wallDirections.Contains(leftDirection))
            {
                return leftDirection;
            }

            return wallDirections[Random.Range(0, wallDirections.Count)];
        }
        return directions[Random.Range(0, directions.Count)];
    }
}