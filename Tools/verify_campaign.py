"""Offline scene placement/reference checks; does not run Unity physics or AI."""
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]
ASSETS = ROOT / 'Assets'
guid = re.search(r'guid: (\w+)', (ASSETS/'Scripts/Enemies/CampaignEnemies.cs.meta').read_text())[1]
for name in ('level1', 'level2'):
    text = (ASSETS/(name+'.unity')).read_text()
    entries = re.findall(r'--- !u!(\d+) &(\d+)\n(.*?)(?=--- !u!|\Z)', text, re.S)
    assert len({i for _, i, _ in entries}) == len(entries), 'Duplicate scene file IDs'
    blocks = {int(i):(int(t), b) for t, i, b in entries}
    transforms, by_go = {}, {}
    for i, (kind, b) in blocks.items():
        if kind != 4: continue
        go = int(re.search(r'm_GameObject: \{fileID: (\d+)', b)[1])
        position = tuple(map(float, re.search(r'm_LocalPosition: \{x: (.*?), y: (.*?), z: (.*?)\}', b).groups()))
        scale = tuple(map(float, re.search(r'm_LocalScale: \{x: (.*?), y: (.*?), z: (.*?)\}', b).groups()))
        parent = int(re.search(r'm_Father: \{fileID: (\d+)', b)[1])
        transforms[i] = (go, position, scale, parent)
        by_go[go] = i
    def world(i):
        _, p, s, parent = transforms[i]
        if not parent: return p, s
        pp, ss = world(parent)
        return tuple(pp[j]+p[j]*ss[j] for j in range(3)), tuple(s[j]*ss[j] for j in range(3))
    floors, obstacles = [], []
    for _, (kind, b) in blocks.items():
        if kind != 61: continue
        go = int(re.search(r'm_GameObject: \{fileID: (\d+)', b)[1])
        if go not in by_go: continue
        label = re.search(r'm_Name: (.*)', blocks[go][1])[1]
        if label not in ('GroundSegment', 'Platform', 'Spikes'): continue
        p, s = world(by_go[go])
        size = tuple(map(float, re.search(r'm_Size: \{x: (.*?), y: (.*?)\}', b).groups()))
        offset = tuple(map(float, re.search(r'm_Offset: \{x: (.*?), y: (.*?)\}', b).groups()))
        cx, cy = p[0]+offset[0]*s[0], p[1]+offset[1]*s[1]
        w, h = size[0]*abs(s[0]), size[1]*abs(s[1])
        rect = (cx-w/2, cy-h/2, cx+w/2, cy+h/2)
        (floors if label == 'GroundSegment' else obstacles).append(rect)
    installers = [b for t, b in blocks.values() if t == 114 and guid in b]
    assert len(installers) == 1, 'One campaign installer per scene'
    spawns = re.findall(r'- kind: (\d+)\n    position: \{x: (.*?), y: (.*?)\}\n    patrolRadius: (.*)', installers[0])
    assert sorted(int(k) for k, *_ in spawns) == [0, 1, 2], 'All three enemy types'
    for kind, x, y, radius in spawns:
        x, y, radius = float(x), float(y), float(radius)
        # Collider half-width=.325, half-height=.45, spawned .1 above the ground.
        assert any(x-radius-.325 >= l and x+radius+.325 <= r and abs(y-.45-top-.1) < .02
                   for l, bottom, r, top in floors), (name, kind, 'patrol route exceeds floor')
        assert not any(x+.325 > l and x-.325 < r and y+.45 > bottom and y-.45 < top
                       for l, bottom, r, top in obstacles), (name, kind, 'spawn overlaps platform/spikes')
        assert x > 5, 'Keep initial player spawn clear'
    assert f'  - {{fileID: 4200000002}}' in text.split('SceneRoots:')[1]
    print('PASS', name, ': three types, supported patrol routes, clear spawns, root registration, unique IDs')
for name in ('Patroller', 'Shooter', 'Charger'):
    for suffix in ('', 'Run'):
        assert (ASSETS/f'Resources/EnemyLab/{name}{suffix}.png').is_file()
        assert (ASSETS/f'Resources/EnemyLab/{name}{suffix}.png.meta').is_file()
print('PASS: all idle/walk sprite pairs and import metadata exist')
assert 'RunFlow.Continue' in (ASSETS/'Scripts/Mobile/Assignment11UI.cs').read_text()
assert 'case Command.Replay: RunFlow.NewGame();' in (ASSETS/'Scripts/Mobile/UIActionButton.cs').read_text()
print('PASS: data-campaign continue and explicit new-run replay routes')
print('Unity gameplay, animation appearance and phone tests remain required.')
