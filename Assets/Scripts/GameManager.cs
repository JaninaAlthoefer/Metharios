using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Enumerations;

public class GameManager : MonoBehaviour {

	public static GameManager s_instance;   //Allows other scripts to call functions from GameManager.   

    public List<Feld> Felder;
    public List<Spieler> Spieler;
    public KreaturChip moving;
    public KreaturChip spezial;
    public ZauberKarte Spell;
    public AusruestungsKarte equip;
    public KreaturKarte summon;
    public int startKarten;
    public int maxKarten;
    public int startmana;
	public int MaxMana;
    private Feld chargeFeld;
    private Feld attackedField;
    public Feld normalFeld;
    public Feld energieFeld;
    private int aktPlayer;

    [Header("Sounds")]
    [Space]
    public AudioClip cardsSfx;
    public AudioClip summonSfx;
    public AudioClip blattgeschossSfx;
    public AudioClip blizzardSfx;
    public AudioClip eisadlerSfx;
    public AudioClip eisblockSfx;
    public AudioClip eislanzeSfx;
    public AudioClip eispfeilSfx;
    public AudioClip eiswandSfx;
    public AudioClip nextTurnSfx;
    public AudioClip energieSfx;
    public AudioClip explosiveSamenSfx;
    public AudioClip fernkampfSfx;
    public AudioClip fieldChangeSfx;
    public AudioClip heilenSfx;
    public AudioClip kreaturKOSfx;
    public AudioClip langsamSfx;
    public AudioClip nahkampfSfx;
    public AudioClip neuesLebenSfx;
    public AudioClip rankenschlagSfx;
    public AudioClip reviveSfx;
    public AudioClip tauschenSfx;
    public AudioClip unsichtbarSfx;
    public AudioClip wiederkehrSfx;
    public AudioClip wucherwurzelnSfx;

    [Header("Particles")]
    [Space]
    public GameObject summonPfx;
    public GameObject blattgeschossPfx;
    public GameObject blizzardPfx;
    public GameObject eisadlerPfx;
    public GameObject eisblockPfx;
    public GameObject eislanzePfx;
    public GameObject eispfeilPfx;
    public GameObject eiswandPfx;
    public GameObject energiePfx;
    public GameObject explosiveSamenPfx;
    public GameObject fernkampfPfx;
    public GameObject fieldChangePfx;
    public GameObject heilenPfx;
    public GameObject kreaturKOPfx;
    public GameObject nahkampfPfx;
    public GameObject neuesLebenPfx;
    public GameObject rankenschlagPfx;
    public GameObject tauschenPfx;
    public GameObject wiederkehrPfx;
    public GameObject wucherwurzelnPfx;


    void Awake()
	{
		s_instance = this;
	}



    // Use this for initialization
    public void Setup()
    {
		FieldCreator.s_Instance.Setup ();

		UIManagerController.s_instance.EnemyMaxMana = MaxMana;
		UIManagerController.s_instance.PlayerMaxMana = MaxMana;

		UIManagerController.s_instance.setMaxlifeEnemy(Spieler[1].live);
		UIManagerController.s_instance.setMaxlife(Spieler[0].live);

		UIManagerController.s_instance.Setup ();
		aktPlayer = 0;
		onEnemyChangeUI ();
		aktPlayer = 1;
		onPlayerChangeUI ();
        for (int i = 1; i < Spieler.Count; i++)
        {
            Spieler[(aktPlayer + i) % Spieler.Count].AktivateHand(false);
        }
    }

    //gibt den aktuellen Spieler zurück
    public Spieler GetAktuellSpieler()
    {
        return Spieler[aktPlayer];
    }

    //setzt die Phase eins nach vorne
    public void nextPhase()
    {
		Spieler[aktPlayer].Phase += 1;
		UIManagerController.s_instance.ShowPhase(Spieler[aktPlayer].Phase);
    }

    //gibt die aktuelle Phase zurück
    public int getPhase()
    {
		return Spieler[aktPlayer].Phase;
    }

