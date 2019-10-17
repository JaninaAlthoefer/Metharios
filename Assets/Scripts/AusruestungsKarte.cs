using UnityEngine;
using System.Collections;

public class AusruestungsKarte :Karten {

    public int LebenPlus;
    public int AngriffPlus;
    public int MagiePlus;
    public int ReichweitePlus;
    public int BewegungPlus;
    public int Spezial;

    //wird aufgerufen wenn auf die Ausrüstungskarte gedrückt wird
    public override void OnMouseDown()
    {
        gesamt.ausruestungsKarteSpielen(this);
    }
}
