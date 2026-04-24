using UnityEngine;

/// <summary>
/// Central place for clips. Add one GameObject with this script to your scene (e.g. "GameAudio"),
/// assign AudioClips in the Inspector, and wire SFX / Music AudioSources (or leave empty to auto-create).
/// </summary>
public class GameAudio : MonoBehaviour
{
    public static GameAudio Instance { get; private set; }

    [Header("Sources (optional — auto-added if missing)")]
    public AudioSource sfxSource;
    [Tooltip("Used for victory / death music (long clips).")]
    public AudioSource musicSource;

    [Range(0f, 1f)]
    public float sfxVolume = 1f;
    [Range(0f, 1f)]
    public float musicVolume = 0.75f;

    [Header("Player / combat")]
    public AudioClip playerHit;
    [Tooltip("Random clip each step while moving.")]
    public AudioClip[] playerFootsteps;

    [Header("Collect")]
    public AudioClip collectWood;
    public AudioClip collectRope;
    public AudioClip collectFood;

    [Header("Build")]
    public AudioClip buildRaft;

    [Header("Win / lose music")]
    public AudioClip victoryMusic;
    public bool victoryMusicLoops = true;
    public AudioClip deathMusic;
    public bool deathMusicLoops = true;

    [Header("Enemy")]
    [Tooltip("Played from the enemy via a 3D AudioSource (see EnemyFollow).")]
    public AudioClip enemyFootstep;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        EnsureSfxSource();
        EnsureMusicSource();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void EnsureSfxSource()
    {
        if (sfxSource != null)
        {
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 0f;
            return;
        }

        sfxSource = GetComponent<AudioSource>();
        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f;
    }

    void EnsureMusicSource()
    {
        if (musicSource != null)
        {
            musicSource.playOnAwake = false;
            musicSource.spatialBlend = 0f;
            return;
        }

        Transform child = transform.Find("MusicSource");
        if (child != null)
            musicSource = child.GetComponent<AudioSource>();
        if (musicSource == null)
        {
            var go = new GameObject("MusicSource");
            go.transform.SetParent(transform);
            musicSource = go.AddComponent<AudioSource>();
        }

        musicSource.playOnAwake = false;
        musicSource.spatialBlend = 0f;
    }

    void PlayOneShot(AudioClip clip, float scale = 1f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, Mathf.Clamp01(sfxVolume * scale));
    }

    public void PlayPlayerHit() => PlayOneShot(playerHit, 1f);

    public void PlayPlayerFootstep()
    {
        if (playerFootsteps == null || playerFootsteps.Length == 0) return;
        AudioClip c = playerFootsteps[Random.Range(0, playerFootsteps.Length)];
        if (c != null)
            PlayOneShot(c, 0.85f);
    }

    public void PlayCollectWood() => PlayOneShot(collectWood, 1f);
    public void PlayCollectRope() => PlayOneShot(collectRope, 1f);
    public void PlayCollectFood() => PlayOneShot(collectFood, 1f);

    public void PlayBuildRaft() => PlayOneShot(buildRaft, 1f);

    public void PlayVictoryMusic()
    {
        if (musicSource == null || victoryMusic == null) return;
        musicSource.Stop();
        musicSource.clip = victoryMusic;
        musicSource.loop = victoryMusicLoops;
        musicSource.volume = Mathf.Clamp01(musicVolume);
        musicSource.Play();
    }

    public void PlayDeathMusic()
    {
        if (musicSource == null || deathMusic == null) return;
        musicSource.Stop();
        musicSource.clip = deathMusic;
        musicSource.loop = deathMusicLoops;
        musicSource.volume = Mathf.Clamp01(musicVolume);
        musicSource.Play();
    }
}
