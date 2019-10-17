using UnityEngine;
using System.Collections;

public class Spezials : MonoBehaviour {

    public void OnMous()
    {
		GameManager.s_instance.allFelderNachbarnNeuzuweisung();
    }
}
