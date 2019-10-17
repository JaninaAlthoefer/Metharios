using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FieldCreator : MonoBehaviour {

	public static FieldCreator s_Instance;

    public Feld normal;
    public Feld start;
    public Feld block;
    public Feld frost;
    public Feld wald;
    public Feld energie;
    public Feld langsam;
    public float fieldX;
    public float fieldZ;
    public List<int> playertyps;
    public List<Deck> decks;

	void Awake()
	{
		s_Instance = this;
	}
		
	public void Setup () {
		TextAsset Feld = (TextAsset)Resources.Load(UIManagerController.s_instance.Feldname);
		string[] input = Feld.text.Split('\n');
		normal.gesamt = GameManager.s_instance;
		start.gesamt = GameManager.s_instance;
		block.gesamt = GameManager.s_instance;
		frost.gesamt = GameManager.s_instance;
		wald.gesamt = GameManager.s_instance;
		energie.gesamt = GameManager.s_instance;
        langsam.gesamt = GameManager.s_instance;
        GameManager.s_instance.normalFeld = normal;
		GameManager.s_instance.energieFeld = energie;
        List<Feld> startfelder = new List<Feld>();
        float scaleX = (fieldX / 100) * 1.7f;
        float scaleZ = (fieldZ / 100) * 1.5f;
        for (int i = 0; i < input.Length; i++)
        {
            float abweichung;
            if (i % 2 == 0)
                abweichung = 0;
            else
                abweichung = 0.85f;
            for (int n = 0; n < input[i].Length; n++)
            {
                if (input[i][n] == 'n')
                {
                    createField(normal,i,n,abweichung,scaleX,scaleZ);
                }
                if (input[i][n] == 's')
                {
                    startfelder.Add(createField(start,i,n,abweichung,scaleX,scaleZ).GetComponent<Feld>());
                }
                if (input[i][n] == 'b')
                {
                    createField(block, i, n, abweichung,scaleX,scaleZ);
                }
                if (input[i][n] == 'f')
                {
                    createField(frost, i, n, abweichung,scaleX,scaleZ);
                }
                if (input[i][n] == 'w')
                {
                    createField(wald, i, n, abweichung,scaleX,scaleZ);
                }
                if (input[i][n] == 'e')
                {
                    createField(energie, i, n, abweichung,scaleX,scaleZ);
                }
                if (input[i][n] == 'l')
                {
                    createField(langsam, i, n, abweichung, scaleX, scaleZ);
                }
            }
        }

        int m = 0;
        foreach (int typ in playertyps)
        {
            Spieler player = decks[typ].player;// UI Spieler farbcode
            Vector3 temp = startfelder[m].transform.position;
            Quaternion temp1 = new Quaternion(0.7f, 0, 0, -0.7f);
            GameObject tempobject = (GameObject)Instantiate(player.gameObject, temp, temp1);
            KreaturChip tempkreatur = tempobject.GetComponent<KreaturChip>();
			tempkreatur.gesamt = GameManager.s_instance;
            tempkreatur.Platzfeld = startfelder[m];
            startfelder[m].Kreatur = tempkreatur;
            tempkreatur.Player = tempobject.GetComponent<Spieler>();
			player.deck = decks[typ].deck;// UI Spieler farbcode
			player.live = tempkreatur.maxLeben;
			player.Decktyp = typ;// UI Spieler farbcode
			GameManager.s_instance.Spieler.Add(tempobject.GetComponent<Spieler>());
			player.Mana = GameManager.s_instance.startmana;
            m++;
        }
	}

    private GameObject createField(Feld input, int i, int n, float abweichung, float scaleX, float scaleZ)
    {
        Vector3 temp = new Vector3(n * scaleX + abweichung, 0, i * scaleZ);
        Quaternion temp1 = new Quaternion(0.5f, 0, 0, -0.5f);
        GameObject tempobject = (GameObject)Instantiate(input.gameObject, temp, temp1);
		GameManager.s_instance.Felder.Add(tempobject.GetComponent<Feld>());
        return tempobject;
    }
}
