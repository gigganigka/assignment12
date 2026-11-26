"""Offline data/reference/geometry checks. Does not execute Unity gameplay."""
from pathlib import Path
import math,re,yaml
ROOT=Path(__file__).resolve().parents[1]
ASSETS=ROOT/'Assets'
def read_asset(path):
    text=path.read_text()
    text=re.sub(r'^%.*\n','',text,flags=re.M)
    text=re.sub(r'--- !u!\d+ &\d+','---',text)
    return yaml.safe_load(text)['MonoBehaviour']
DATA=ASSETS/'Resources/Data'
ids={}
for p in ASSETS.rglob('*.meta'):
    match=re.search(r'^guid: (\w+)',p.read_text(),re.M)
    if match:
        assert match[1] not in ids,(p,ids.get(match[1]))
        ids[match[1]]=p
print('PASS',len(ids),'unique asset GUIDs')
for p in DATA.glob('*.asset'):
    text=p.read_text()
    for guid in re.findall(r'guid: (\w+)',text):assert guid in ids,(p,guid)
    d=read_asset(p)
    assert d['m_Script']['guid'] in ids
print('PASS all data-asset script, art, projectile and cross-asset references')
player=read_asset(DATA/'PlayerStats.asset');items=read_asset(DATA/'ItemStats.asset')
assert player['maxHp']>=1 and player['blinkInterval']>0 and player['bulletDamage']>0
v=player['jumpVelocity'];g=9.81*player['gravityScale'];speed=player['moveSpeed'];dt=.02
height=0;velocity=v;apex=0
while velocity>0:
    velocity-=g*dt;height+=velocity*dt;apex=max(apex,height)
for index in (1,2,3):
    p=DATA/f'Level_{index:02}.asset';level=read_asset(p)
    assert level['id']==index and level['timeLimit']>0
    assert level['coinsRequired']<=len(level['coins'])
    surfaces=[]
    for f in level['surfaces']:
        c,s=f['center'],f['size'];assert s['x']>0 and s['y']>0
        surfaces.append((c['x']-s['x']/2,c['x']+s['x']/2,c['y']+s['y']/2))
    reachable={i for i,(left,right,top) in enumerate(surfaces) if left<=level['playerStart']['x']<=right and top<=level['playerStart']['y']}
    for _ in range(len(surfaces)):
        for i,(left,right,top) in enumerate(surfaces):
            for j in tuple(reachable):
                l,r,y=surfaces[j];dy=top-y
                if dy>apex-.05:continue
                discriminant=v*v-2*g*dy
                if discriminant<0:continue
                duration=(v+math.sqrt(discriminant))/g-dt
                gap=max(left-r,l-right,0)
                if gap<=duration*speed-.05:reachable.add(i)
    assert len(reachable)==len(surfaces),(p,'unreachable surfaces',set(range(len(surfaces)))-reachable)
    for coin in level['coins']:
        assert any(left<=coin['x']<=right and top<=coin['y']<=top+apex+.6+items['pickupRadius']
                   for left,right,top in surfaces),(p,'unreachable coin',coin)
    for spawn in level['enemies']:
        stats_path=Path(str(ids[spawn['stats']['guid']])[:-5]);stats=read_asset(stats_path)
        assert stats['hitPoints']>0 and .3<=stats['windUp']<=.8 and stats['cooldown']>0
        x,y=spawn['position']['x'],spawn['position']['y'];radius=spawn['patrolRadius'];half=stats['colliderSize']['x']/2
        assert any(x-radius-half>=left and x+radius+half<=right and abs(y-stats['colliderSize']['y']/2-top-.1)<.05
                   for left,right,top in surfaces),(p,'unsupported enemy route',spawn)
    assert any(l<=level['exit']['x']<=r for l,r,_ in surfaces)
    print('PASS Level',index,': asset, goals, supported enemy routes, connected surfaces, reachable coins')
assert (ASSETS/'campaign.unity').exists()
assert 'Assets/campaign.unity' in (ROOT/'ProjectSettings/EditorBuildSettings.asset').read_text()
assert 'Assets/campaign.unity' in (ASSETS/'Editor/MobileProjectSetup.cs').read_text()
scene=(ASSETS/'campaign.unity').read_text();components=re.findall(r'^--- !u!\d+ &(\d+)',scene,re.M)
assert len(components)==len(set(components))
guid=re.search(r'guid: (\w+)',(ASSETS/'Scripts/Data/DataLevelController.cs.meta').read_text())[1]
assert guid in scene and '  - {fileID: 4500000001}' in scene.split('SceneRoots:')[1]
assert 'SceneNames.Campaign' in (ASSETS/'Scripts/Mobile/MobileGameUI.cs').read_text()
print('PASS shared scene, root identity and both build lists')
for name in ['PlayerStats','ItemStats','BossStats','AppDefaults']:
    s=(ASSETS/f'Scripts/Data/{name}.cs').read_text()
    assert not re.search(r'public (?:float|int) \w+\s*=\s*[\d.]',s),name
for name in ['PlayerController','Health','PlayerBullet','Projectile','Hazard','GameSettings']:
    s=(ASSETS/f'Scripts/{name}.cs').read_text()
    assert not re.search(r'public (?:float|int) \w+\s*=\s*[\d.]',s),name
print('PASS core balance defaults removed from gameplay scripts; values resolve through assets')
assert 'Application.persistentDataPath' in (ASSETS/'Scripts/Saves/SaveService.cs').read_text()
assert not any('BinaryFormatter' in p.read_text() for p in (ASSETS/'Scripts').rglob('*.cs'))
assert 'if(paused) Save();' in (ASSETS/'Scripts/Saves/AppSaveLifecycle.cs').read_text()
print('PASS JSON save path, pause hook and no BinaryFormatter')
print('Jump apex estimate:',round(apex,3),'units with standard gravity; Unity physics/device tests remain pending.')
