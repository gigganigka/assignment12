"""Original procedural Coinbound audio. Python 3 + numpy. No external samples.
Generated PCM and the note sequences below are dedicated to CC0-1.0.
"""
from pathlib import Path
import wave, numpy as np
ROOT=Path(__file__).resolve().parents[1]
DEST=ROOT/'Assets/Resources/GameAudio'
SR=32000
rng=np.random.default_rng(12026)
def envelope(n,attack=.005,release=.025):
 e=np.ones(n);a=min(int(attack*SR),n//2);r=min(int(release*SR),n//2)
 if a:e[:a]=np.linspace(0,1,a)**2
 if r:e[-r:]=np.linspace(1,0,r)**2
 return e
def tone(freq,seconds,kind='soft'):
 t=np.arange(round(seconds*SR))/SR
 phase=np.cumsum(np.broadcast_to(freq,t.shape))/SR*2*np.pi
 x=np.sin(phase)
 if kind=='bell':x+=.25*np.sin(phase*2)+.12*np.sin(phase*3)
 if kind=='buzz':x+=.3*np.sin(phase*3)+.15*np.sin(phase*5)
 return x*envelope(len(t))*np.exp(-t*2.5)
def sweep(a,b,d,noise=0):
 n=round(d*SR);f=np.linspace(a,b,n);x=tone(f,d,'buzz')
 return x+noise*rng.uniform(-1,1,n)*envelope(n)*np.exp(-np.arange(n)/SR*14)
def notes(seq,step=.105):
 return np.concatenate([tone(f,step,'bell') for f in seq])
def write(name,x,peak=.7):
 x=np.asarray(x);x-=x.mean();x*=envelope(len(x),.002,.01)
 x=x/max(np.max(np.abs(x)),1e-12)*peak;x[0]=x[-1]=0
 with wave.open(str(DEST/(name+'.wav')),'wb') as w:
  w.setnchannels(1);w.setsampwidth(2);w.setframerate(SR);w.writeframes(np.round(x*32767).astype('<i2').tobytes())
 return x

def main():
 DEST.mkdir(parents=True,exist_ok=True)
 sounds={
 'jump':sweep(280,750,.16),'hurt':sweep(190,65,.22,.45),
 'coin':notes([1046.5,1568],.08),'heal':notes([523.25,659.25,783.99],.10),
 'shoot':sweep(1100,180,.095,.05),'patroller_windup':notes([220,330,440],.09),
 'shooter_windup':notes([740,0,988],.10),'charger_windup':sweep(100,340,.34,.10),
 'boss_fan_windup':notes([196,246.94,293.66,392],.11),
 'boss_rain_windup':notes([880,659.25,880,659.25],.12),
 'enemy_shot':sweep(560,110,.17,.2),'enemy_strike':sweep(300,70,.13,.4),
 'enemy_hit':sweep(320,110,.11,.18),'enemy_death':notes([392,261.63,130.81],.10),
 'death':sweep(420,48,.42,.08),'ui_click':tone(880,.055,'bell'),
 'win':notes([523.25,659.25,783.99,1046.5],.16),
 'lose':notes([392,329.63,261.63,196],.18),
 'level_clear':notes([659.25,783.99,1046.5],.13)}
 for name,x in sounds.items():write(name,x)
 # Eight bars at 120 BPM: two distinct, original 16-second arrangements.
 for level in [False,True]:
  mix=np.zeros(SR*16);beat=.5
  chords=[(48,55,60),(45,52,57),(53,60,65),(55,62,67)]*2
  melody=([72,76,79,76,74,77,81,77,72,77,79,84,74,79,83,86] if level else
          [72,0,76,79,69,0,72,76,65,0,69,72,67,71,74,0])
  def add(x,at,gain):
   start=round(at*SR);end=min(len(mix),start+len(x));mix[start:end]+=x[:end-start]*gain
  for bar,chord in enumerate(chords):
   for quarter in range(4):
    midi=chord[quarter%3];f=440*2**((midi-69)/12)
    add(tone(f, .42 if level else .8),bar*2+quarter*beat,.24)
    if level:
     add(sweep(100,35,.11),bar*2+quarter*beat,.25)
     if quarter%2:add(sweep(170,90,.08,.7),bar*2+quarter*beat,.08)
   for eighth in range(8):
    note=melody[(bar*2+eighth//4)%len(melody)]
    if note and (level or eighth%2==0):
     add(tone(440*2**((note-69)/12),.20 if level else .42,'bell'),bar*2+eighth*.25,.14 if level else .2)
  normal=write('level_music' if level else 'menu_music',mix,.6)
  # Two-pole 1600 Hz lowpass, circular warmup for a seamless Web fallback.
  alpha=1-np.exp(-2*np.pi*1600/SR)
  filtered=np.tile(normal,2)
  for stage in range(2):
   previous=0.
   for j in range(len(filtered)):
    previous+=alpha*(filtered[j]-previous);filtered[j]=previous
  write('level_music_lowpass' if level else 'menu_music_lowpass',filtered[len(normal):],.6)
 # Preserve every legacy serialized GUID while replacing unknown old sounds.
 aliases={'music':'level_music','boss_hit':'enemy_hit'}
 for p in (ROOT/'Assets/Audio').glob('*.wav'):
  source=DEST/(aliases.get(p.stem,p.stem)+'.wav')
  if source.exists():p.write_bytes(source.read_bytes())
 (DEST/'music.wav').write_bytes((DEST/'level_music.wav').read_bytes())
 print('Generated 19 event clips, 2 original tracks and legacy aliases (CC0).')
if __name__=='__main__':main()
