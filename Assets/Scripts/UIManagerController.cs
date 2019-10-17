using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;


public class UIManagerController : NetworkBehaviour {

	public static UIManagerController s_instance;   //Allows other scripts to call functions from GameManager.   

	// Player UI
	[Header("Player UI")]
	[Space]
	public Image PlayerLive;
	public Image PlayerMana;
	public Image PlayerCharFrost;
	public Image PlayerCharWald;
	string[] MeldungsTexte = new string[] { "Falsche Phase", "Kein Ziel", "Your Turn", "Nicht genug Mana", "Karten Ablegen", "Falscher_Spieler", "Game Ended" };
	public Text Meldung;
	// 0 Beschoeren 1 Kampf/Bewegen 2 Abwerfen 3 Warten 
	public List<Image> Phasen;
	public List<RawImage> PlayerHand;

	// Enemy Ui
	[Header("Enemy Ui")]
	[Space]
	public Image EnemyLive;
	public Image EnemyMana;
	public Image EnemyCharFrost;
	public Image EnemyCharWald;
	public List<RawImage> EnemyHand;



	// UI-Referncen
	[Header("UI-Referncen")]
	[Space]
	public GameObject Regeln;
	public GameObject Ablagestapel;
	public GameObject Ziehstapel;
	public GameObject HoverCard;

	[Header("Spieler Einstellungen")]
	[Space]
	public Color Spieler1Color = Color.blue;
	public Color Spieler2Color = Color.green;
	public Button Spieler1Button;
	public Button Spieler2Button;
	public string Feldname;


	// Player Reference
	public float EnemyMaxLive = 40;
	public float EnemyMaxMana;

	public float PlayerMaxLive = 40;
	public float PlayerMaxMana;

    // Temp Variablen
	bool ablagesichtbar = false;







	//                                              ----------------- UI - Start --------------

	void Awake()
	{
		s_instance = this;
	}

	// Setzen der UI in Startzustand
	public void Setup()
	{
		HoverCard.SetActive(false);
		Meldung.enabled = false;
		onManaChangePlayer (GameManager.s_instance.startmana);
		onManaChangeEnemy (GameManager.s_instance.startmana);
	}








	//                                              ----------------- Player -------------------

	// Spieler Leben setzen
	public void setMaxlife(int value)
	{
		PlayerMaxLive = value;
	}

	// CharImage Player Aktivieren / Deaktivieren
	public void CharImage(int deck)
	{
		if (deck == 0) 
		{
			PlayerCharWald.gameObject.SetActive (false);
			PlayerCharFrost.gameObject.SetActive (true);
		}
		else
		{
			PlayerCharWald.gameObject.SetActive (true);
			PlayerCharFrost.gameObject.SetActive (false);
		}	
	}

	//Hand des Spielers Aktualisieren
	public void onChangePlayerCardHand(List<Karten> hand)
	{
		int x = 0;

		//Hand des Spielers Aktualisieren  List<RawImage> PlayerHand;
		foreach (RawImage element in PlayerHand)
		{
			HandScript handKarte = element.GetComponent<HandScript>();
			handKarte.Karte = null;

			if (hand.Count > x)
			{
				handKarte.Karte = hand[x].GetComponent<Karten>();
				handKarte.HoverCard = HoverCard;
				element.enabled = true;
				element.GetComponent<RawImage>().texture = hand[x].gameObject.GetComponent<Renderer>().sharedMaterials[2].mainTexture;
			}
			else
				element.enabled = false;
			x++;
		}
	}

	//Spieler Leben in der Lebenskugel ändern
	public void onLiveChangePlayer(float live)
	{
		float Prozentoal;

		Prozentoal = live / PlayerMaxLive;
		PlayerLive.fillAmount = Prozentoal;
	}

	//Spieler Mana in der Manakugel ändern
	public void onManaChangePlayer(float mana)
	{
		float Prozentoal;
		Prozentoal = mana / PlayerMaxMana;
		PlayerMana.fillAmount = Prozentoal;
	}








