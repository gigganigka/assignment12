"""Offline integrity/signal tests, not Unity compilation or an ears/device test."""
from pathlib import Path
import re, wave, math, hashlib
import numpy as np
r=Path(__file__).resolve().parents[1];a=r/'Assets';audio=a/'Resources/GameAudio'
checks=[]
def check(ok,msg):
 assert ok,msg
 checks.append(msg)
def read(p):return (r/p).read_text()
# Mixer object references, hierarchy, snapshots and independent pause parameters.
mix=(audio/'Coinbound.mixer').read_text()
blocks={int(i):body for i,body in re.findall(r'--- !u!\d+ &(-?\d+)\n(.*?)(?=--- !u!|\Z)',mix,re.S)}
check(all(int(i) in blocks for i in re.findall(r'fileID: (\d+)',mix) if i!='0'),'All internal mixer object references resolve')
for name in ['Master','Music','SFX','PauseDuck','Normal','Paused']:check('m_Name: '+name+'\n' in mix,'Mixer object '+name)
check('m_EffectName: Lowpass' in mix and 'm_ParameterName: Cutoff freq' in mix,'Native lowpass effect present')
exposed=dict(re.findall(r'- guid: (\w+)\n    name: (\w+)',mix))
check(set(exposed.values())=={'MasterDb','MusicDb','SfxDb','PauseDuckDb'},'Exposed parameters separate user gain and pause duck')
for key,val in [('masterGroup',24300000)]:check(f'm_MasterGroup: {{fileID: {val}}}' in mix,'Master root')
check('fileID: 24300003' in blocks[24300001],'Music contains PauseDuck')
check('fileID: 24300001' in blocks[24300000] and 'fileID: 24300002' in blocks[24300000],'Master contains Music and SFX')
check('fileID: 24400010' in blocks[24300003],'Lowpass lives on music pause bus only')
cutoff=re.search(r'm_ParameterName: Cutoff freq\n    m_GUID: (\w+)',mix)[1]
duck=next(g for g,n in exposed.items() if n=='PauseDuckDb')
check(f'{cutoff}: 1600' in blocks[24500001] and f'{duck}: -12' in blocks[24500001],'Paused snapshot: 1600 Hz and -12 dB')
check(f'{cutoff}: 22000' in blocks[24500000] and f'{duck}: 0' in blocks[24500000],'Normal snapshot restores cutoff and gain')
bank=(audio/'SoundBank.asset').read_text();ids=re.findall(r'- id: (\d+)',bank)
check(sorted(map(int,ids))==list(range(19)),'19 unique event IDs configured')
meta={re.search(r'^guid: (\w+)',p.read_text(),re.M)[1]:p for p in a.rglob('*.meta') if re.search(r'^guid: (\w+)',p.read_text(),re.M)}
for guid in re.findall(r'guid: (\w+)',bank):check(guid in meta,'SoundBank reference resolves: '+str(meta.get(guid,'missing')).split('/')[-1])
for p in audio.glob('*.wav'):
 with wave.open(str(p),'rb') as w:
  check(w.getnchannels()==1 and w.getsampwidth()==2 and w.getframerate()==32000,p.stem+': supported mono PCM')
  data=np.frombuffer(w.readframes(w.getnframes()),dtype='<i2').astype(float)/32768
 check(len(data)>0 and 0<np.max(np.abs(data))<.8,p.stem+': audible signal, sample headroom')
 check(data[0]==data[-1]==0,p.stem+': zero-valued endpoints')
 check(abs(np.mean(data))<.01,p.stem+': no material DC offset')
 check(np.max(np.abs(np.diff(data[:32])))<.01 and np.max(np.abs(np.diff(data[-32:])))<.01,p.stem+': softened boundaries')
# A genuine lowpass fallback should reduce treble, not merely rename the WAV.
for name in ['menu_music','level_music']:
 def signal(n):
  with wave.open(str(audio/(n+'.wav')),'rb') as w:return np.frombuffer(w.readframes(w.getnframes()),dtype='<i2').astype(float)
 x,y=signal(name),signal(name+'_lowpass');check(len(x)==len(y),name+': aligned Web loop length')
 def ratio(s):
  spec=np.abs(np.fft.rfft(s))**2;f=np.fft.rfftfreq(len(s),1/32000)
  return spec[f>4000].sum()/spec.sum()
 check(ratio(y)<ratio(x)*.5,name+': Web copy attenuates >4 kHz energy')
check(hashlib.sha256((audio/'menu_music.wav').read_bytes()).digest()!=hashlib.sha256((audio/'level_music.wav').read_bytes()).digest(),'Distinct music compositions')
manager=read('Assets/Scripts/Audio/SoundPass.cs');ui=read('Assets/Scripts/Mobile/Assignment11UI.cs')
check('Mathf.Log10(Mathf.Clamp01(linear)) * 20f' in manager,'Linear slider mapped to decibels')
check('if (targetGain[index] == 1f) return;' in manager,'Same track does not restart on level reload')
check('Time.unscaledDeltaTime' in manager and 'AudioMixerUpdateMode.UnscaledTime' in manager,'Fades and snapshots continue at timeScale zero')
check('Random.Range(0.92f, 1.08f)' in manager,'Repeated cue pitch variation')
check('s.outputAudioMixerGroup = group' in manager,'Every new AudioSource routed')
check('MakeSlider' in ui and 'onValueChanged.AddListener' in ui and 'SaveService.Write()' in ui,'Touch volume sliders use existing save system')
for name,state,cue in [('PatrollerBrain','WindUp','PatrollerWindUp'),('ShooterBrain','Aim','ShooterWindUp'),('ChargerBrain','WindUp','ChargerWindUp')]:
 s=read('Assets/Scripts/Enemies/'+name+'.cs')
 check(f'if (next == State.{state}) Sfx.Play(SoundEvent.{cue});' in s,name+': sound at wind-up entry')
check('Sfx.Play(SoundEvent.BossFanWindUp)' in read('Assets/Scripts/Boss.cs') and 'Sfx.Play(SoundEvent.BossRainWindUp)' in read('Assets/Scripts/Boss.cs'),'Both boss attacks have cues')
check('AudioListener.pause = true' not in read('Assets/Scripts/Mobile/MobileGameUI.cs'),'Pause does not silence music')
print('PASS',len(checks),'sound asset, signal and source-structure checks')
print('Pending: Unity semantic compilation/import, ears test, screenshot, iPhone playback/restart and real video.')