    //getter und setter für das Charge feld, das Feld auf dem ein Nahkämpfer ziehen wird mit einem angriff
    public Feld ChargeFeld
    {
        get
        {
            return chargeFeld;
        }

        set
        {
            chargeFeld = value;
        }
    }

    public Feld AttackedField
    {
        get
        {
            return attackedField;
        }

        set
        {
            attackedField = value;
        }
    }

    //Startet das ausspielen eioner KreaturenKarte
    public void kreaturkartespielen(KreaturKarte karte)
    {
        //Überprüft ob der Spieler genug Mana hat
        if (karte.Player.Mana >= karte.kosten)
        {

            //Schaut ob der Spieler aktiv ist 
            if (GetAktuellSpieler() == karte.Player)
            {
                //Überprüft ob es noch die Beschwörungsphase ist.
                if (getPhase() == 0)
                {
                    //Wurde dieser Kreatur bereits ausgewählt wird sie wieder abgewählt
                    if (moving != karte.Kreatur)
                    {
                        //War eine andere action am laufen wird diese beendet
                        if (Spell != null || equip != null || moving != null)
                        {
                            actionend();
                        }
                        //es wird nach der Hauptkreatur gesucht
                        foreach (Feld element in Felder)
                        {
                            if (element.Kreatur != null)
                                if (element.Kreatur.Player == karte.Player && element.Kreatur.anfuehrer == true)
                                {
                                    karte.Kreatur.Player = karte.Player;
                                    moving = karte.Kreatur;
                                    summon = karte;
                                    element.Beschwoerung();  
                                }
                        }
                    }
                    else
                    {
                        actionend();
                    }
                }
                else
                {
					UIManagerController.s_instance.Meldungen((int)enMeldungen.Falsche_Phase);
                    Debug.Log("Wrong Phase");
                }
            }
            else
            {
				UIManagerController.s_instance.Meldungen((int)enMeldungen.Falscher_Spieler);
                Debug.Log("Wrong Player");
            }
        }
        else
        {
			UIManagerController.s_instance.Meldungen((int)enMeldungen.Nicht_genug_Mana);
            Debug.Log("Nicht genug Mana");
        }
    }

    //Startet das ausspielen einer Ausrüstungskarte
    public void ausruestungsKarteSpielen(AusruestungsKarte karte)
    {
        //überprüft ob genug Mana da ist und es der richtige Spieler ist
        if (GetAktuellSpieler() == karte.Player && karte.Player.Mana >= karte.kosten)
        {
            //wenn die Karte bereits ausgewählt wurde, wird sie abgewählt
            if (equip != karte)
            {
                //wenn eine andere Karte oder Kreaturt vorher ausgewählt wurde, wird diese zuerst abgewählt
                if (Spell != null || equip != null || moving != null)
                {
                    actionend();
                }
                //Es werden nach möglichen zielen für die Ausrüstung gesucht
                foreach (Feld element in Felder)
                {

                    if (element.Kreatur != null)
                        if (element.Kreatur.Player == karte.Player)
                        {
                            element.setSpellCaster();
                        }
                }
                equip = karte;
            }
            else
            {
                actionend();
            }
        }
        else
        {
			UIManagerController.s_instance.Meldungen((int)enMeldungen.Nicht_genug_Mana);
            Debug.Log("Nicht genug Mana");
        }
    }

    //Startet das ausspielen einer ZauberKarte
    public void zauberKarteSpielen(ZauberKarte karte)
    {

        //Es wird überprüft ob der Spieler genug Mana hat
        if (GetAktuellSpieler() == karte.Player && karte.Player.Mana >= karte.kosten)
        {  
            //Wurde dieser Zauber bereits ausgewählt wird sie wieder abgewählt
            if (Spell != karte)
            {
                //War eine andere action am laufen wird diese beendet
                if (Spell != null || equip != null || moving != null)
                {
                    actionend();
                }
                //Es wird nach möglichen Zaubereren gesucht
                findSpellCaster(karte.Player);              
                Spell = karte;
            }
            else
            {
                actionend();
            }
        }
        else
        {
			UIManagerController.s_instance.Meldungen((int)enMeldungen.Nicht_genug_Mana);
            Debug.Log("Nicht genug Mana");
        }
    }

