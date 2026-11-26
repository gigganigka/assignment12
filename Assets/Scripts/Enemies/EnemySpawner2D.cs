using UnityEngine;

public static class EnemySpawner2D
{
    public enum Kind { Patroller, Shooter, Charger }
    public static EnemyBrain2D Spawn(Kind kind, Vector2 position, float radius, Transform parent, EnemyTuning statsOverride = null)
    {
        EnemyTuning stats=statsOverride!=null?statsOverride:GameCatalog.Current.enemyStats[(int)kind];
        string spritePath = "EnemyLab/" + kind;
        GameObject go = new GameObject(kind.ToString());
        go.SetActive(false);
        go.transform.SetParent(parent);
        go.transform.position = position;
        go.layer = 7;
        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = MobileSprites.Load(spritePath);
        renderer.sortingOrder = 5;
        Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            Material material = new Material(shader);
            renderer.sharedMaterial = material;
            go.AddComponent<EnemyLabMaterial>().owned = material;
        }
        if (renderer.sprite != null)
            go.transform.localScale = Vector3.one * stats.artHeight / Mathf.Max(renderer.sprite.bounds.size.y, 0.01f);
        BoxCollider2D box = go.AddComponent<BoxCollider2D>();
        Vector3 scale = go.transform.lossyScale;
        box.size = new Vector2(stats.colliderSize.x / scale.x, stats.colliderSize.y / scale.y);
        Rigidbody2D body = go.AddComponent<Rigidbody2D>();
        body.gravityScale = stats.gravityScale;
        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        EnemyBrain2D brain;
        switch (kind)
        {
            case Kind.Shooter: brain = go.AddComponent<ShooterBrain>(); break;
            case Kind.Charger: brain = go.AddComponent<ChargerBrain>(); break;
            default: brain = go.AddComponent<PatrollerBrain>(); break;
        }
        brain.tuning = stats;
        EnemyVisual2D animation = go.AddComponent<EnemyVisual2D>();
        animation.idleSprite = renderer.sprite;
        animation.walkSprite = MobileSprites.Load(spritePath + "Run");
        Transform[] points = new Transform[2];
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = new GameObject(kind + " waypoint " + i).transform;
            points[i].SetParent(parent);
            points[i].position = position + Vector2.right * radius * (i == 0 ? -1 : 1);
        }
        brain.SetWaypoints(points);
        go.SetActive(true);
        return brain;
    }
}
