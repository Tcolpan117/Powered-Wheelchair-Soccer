using System.Collections.Generic;
using UnityEngine;

// Personal bubble follows the chair. An optional existing Pen further limits detection.
[RequireComponent(typeof(Player))]
public class PlayerAwareness : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float radius = 4f;
    [SerializeField] private Pen pen;
    [SerializeField] private string ballId = "Ball";
    private readonly List<Player> nearbyPlayers = new();
    private Player self;
    private float nextScan;
    public IReadOnlyList<Player> NearbyPlayers => nearbyPlayers;
    public Ball Ball { get; private set; }
    public bool BallInBubble { get; private set; }

    private void Awake() => self = GetComponent<Player>();

    public void Refresh()
    {
        if (Time.time < nextScan) return;
        nextScan = Time.time + 0.1f;
        Ball = WorldRegister.TryGetItem(ballId, out RegistryItem item)
            && item.gameObject != null ? item.Get<Ball>() : null;
        BallInBubble = Ball != null && Contains(Ball.GetPosition());
        nearbyPlayers.Clear();
        foreach (Player player in FindObjectsByType<Player>(FindObjectsSortMode.None))
            if (player != self && player.isActiveAndEnabled && Contains(player.GetPosition()))
                nearbyPlayers.Add(player);
    }

    public bool Contains(Vector3 position)
    {
        Vector3 offset = position - transform.position;
        offset.y = 0f;
        if (offset.sqrMagnitude > radius * radius) return false;
        if (pen == null) return true;
        // GetBounds requires a Renderer on the same object as Pen.
        if (!pen.TryGetComponent<Renderer>(out var renderer) || !renderer.enabled) return false;
        return ContainsOnCourt(pen.GetBounds(), position);
    }

    public static bool ContainsOnCourt(Bounds bounds, Vector3 position)
    {
        position.y = bounds.center.y;
        return bounds.Contains(position);
    }

    public bool PlayerAhead(float distance)
    {
        Vector3 forward = transform.forward;
        forward.y = 0f;
        foreach (Player player in nearbyPlayers)
        {
            if (player == null) continue;
            Vector3 delta = player.GetPosition() - transform.position;
            delta.y = 0f;
            if (delta.sqrMagnitude < distance * distance &&
                (delta.sqrMagnitude < 0.001f || Vector3.Dot(forward.normalized, delta.normalized) > 0.5f))
                return true;
        }
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}