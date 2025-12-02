using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class slot : MonoBehaviour
{
    float[] slotRotation = new float[] {8.0f, 35.0f, 59.0f, 81.0f, 106.0f, 132.0f, 161.0f, 188.0f};
    float speed;
    float x;
    int playcount;
    int[,] Setting = new int[10, 3] { { 0, 4, 6 }, { 3, 6, 6 }, { 5, 2, 4 }, { 1, 4, 3 }, { 5, 5, 5 }, { 2, 1, 0 }, { 5, 4, 6 }, { 6, 1, 6 }, { 3, 3, 3 }, { 1, 6, 3 } };
    int flg;
    public int slot_number;
    public Effect effect;
    public Sound sound;
    public cutinFlash[] flash = new cutinFlash[3];
    public cutinFlash[] Berryflash = new cutinFlash[3];
    public CutIn cutin;
    public Serial serialMaster;
    [SerializeField] private AudioSource myAudioSource;
    [SerializeField] private AudioClip slotClip;
    [SerializeField] private AudioClip buttonClickClip;
    // Start is called before the first frame update
    void Start()
    {
        x = 81.0f;
        flg = 0;
        playcount = 1;
        speed = (500.0f + 200 * Random.value);
        if (myAudioSource == null)
        {
            myAudioSource = GetComponent<AudioSource>();
            if (myAudioSource == null) return;
        }
        myAudioSource.clip = slotClip;
        myAudioSource.loop = true;
        myAudioSource.playOnAwake = true;
        myAudioSource.volume = 0.7f;
    }

    // Update is called once per frame
    void Update()
    {
        Transform myTransform = this.transform;

        if (flg == 1)
        {
            flg = 2;
            x = slotRotation[Setting[playcount, slot_number]];
            speed = 0.0f;
        }
        /*if(flg == 1)
        {
            flg = 2;
            for (int i = 0; i < slotRotation.Length; i++)
            {
                if (x - slotRotation[i] <= 0.0f)
                {
                    x = slotRotation[i];
                    speed = 0.0f;
                    i = slotRotation.Length + 1;
                }
            }
        }*/
        x -= speed * Time.deltaTime;
        if (x < 0.0f)
        {
            x = 180.0f + x;
        }
        myTransform.rotation = Quaternion.Euler(x, 0.0f, 90.0f);
    }

    public void Stop()
    {
        if(flg == 0)
            flg = 1;
        myAudioSource.Stop();
        myAudioSource.PlayOneShot(buttonClickClip);
        if (playcount == 4)
        {
            foreach (var elm in flash)
                elm.On();
            foreach (var elm in Berryflash)
                elm.On();
        }
        if (playcount == 8)
        {
            foreach (var elm in flash)
                elm.On();
        }
    }
    public void Restart()
    {
        foreach (var elm in flash)
            elm.Off();
        foreach (var elm in Berryflash)
            elm.Off();
        flg = 0;
        playcount += 1;
        playcount = playcount % 10;
        speed = (500.0f + 200 * Random.value);
        if (playcount == 8)
        {
            serialMaster.Write("1");
            cutin.ShowCutIn();
            effect.PlayParticles();
            sound.StartLoopSound();
        }
        else
        {
            serialMaster.Write("0");
        }
        if (playcount == 9)
        {
            effect.StopParticles();
            sound.StopLoopSound();
        }
        myAudioSource.Play();
    }
}
