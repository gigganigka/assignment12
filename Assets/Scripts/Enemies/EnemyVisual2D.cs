using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D))]
public sealed class EnemyVisual2D : MonoBehaviour
{
    public Sprite idleSprite;
    public Sprite walkSprite;
    [Min(1)] public float framesPerUnit = 3f;
    SpriteRenderer visual;
    Rigidbody2D body;
    Vector2 previousPosition;
    float phase;
    void Awake()
    {
        visual = GetComponent<SpriteRenderer>();
        body = GetComponent<Rigidbody2D>();
        previousPosition = body.position;
    }
    void LateUpdate()
    {
        if (Time.timeScale <= 0 || idleSprite == null) return;
        float travelled = Mathf.Abs(body.position.x - previousPosition.x);
        previousPosition = body.position;
        // Distance drives the gait: a faster chase animates faster, a wall stops it.
        if (travelled > 0.0001f && Mathf.Abs(body.linearVelocity.x) > 0.05f)
            phase = Mathf.Repeat(phase + travelled * framesPerUnit, 2f);
        else phase = 0;
        visual.sprite = walkSprite != null && phase >= 1f ? walkSprite : idleSprite;
        // Facing and warning/hit colours belong to the brain and are preserved.
    }
}
