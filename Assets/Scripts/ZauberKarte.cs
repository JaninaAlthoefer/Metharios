using UnityEngine;
using System.Collections;

public class ZauberKarte :Karten {

    public int Schaden;
    public int Reichweite;
    public int flaeche;
    public int spezial; //1 = Durchschuss, 2 = festhalten, 3 = Feld ändern, 4 = Blizzard, 5 = Eisblock, 6 = temporär feld ändern
    public Feld ändern;

    //Wird aufgerufen wenn auf eine Zauberkarte geklickt wird
    public override void OnMouseDown()
    {
        gesamt.zauberKarteSpielen(this);
    }
}
