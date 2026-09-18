using System.Collections;
using UnityEngine;

public class PacStudentMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1.0f;
    [SerializeField] private AudioSource moveAudioSource;

    private Animator animator;
    private Vector3[] waypoints;

    private readonly string[] movementAnimations =
    {
        "Ship_Walk_Right",
        "Ship_Walk_Down",
        "Ship_Walk_Left",
        "Ship_Walk_Up"
    };

    private void Awake()
    {
        animator = GetComponent<Animator>();

        waypoints = new Vector3[]
        {
            new Vector3(-4.0f, 4.16f, -0.1f),
            new Vector3(-2.4f, 4.16f, -0.1f),
            new Vector3(-2.4f, 2.88f, -0.1f),
            new Vector3(-4.0f, 2.88f, -0.1f)
        };

        transform.position = waypoints[0];
    }

    private IEnumerator Start()
    {
        if (moveAudioSource != null)
        {
            moveAudioSource.loop = true;
            moveAudioSource.Play();
        }

        int currentPoint = 0;

        while (true)
        {
            int nextPoint = (currentPoint + 1) % waypoints.Length;

            animator.Play(
                movementAnimations[currentPoint],
                0,
                0f
            );

            yield return MoveLinearly(
                waypoints[currentPoint],
                waypoints[nextPoint]
            );

            currentPoint = nextPoint;
        }
    }

    private IEnumerator MoveLinearly(Vector3 start, Vector3 end)
    {
        float distance = Vector3.Distance(start, end);
        float duration = distance / Mathf.Max(moveSpeed, 0.01f);
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / duration);

            transform.position =
                Vector3.LerpUnclamped(start, end, progress);

            yield return null;
        }

        transform.position = end;
    }
}