    public void allFelderNachbarnNeuzuweisung()
    {
        foreach (Feld element in Felder)
        {
            element.nachbarNeuZuweisung();
        }
    }

    //wird aufgerufen wenn der Spieler gewechselt wird
    public void changeAktPlayer()
    {

		if( Spieler[aktPlayer].hand.Count > 6)
		{
			UIManagerController.s_instance.Meldungen((int)enMeldungen.Karten_Ablegen);
			return;

		}

        //Standart Manazuwachs
        int managain = 2;
        foreach (Feld element in Felder)
        {
            if (element.Kreatur != null)
            {
                //resettet sämtliche spezialeffekte oder verringert wie lange diese noch wirkt 
                element.Kreatur.Aktiondone = false;
                element.Kreatur.Spezialused = false;
                if (element.Kreatur.checkSpezial(8) && element.Kreatur.Player == Spieler[(aktPlayer + 1) % Spieler.Count])
                    element.Kreatur.addCount();
                if (element.Kreatur.checkeffekt(2) && element.Kreatur.Player == Spieler[(aktPlayer + 1) % Spieler.Count])
                    element.Kreatur.timeoutEffekt(2);
                if (element.Kreatur.checkeffekt(5) && element.Kreatur.Player == Spieler[(aktPlayer + 1) % Spieler.Count])
                    element.Kreatur.timeoutEffekt(5);
                if (element.Kreatur.checkeffekt(13))
                    element.Kreatur.resetCount();
                if (element.gameObject.tag == "Energy_Feld" && element.Kreatur.Player == Spieler[(aktPlayer + 1) % Spieler.Count])
                {
                    // steht eine Kreatur auf einem Energiefeld, wird dem Spieler ein mehr Mana gegeben, doch das Feld bewegt sich nachdem die Kreatur für drei runden draufstand
                    element.timeOut();
                    managain += 1;
                }
            }
        }

        SoundManagerScript.instance.ChangePlayerSfx();

        //Temporär fürs austauschen der Kartenhand
        Spieler[aktPlayer].AktivateHand(false);
		Spieler[aktPlayer].Phase = 3;
		onEnemyChangeUI ();
        aktPlayer = (aktPlayer + 1) % Spieler.Count;
		Spieler [aktPlayer].SetMana (managain); //+= managain;
		Spieler[aktPlayer].Phase = 0;
		onPlayerChangeUI ();
        Spieler[aktPlayer].KarteZiehen();
        //Temporär fürs austauschen der Kartenhand
        Spieler[aktPlayer].AktivateHand(true);
    }

	public void onPlayerChangeUI()
	{
		UIManagerController.s_instance.CharImage (Spieler[aktPlayer].Decktyp); 
		UIManagerController.s_instance.onLiveChangePlayer (Spieler[aktPlayer].live); 
		UIManagerController.s_instance.onManaChangePlayer (Spieler[aktPlayer].Mana);
		UIManagerController.s_instance.ShowPhase (Spieler[aktPlayer].Phase);
		UIManagerController.s_instance.Meldungen ((int)enMeldungen.Your_Turn);
	}

	public void onEnemyChangeUI()
	{
		float templive = UIManagerController.s_instance.EnemyMaxLive;
		UIManagerController.s_instance.EnemyMaxLive = UIManagerController.s_instance.PlayerMaxLive;
		UIManagerController.s_instance.PlayerMaxLive = templive;


		UIManagerController.s_instance.CharImageEnemy (Spieler[aktPlayer].Decktyp); 
		UIManagerController.s_instance.onLiveChangeEnemy (Spieler[aktPlayer].live); 
		UIManagerController.s_instance.onManaChangeEnemy (Spieler[aktPlayer].Mana); 
		UIManagerController.s_instance.EnemyCardsSee (Spieler[aktPlayer].hand.Count);
	}
		
