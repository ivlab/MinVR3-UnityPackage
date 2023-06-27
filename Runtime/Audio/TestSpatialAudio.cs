using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IVLab.MinVR3;
using System.Threading.Tasks;
using System.Threading;

public class TestSpatialAudio : MonoBehaviour
{
    public bool testOnStart = true;

    void Start()
    {
        if (testOnStart)
        {
            TestAudio();
        }
    }

    // Start is called before the first frame update
    public void TestAudio()
    {
        Task.Run(() =>
        {
            try
            {
                // Make sure is initialized
                SpatialAudioClient.GetInstance();

                // # EXAMPLE 1: TESTING THE SIMPLE API
                // # start a background sound on loop
                SpatialAudioClient.GetInstance().LoopSimple("testloop.wav");

                // # play a few short sounds
                SpatialAudioClient.Instance.PlaySimple("beep-01.wav");
                Thread.Sleep(500);
                SpatialAudioClient.Instance.PlaySimple("beep-02.wav");
                Thread.Sleep(500);
                SpatialAudioClient.Instance.PlaySimple("beep-03.wav");
                Thread.Sleep(500);
                SpatialAudioClient.Instance.PlaySimple("beep-04.wav");
                Thread.Sleep(500);
                SpatialAudioClient.Instance.PlaySimple("click-01.wav");
                Thread.Sleep(500);
                SpatialAudioClient.Instance.PlaySimple("click-02.wav");

                // # wait a bit more, then stop the background loop
                Thread.Sleep(1000);
                SpatialAudioClient.Instance.StopSimple("testloop.wav");


                // # EXAMPLE 2: USE THE SPATIAL API WITH A MOVING LISTENER AND STATIONARY SOURCE (SHOULD SOUND LIKE BEEP MOVES FROM RIGHT TO LEFT)
                // # move listener to -10,0,0
                Vector3 pos = new Vector3(-10, 0, 0);
                SpatialAudioClient.Instance.SetListenerPosition(pos);

                // # create a repeating beep at (0,0,0)
                SpatialAudioClient.Instance.CreateSource(11, "beep-01.wav", Vector3.zero, true);

                // # move listener from (-10,0,0) to (10,0,0)
                while (pos.x < 10.0f)
                {
                    SpatialAudioClient.Instance.SetListenerPosition(pos);
                    pos.x += 1.0f;
                    Thread.Sleep(100);
                }

                // # stop the source
                SpatialAudioClient.Instance.StopSource(11);
                // # deleting the source (will also stop it, so prev command is not technically necessary here)
                SpatialAudioClient.Instance.DeleteSource(11);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
        });
    }
}
