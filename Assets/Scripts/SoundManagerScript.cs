using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SoundManagerScript : MonoBehaviour {

	[Header("AudioSource")]
	[Space]
	public AudioSource[] efxSource;                     //Drag a reference to the audio source which will play the sound effects.
    private int num, point = 0;
    public AudioSource startSource;
    public AudioSource musicSource;                 	//Drag a reference to the audio source which will play the music.
    public AudioSource helperSource;

    public static SoundManagerScript instance = null;   //Allows other scripts to call functions from SoundManager.             

    [Header("used Clips")]
    [Space]
    public AudioClip startClip;
    public AudioClip gameClip;
    public AudioClip pauseClip;
    [Space]
    public AudioClip nextClip;

    [Header("PitchRange")]
	[Space]
	public float lowPitchRange = .95f;              	//The lowest a sound effect will be randomly pitched.
	public float highPitchRange = 1.05f;            	//The highest a sound effect will be randomly pitched.

	[Header("OptionsSlider")]
	[Space]

	public Slider VolMusic;
	public Slider VolSfx;

    private float efxVolume = 1.0f;
    private float musicVolume = 1.0f;


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

        num = efxSource.Length;
	}

	public void Start()
	{
		VolMusic.onValueChanged.AddListener (delegate {
			ValueChangeVolMusic ();
		});

		VolSfx.onValueChanged.AddListener (delegate {
			ValueChangeVolSfx ();
		});

	}

	//Used to play single sound clips.
	public void PlaySingle(AudioClip clip)
	{
        if (clip == null)
            return;

        float randomPitch = Random.Range(lowPitchRange, highPitchRange);

        //Set the clip of our efxSource audio source to the clip passed in as a parameter.
        efxSource[point].clip = clip;
        efxSource[point].pitch = randomPitch;
        //Play the clip.
        efxSource[point].Play ();

        point = (point + 1) % num;
	}


	//RandomizeSfx chooses randomly between various audio clips and slightly changes their pitch.
	public void RandomizeSfx (params AudioClip[] clips)
	{
		//Generate a random number between 0 and the length of our array of clips passed in.
		int randomIndex = Random.Range(0, clips.Length);

		//Choose a random pitch to play back our clip at between our high and low pitch ranges.
		float randomPitch = Random.Range(lowPitchRange, highPitchRange);

		//Set the pitch of the audio source to the randomly chosen pitch.
		efxSource[point].pitch = randomPitch;

		//Set the clip to the clip at our randomly chosen index.
		efxSource[point].clip = clips[randomIndex];

		//Play the clip.
		efxSource[point].Play();

        point = (point + 1) % num;
    }

	public void ValueChangeVolMusic()
	{
        musicVolume = VolMusic.value;

        if (startSource.volume >= 0.01f)
        {
            startSource.volume = musicVolume;
        }
        else
        {
            if (helperSource.volume <= 0.01f)
                musicSource.volume = musicVolume;
            else
                helperSource.volume = musicVolume;

        }

    }

	public void ValueChangeVolSfx()
	{
        efxVolume = VolSfx.value;
		
        foreach (AudioSource efx in efxSource)
        {
            efx.volume = efxVolume;
        }
	}


    //should better use audioclip for fading
    public IEnumerator FadeOutBGM(AudioSource audioSource)
    {
        audioSource.loop = true;
        float startVol = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= (startVol * Time.deltaTime) / 2.0f;

            yield return null;
        }

        audioSource.volume = 0.0f;
        if (audioSource == startSource)
        {
            audioSource.Stop();
            audioSource.volume = musicVolume;
        }
    }

    //should better use audioclip for fading
    public IEnumerator FadeInBGM(AudioSource audioSource)
    {
        audioSource.Play();
        audioSource.volume = 0.0f;
        audioSource.loop = true;
        float endVol = musicVolume;

        while (audioSource.volume < 1.0f)
        {
            audioSource.volume += (endVol * Time.deltaTime) / 2.0f;

            yield return null;
        }

        audioSource.volume = endVol;
    }


    public void changeBGMToGame()
    {
        StartCoroutine(FadeOutBGM(startSource));
        
        helperSource.volume = 0.0f;
        helperSource.clip = pauseClip;
        helperSource.Play();

        musicSource.volume = musicVolume;
        musicSource.clip = gameClip;

        StartCoroutine(FadeInBGM(musicSource));
    }

    public void changeBGMToMainMenu()
    {
        musicSource.Stop();
        helperSource.Stop();

        startSource.Play();

        point = 0;
    }

    public void toggleGamePauseMusic()
    {
        if (helperSource.volume <= 0.01f)
        {
            musicSource.volume = 0.0f;
            helperSource.volume = musicVolume;
        }
        else if (musicSource.volume <= 0.01f)
        {
            musicSource.volume = musicVolume;
            helperSource.volume = 0.0f;
        }
    }

    public void ChangePlayerSfx()
    {
        efxSource[point].clip = nextClip;
        efxSource[point].Play();

        point = (point + 1) % num;
    }
}