    //Wird zum ausführen einiger Spezialeffekte ausgeführt
    public void aktivatespezial()
    {
        if (moving.Spezial == 12) //Counter -> Kristallhirsch
        {
            moving.Spezialused = true;
        }
        if (moving.Spezial == 13) //Energie -> Wiesenrücken
        {
            letSoundPlay(Enumerations.enSfxAndPfx.Nahkampf);
            letParticlePlayMonster(Enumerations.enSfxAndPfx.Nahkampf, 2, moving);

            letSoundPlay(Enumerations.enSfxAndPfx.Energie);
            letParticlePlayMonster(Enumerations.enSfxAndPfx.Energie, 2, moving);

            Spieler[aktPlayer].SetMana (1);//Mana += 1;
             moving.addCount();
             moving.Platzfeld.SchadenNehmen(5);
			if (moving.getCount () < 4)
				Debug.Log ("fähigkeit? moving.button");     
		}
        if (moving.Spezial == 14) //Eishauch -> Eisdrache
        {
            if (moving.Spezialused == false)
            {
                allMoveEnd();
                moving.getPlatzFeld().findSpezialTarget(true);
            }
        }
    }

    // setzt ein Feld auf ein normalFeld zurück und wenn es vorher ein Energiefeld war, wird diese nue positioniert
    public void noMoreEnergie(Feld target, bool newEnergieField)
    {
        for (; newEnergieField == true;)
        {
            int ran = Random.Range(0,Felder.Count-1);
            if (Felder[ran].gameObject.tag != "Energie_Feld" && Felder[ran].gameObject.tag != "Start_Feld")
            {
                Felder[ran].changeFieldTyp(energieFeld);
                newEnergieField = false;
            }
        }
        target.changeFieldTyp(normalFeld);
    }

    //bewegt die zufohr ausgewählte Kreatur auf das Feld Target
    public void Move(Feld target)
    {
        //beendet die bewegung auf jedem Feld und entfernt die Kreatur vom Feld wo es herkam
        foreach (Feld element in Felder)
        {
            if (element.Kreatur == moving)
                element.Kreatur = null;
            element.MoveEnd();
        }
        moving.Platzfeld = target;
        moving.MoveTo();
        target.Kreatur = moving;
        moving.Aktiondone = true;
        //War zuerst die Summon Phase so wird diese beendet
        if (getPhase() == 0)
        {
            nextPhase();
        }
        else
        {
            //Wenn keine andere Kreatur sich bewegen kann  wird die nexte phase ausgewählt
            if (NoMoreMovment())
                nextPhase();
        }

        Debug.Log("vor actionend in move");

        actionend();
    }

    //bewegt die zufohr ausgewählte Kreatur auf das Feld Target
    public void MoveToFeld(Feld target)
    {
        //beendet die bewegung auf jedem Feld und entfernt die Kreatur vom Feld wo es herkam
        foreach (Feld element in Felder)
        {
            if (element.Kreatur == moving)
                element.Kreatur = null;
            element.MoveEnd();
        }
        moving.Platzfeld = target;
        moving.MoveTo();
        target.Kreatur = moving;
        moving.Aktiondone = true;
        //War zuerst die Summon Phase so wird diese beendet
        if (getPhase() == 0)
        {
            nextPhase();
        }
        else
        {
            //Wenn keine andere Kreatur sich bewegen kann  wird die nexte phase ausgewählt
            if (NoMoreMovment())
                nextPhase();
        }

    }


    public void Move(KreaturChip moving, Feld target)
    {
        //beendet die bewegung auf jedem Feld und entfernt die Kreatur vom Feld wo es herkam
        foreach (Feld element in Felder)
        {
            if (element.Kreatur == moving)
                element.Kreatur = null;
            element.MoveEnd();
        }
        moving.Platzfeld = target;
        moving.MoveTo();
        target.Kreatur = moving;
        moving.Aktiondone = true;
        //War zuerst die Summon Phase so wird diese beendet
        if (getPhase() == 0)
        {
            nextPhase();
        }
        else
        {
            //Wenn keine andere Kreatur sich bewegen kann  wird die nexte phase ausgewählt
            if (NoMoreMovment())
                nextPhase();
        }
    }

