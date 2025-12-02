using UnityEngine;

public class Sound :MonoBehaviour
{
    [SerializeField] private AudioSource[] audioSource = new AudioSource[3];

    public void StartLoopSound()
    {
        if (audioSource != null)
        {
            foreach (var elm in audioSource) {
                elm.loop = true;
                if (!elm.isPlaying)
                {
                    elm.Play();
                }
            }
        }
    }

    public void StopLoopSound()
    {
        if(audioSource != null)
        {
            foreach (var elm in audioSource)
            {
                if (elm.isPlaying)
                {
                    elm.Stop();
                }
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            foreach (var elm in audioSource)
            {
                elm.Play();
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            foreach(var elm in audioSource)
            {
                elm.Stop();
            }
        }
    }
}
