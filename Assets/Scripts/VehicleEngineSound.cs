using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class VehicleEngineSound : MonoBehaviour
{
    [Header("Engine Audio Clips")]
    [SerializeField] private AudioClip startClip;
    [SerializeField] private AudioClip loopClip;
    [SerializeField] private AudioClip endClip;

    private AudioSource audioSource;

    private enum EngineSoundState
    {
        Idle,
        Starting,
        Looping,
        Ending
    }

    private EngineSoundState currentState = EngineSoundState.Idle;
    private float startClipTimer = 0f;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        
        // 3D spatial sound configurations
        audioSource.spatialBlend = 1.0f;
        audioSource.dopplerLevel = 0.5f;
        audioSource.minDistance = 2f;
        audioSource.maxDistance = 50f;
    }

    private void Update()
    {
        // Detect W key, Up Arrow, or positive vertical input axis
        bool isAccelerating = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) || Input.GetAxis("Vertical") > 0.05f;

        switch (currentState)
        {
            case EngineSoundState.Idle:
                if (isAccelerating)
                {
                    PlayStartClip();
                }
                break;

            case EngineSoundState.Starting:
                startClipTimer += Time.deltaTime;

                // Wait until startClip finishes completely before transitioning
                if (startClipTimer >= (startClip != null ? startClip.length : 0.5f))
                {
                    if (isAccelerating)
                    {
                        PlayLoopClip();
                    }
                    else
                    {
                        PlayEndClip();
                    }
                }
                break;

            case EngineSoundState.Looping:
                // When accelerator released, play engine deceleration wind-down sound
                if (!isAccelerating)
                {
                    PlayEndClip();
                }
                break;

            case EngineSoundState.Ending:
                // If accelerator pressed again during ending, restart acceleration immediately
                if (isAccelerating)
                {
                    PlayStartClip();
                }
                else if (!audioSource.isPlaying)
                {
                    currentState = EngineSoundState.Idle;
                }
                break;
        }
    }

    private void PlayStartClip()
    {
        currentState = EngineSoundState.Starting;
        startClipTimer = 0f;

        if (startClip != null)
        {
            audioSource.clip = startClip;
            audioSource.loop = false;
            audioSource.Play();
        }
    }

    private void PlayLoopClip()
    {
        currentState = EngineSoundState.Looping;

        if (loopClip != null)
        {
            audioSource.clip = loopClip;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    private void PlayEndClip()
    {
        currentState = EngineSoundState.Ending;

        if (endClip != null)
        {
            audioSource.clip = endClip;
            audioSource.loop = false;
            audioSource.Play();
        }
    }

    // Helper method to assign audio clips in Editor setup scripts
    public void Setup(AudioClip start, AudioClip loop, AudioClip end)
    {
        startClip = start;
        loopClip = loop;
        endClip = end;
    }
}