    //Erstellt eine neue Kreatur und plaziert diese auf das target Feld 
    public void Beschwoerung(Feld target)
    {
        moving.gesamt = this;
        moving.Platzfeld = target;
        Vector3 temp = target.transform.position;
        Quaternion temp1 = new Quaternion(0.7f, 0,0 , -0.7f);
        Instantiate(moving.gameObject, temp, temp1);

        letSoundPlay(Enumerations.enSfxAndPfx.Beschwoeren);
        letParticlePlayField(Enumerations.enSfxAndPfx.Beschwoeren, 2, target);

        foreach (Feld element in Felder)
        {
            if (element.Kreatur == moving)
                element.Kreatur = null;
            element.MoveEnd();
        }
        target.Kreatur = moving;
        GetAktuellSpieler().KarteGespielt(summon);
        actionend();
    }

    //Beendet eine bestehende aktion
    public void actionend()
    {
        Debug.Log("actionend");
        allMoveEnd();
        moving = null;
        Spell = null;
        equip = null;
        summon = null;
        chargeFeld = null;
        attackedField = null;
    }

    //Beendet bewegungssuche für alle Kreaturen
    public void allMoveEnd()
    {
        
        foreach (Feld element in Felder)
        {
            element.MoveEnd();
        }
    }

    // Sucht alle möglichen Spellcaster eines Spielers
    public void findSpellCaster(Spieler Player)
    {
        foreach (Feld element in Felder)
        {
            if (element.Kreatur != null)
            {
                if (element.Kreatur.Player == Player)
                {
                    element.setSpellCaster();   
                }
            }
        }
    }

    //es wurde ein Spellcaster ausgewählt, alle möglichen Spellcaster werden deaktiviert
    public void AllSpellCasterset()
    {
        foreach (Feld element in Felder)
        {
            element.SpellCasterSet();
        }
     }

    //überprüft ob es noch Kreaturen gibt die sich bewegen können
    public bool NoMoreMovment()
    {
        bool temp = true;
        foreach (Feld element in Felder)
        {
            if (element.Kreatur != null)
                if (element.Kreatur.Player == GetAktuellSpieler() && element.Kreatur.Aktiondone == true)
                    temp = false;
        }
        return temp;
    }

    public bool checkSpezial(int i)
    {
        return moving.checkSpezial(i);
    }

    public void onAblegen(Karten Ablegen) {
        if (getPhase() < 3)
            GetAktuellSpieler().Ablegen(Ablegen);
    }



//Manages Sound and Particle Effects

    public void letParticlePlayMonster(Enumerations.enSfxAndPfx e, int duration, KreaturChip target, KreaturChip origin=null)
    {
        GameObject play = null;

        switch (e)
        {
            case Enumerations.enSfxAndPfx.Blattgeschoss:
                play = blattgeschossPfx;
                break;
            case Enumerations.enSfxAndPfx.Blizzard:
                play = blizzardPfx;
                break;
            case Enumerations.enSfxAndPfx.Eisadler:
                play = eisadlerPfx;
                break;
            case Enumerations.enSfxAndPfx.Eisblock:
                play = eisblockPfx;
                break;
            case Enumerations.enSfxAndPfx.Eislanze:
                play = eislanzePfx;
                break;
            case Enumerations.enSfxAndPfx.Eispfeil:
                play = eispfeilPfx;
                break;
            case Enumerations.enSfxAndPfx.Energie:
                play = energiePfx;
                break;
            case Enumerations.enSfxAndPfx.ExplosiveSamen:
                play = explosiveSamenPfx;
                break;
            case Enumerations.enSfxAndPfx.Fernkampf:
                play = fernkampfPfx;
                break;
            case Enumerations.enSfxAndPfx.Heilen:
                play = heilenPfx;
                break;
            case Enumerations.enSfxAndPfx.KreaturKO:
                play = kreaturKOPfx;
                break;
            case Enumerations.enSfxAndPfx.Nahkampf:
                play = nahkampfPfx;
                break;
            case Enumerations.enSfxAndPfx.Rankenschlag:
                play = rankenschlagPfx;
                break;
            case Enumerations.enSfxAndPfx.Teleport:
                play = tauschenPfx;
                break;
            case Enumerations.enSfxAndPfx.Wiederkehr:
                play = wiederkehrPfx;
                break;
            case Enumerations.enSfxAndPfx.Wucherwurzeln:
                play = wucherwurzelnPfx;
                break;
        }

        SpellParticleManager.SpawnSpellParticles(play, duration, target, origin);
    }

