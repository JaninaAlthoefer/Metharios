using UnityEngine;
using System.Collections;

public class onAblegenScript : MonoBehaviour {

    public Karten karte;

 public void ablegen()
    {
		GameManager.s_instance.onAblegen(karte);
        this.transform.parent.gameObject.SetActive(false);
    }
}
