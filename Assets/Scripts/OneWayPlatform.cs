using UnityEngine;

// Only attached to floating Platform roots, never to the terrain or hazards.
[RequireComponent(typeof(BoxCollider2D))]
public sealed class OneWayPlatform : MonoBehaviour
{
    void Awake()
    {
        BoxCollider2D platform = GetComponent<BoxCollider2D>();
        PlatformEffector2D effector = GetComponent<PlatformEffector2D>();
        if (effector == null) effector = gameObject.AddComponent<PlatformEffector2D>();
        effector.useOneWay = true;
        effector.useOneWayGrouping = true;
        effector.useColliderMask = false;
        effector.surfaceArc = 160f;
        effector.useSideFriction = false;
        effector.useSideBounce = false;
        platform.usedByEffector = true;
    }
}
