using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Spieler : MonoBehaviour {

    public int live = 0;
    private int MaxMana = 25;
    public int Mana = 0;
	public int Phase;
	public int Decktyp;
    public List<Karten> deck;
    public List<Karten> hand;
    public List<Karten> feld;
    public List<Karten> ablagestapel;

    
    void Awake () {

		foreach (Karten element in deck)
		{
			element.gesamt = GameManager.s_instance;
			element.Player = this;
		}
		for (int i = 0; i < GameManager.s_instance.startKarten; i++)
		{
			KarteZiehen();
		}
	}

	public void SetMana(int value)
	{
		Mana += value;
		UIManagerController.s_instance.onManaChangePlayer (Mana);
	}



    //entnimmt dem Deck eine Karte und fügt diese der Hand hinzu
    public void KarteZiehen()
	{
		if (deck.Count != 0) {
			int Kartennummer = (int)(Random.value * deck.Count);
			hand.Add (deck [Kartennummer].GetComponent<Karten> ());
			deck.RemoveAt (Kartennummer);
            GameManager.s_instance.letSoundPlay(Enumerations.enSfxAndPfx.KarteBewegen);
            onCardHandChange ();
		} 
		else 
		{
			UIManagerController.s_instance.onDeckNull ();
		}
    }

    private void Kartebewegen(List<Karten> aus, Karten karte, List<Karten> into)
    {
        //GameManager.s_instance.letSoundPlay(Enumerations.enSfxAndPfx.KarteBewegen);
        aus.Remove(karte);
        into.Add(karte);
    }

    //wird aufgerufen wenn eine Karte gespielt wird
    public void KarteGespielt(Karten ablegen)
    {
        GameManager.s_instance.letSoundPlay(Enumerations.enSfxAndPfx.KarteBewegen);
        //Mana -= ablegen.kosten;
        SetMana(-ablegen.kosten);
        if (ablegen is ZauberKarte){
            Kartebewegen(hand, ablegen, ablagestapel);
            onCardDeath(ablegen);
        }
        else
            Kartebewegen(hand, ablegen, feld);
            
        onCardHandChange();
    }

    //wird aufgerufen wenn eine Kreatur auf dem Feld zerstört wird
    public void Kreaturzerstört(KreaturKarte besiegt, KreaturChip chip)
    {
        GameManager.s_instance.letSoundPlay(Enumerations.enSfxAndPfx.KarteBewegen);

        List<AusruestungsKarte> templist = chip.removeequip();
        foreach (AusruestungsKarte equip in templist)
            Kartebewegen(feld, equip, ablagestapel);
        Kartebewegen(feld, besiegt, ablagestapel);
        onCardDeath(besiegt);
    }

    //Temp funktion um zwichen den beiden Handkarten zu wechslen
    public void AktivateHand(bool input)
    {
        foreach (Karten element in hand)
        {
            element.gameObject.SetActive(input);
        }
    }

    //Zum Ablgen von Karten aus der Hand um Mana zu erhalten
    public void Ablegen(Karten ablegen)
    {
        if (Mana == MaxMana)
			SetMana(MaxMana);
			//Mana = MaxMana;
        else
			SetMana(1);
        //Mana += 1;

        GameManager.s_instance.letSoundPlay(Enumerations.enSfxAndPfx.KarteBewegen);

        Kartebewegen(hand, ablegen, ablagestapel);
        onCardDeath(ablegen);
        onCardHandChange();
    }
		
    void onCardDeath(Karten ablegen)
    {
		UIManagerController.s_instance.onCardDead(ablegen.gameObject.GetComponent<Renderer>().sharedMaterials[2].mainTexture);
    }

    void onCardHandChange()
    {
		UIManagerController.s_instance.onChangePlayerCardHand(hand);     
    }
}