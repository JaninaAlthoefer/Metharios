using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HandScript : MonoBehaviour
{
    public static HandScript instance = null;

    private Karten karte;
    public GameObject HoverCard;


    void Awake()
    {
        HoverCard = GameObject.FindWithTag("HoverCard");
        instance = this;
    }

    public Karten Karte
    {
        get
        {
            return karte;
        }

        set
        {
            karte = value;
        }
    }

    public void hideHoverCard()
    {
        if (HoverCard.activeSelf)
        {
            GameManager.s_instance.letSoundPlay(Enumerations.enSfxAndPfx.KarteBewegen);
            HoverCard.gameObject.SetActive(false);
        }
    }

    public void OnMouseDown()
    {
        GameManager.s_instance.letSoundPlay(Enumerations.enSfxAndPfx.KarteBewegen);

        if (!HoverCard.activeSelf)
        {
            HoverCard.GetComponent<RawImage>().texture = this.gameObject.GetComponent<RawImage>().texture;
            HoverCard.GetComponentInChildren<onAblegenScript>().karte = Karte;
            HoverCard.gameObject.SetActive(true);
        }
        else
        {
            HoverCard.gameObject.SetActive(false);
            karte.OnMouseDown();
        }
    }
}
