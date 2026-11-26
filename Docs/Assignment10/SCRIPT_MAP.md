# Script map

```mermaid
flowchart TD
    T["EnemyTuning assets"] --> B["EnemyBrain2D: sight, health, movement"]
    H["Player: Health"] --> B
    L["EnemyLabBootstrap: optional arena"] --> F["EnemySpawner2D"]
    M["CampaignEnemies: level1 and level2 spawns"] --> F
    F --> B
    F --> V["EnemyVisual2D: distance-driven sprite animation"]
    B --> P["PatrollerBrain: Patrol / Chase / WindUp / Recover"]
    B --> S["ShooterBrain: Idle / Aim / Shoot / Cooldown"]
    B --> C["ChargerBrain: Patrol / WindUp / Charge / Recover"]
    S --> Q["EnemyShot2D: swept collision"]
    P --> D["Health.TakeDamage"]
    C --> D
    Q --> D
    U["PlayerBullet -> IDamageable.TakeHit"] --> B
```

The base shares mechanics, while each subclass owns its own transitions and attack behaviour. EnemyTuning provides persistent Inspector values. CampaignEnemies adds all three types to each normal level and removes legacy patrol duplicates; original scene content stays intact. EnemySpawner2D shares construction with the optional lab, which switches to level1 after clearing. The lab displays state labels for the video. Campaign enemies use the same red warning tint without overhead debug labels.
