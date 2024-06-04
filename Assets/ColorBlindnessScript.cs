using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;
using Math = ExMath;

public class ColorBlindnessScript: MonoBehaviour {

  public KMBombInfo Bomb;
  public KMAudio Audio;
  public TextMesh[] DisplayTexts;
  public KMSelectable[] Buttons;
  public SpriteRenderer artRenderer;
  public Sprite[] artPieces;
  private int artIndex = 0;
  private int correctArtIndex = 0;

  static int ModuleIdCounter = 1;
  int ModuleId;
  private bool ModuleSolved;

  void Awake() {
    ModuleId = ModuleIdCounter++;
    GetComponent < KMBombModule > ().OnActivate += Activate;
    /*
    foreach (KMSelectable object in keypad) {
        object.OnInteract += delegate () { keypadPress(object); return false; };
    }
    */

    Buttons[0].OnInteract += delegate() {
      submit();
      return false;
    };
    Buttons[1].OnInteract += delegate() {
      increaseArtIndex();
      return false;
    };
    Buttons[2].OnInteract += delegate() {
      decreaseArtIndex();
      return false;
    };
  }

  void submit() {
      if(!ModuleSolved){
    Audio.PlaySoundAtTransform("paintstroke", Buttons[0].transform);
    int paintingChosen = artIndex + 1;
    //Debug.LogFormat("[Color Blindness " + "#" + ModuleId + "] Submitted painting number: " + paintingChosen + ".", ModuleId);
    if(artIndex == correctArtIndex)
    {
        Solve();
        ModuleSolved = true;
    }
    else
    {
        Strike();
    }
    }
    else
    {
        //Disabled
    }
  }

  void increaseArtIndex() {
    if (artIndex == 7) {
      artIndex = 0;
    } else {
      artIndex++;
    }
    UpdateArt();
  }

  void decreaseArtIndex() {
    if (artIndex == 0) {
      artIndex = 7;
    } else {
      artIndex--;
    }
    UpdateArt();
  }

  void UpdateArt() {
    Audio.PlaySoundAtTransform("flipart", Buttons[0].transform);
    switch (artIndex) {
    case 0:
      artRenderer.sprite = artPieces[0];
      break;

    case 1:
      artRenderer.sprite = artPieces[1];
      break;

    case 2:
      artRenderer.sprite = artPieces[2];
      break;

    case 3:
      artRenderer.sprite = artPieces[3];
      break;

    case 4:
      artRenderer.sprite = artPieces[4];
      break;

    case 5:
      artRenderer.sprite = artPieces[5];
      break;

    case 6:
      artRenderer.sprite = artPieces[6];
      break;

    case 7:
      artRenderer.sprite = artPieces[7];
      break;
    }
  }

  void OnDestroy() { //Shit you need to do when the bomb ends

  }
  
  void createCorrectArtIndex(){
    //Here we go...
    int batteries = Bomb.GetBatteryCount();
    int modules = Bomb.GetSolvableModuleNames().Count;
    int lastSerialNum = Bomb.GetSerialNumberNumbers().Last();
    bool serialExists = Bomb.IsPortPresent(Port.Serial);
    Debug.Log("Batteries are: "+batteries+".");
if (batteries > 3)
{
    if (serialExists)
    {
        correctArtIndex = 1; 
    }
    else
    {
        correctArtIndex = 3;
    }
}
    else
    {
        int calcNum = modules % 8 + lastSerialNum;
        bool hasLetterA = Bomb.GetSerialNumber().Any("A".Contains);
        if(hasLetterA)
        {
            //Nothing
        }
        else
        {
            calcNum = calcNum + 5;
        }
        int lastDigit = calcNum % 10;
        calcNum = lastDigit; //last digit
        int numAfterCalc = calcNum+1;
        //Debug.LogFormat("[Color Blindness " + "#" + ModuleId + "] Correct painting in reading order is: " + numAfterCalc + ".", ModuleId);
        if (numAfterCalc > 8)
            {
                correctArtIndex = 7;
            }
            else
            {
                switch(calcNum)
                {
                    case 0:
                        correctArtIndex = 0;
                        break;

                    default:
                        correctArtIndex = calcNum;
                        break;
                }
            }
    }
  }
  
  void Activate() { //Shit that should happen when the bomb arrives (factory)/Lights turn on
  createCorrectArtIndex();
  int artLogIndex = correctArtIndex + 1;
  Debug.LogFormat("[Color Blindness " + "#" + ModuleId + "] Correct painting number is: " + artLogIndex + ".", ModuleId);
  }

  void Start() { //Shit

  }

  void Update() { //Shit that happens at any point after initialization
  }

  void Solve() {
    int winningPainting = artIndex + 1;
    Audio.PlaySoundAtTransform("solve", Buttons[0].transform);
    GetComponent <KMBombModule> ().HandlePass();
    Debug.LogFormat("[Color Blindness " + "#" + ModuleId + "] Solved! Defuser chose correct painting number: " + winningPainting + ".", ModuleId);
  }

  void Strike() {
    int correctPainting = correctArtIndex+1;
    int incorrectPainting = artIndex+1;
    Debug.LogFormat("[Color Blindness " + "#" + ModuleId + "] Strike! Defuser chose painting number " + incorrectPainting + " instead of painting number " + correctPainting+".", ModuleId);
    Audio.PlaySoundAtTransform("strike", Buttons[0].transform);
    GetComponent < KMBombModule > ().HandleStrike();
  }

IEnumerator WaitOneSecond()
    {
        yield return new WaitForSeconds(1);
    }
    
#pragma warning disable 414
   private readonly string TwitchHelpMessage = @"Use `!{0} left/right` to navigate through the paintings. Use `!{0} submit` to submit the current painting.";
#pragma warning restore 414

  IEnumerator ProcessTwitchCommand(string Command) {

     Command = Command.ToUpper();
     yield return null;

     switch (Command)
     {
         case "LEFT":
             Buttons[2].OnInteract();
             break;
         case "RIGHT":
             Buttons[1].OnInteract();
             break;
         case "SUBMIT":
             Buttons[0].OnInteract();
             break;
     }

  }

  IEnumerator TwitchHandleForcedSolve() {
    artIndex = correctArtIndex;
    UpdateArt();
    yield return new WaitForSeconds(0.5f);
    yield return ProcessTwitchCommand("submit");
  }
}