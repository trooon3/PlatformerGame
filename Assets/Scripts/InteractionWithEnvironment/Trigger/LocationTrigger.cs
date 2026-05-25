using UnityEngine;

public class LocationTrigger2D : MonoBehaviour, ILocationTrigger
{
    [SerializeField] private string locationName = "MiddleLevel";

    public string LocationName => locationName;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!enabled || !other.CompareTag("Player"))
        {
            return;
        }

        TriggerLocation();
    }

    public void TriggerLocation()
    {
        MiniMapController miniMap = FindObjectOfType<MiniMapController>();

        if (miniMap != null)
        {
            miniMap.SetMiniMap(locationName);
        }
    }

    private void OnDrawGizmos()
    {
        float gizmosDepth = 0.1f;
        float zOffset = 0f;

        if (!enabled)
        {
            return;
        }

        BoxCollider2D collider = GetComponent<BoxCollider2D>();

        if (collider == null)
        {
            return;
        }

        Gizmos.color = Color.green;

        Vector3 center = transform.position + new Vector3(collider.offset.x, collider.offset.y, zOffset);
        Vector3 size = new Vector3(collider.size.x, collider.size.y, gizmosDepth);

        Gizmos.DrawWireCube(center, size);
    }

    [ContextMenu("Test This 2D Trigger")]
    public void TestTrigger()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            Collider2D playerCollider = player.GetComponent<Collider2D>();

            if (playerCollider != null)
            {
                OnTriggerEnter2D(playerCollider);
            }
        }
    }
}