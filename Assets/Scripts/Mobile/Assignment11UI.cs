using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public sealed partial class MobileGameUI
{
    TMP_Text campaignStatus, musicLabel, soundLabel, menuProgress, menuSaveStatus;
    Slider musicSlider, soundSlider;
    GameObject levelSelectCard, resetConfirm;
    Button continueButton;
    Button DataButton(Transform parent,string text,Vector2 pos,Vector2 size,Color color,UnityAction action)
    {
        Image image=Box(text+"Button",parent,color,true);
        Place(image.rectTransform,Center,pos,size);
        Button button=image.gameObject.AddComponent<Button>(); button.targetGraphic=image;
        Navigation nav=button.navigation; nav.mode=Navigation.Mode.None; button.navigation=nav;
        button.onClick.AddListener(() => Sfx.Play(SoundEvent.UIClick));
        if(action!=null) button.onClick.AddListener(action);
        Label("Label",image.transform,text,42,Vector2.zero,size-new Vector2(20,10)).color=Ink;
        return button;
    }
    void BuildAssignment11Menu()
    {
        Image card=Box("MenuCard",safe,Ink); menuCard=card.gameObject;
        Place(card.rectTransform,Center,Vector2.zero,new Vector2(1120,940));
        Label("Title",card.transform,GameTitle,88,new Vector2(0,365),new Vector2(1000,110)).color=Gold;
        menuProgress=Label("Progress",card.transform,"",36,new Vector2(0,270),new Vector2(1000,70));
        continueButton=DataButton(card.transform,"CONTINUE",new Vector2(0,145),new Vector2(800,110),Mint,RunFlow.Continue);
        DataButton(card.transform,"NEW RUN",new Vector2(0,20),new Vector2(800,110),Gold,RunFlow.NewGame);
        DataButton(card.transform,"LEVEL SELECT",new Vector2(0,-105),new Vector2(800,110),Cream,ShowLevelSelect);
        DataButton(card.transform,"SETTINGS",new Vector2(0,-230),new Vector2(800,110),Cream,()=>ShowSettings(true));
        DataButton(card.transform,"QUIT",new Vector2(0,-355),new Vector2(800,95),Gold,()=>{
            RunFlow.SaveCurrent();
            MainMenu menu=FindFirstObjectByType<MainMenu>(); if(menu!=null)menu.Quit();
        });
        menuSaveStatus=Label("MenuHint",card.transform,SaveService.Status,36,new Vector2(0,-435),new Vector2(1000,50));
        RefreshRunMenu(); BuildAssignment11Settings();
    }
    void RefreshRunMenu()
    {
        SaveData d=SaveService.Data;
        if(menuSaveStatus!=null) menuSaveStatus.text=SaveService.Status;
        if(continueButton!=null)
        {
            continueButton.interactable=d.runActive;
            continueButton.GetComponentInChildren<TMP_Text>().text=d.runActive?(d.currentHp>0?"CONTINUE  L"+d.currentLevel+"   HP "+d.currentHp:"RETRY LEVEL "+d.currentLevel):"NO SAVED RUN";
        }
        if(menuProgress!=null) menuProgress.text="BEST "+d.bestScore+"    OPEN LEVELS "+d.unlocked.Length+" / "+GameCatalog.Current.levels.Length;
    }
    void ShowLevelSelect()
    {
        menuCard.SetActive(false); settingsCard.SetActive(false);
        if(levelSelectCard!=null) { levelSelectCard.SetActive(false); Destroy(levelSelectCard); }
        Image card=Box("LevelSelectCard",safe,Ink); levelSelectCard=card.gameObject;
        Place(card.rectTransform,Center,Vector2.zero,new Vector2(1120,940));
        Label("Title",card.transform,"CHOOSE A LEVEL",68,new Vector2(0,365),new Vector2(1000,110)).color=Gold;
        LevelData[] levels=GameCatalog.Current.levels;
        for(int i=0;i<levels.Length;i++)
        {
            int id=levels[i].id;
            bool unlocked=SaveService.Data.unlocked.Contains(id);
            bool complete=SaveService.Data.completed.Contains(id);
            string caption=unlocked?"L"+id+"  "+levels[i].title+(complete?"  [CLEAR]":""):"L"+id+"  LOCKED";
            Button b=DataButton(card.transform,caption,new Vector2(0,180-i*180),new Vector2(920,145),unlocked?Mint:Color.gray,()=>RunFlow.SelectLevel(id));
            b.interactable=unlocked;
        }
        DataButton(card.transform,"BACK",new Vector2(0,-365),new Vector2(700,110),Gold,()=>{
            levelSelectCard.SetActive(false); menuCard.SetActive(true); RefreshRunMenu();
        });
    }
    void BuildAssignment11Settings()
    {
        Image card=Box("SettingsCard",safe,Ink); settingsCard=card.gameObject;
        Place(card.rectTransform,Center,Vector2.zero,new Vector2(1120,960));
        Label("Title",card.transform,"SETTINGS",72,new Vector2(0,395),new Vector2(1000,100)).color=Gold;
        settingsHealth=Label("Health",card.transform,"",36,new Vector2(0,270),new Vector2(600,80));
        DataButton(card.transform,"HP -",new Vector2(-390,270),new Vector2(180,100),Cream,()=>ChangeSetting(UIActionButton.Command.HealthDown));
        DataButton(card.transform,"HP +",new Vector2(390,270),new Vector2(180,100),Mint,()=>ChangeSetting(UIActionButton.Command.HealthUp));
        settingsRate=Label("Rate",card.transform,"",36,new Vector2(0,150),new Vector2(600,80));
        DataButton(card.transform,"RATE -",new Vector2(-390,150),new Vector2(180,100),Cream,()=>ChangeSetting(UIActionButton.Command.RateDown));
        DataButton(card.transform,"RATE +",new Vector2(390,150),new Vector2(180,100),Mint,()=>ChangeSetting(UIActionButton.Command.RateUp));
        musicLabel=Label("Music",card.transform,"",36,new Vector2(-320,30),new Vector2(410,80));
        musicSlider=MakeSlider(card.transform,new Vector2(210,30),0f,1f,false,SaveService.Data.musicVolume);
        musicSlider.onValueChanged.AddListener(value => ChangeAudioVolume(true,value));
        soundLabel=Label("Sound",card.transform,"",36,new Vector2(-320,-90),new Vector2(410,80));
        soundSlider=MakeSlider(card.transform,new Vector2(210,-90),0f,1f,false,SaveService.Data.soundVolume);
        soundSlider.onValueChanged.AddListener(value => ChangeAudioVolume(false,value));
        settingsHazards=DataButton(card.transform,"HAZARDS",new Vector2(-250,-215),new Vector2(460,110),Cream,()=>ChangeSetting(UIActionButton.Command.Hazards)).GetComponentInChildren<TMP_Text>();
        settingsEnemies=DataButton(card.transform,"ENEMIES",new Vector2(250,-215),new Vector2(460,110),Cream,()=>ChangeSetting(UIActionButton.Command.Enemies)).GetComponentInChildren<TMP_Text>();
        DataButton(card.transform,"RESET PROGRESS",new Vector2(-255,-365),new Vector2(470,110),Gold,ConfirmReset);
        DataButton(card.transform,"BACK",new Vector2(255,-365),new Vector2(470,110),Mint,()=>{ShowSettings(false);RefreshRunMenu();});
        RefreshSettings(); settingsCard.SetActive(false);
    }
    void ChangeAudioVolume(bool music,float value)
    {
        if(music) SaveService.Data.musicVolume=Mathf.Clamp01(value);
        else SaveService.Data.soundVolume=Mathf.Clamp01(value);
        SoundPass.Ensure(); SoundPass.Instance.ApplyVolumes();
        SaveService.Write(); RefreshAssignment11Audio();
        if(!music && Time.unscaledTime >= nextVolumePreview)
        { nextVolumePreview=Time.unscaledTime+0.15f; Sfx.Play(SoundEvent.UIClick); }
    }
    float nextVolumePreview;
    static string VolumeText(float value)
    { return value <= 0.0001f ? "MUTED" : SoundPass.ToDecibels(value).ToString("0.0")+" dB"; }
    void RefreshAssignment11Audio()
    {
        if(musicLabel!=null) musicLabel.text="MUSIC  "+VolumeText(SaveService.Data.musicVolume);
        if(soundLabel!=null) soundLabel.text="SFX  "+VolumeText(SaveService.Data.soundVolume);
        if(musicSlider!=null) musicSlider.SetValueWithoutNotify(SaveService.Data.musicVolume);
        if(soundSlider!=null) soundSlider.SetValueWithoutNotify(SaveService.Data.soundVolume);
    }
    void ConfirmReset()
    {
        if(resetConfirm!=null) return;
        Image overlay=Box("ResetOverlay",safe,new Color(0,0,0,0.85f),true); resetConfirm=overlay.gameObject; Stretch(overlay.rectTransform);
        Image card=Box("ResetConfirmCard",overlay.transform,Ink,true); Place(card.rectTransform,Center,Vector2.zero,new Vector2(1000,540));
        Label("Title",card.transform,"RESET PROGRESS?",64,new Vector2(0,150),new Vector2(900,100)).color=Gold;
        Label("Warning",card.transform,"Unlocks, scores and the current run will be erased. Your settings stay.",36,new Vector2(0,20),new Vector2(900,120));
        DataButton(card.transform,"CANCEL",new Vector2(-225,-160),new Vector2(410,120),Mint,CloseReset);
        DataButton(card.transform,"RESET",new Vector2(225,-160),new Vector2(410,120),Gold,()=>{
            SaveService.ResetProgress(); CloseReset(); ShowSettings(false); RefreshRunMenu();
        });
    }
    void CloseReset() { if(resetConfirm!=null) {resetConfirm.SetActive(false);Destroy(resetConfirm);resetConfirm=null;} }
}
