using UnityEngine;
using System.Collections;

public class KreaturKarte :Karten {

    public KreaturChip Kreatur;

    //Wird aufgerufen wenn die Karte ausgewählt wird
    public override void OnMouseDown()
    {
        //Es wird überprüft ob der Spieler genug Mana hat und er noch in der Summon phase ist
        gesamt.kreaturkartespielen(this);
    }
}