    public void letParticlePlayField(Enumerations.enSfxAndPfx e, int duration, Feld target)
    {
        GameObject play = null;

        switch (e)
        {
            case Enumerations.enSfxAndPfx.Beschwoeren:
                play = summonPfx;
                break;
            case Enumerations.enSfxAndPfx.Eiswand:
                play = eiswandPfx;
                break;
            case Enumerations.enSfxAndPfx.FieldChange:
                play = fieldChangePfx;
                break;
        }

        SpellParticleManager.SpawnSpellParticles(play, duration, target);
    }

    public void letSoundPlay(Enumerations.enSfxAndPfx e)
    {
        AudioClip play = null;

        switch (e)
        {
            case Enumerations.enSfxAndPfx.Beschwoeren:
                play = summonSfx;
                break;
            case Enumerations.enSfxAndPfx.Blattgeschoss:
                play = blattgeschossSfx;
                break;
            case Enumerations.enSfxAndPfx.Blizzard:
                play = blizzardSfx;
                break;
            case Enumerations.enSfxAndPfx.Eisadler:
                play = eisadlerSfx;
                break;
            case Enumerations.enSfxAndPfx.Eisblock:
                play = eisblockSfx;
                break;
            case Enumerations.enSfxAndPfx.Eislanze:
                play = eislanzeSfx;
                break;
            case Enumerations.enSfxAndPfx.Eispfeil:
                play = eispfeilSfx;
                break;
            case Enumerations.enSfxAndPfx.Eiswand:
                play = eiswandSfx;
                break;
            case Enumerations.enSfxAndPfx.Energie:
                play = energieSfx;
                break;
            case Enumerations.enSfxAndPfx.ExplosiveSamen:
                play = explosiveSamenSfx;
                break;
            case Enumerations.enSfxAndPfx.Fernkampf:
                play = fernkampfSfx;
                break;
            case Enumerations.enSfxAndPfx.FieldChange:
                play = fieldChangeSfx;
                break;
            case Enumerations.enSfxAndPfx.Heilen:
                play = heilenSfx;
                break;
            case Enumerations.enSfxAndPfx.KarteBewegen:
                play = cardsSfx;
                break;
            case Enumerations.enSfxAndPfx.KreaturKO:
                play = kreaturKOSfx;
                break;
            case Enumerations.enSfxAndPfx.Langsam:
                play = langsamSfx;
                break;
            case Enumerations.enSfxAndPfx.Nahkampf:
                play = nahkampfSfx;
                break;
            case Enumerations.enSfxAndPfx.NeuesLeben:
                play = neuesLebenSfx;
                break;
            case Enumerations.enSfxAndPfx.NextTurn:
                play = nextTurnSfx;
                break;
            case Enumerations.enSfxAndPfx.Rankenschlag:
                play = rankenschlagSfx;
                break;
            case Enumerations.enSfxAndPfx.Revive:
                play = reviveSfx;
                break;
            case Enumerations.enSfxAndPfx.Teleport:
                play = tauschenSfx;
                break;
            case Enumerations.enSfxAndPfx.Unsichtbar:
                play = unsichtbarSfx;
                break;
            case Enumerations.enSfxAndPfx.Wiederkehr:
                play = wiederkehrSfx;
                break;
            case Enumerations.enSfxAndPfx.Wucherwurzeln:
                play = wucherwurzelnSfx;
                break;
        }

        SoundManagerScript.instance.PlaySingle(play);
    }

}