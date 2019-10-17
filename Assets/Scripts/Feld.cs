using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Feld : MonoBehaviour {

    public List<Feld> Nachbarn;
    public GameManager gesamt;
    private bool moveTarget;
    private bool attackTarget;
    private bool summontarget;
    private bool Spelltarget;
    private bool spellcaster;
    private bool hit;
    private bool spezialtarget;
    private Behaviour signal;
    public KreaturChip Kreatur;
    private int leftmove;
    private int timer;


    
    void Start () {
        Nachbarn.Clear();
        Collider[] colliders;
        if ((colliders = Physics.OverlapSphere(transform.position, 1f)).Length > 1)
        {
            foreach (var collider in colliders)
            {
                GameObject neigbor = collider.gameObject;
                if (neigbor.GetComponent<Feld>() != null&&neigbor.GetComponent<Feld>() != this)
                {
                    Nachbarn.Add(neigbor.GetComponent<Feld>());
                }
            }
        }
        moveTarget = false;
        attackTarget = false;
        summontarget = false;
        signal = (Behaviour)GetComponent("Halo");
        signal.enabled = false;
        if (this.gameObject.tag == "Energy_Feld")
            timer = 3;
	}

    public void timeOut()
    {
        Debug.Log("timeOut: " + timer);
        timer -= 1;
        if (timer == 0)
        {
            bool temp = false;
            if (gameObject.tag == "Energy_Feld")
                temp = true;
            gesamt.noMoreEnergie(this, temp);
        }
    }

    //Beginnt die suche nach möglichen zielen um sich hin zu bewegen
    public void MoveStart(int beweg)
    {
        //Debug.Log("MoveStart beweg:" + beweg);
        gesamt.moving = Kreatur;
        foreach (Feld element in Nachbarn)
        {
            
            if (element.Kreatur != null)
            {
                if(checkattackable(element,Kreatur.Reichweite))
                    element.attackierbar();
                if (element.Kreatur.Player == gesamt.moving.Player && gesamt.moving.checkSpezial(10)&&gesamt.moving.Spezialused==false)
                {                                                                   //heilen - kroko
                    element.spezialtarget = true;
                    element.signal.enabled = true;
                }
            }
            else if ((gesamt.checkSpezial(2)||element.gameObject.tag != "Block_Feld")&&!gesamt.moving.checkeffekt(2))
            {                   //Fliegen                                                       //festhalten
                if (Kreatur.Reichweite == 1)
                    element.Charge(beweg - 1);
                else
                    element.MoveCountinue(beweg - 1, Kreatur.Reichweite - 1);
            }             
        }
    }

    public void Durchschuss(int Reichweite)
    {
        foreach (Feld element in Nachbarn)
        {
            element.signal.enabled = true;
            element.Spelltarget = true;
            element.leftmove = Reichweite - 1;
        }
    }

    public void destroyField()
    {

        gesamt.Felder.Remove(this);
        foreach (Feld element in Nachbarn)
        {
            element.Nachbarn.Remove(this);
        }
        //Destroy(gameObject);
        DestroyImmediate(gameObject, true);
    }

    //Wird nur bei der Hauptkreatur aufgerufen, zeigt alle unbesetzten Nachbarfelder als mögliche beschwörungsziele an
    public void Beschwoerung()
    {
        foreach (Feld element in Nachbarn)
        {
            if (element.Kreatur == null&& element.tag != "Block_Feld")
            {
                element.signal.enabled = true;
                element.summontarget = true;
            }
        }
    }

    //Setzt ein Feld als möglichen Spellcaster
    public void setSpellCaster()
    {
        spellcaster = true;
        signal.enabled = true;
    }

    //findet Ziele für einen Zauber
    public void findspellTarget(int reich)
    {
        if (gesamt.Spell.spezial == 5) //Eisblock
        {
            Zauberwirken();
        }
        else
        {
            foreach (Feld element in Nachbarn)
            {
                //Gegner sind stehts ein Ziel                                                             //goldenes blatt              //Eisblock                        //Feld ändern             //Eiswand  
                if (element.Kreatur != null && element.Kreatur.Player != gesamt.Spell.Player && !Kreatur.checkSpezial(11) && !Kreatur.checkeffekt(5) && gesamt.Spell.spezial != 3 && gesamt.Spell.spezial != 6)
                {
                    element.signal.enabled = true;
                    element.Spelltarget = true;
                }
                // führt die suche weiter fort
                if (element.gameObject.tag != "Block_Feld" && element.Kreatur == null && reich >= 1)
                {
                    //Flächenzauber könne auch auf leere Felder gesetzt werden
                    if (gesamt.Spell.flaeche > 0 || gesamt.Spell.spezial == 3 || gesamt.Spell.spezial == 6)
                    {                                   //FieldChange                       //Eiswand
                        element.signal.enabled = true;
                        element.Spelltarget = true;
                    }
                    element.findspellTarget(reich - 1);
                }
            }
        }
    }

    //Sucht nach weiteren Zeilen zur bewegung und Fernkampfziele
    void MoveCountinue(int beweg,int reich)
    {
        if (checkmovable(beweg))
        {
            moveTarget = true;
            signal.enabled = true;
        }
        if (beweg > 0||reich > 0)
        {
            foreach (Feld element in Nachbarn)
            {
                if (element.Kreatur != null)
                {
                    if (checkattackable(element,reich))
                            element.attackierbar();
                }           
                            //fliegen
                if ((gesamt.checkSpezial(2) || (element.gameObject.tag != "Block_Feld" && element.Kreatur == null)) && beweg > 0)
                    element.MoveCountinue(beweg - 1,reich - 1);
                else if (gesamt.checkSpezial(1) && (element.gameObject.tag == "Block_Feld"|| element.Kreatur != null))
                {                   //Überschuss - hase
                    element.MoveCountinue(-1, reich - 1);
                }
            }
        }
    }

    public void findSpezialTarget(bool enemy)
    {
        hit = true;
        foreach (Feld element in Nachbarn)
        {

            //Wenn die Kreatur auf der seite des spielenden Spielers ist
            if (element.Kreatur != null && (element.Kreatur.Player == gesamt.moving.Player) != enemy)
            {
                if (element.Kreatur.Aktiondone == false)
                { 
                    element.spezialtarget = true;
                    signal.enabled = true;
                }
            }
            //Wenn die Kreatur ein Gegner ist
            if (element.Kreatur != null && (element.Kreatur.Player == gesamt.moving.Player) == enemy)
            {
                element.spezialtarget = true;
                signal.enabled = true;
            }
            if (element.hit == false)
                element.findSpezialTarget(enemy);
        }
    }

    private bool checkattackable(Feld element, int reich)
    {
        if (gesamt.moving.Aktiondone == true)
            return false;

        if (element.Kreatur.Player != gesamt.moving.Player)
        {
            if (gesamt.moving.Reichweite == 1 || reich >= 0)
            {
                if (!(element.Kreatur.checkSpezial(8) && element.Kreatur.getCount() > 2))
                {                   //unsichtbar
                    if (!(element.Kreatur.checkeffekt(5)))
                    {                   //Eisblock
                        if (!(gesamt.moving.checkSpezial(4) && gesamt.moving.Spezialused == true))
                        {                   //MoveAttack
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    private bool checkmovable(int beweg)
    {
        if (beweg >= 0)
        {
            if (gameObject.tag != "Block_Feld")
            {
                if (Kreatur == null)
                {
                    return true;
                }
            }
        }
        return false;
    }

    //sucht nach weiterem Zielen zur bewegung für Nahkämpfer
    void Charge(int beweg)
    {
        //Debug.Log("Charge");
        if (checkmovable(beweg))
        {
            moveTarget = true;
            signal.enabled = true;
        }
        if (beweg >= leftmove)
        {
            leftmove = beweg;
        }
        if (beweg >= 0)
        {
            foreach (Feld element in Nachbarn)
            {
                if (element.Kreatur != null)
                {
                    if (checkattackable(element, -1))
                        element.attackierbar();
                }
                               //fliegen
                if ((gesamt.checkSpezial(2)||(element.gameObject.tag != "Block_Feld" && element.Kreatur == null)) && beweg > 0)
                    element.Charge(beweg - 1);
            }
        }
    }

    //beendet aktion
    public void MoveEnd()
    {
        //Debug.Log("feld.moveend");

        attackTarget = false;
        moveTarget = false;
        summontarget = false;
        spellcaster = false;
        Spelltarget = false;
        spezialtarget = false;
        hit = false;
        signal.enabled = false;
        leftmove = 0;
    }

    public void nachbarNeuZuweisung()
    {
        Nachbarn.Clear();
        Collider[] colliders;
        if ((colliders = Physics.OverlapSphere(transform.position, 1f)).Length > 1)
        {
            foreach (var collider in colliders)
            {
                GameObject neigbor = collider.gameObject;
                if (neigbor.GetComponent<Feld>() != null && neigbor.GetComponent<Feld>() != this)
                {
                    Nachbarn.Add(neigbor.GetComponent<Feld>());
                }
            }
        }
        
    }

    //Setzt Spellcaster auf false
    public void SpellCasterSet()
    {
        spellcaster = false;
        signal.enabled = false;
    }

    void OnMouseUpAsButton()
    {
        //Bewegt Kreatur auf dieses Feld
        if (moveTarget == true)
            gesamt.Move(this);
        //Beschwört Kreatur auf dieses Feld
        if (summontarget == true)
            gesamt.Beschwoerung(this);
        //Wirkt Zauber auf dieses Feld
        if (Spelltarget == true)
        {
            if (gesamt.Spell.spezial == 1) // Eislanze
            {
                if (Kreatur != null)
                {
                    GameManager.s_instance.letSoundPlay(Enumerations.enSfxAndPfx.Eislanze);
                    GameManager.s_instance.letParticlePlayMonster(Enumerations.enSfxAndPfx.Eislanze, 3, Kreatur);

                    gesamt.moving.Aktiondone = true;

                    SchadenNehmen(gesamt.moving.Magie + gesamt.Spell.Schaden);
                }
                if (leftmove > 0)
                {
                    int temp = leftmove;
                    gesamt.allMoveEnd();
                    Durchschuss(temp);
                }
                else
                {
                    gesamt.GetAktuellSpieler().KarteGespielt(gesamt.Spell);
                    gesamt.actionend();
                }
            }
            else
            {
                Zauberwirken();
                gesamt.moving.Aktiondone = true;
            }

        }
    }

    public void attacke()
    {
        //Attackiert ziel
        if (attackTarget == true)
        {
            //wurde ein Nahkämpfer ohne Chargefeld ausgewählt, so wird nichts getan
            if (gesamt.ChargeFeld == null && gesamt.moving.Reichweite == 1)
            { }
            else
            {
                //War der nutzer ein Nahkämpfer wird auf das Chargefeld bewegt
                if (gesamt.ChargeFeld != null)
                {
                    if (gesamt.moving.checkSpezial(4)) // ATTbeiBewegen
                    {

                        gesamt.moving.getPlatzFeld().Kreatur = null;
                        gesamt.moving.Platzfeld = gesamt.ChargeFeld;
                        gesamt.moving.MoveTo();
                        int temp = gesamt.ChargeFeld.leftmove;
                        gesamt.allMoveEnd();
                        gesamt.moving.Spezialused = true;
                        gesamt.ChargeFeld.MoveStart(temp);
                    }
                    else
                    {
                        Debug.Log("Feld AttbB else");

                        gesamt.MoveToFeld(gesamt.ChargeFeld);
                        gesamt.moving.Aktiondone = true;
                    }
                }

                if (gesamt.moving.checkSpezial(8)) //unsichtbar -> Farnchamäleon 
                    gesamt.moving.resetCount();
                if (gesamt.moving.checkSpezial(3)) //Rückstoss -> Schneebison
                {
                    GameManager.s_instance.letSoundPlay(Enumerations.enSfxAndPfx.Nahkampf);
                    GameManager.s_instance.letParticlePlayMonster(Enumerations.enSfxAndPfx.Nahkampf, 2, Kreatur);

                    //Debug.Log("left move: " + gesamt.ChargeFeld.leftmove + " gesamt.moving.Bewegung: " + gesamt.moving.Bewegung + " Schaden: " + (gesamt.moving.Angriff + ((gesamt.moving.Bewegung - gesamt.ChargeFeld.leftmove) * 5)));
                    SchadenNehmen(gesamt.moving.Angriff + ((gesamt.moving.Bewegung - gesamt.ChargeFeld.leftmove) * 5));
                }
                else if (gesamt.moving.checkSpezial(5)) //SwipeATT -> KönigdesWaldes
                {
                    GameManager.s_instance.letSoundPlay(Enumerations.enSfxAndPfx.Nahkampf);
                    GameManager.s_instance.letParticlePlayMonster(Enumerations.enSfxAndPfx.Nahkampf, 2, Kreatur);

                    SchadenNehmen(gesamt.moving.Angriff);
                    foreach (Feld element in Nachbarn)
                    {
                        foreach (Feld element2 in gesamt.moving.getPlatzFeld().Nachbarn)
                        {
                            if (element == element2)
                            {
                                GameManager.s_instance.letSoundPlay(Enumerations.enSfxAndPfx.Nahkampf);
                                GameManager.s_instance.letParticlePlayMonster(Enumerations.enSfxAndPfx.Nahkampf, 2, Kreatur);

                                element.SchadenNehmen(gesamt.moving.Angriff);
                            }
                        }
                    }
                }
                else if (gesamt.moving.checkSpezial(12) && gesamt.moving.Spezialused == true)
                {                       //Counter -> Kristallhirsch
                    GameManager.s_instance.letSoundPlay(Enumerations.enSfxAndPfx.Fernkampf);
                    GameManager.s_instance.letParticlePlayMonster(Enumerations.enSfxAndPfx.Fernkampf, 2, Kreatur);

                    gesamt.moving.addCount();
                    SchadenNehmen(gesamt.moving.maxLeben - gesamt.moving.Leben);
                }
                else //nahkampf oder eierwurfhase
                {
                    if (gesamt.moving.Spezial == 1) //eierwurfhase
                    {
                        GameManager.s_instance.letSoundPlay(Enumerations.enSfxAndPfx.Fernkampf);
                        GameManager.s_instance.letParticlePlayMonster(Enumerations.enSfxAndPfx.Fernkampf, 2, Kreatur);
                    }
                    else
	                {
                        GameManager.s_instance.letSoundPlay(Enumerations.enSfxAndPfx.Nahkampf);
                        GameManager.s_instance.letParticlePlayMonster(Enumerations.enSfxAndPfx.Nahkampf, 2, Kreatur);
                    }

                    SchadenNehmen(gesamt.moving.Angriff);
                }
                    
                //War bis jetzt die Beschwöungsphase wird diese beendet
                if (gesamt.getPhase() == 0)
                {
                    gesamt.nextPhase();
                }
                else
                {
                    if (gesamt.NoMoreMovment())
                        gesamt.nextPhase();
                }
                //if (!(gesamt.moving.checkSpezial(4) && gesamt.moving.Spezialused == true))
                    //gesamt.actionend(); //ATTbeiBewegen
                if (gesamt.spezial != null && gesamt.spezial.checkSpezial(9))
                {                           // Wiederkehr -> Eisphönix
                    gesamt.findSpellCaster(gesamt.spezial.Player);
                }
                gesamt.moving.Aktiondone = true;
                gesamt.actionend();
            }
        }
        else
        {

            //War der Gegner ziel eines Zaubers
            if (Spelltarget == true)
            {
                Zauberwirken();
                gesamt.moving.Aktiondone = true;
            }
            else
            {

                if (spezialtarget == true)
                {
                    //Platztauschen
                    if (gesamt.moving.checkSpezial(7)) //Tauschen -> Eiskrabbler
                    {
                        GameManager.s_instance.letSoundPlay(Enumerations.enSfxAndPfx.Teleport);
                        GameManager.s_instance.letParticlePlayMonster(Enumerations.enSfxAndPfx.Teleport, 1, Kreatur);
                        GameManager.s_instance.letParticlePlayMonster(Enumerations.enSfxAndPfx.Teleport, 1, gesamt.moving);

                        gesamt.Move(Kreatur, gesamt.moving.getPlatzFeld());
                        gesamt.Move(this);

                        //gesamt.moving.Aktiondone = true;
                    }
                    else if (gesamt.moving.checkSpezial(10)) //Heilen -> Rettungskroko
                    {
                        GameManager.s_instance.letSoundPlay(Enumerations.enSfxAndPfx.Nahkampf);
                        GameManager.s_instance.letParticlePlayMonster(Enumerations.enSfxAndPfx.Nahkampf, 2, Kreatur);

                        GameManager.s_instance.letSoundPlay(Enumerations.enSfxAndPfx.Heilen);
                        GameManager.s_instance.letParticlePlayMonster(Enumerations.enSfxAndPfx.Heilen, 2, Kreatur);

                        gesamt.moving.Spezialused = true;
                        gesamt.moving.Platzfeld.SchadenNehmen(10);
                        Kreatur.Leben += 10;
                        gesamt.actionend();
                    }
                    else if (gesamt.moving.checkSpezial(14))
                    {
                        //Eishauch -> Eisdrache
                    }
                }
            }
        }
    }

    void Zauberwirken()
    {
        if (gesamt.Spell.flaeche > 0)
        { // größere spellfläche
            int flaeche = gesamt.Spell.flaeche;
            if (gesamt.moving.checkSpezial(6)) //Buntfederhörnchen
                flaeche += 1;
            AoE(flaeche);
        }
        else
        {
            if (gesamt.Spell.spezial == 2)
            { //festhalten -> Wucherwurzeln
                GameManager.s_instance.letSoundPlay(Enumerations.enSfxAndPfx.Wucherwurzeln);
                GameManager.s_instance.letParticlePlayMonster(Enumerations.enSfxAndPfx.Wucherwurzeln, 2, Kreatur);

                Kreatur.effekte.Add(gesamt.Spell);
                int temp = gesamt.moving.Magie / 10;
                if (gesamt.moving.Magie % 10 == 5)
                    temp += 1;
                Kreatur.effektzeit.Add(temp);
            }
            else if(gesamt.Spell.spezial == 3)
            { //Feld ändern -> Schnellwachstum, Eishauch
                changeFieldTyp(gesamt.Spell.ändern);
            }
            else if (gesamt.Spell.spezial == 6)
            { // feld temporär ändern -> Eiswand
                changeFieldTyp(gesamt.Spell.ändern);
                timer = gesamt.moving.Magie / 5;
            }
            else if (gesamt.Spell.spezial == 5)
            { // Eisblock
                GameManager.s_instance.letSoundPlay(Enumerations.enSfxAndPfx.Eisblock);
                GameManager.s_instance.letParticlePlayMonster(Enumerations.enSfxAndPfx.Eisblock, 2, Kreatur);

                Kreatur.effekte.Add(gesamt.Spell);
                Kreatur.effektzeit.Add(2);
            }
            else // REST!!!
            {
                int spellCost = gesamt.Spell.kosten;
                int spellDamage = gesamt.Spell.Schaden;

                switch (spellCost)
                {
                    case 4:
                        if (spellDamage == 0)
                        {
                            GameManager.s_instance.letSoundPlay(Enumerations.enSfxAndPfx.Blattgeschoss);
                            GameManager.s_instance.letParticlePlayMonster(Enumerations.enSfxAndPfx.Blattgeschoss, 2, Kreatur);
                        }
                        else
                        {
                            GameManager.s_instance.letSoundPlay(Enumerations.enSfxAndPfx.Rankenschlag);
                            GameManager.s_instance.letParticlePlayMonster(Enumerations.enSfxAndPfx.Rankenschlag, 2, Kreatur);
                        }
                        break;
                    case 6:
                        if (spellDamage == 15)
                        {
                            GameManager.s_instance.letSoundPlay(Enumerations.enSfxAndPfx.Eislanze);
                            GameManager.s_instance.letParticlePlayMonster(Enumerations.enSfxAndPfx.Eislanze, 2, Kreatur);
                        }
                        else
                        {
                            GameManager.s_instance.letSoundPlay(Enumerations.enSfxAndPfx.Eisadler);
                            GameManager.s_instance.letParticlePlayMonster(Enumerations.enSfxAndPfx.Eisadler, 2, Kreatur);
                        }
                        break;
                    case 8:
                        GameManager.s_instance.letSoundPlay(Enumerations.enSfxAndPfx.Eispfeil);
                        GameManager.s_instance.letParticlePlayMonster(Enumerations.enSfxAndPfx.Eispfeil, 2, Kreatur);
                        break;
                    case 9:
                        if (spellDamage == 0)
                        {
                            GameManager.s_instance.letSoundPlay(Enumerations.enSfxAndPfx.Blizzard);
                            GameManager.s_instance.letParticlePlayMonster(Enumerations.enSfxAndPfx.Blizzard, 2, Kreatur);
                        }
                        else
                        {
                            GameManager.s_instance.letSoundPlay(Enumerations.enSfxAndPfx.ExplosiveSamen);
                            GameManager.s_instance.letParticlePlayMonster(Enumerations.enSfxAndPfx.ExplosiveSamen, 2, Kreatur);
                        }
                        break;
                }

                SchadenNehmen(gesamt.moving.Magie + gesamt.Spell.Schaden);
            }
        }

        if (gesamt.moving.checkeffekt(4))
        { // MoveAttack
            gesamt.moving.timeoutEffekt(4);
        }
        else
        {
            gesamt.GetAktuellSpieler().KarteGespielt(gesamt.Spell);
            gesamt.actionend();
        }
    }

    //Ermöglicht Flächeneffekt
    void AoE(int flaeche)
    {
        hit = true;
        if (Kreatur != null && !Kreatur.checkeffekt(5)) //Eisblock
        {
            int spellCost = gesamt.Spell.kosten;
            int spellDamage = gesamt.Spell.Schaden;

            switch (spellCost)
            {
                case 4:
                    if (spellDamage == 0)
                    {
                        GameManager.s_instance.letSoundPlay(Enumerations.enSfxAndPfx.Blattgeschoss);
                        GameManager.s_instance.letParticlePlayMonster(Enumerations.enSfxAndPfx.Blattgeschoss, 2, Kreatur);
                    }
                    else
                    {
                        GameManager.s_instance.letSoundPlay(Enumerations.enSfxAndPfx.Rankenschlag);
                        GameManager.s_instance.letParticlePlayMonster(Enumerations.enSfxAndPfx.Rankenschlag, 2, Kreatur);
                    }
                    break;
                case 6:
                    if (spellDamage == 15)
                    {
                        GameManager.s_instance.letSoundPlay(Enumerations.enSfxAndPfx.Eislanze);
                        GameManager.s_instance.letParticlePlayMonster(Enumerations.enSfxAndPfx.Eislanze, 2, Kreatur);
                    }
                    else
                    {
                        GameManager.s_instance.letSoundPlay(Enumerations.enSfxAndPfx.Eisadler);
                        GameManager.s_instance.letParticlePlayMonster(Enumerations.enSfxAndPfx.Eisadler, 2, Kreatur);
                    }
                    break;
                case 8:
                    GameManager.s_instance.letSoundPlay(Enumerations.enSfxAndPfx.Eispfeil);
                    GameManager.s_instance.letParticlePlayMonster(Enumerations.enSfxAndPfx.Eispfeil, 2, Kreatur);
                    break;
                case 9:
                    if (spellDamage == 0)
                    {
                        GameManager.s_instance.letSoundPlay(Enumerations.enSfxAndPfx.Blizzard);
                        GameManager.s_instance.letParticlePlayMonster(Enumerations.enSfxAndPfx.Blizzard, 2, Kreatur);
                    }
                    else
                    {
                        GameManager.s_instance.letSoundPlay(Enumerations.enSfxAndPfx.ExplosiveSamen);
                        GameManager.s_instance.letParticlePlayMonster(Enumerations.enSfxAndPfx.ExplosiveSamen, 2, Kreatur);
                    }
                    break;
            }

            SchadenNehmen(gesamt.moving.Magie + gesamt.Spell.Schaden);
        }
        if (flaeche > 0)
        {
            foreach (Feld element in Nachbarn)
            {
                if(element.hit ==false)
                    element.AoE(flaeche - 1);
            }
        }
        gesamt.moving.Aktiondone = true;
    }

    //siganlisisert ein Feld als attackierbar
    void attackierbar()
    {
        attackTarget = true;
        signal.enabled = true;
    }

    //wählt ein Chargefeld
    void OnMouseDown()
    {
        if (gesamt.moving != null && gesamt.moving.Reichweite == 1)
             gesamt.ChargeFeld = this;
    }

    void OnMouseUp()
    {
        Debug.Log("Feld mouseUp");
        if (gesamt.AttackedField != null)
            gesamt.AttackedField.attacke();
    }

    //verursacht Schaden an der Kreatur auf dem Feld
    public void SchadenNehmen(int schaden)
    {
        if (Kreatur != null)
        {
            Kreatur.Leben -= schaden;
            KreaturChip.s_instance.ShowDamageTaken(Kreatur, schaden);

            if (Kreatur.anfuehrer == true)
                UIManagerController.s_instance.onLiveChangeEnemy((Kreatur.Leben < 0) ? 0 : Kreatur.Leben);

            if (Kreatur.Leben <= 0)
            {
                if (Kreatur.checkSpezial(9))
                {
                    gesamt.spezial = Kreatur;
                }
                else
                {
                    GameManager.s_instance.letSoundPlay(Enumerations.enSfxAndPfx.KreaturKO);
                    GameManager.s_instance.letParticlePlayMonster(Enumerations.enSfxAndPfx.KreaturKO, 2, Kreatur);
                    kreaturZerstören();
                }
            }
        }
    }

    public void kreaturZerstören() //war void
    {
        foreach (KreaturKarte element in Kreatur.Player.feld)
        {
            if (element.Kreatur == Kreatur)
                Kreatur.Player.Kreaturzerstört(element, Kreatur);
        }
        Destroy(Kreatur.gameObject);

        if (Kreatur.anfuehrer == true)
        {
            StartCoroutine(waitingForQuit());
        }
    }

    public IEnumerator waitingForQuit()
    {
        UIManagerController.s_instance.GameEndMeldung();
        //Debug.Log("gameended");
        yield return new WaitForSeconds(5);
        MenuManager.instance.Quit();
    }

    public void changeFieldTyp(Feld replacer)
    {
        GameObject tempobject = (GameObject)Instantiate(replacer.gameObject, gameObject.transform.position, gameObject.transform.rotation);
        Feld tempfeld = tempobject.GetComponent<Feld>();

        if (gesamt.Spell.spezial == 6)
        {
            GameManager.s_instance.letSoundPlay(Enumerations.enSfxAndPfx.Eiswand);
            GameManager.s_instance.letParticlePlayField(Enumerations.enSfxAndPfx.Eiswand, 2, this);
        }
        else
        {
            GameManager.s_instance.letSoundPlay(Enumerations.enSfxAndPfx.FieldChange);
            GameManager.s_instance.letParticlePlayField(Enumerations.enSfxAndPfx.FieldChange, 2, this);
        }


        if (Kreatur != null)
        {         
            tempfeld.Kreatur = Kreatur;
            tempfeld.Kreatur.Platzfeld = tempfeld;
            tempfeld.Kreatur.MoveTo();
        }
        gesamt.Felder.Add(tempfeld);
        destroyField();
        gesamt.allFelderNachbarnNeuzuweisung();
    }

    //sagt an ob auf dem Feld ein möglicher Spellcaster ist
    public bool getSpellCaster()
    {
        return spellcaster;
    }
}
