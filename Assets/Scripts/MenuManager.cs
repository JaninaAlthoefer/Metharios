using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {

	[Header("MenuText Objects")]
	[Space]
	public GameObject OptionsPanel;		//Store a reference to the Game Object OptionsPanel 
	public GameObject MainMenu;			//Store a reference to the Game Object MainMenu 
	public GameObject PauseMenu;		//Store a reference to the Game Object PauseMenu
	public GameObject EinstellungsPanel;
    [Space]
    public Dropdown dropDownMenu;

	[Header("FadingScreen")]
	[Space]
	public CanvasGroup FadingScreen;

	bool inMainMenu = true;
	private bool isPaused = false;
	public static MenuManager instance = null;   //Allows other scripts to call functions from SoundManager.   


	void Awake ()
	{
		//Check if there is already an instance of SoundManager
		if (instance == null)
			//if not, set it to this.
			instance = this;
		//If instance already exists:
		else if (instance != this)
			//Destroy this, this enforces our singleton pattern so there can only be one instance of SoundManager.
			Destroy (gameObject);

		//Set SoundManager to DontDestroyOnLoad so that it won't be destroyed when reloading our scene.
		DontDestroyOnLoad (gameObject);
	}


	//-------------------------------- Show and Hide Methoden

	//Call this function to activate and display the Options panel during the main menu
	public void ShowOptionsPanel()
	{
		OptionsPanel.SetActive(true);
		HidePauseMenu();
		HideMainMenu();
	}

	//Call this function to deactivate and hide the Options panel during the main menu
	public void HideOptionsPanel()
	{
		OptionsPanel.SetActive(false);

		if (inMainMenu) 
		{
			ShowMainMenu ();
		}else
		{
			ShowPauseMenu();
		}
	}

	//Call this function to activate and display the Options panel during the main menu
	public void ShowEinstellungsPanel()
	{
		EinstellungsPanel.SetActive(true);
		HideMainMenu ();
	}

	//Call this function to deactivate and hide the Options panel during the main menu
	public void HideEinstellungsPanel()
	{
		EinstellungsPanel.SetActive(false);
	}


	//Call this function to activate and display the main menu panel during the main menu
	public void ShowMainMenu()
	{
		MainMenu.SetActive (true);
		FadingScreen.alpha = 1.0f;
	}

	//Call this function to deactivate and hide the main menu panel during the main menu
	public void HideMainMenu()
	{
		MainMenu.SetActive (false);
	}

	//Call this function to activate and display the Pause panel during game play
	public void ShowPauseMenu()
	{
		PauseMenu.SetActive (true);
		isPaused = true;
        SoundManagerScript.instance.toggleGamePauseMusic();
	}

	//Call this function to deactivate and hide the Pause panel during game play
	public void HidePauseMenu()
	{
		PauseMenu.SetActive (false);
		isPaused = false;
        SoundManagerScript.instance.toggleGamePauseMusic();
    }


	//-------------------------------- Quit and Update

	public void Quit()
	{
		if (!inMainMenu)
        {
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            SoundManagerScript.instance.changeBGMToMainMenu();
		}
        else 
		{
			//If we are running in a standalone build of the game
			#if UNITY_STANDALONE
			//Quit the application
			Application.Quit();
			#endif

			//If we are running in the editor
			#if UNITY_EDITOR
			//Stop playing the scene
			UnityEditor.EditorApplication.isPlaying = false;
			#endif
		}
	}

	// Update is called once per frame
	void Update () 
	{
		//Check if the Cancel button in Input Manager is down this frame (default is Escape key) and that game is not paused, and that we're not in main menu
		if (Input.GetButtonDown ("Cancel") && !isPaused && !inMainMenu) 
			{
				ShowPauseMenu ();
			} 
		//If the button is pressed and the game is paused and not in main menu
		else if (Input.GetButtonDown ("Cancel") && isPaused && !inMainMenu) 
		{
			HidePauseMenu ();
		}
	}


	//-------------------------------- MainMenu Methoden

	public void FadingScene()
	{
        if (dropDownMenu.value == -1)
        {
            UIManagerController.s_instance.noFieldChosen();
            return;
        }

		inMainMenu = false;
		HideEinstellungsPanel();
		GameManager.s_instance.Setup ();
		StartCoroutine(StartingFade());
        SoundManagerScript.instance.changeBGMToGame();
    }

	private IEnumerator StartingFade()
	{
		float elapsedTime = 0.0f;
		float wait = 6f - 0.5f;

		yield return null;

		while (elapsedTime < wait)
		{
			FadingScreen.alpha = 1.0f - (elapsedTime / wait);
			elapsedTime += Time.deltaTime;

			//sometime, synchronization lag behind because of packet drop, so we make sure our tank are reseted
			if (elapsedTime / wait < 0.5f) 

				yield return null;
		}
	}
}