using UnityEngine;


[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [Header("Objetivo")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector2 offset = new Vector2(0f, 1f);

    [Header("Suavizado")]
    [SerializeField] private float smoothTime = 0.15f;

    [Header("Look Ahead (adelantarse hacia donde mira)")]
    [SerializeField] private bool useLookAhead = true;
    [SerializeField] private float lookAheadDistance = 2f;
    [SerializeField] private float lookAheadSmoothTime = 0.3f;

    [Header("Límites del nivel (opcional)")]
    [SerializeField] private bool useBounds = false;
    [SerializeField] private Vector2 minBounds = new Vector2(-20f, -10f);
    [SerializeField] private Vector2 maxBounds = new Vector2(20f, 10f);

    private Camera cam;
    private Vector3 velocity;
    private float lookAheadX;
    private float lookAheadVelocity;
    private float targetLookAhead;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        
        if (target != null)
            transform.position = GetDesiredPosition(0f);
    }

    private void LateUpdate()
    {
        if (target == null) return;

       
        if (useLookAhead)
            targetLookAhead = Mathf.Sign(target.localScale.x) * lookAheadDistance;

        lookAheadX = Mathf.SmoothDamp(lookAheadX, targetLookAhead, ref lookAheadVelocity, lookAheadSmoothTime);

        Vector3 desired = GetDesiredPosition(useLookAhead ? lookAheadX : 0f);
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
    }

    private Vector3 GetDesiredPosition(float lookAhead)
    {
        Vector3 pos = new Vector3(
            target.position.x + offset.x + lookAhead,
            target.position.y + offset.y,
            transform.position.z);

        if (useBounds)
            pos = ClampToBounds(pos);

        return pos;
    }

    private Vector3 ClampToBounds(Vector3 pos)
    {
       
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        float minX = minBounds.x + halfWidth;
        float maxX = maxBounds.x - halfWidth;
        float minY = minBounds.y + halfHeight;
        float maxY = maxBounds.y - halfHeight;

        
        pos.x = minX > maxX ? (minBounds.x + maxBounds.x) * 0.5f : Mathf.Clamp(pos.x, minX, maxX);
        pos.y = minY > maxY ? (minBounds.y + maxBounds.y) * 0.5f : Mathf.Clamp(pos.y, minY, maxY);

        return pos;
    }

    private void OnDrawGizmosSelected()
    {
        if (!useBounds) return;

        Gizmos.color = Color.yellow;
        Vector3 center = (minBounds + maxBounds) * 0.5f;
        Vector3 size = maxBounds - minBounds;
        Gizmos.DrawWireCube(center, size);
    }
}
