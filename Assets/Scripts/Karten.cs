using UnityEngine;
using System.Collections;

//Überlegende Klasse für alle Karten
public abstract class Karten : MonoBehaviour
{
    public GameManager gesamt;
    public Spieler Player;
    public int kosten;

    public abstract void OnMouseDown();
}
