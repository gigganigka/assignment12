using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
[DefaultExecutionOrder(-1000)]
public sealed class DataLevelController : MonoBehaviour
{
    public static DataLevelController Instance { get; private set; }
    public LevelData Level { get; private set; }
    public float TimeLeft { get; private set; }
    public string GoalStatus { get; private set; }
    PlayerController hero;
    Health health;
    EnemyBrain2D[] enemies;
    Boss boss;
    Sprite ground;
    Material unlit;
    bool completing;
    SpriteRenderer exitVisual;
    void Awake()
    {
        Instance=this;
        SaveData d=SaveService.Data;
        if (!d.runActive || d.currentHp<=0) RunFlow.PrepareAttempt(d.currentLevel,GameSettings.playerMaxHp,d.levelEntryScore);
        Level=GameCatalog.Current.Level(d.currentLevel);
        hero=FindFirstObjectByType<PlayerController>();
        if (hero==null || Level==null) { Debug.LogError("Campaign requires a player and valid LevelData."); enabled=false; return; }
        health=hero.GetComponent<Health>();
        foreach (GameObject root in gameObject.scene.GetRootGameObjects())
        {
            if (root==gameObject || Keep(root)) continue;
            root.SetActive(false); Destroy(root);
        }
        Shader shader=Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (shader==null) shader=Shader.Find("Sprites/Default");
        if (shader!=null) unlit=new Material(shader);
        Sprite source=MobileSprites.Load("Mobile/Ground");
        ground=Sprite.Create(source.texture,source.rect,new Vector2(0.5f,0.5f),source.pixelsPerUnit,0,SpriteMeshType.FullRect);
        bool resume=d.hasCheckpoint && d.collectedCoins.Length==Level.coins.Length &&
                    d.collectedHearts.Length==Level.hearts.Length && d.enemyHp.Length==Level.enemies.Length+(Level.hasBoss?1:0) && d.enemyX.Length==d.enemyHp.Length && d.enemyY.Length==d.enemyHp.Length;
        if (!resume)
        {
            d.collectedCoins=new int[Level.coins.Length]; d.collectedHearts=new int[Level.hearts.Length];
            d.enemyHp=new int[Level.enemies.Length+(Level.hasBoss?1:0)];
            d.enemyX=new float[d.enemyHp.Length]; d.enemyY=new float[d.enemyHp.Length];
            for(int i=0;i<Level.enemies.Length;i++)
            {
                d.enemyHp[i]=Level.enemies[i].stats.hitPoints;
                d.enemyX[i]=Level.enemies[i].position.x;d.enemyY[i]=Level.enemies[i].position.y;
            }
            if(Level.hasBoss)
            {
                int last=d.enemyHp.Length-1; d.enemyHp[last]=GameCatalog.Current.boss.maxHp;
                d.enemyX[last]=Level.bossPosition.x;d.enemyY[last]=Level.bossPosition.y;
            }
            d.playerX=Level.playerStart.x; d.playerY=Level.playerStart.y; d.playerVelocityY=0; d.timeLeft=Level.timeLimit;
        }
        hero.transform.position=new Vector2(d.playerX,d.playerY);
        hero.GetComponent<Rigidbody2D>().linearVelocity=new Vector2(0,d.playerVelocityY);
        Camera camera=Camera.main;
        if(camera!=null) { CameraFollow follow=camera.GetComponent<CameraFollow>(); if(follow!=null) camera.transform.position=hero.transform.position+follow.offset; }
        TimeLeft=d.timeLeft;
        foreach(LevelData.Surface surface in Level.surfaces) MakeSurface(surface);
        for(int i=0;i<Level.coins.Length;i++) if(d.collectedCoins[i]==0) MakePickup(Level.coins[i],i,false);
        for(int i=0;i<Level.hearts.Length;i++) if(d.collectedHearts[i]==0) MakePickup(Level.hearts[i],i,true);
        foreach(Vector2 position in Level.hazards) MakeHazard(position);
        enemies=new EnemyBrain2D[Level.enemies.Length];
        if(GameSettings.enemiesEnabled)
        {
            for(int i=0;i<enemies.Length;i++)
            {
                if(d.enemyHp[i]<=0) continue;
                LevelData.EnemySpawn s=Level.enemies[i];
                enemies[i]=EnemySpawner2D.Spawn(s.kind,s.position,s.patrolRadius,transform,s.stats);
                enemies[i].RestoreHealth(d.enemyHp[i]);
                enemies[i].GetComponent<Rigidbody2D>().position=new Vector2(d.enemyX[i],d.enemyY[i]);
            }
            if(Level.hasBoss && d.enemyHp[d.enemyHp.Length-1]>0) MakeBoss(d.enemyHp[d.enemyHp.Length-1]);
        }
        GameObject goal=Visual("EXIT",Level.exit,MobileSprites.Load("Mobile/Circle"),1f,Color.green);
        BoxCollider2D box=goal.AddComponent<BoxCollider2D>(); box.isTrigger=true;
        box.size=new Vector2(1f/goal.transform.localScale.x,2f/goal.transform.localScale.y);
        goal.AddComponent<DataExit>().owner=this; exitVisual=goal.GetComponent<SpriteRenderer>();
        LevelManager manager=FindFirstObjectByType<LevelManager>();
        if(manager!=null) manager.coinsRequired=Level.coinsRequired;
        Physics2D.SyncTransforms();
    }
    bool Keep(GameObject root)
    {
        return root.GetComponent<PlayerController>()!=null || root.GetComponent<Camera>()!=null ||
               root.GetComponent<Canvas>()!=null || root.GetComponent<EventSystem>()!=null ||
               root.GetComponent<Music>()!=null || root.GetComponent<MobileGameUI>()!=null ||
               root.GetComponent<DeathScreen>()!=null || root.GetComponent<LevelManager>()!=null;
    }
    void Start() { CaptureCheckpoint(); SaveService.Write(); }
    void Update()
    {
        if(hero==null || health.CurrentHp<=0 || Time.timeScale<=0 || completing) return;
        TimeLeft=Mathf.Max(0,TimeLeft-Time.deltaTime);
        if(hero.transform.position.y<Level.fallY || TimeLeft<=0) { health.Die(); return; }
        int coins=SaveService.Data.collectedCoins.Count(x=>x!=0);
        int remaining=enemies.Count(e=>e!=null)+(boss!=null?1:0);
        bool open=coins>=Level.coinsRequired && (!Level.defeatAllEnemies || remaining==0);
        GoalStatus="L"+Level.id+"  "+Mathf.CeilToInt(TimeLeft)+"s  SCORE "+SaveService.Data.runScore;
        LevelManager manager=LevelManager.Instance;
        if(manager!=null && manager.coinText!=null)
            manager.coinText.text="COINS "+coins+" / "+Level.coinsRequired+(Level.defeatAllEnemies?"   ENEMIES "+remaining:"");
        if(exitVisual!=null) exitVisual.color=open?Color.green:Color.gray;
    }
    public void CaptureCheckpoint()
    {
        SaveData d=SaveService.Data;
        if(health==null || Level==null || d.currentLevel!=Level.id || !d.runActive || completing) return;
        d.currentHp=health.CurrentHp;
        Vector2 position=hero.transform.position;
        // Dead/falling attempts restart explicitly rather than saving an invalid location.
        d.playerX=position.x; d.playerY=position.y; d.playerVelocityY=hero.GetComponent<Rigidbody2D>().linearVelocity.y;
        d.timeLeft=TimeLeft; d.hasCheckpoint=health.CurrentHp>0 && position.y>=Level.fallY;
        for(int i=0;i<enemies.Length;i++)
        {
            d.enemyHp[i]=enemies[i]!=null?enemies[i].CurrentHp:0;
            if(enemies[i]!=null) { d.enemyX[i]=enemies[i].transform.position.x; d.enemyY[i]=enemies[i].transform.position.y; }
        }
        if(Level.hasBoss) d.enemyHp[d.enemyHp.Length-1]=boss!=null?boss.CurrentHp:0;
    }
    public void Collect(int index,bool heart,Health hp)
    {
        SaveData d=SaveService.Data;
        if(heart) { d.collectedHearts[index]=1; hp.Heal(GameCatalog.Current.items.healing); }
        else { d.collectedCoins[index]=1; d.runScore+=GameCatalog.Current.items.coinScore; }
        CaptureCheckpoint(); SaveService.Write();
    }
    public void TryComplete()
    {
        if(completing || health==null || health.CurrentHp<=0) return;
        int coins=SaveService.Data.collectedCoins.Count(x=>x!=0);
        if(coins<Level.coinsRequired || (Level.defeatAllEnemies && (enemies.Any(e=>e!=null) || boss!=null))) return;
        completing=true;
        RunFlow.Complete(Level,health.CurrentHp,TimeLeft);
    }
    GameObject Visual(string title,Vector2 position,Sprite sprite,float height,Color color)
    {
        GameObject go=new GameObject(title); go.transform.SetParent(transform); go.transform.position=position;
        SpriteRenderer r=go.AddComponent<SpriteRenderer>(); r.sprite=sprite; r.color=color; r.sortingOrder=3;
        if(unlit!=null) r.sharedMaterial=unlit;
        if(sprite!=null) go.transform.localScale=Vector3.one*(height/sprite.bounds.size.y);
        return go;
    }
    void MakeSurface(LevelData.Surface surface)
    {
        GameObject go=new GameObject(surface.oneWay?"Floating platform":"Ground");
        go.transform.SetParent(transform); go.transform.position=surface.center; go.layer=6;
        BoxCollider2D box=go.AddComponent<BoxCollider2D>(); box.size=surface.size;
        GameObject art=new GameObject("Tiles"); art.transform.SetParent(go.transform,false);
        SpriteRenderer r=art.AddComponent<SpriteRenderer>(); r.sprite=ground; r.sortingOrder=0;
        if(unlit!=null) r.sharedMaterial=unlit;
        float scale=0.6f/ground.bounds.size.x;
        art.transform.localScale=Vector3.one*scale; r.drawMode=SpriteDrawMode.Tiled; r.size=surface.size/scale;
        if(surface.oneWay) go.AddComponent<OneWayPlatform>();
    }
    void MakePickup(Vector2 position,int index,bool heart)
    {
        ItemStats stats=GameCatalog.Current.items;
        GameObject go=Visual(heart?"Heart":"Coin",position,heart?MobileSprites.Load("Mobile/Heart"):GameCatalog.Current.coinSprite,stats.pickupHeight,Color.white);
        CircleCollider2D c=go.AddComponent<CircleCollider2D>(); c.isTrigger=true; c.radius=stats.pickupRadius/go.transform.localScale.x;
        DataPickup pickup=go.AddComponent<DataPickup>(); pickup.owner=this; pickup.index=index; pickup.heart=heart;
    }
    void MakeHazard(Vector2 position)
    {
        GameObject go=Visual("Spikes",position,MobileSprites.Load("Data/Spikes"),Level.hazardSize.y,Color.white);
        BoxCollider2D c=go.AddComponent<BoxCollider2D>(); c.isTrigger=true; c.size=Level.hazardSize/(Vector2)go.transform.localScale;
        go.AddComponent<Hazard>();
    }
    void MakeBoss(int hp)
    {
        GameObject go=new GameObject("Boss"); go.SetActive(false); go.transform.SetParent(transform); go.transform.position=Level.bossPosition;
        SpriteRenderer r=go.AddComponent<SpriteRenderer>(); r.sprite=GameCatalog.Current.bossSprite; r.sortingOrder=5;
        if(unlit!=null) r.sharedMaterial=unlit;
        go.transform.localScale=new Vector3(Level.bossSize.x/r.sprite.bounds.size.x,Level.bossSize.y/r.sprite.bounds.size.y,1);
        BoxCollider2D box=go.AddComponent<BoxCollider2D>(); box.size=Level.bossSize/(Vector2)go.transform.localScale;
        boss=go.AddComponent<Boss>(); boss.fireballPrefab=GameCatalog.Current.fireballPrefab; boss.player=hero.transform;
        GameObject labelObject=new GameObject("Boss health");
        labelObject.transform.position=Level.bossPosition+Vector2.up*(Level.bossSize.y/2+0.6f);
        labelObject.transform.SetParent(go.transform,true);
        TextMeshPro label=labelObject.AddComponent<TextMeshPro>();
        label.font=Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        label.fontSize=3; label.alignment=TextAlignmentOptions.Center;
        label.rectTransform.sizeDelta=new Vector2(5,1); label.sortingOrder=15;
        boss.hpLabel=label;
        go.SetActive(true); boss.RestoreHealth(hp);
    }
    void OnDestroy()
    {
        if(Instance==this) Instance=null;
        if(ground!=null) Destroy(ground);
        if(unlit!=null) Destroy(unlit);
    }
}
