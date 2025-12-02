using UnityEngine;
public class Effect : MonoBehaviour
{
    [SerializeField] private ParticleSystem[] particle = new ParticleSystem[2];

    public void PlayParticles()
    {
        for (int i = 0; i < particle.Length; i++)
        {
            if (particle[i] != null) // nullチェックを追加
            {
                particle[i].Play();
            }
        }
    }

    public void StopParticles()
    {
        for (int i = 0; i < particle.Length; i++)
        {
            particle[i].Stop();
        }
    }

    private void Start()
    {
        for (int i = 0; i < particle.Length; i++)
        {
            particle[i].Stop();
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            for (int i = 0; i < particle.Length; i++)
            {
                particle[i].Play();
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            for (int i = 0; i < particle.Length; i++)
            {
                particle[i].Stop();
            }
        }
    }
}