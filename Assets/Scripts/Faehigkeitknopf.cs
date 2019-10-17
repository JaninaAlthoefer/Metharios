using UnityEngine;
using System.Collections;

public class Faehigkeitknopf : MonoBehaviour {

    public GameManager gesamt;

    void OnMouseUpAsButton()
    {
        gesamt.aktivatespezial();
        Destroy(this.gameObject);
    }
}
