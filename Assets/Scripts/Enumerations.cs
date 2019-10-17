using System;

namespace Enumerations
{
	public enum enMeldungen
	{
		Falsche_Phase,
		Kein_Ziel,
		Your_Turn,
		Nicht_genug_Mana,
		Karten_Ablegen,
		Falscher_Spieler
	};

	public enum enPhase
	{
		Beschwoeren,
		Kaempfen,
		Abwerfen,
		Warten
	};

    public enum enSfxAndPfx
    {
        KarteBewegen,
        Beschwoeren, 
        Blattgeschoss,
        Blizzard,
        Eisadler,
        Eisblock,
        Eislanze,
        Eispfeil,
        Eiswand,
        EndTurn,
        Energie,
        ExplosiveSamen,
        Fernkampf,
        FieldChange,
        Heilen,
        KreaturKO,
        Langsam,
        Nahkampf,
        NeuesLeben,
        NextTurn, 
        Rankenschlag,
        Revive,
        Teleport,
        Unsichtbar,
        Wiederkehr,
        Wucherwurzeln
    };
}