using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class KreaturChip : MonoBehaviour {

    public static KreaturChip s_instance;

    public GameManager gesamt;
    public Feld Platzfeld;
    private bool disable;
    private bool aktiondone;
    public bool anfuehrer;
    public int maxLeben;
    public int Angriff;
    public int Magie;
    public int Reichweite;
    public int Bewegung;
    //1 = überschuss,2 = Fliegen,3 = Ramme,4 = Moveattack,5 = swipe,6 = größere Spellfläche,7 = Kreaturswap, 8 = notarget, 9 = reincarnate, 10 = heal, 11 = nospelltarget, 12 = Revenge,13 = Methasbau, 14 = Eishauch
    public int Spezial; 
    private bool spezialused;
    private int count;
    private int leben;
    public List<AusruestungsKarte> eguiped;
    public List<ZauberKarte> effekte;
    public List<int> effektzeit;
    public Spieler Player;
	public int DeckTyp;
    public Texture textur;
    private GameObject HoverCard;
	GameObject SchadensText;

    //Getter und Setter für aktiondone
    public bool Aktiondone
    {
        get
        {
            return aktiondone;
        }

        set
        {
            aktiondone = value;
        }
    }

    public int Leben
    {
        get
        {
            return leben;
        }

        set
        {
            leben = value;
        }
    }

    public bool Spezialused
    {
        get
        {
            return spezialused;
        }

        set
        {
            spezialused = value;
        }
    }

    public int getCount()
    {
        return count;
    }

    public void resetCount()
    {
        count = 0;
    }

    public void addCount()
    {
        count++;
    }

    public Feld getPlatzFeld()
    {
        return Platzfeld;
    }


    void Awake()
    {
        s_instance = this;

        HoverCard = GameObject.FindWithTag("HoverCard");
		SchadensTextSetup ();
    }

	void SchadensTextSetup()
	{
		SchadensText = new GameObject ("SchadensText");
		SchadensText.transform.parent = this.transform;
		SchadensText.AddComponent<TextMesh> ();
		SchadensText.GetComponent<TextMesh> ().text = "100";
		SchadensText.GetComponent<TextMesh> ().characterSize = 0.01f;
        SchadensText.GetComponent<TextMesh>().color = Color.red;
        // Rotation sollte x 0 Y 180 Z 40 ist aber irgendwie x 270 y 220 z 0
        Quaternion tmp = new Quaternion(0.3f,0.9f,0.0f,0.0f);
		SchadensText.transform.rotation = tmp;
		Vector3 tmp1 = new Vector3 (1,1,1);
		SchadensText.transform.localScale = tmp1;
		SchadensText.SetActive (false);
	}
	
    public IEnumerator ShowDamageTaken(KreaturChip target, int damage)
    {
        SchadensText.transform.position = target.transform.position + new Vector3(0.0f, 0.0f, 0.2f);
        SchadensText.GetComponent<TextMesh>().text = damage + "";
        SchadensText.SetActive(true);

        yield return new WaitForSeconds(1);

        SchadensText.SetActive(false);

    }
    	
    // Use this for initialization
    void Start () {
        Quaternion temp = Quaternion.Euler(270 , 0, 220);
        this.transform.rotation = temp;
        disable = false;
        aktiondone = false;
        leben = maxLeben;
        MoveTo();
	}

    public void onHover()
    {
        GameManager.s_instance.letSoundPlay(Enumerations.enSfxAndPfx.KarteBewegen);

        HoverCard.GetComponent<RawImage>().texture = this.gameObject.GetComponent<RawImage>().texture;
        HoverCard.GetComponentInChildren<onAblegenScript>().enabled = false;
        HoverCard.gameObject.SetActive(true);
    }

    public void onHoverExit()
    {
        //GameManager.s_instance.letSoundPlay(Enumerations.enSfxAndPfx.KarteBewegen);

        HoverCard.GetComponentInChildren<onAblegenScript>().enabled = true;
        HoverCard.gameObject.SetActive(false);      
    }

    public void addequip(AusruestungsKarte equip)
    {
        eguiped.Add(equip);
        GameManager.s_instance.letSoundPlay(Enumerations.enSfxAndPfx.KarteBewegen);
    }

    public List<AusruestungsKarte> removeequip()
    {
        GameManager.s_instance.letSoundPlay(Enumerations.enSfxAndPfx.KarteBewegen);
        List<AusruestungsKarte> templist = eguiped;
        eguiped.Clear();
        return templist;
    }

    //Bewegt Kreatur zu seinem zugewiesenen Feld
    public void MoveTo()
    {
        Vector3 target = Platzfeld.transform.position;
        target.y += 0.075f;
        gameObject.transform.position = target;
        Platzfeld.Kreatur = this;
    }

    public bool checkSpezial(int i)
    {
        if (Spezial == i)
            return true;
        if (eguiped != null)
        { 
            foreach (AusruestungsKarte e in eguiped)
            {
                if (e.Spezial == i)
                    return true;
            }
        }
        return false;
    }

    public bool checkeffekt(int i)
    {
        if (effekte != null)
        {
            foreach (ZauberKarte z in effekte)
            {
                if (z.spezial == i)
                    return true;
            }
        }
        return false;
    }

    public void timeoutEffekt(int i)
    {
        if (effekte != null)
        {
            for (int z = 0; z < effekte.Count; z++)
            {
                if (effekte[z].spezial == i)
                {
                    effektzeit[z] -= 1;
                    if (effektzeit[z] == 0)
                    {
                        effekte.RemoveAt(z);
                        effektzeit.RemoveAt(z);
                    }
                }
            }
        }
    }

    void OnMouseDown()
    {
        if (gesamt.moving == null&&gesamt.GetAktuellSpieler() == Player&&!checkeffekt(5))
        {                                                                   //Eisblock
            //entscheidung ob die Kreatur Zaubern soll, eine ausrüstung bekommt oder sich bewegen soll
            if (Platzfeld.getSpellCaster())
            {
                if (gesamt.Spell != null)
                {
                    gesamt.moving = this;
                    gesamt.AllSpellCasterset();
                    if (gesamt.Spell.spezial == 4) //Blizzard 
                    {
                        effekte.Add(gesamt.Spell);
                        int temp = Magie / 10;
                        if (Magie % 10 == 5)
                            temp += 1;
                        effektzeit.Add(temp);
                    }
                    if (gesamt.Spell.spezial == 1) //Eislanze
                        Platzfeld.Durchschuss(gesamt.Spell.Reichweite);
                    else
                        Platzfeld.findspellTarget(gesamt.Spell.Reichweite);
                }
                if (gesamt.equip != null)
                {
                    leben += gesamt.equip.LebenPlus;
                    Angriff += gesamt.equip.AngriffPlus;
                    Magie += gesamt.equip.MagiePlus;
                    Bewegung += gesamt.equip.BewegungPlus;
                    Reichweite += gesamt.equip.ReichweitePlus;
                    addequip(gesamt.equip);
                    gesamt.GetAktuellSpieler().KarteGespielt(gesamt.equip);

                    gesamt.actionend();
                }
                if (gesamt.spezial != null && gesamt.spezial.checkSpezial(9))
                {                               //Wiederkehr -> Eisphönix
                    GameManager.s_instance.letSoundPlay(Enumerations.enSfxAndPfx.Wiederkehr);
                    GameManager.s_instance.letParticlePlayMonster(Enumerations.enSfxAndPfx.Wiederkehr, 5, Platzfeld.Kreatur);

                    gesamt.spezial.leben = gesamt.spezial.maxLeben;
                    gesamt.Move(gesamt.spezial, Platzfeld);
                    gesamt.spezial = null;
                    gesamt.actionend();
                    Platzfeld.kreaturZerstören();
                }
                disable = true;
            }
            else if(aktiondone == false)
            {
                Platzfeld.MoveStart(Bewegung);
                if (Spezial == 7) //Tauschen -> Eiskrabbler
                    Platzfeld.findSpezialTarget(false);
				if (Spezial == 12 && count == 0) //Counter -> Kristallhirsch
					//Debug.Log ("KristallhirschButton");
				if (Spezial == 13 && count < 4) // Energie -> Wiesnrücken
				    //Debug.Log ("WiesenrückenButton");
                if (Spezial == 14 && spezialused==false) //Eishauch -> Eisdrache
                    //Debug.Log("EisdracheButton");
                disable = true;
            }
        }
    }

   

    void OnMouseUpAsButton()
    {
        //beendet bestehende bewegung
        if (gesamt.moving == this && disable == false)
        {
            if (Spezial == 4 && spezialused == true)
            {
                //Debug.Log("KreaturChip, 301, aktiondone");
                aktiondone = true;
            }
            //Debug.Log("onmouseupasbutton KC");
            gesamt.actionend();
        }
        if (gesamt.moving == this)
        {
            disable = false;
        }
    }

    void OnMouseUp()
    {
        Debug.Log("KC mouseUp");
        //attackiert diese Kreatur
        if (gesamt.moving != null && gesamt.moving != this)
            Platzfeld.attacke();
    }

    void OnMouseEnter()
    {
        if (gesamt.ChargeFeld != null)
        {
            gesamt.AttackedField = Platzfeld;
        }
    }

	public void OnMouseOver()
	{
       
        if (Input.GetMouseButtonDown(1))
		{
            GameManager.s_instance.letSoundPlay(Enumerations.enSfxAndPfx.KarteBewegen);

            HoverCard.GetComponent<RawImage>().texture = textur;
			HoverCard.transform.GetChild(0).gameObject.SetActive(false);
			HoverCard.transform.GetChild(1).gameObject.SetActive(false);
			if (Spezial != 0)
			{
				HoverCard.transform.GetChild(1).gameObject.SetActive(true);
			}

			HoverCard.transform.GetChild(2).gameObject.SetActive(true);
			HoverCard.transform.GetChild(2).GetChild(0).GetChild(0).GetComponent<Text>().text = leben + "";
			HoverCard.transform.GetChild(2).GetChild(1).GetChild(0).GetComponent<Text>().text = Angriff + "";
			HoverCard.transform.GetChild(2).GetChild(2).GetChild(0).GetComponent<Text>().text = Magie + "";
			HoverCard.transform.GetChild(2).GetChild(3).GetChild(0).GetComponent<Text>().text = Reichweite + "";
			HoverCard.transform.GetChild(2).GetChild(4).GetChild(0).GetComponent<Text>().text = Bewegung + "";

			HoverCard.SetActive(true);
		}
	}

}
