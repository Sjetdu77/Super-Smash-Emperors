using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MusicManager : MonoBehaviour
{
    public AudioMixer AudioMixer;
    public AudioClip DisplayScreenTheme;
    public AudioClip[] Playlist;
    public AudioSource PlaySource;
    public Texture2D VolumeOn;
    public Texture2D VolumeOff;

    private Button MusicButton;
    private RawImage MusicIcon;
    private bool Activated = true;
    private int MusicIndex = 0;
    private float minTime = 0, maxTime = 0;

    private Fighter Victorious = null;

    static public MusicManager instance = null;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("MusicManager already exists.");
            return;
        }
        instance = this;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        AudioMixer.SetFloat("Volume", Activated ? -3f : -80f);
        PlaySource.clip = Playlist[MusicIndex];
        PlaySource.Play();
    }

    private void Update()
    {
        if (Victorious == null && !PlaySource.isPlaying) PlayNextSong();
        else if (Victorious != null)
        {
            if (!PlaySource.isPlaying)
            {
                minTime = 1f;
                maxTime = 30.1f;
                PlaySource.clip = DisplayScreenTheme;
                PlaySource.Play();
            }
            else if (minTime != 0 && maxTime != 0 && PlaySource.time >= maxTime)
            {
                PlaySource.time = minTime;
                //PlaySource.Play();
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (MusicButton != null) MusicButton.onClick.RemoveAllListeners();
        var VolumeButton = GameObject.FindGameObjectWithTag("VolumeButton");
        MusicButton = VolumeButton.GetComponent<Button>();
        MusicButton.onClick.AddListener(OnMusicButtonClicked);
        MusicIcon = VolumeButton.GetComponentInChildren<RawImage>();
        MusicIcon.texture = Activated ? VolumeOn : VolumeOff;
    }

    public void OnMusicButtonClicked() {
        AudioMixer.SetFloat("Volume", (Activated = !Activated) ? -3f : -80f);
        MusicIcon.texture = Activated ? VolumeOn : VolumeOff;
    }

    private void PlayNextSong()
    {
        PlaySource.clip = Playlist[MusicIndex = (MusicIndex + 1) % Playlist.Length];
        PlaySource.Play();
    }

    public void SetVictorious(Fighter Victorious = null)
    {
        if (this.Victorious != Victorious)
        {
            AudioClip PlayVictory = Victorious?.PlayVictory();
            PlaySource.Stop();
            PlaySource.loop = false;
            this.Victorious = Victorious;

            if (Victorious == null) PlaySource.clip = Playlist[MusicIndex];
            else if (PlayVictory.length > 15f)
            {
                minTime = Victorious.LoopStart;
                maxTime = Victorious.LoopStop;
                PlaySource.clip = PlayVictory;
            }
            else PlaySource.clip = PlayVictory;

            PlaySource.Play();
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