	//                                              ----------------- Enemy --------------------

	// Gegner Leben setzten
	public void setMaxlifeEnemy(int value)
	{
		EnemyMaxLive = value;
	}

	// CharImage Enemy  Aktivieren / Deaktivieren
	public void CharImageEnemy(int deck)
	{
		if (deck == 0) 
		{
			EnemyCharFrost.gameObject.SetActive (true);
			EnemyCharWald.gameObject.SetActive (false);
		} else 
		{
			EnemyCharWald.gameObject.SetActive (true);
			EnemyCharFrost.gameObject.SetActive (false);
		}
	}

	// Cards Aktivieren / Deaktivieren
	public void EnemyCardsSee(int count)
	{
		int x = 0;

		//Hand des Spielers Aktualisieren  List<RawImage> PlayerHand;
		foreach (RawImage element in EnemyHand)
		{
			element.enabled = false;

			if (count == 0) 
			{
			}
				
			if (count > x)
			{
				element.enabled = true;
			}

				
			x++;
		}
	}

	//Gegner Leben in der Lebenskugel ändern
	public void onLiveChangeEnemy(float live)
	{
		float Prozentoal;

		Prozentoal = live / EnemyMaxLive;
		EnemyLive.fillAmount = Prozentoal;
	}

	//Spieler Mana in der Manakugel ändern
	public void onManaChangeEnemy(float mana)
	{
		float Prozentoal;

		Prozentoal = mana / EnemyMaxMana;
		EnemyMana.fillAmount = Prozentoal;
	}







	//                                              ----------------- UI Controll ----------------

	// Aktiv Player ändern
	public void endTurn ()
	{
		GameManager.s_instance.changeAktPlayer();
        HandScript.instance.hideHoverCard();
	}

	// Panel vom Regelbuch Aktivieren / Deaktivieren
	public void regelbuch () {


		if (Regeln.activeSelf)
		{
			Regeln.SetActive(false);
		}
		else
		{
			Regeln.SetActive(true);
		}
	}

	// Karten textur im Ablagestapel anzeigen
	public void onCardDead (Texture tex) {

		if(!ablagesichtbar){
			Ablagestapel.SetActive(true);
			ablagesichtbar = true;
		}
		Ablagestapel.GetComponent<Renderer>().materials[2].mainTexture = tex; 
	}

	public void onDeckNull()
	{
		Ziehstapel.SetActive(false);
	}

	public void ShowPhase(int phase)
	{
		foreach (Image p in Phasen)
		{
			p.gameObject.SetActive(false);
		}

		Phasen[phase].gameObject.SetActive(true);
	}

	public void Meldungen(int fehlercode) {
		Meldung.text = MeldungsTexte[fehlercode];
		StartCoroutine(ShowMessage());
	}

    public void noFieldChosen()
    {
        Meldung.text = "Kein Spielfeld";
        StartCoroutine(ShowMessage());
    }
    
    public void GameEndMeldung()
    { 
        Meldung.text = MeldungsTexte[7];
        //Meldung.enabled = true;
        StartCoroutine(ShowMessage(5));
        //ShowMessage(5);

    }

    IEnumerator ShowMessage(int dur = 2)
	{
		Meldung.enabled = true;
		yield return new WaitForSeconds(dur);
		Meldung.enabled = false;
	}

	public void ColorChangePlayer1()
	{
		if (Spieler1Color == Color.blue)
		{
			Spieler1Color = Color.green;
		}
		else
		{
			Spieler1Color = Color.blue;
		}

		Spieler1Button.GetComponent<Image>().color = Spieler1Color;
	}

	public void ColorChangePlayer2()
	{
		if (Spieler2Color == Color.blue)
		{
			Spieler2Color = Color.green;
		}
		else
		{
			Spieler2Color = Color.blue;
		}

		Spieler2Button.GetComponent<Image>().color = Spieler2Color;
	}

	public void FeldChange(GameObject item)
	{
		Feldname = item.GetComponent<Dropdown>().captionText.text;
	}

}