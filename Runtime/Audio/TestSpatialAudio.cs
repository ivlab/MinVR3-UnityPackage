using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IVLab.MinVR3;
using System.Threading.Tasks;
using System.Threading;


namespace IVLab.MinVR3
{
    [RequireComponent(typeof(SpatialAudioClient))]
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
                    SpatialAudioClient client = this.GetComponent<SpatialAudioClient>();

                    Debug.Log("# EXAMPLE 1: TESTING THE SIMPLE API");
                    // # start a background sound on loop
                    client.LoopSimple("testloop.wav");

                    // # play a few short sounds
                    client.PlaySimple("beep-01.wav");
                    Thread.Sleep(500);
                    client.PlaySimple("beep-02.wav");
                    Thread.Sleep(500);
                    client.PlaySimple("beep-03.wav");
                    Thread.Sleep(500);
                    client.PlaySimple("beep-04.wav");
                    Thread.Sleep(500);
                    client.PlaySimple("click-01.wav");
                    Thread.Sleep(500);
                    client.PlaySimple("click-02.wav");

                    // # wait a bit more, then stop the background loop
                    Thread.Sleep(1000);
                    client.StopSimple("testloop.wav");


                    Debug.Log("# EXAMPLE 2: USE THE SPATIAL API WITH A MOVING LISTENER AND STATIONARY SOURCE (SHOULD SOUND LIKE BEEP MOVES FROM RIGHT TO LEFT)");
                    // # move listener to -10,0,0
                    Vector3 pos = new Vector3(-10, 0, 0);
                    client.SetListenerPosition(pos);

                    // # create a repeating beep at (0,0,0)
                    client.CreateSource(11, "beep-01.wav", Vector3.zero, true);

                    // # move listener from (-10,0,0) to (10,0,0)
                    while (pos.x < 10.0f)
                    {
                        client.SetListenerPosition(pos);
                        pos.x += 1.0f;
                        Thread.Sleep(100);
                    }

                    // # stop the source
                    client.StopSource(11);
                    // # deleting the source (will also stop it, so prev command is not technically necessary here)
                    client.DeleteSource(11);



                    Debug.Log("# EXAMPLE 3: USE THE SPATIAL API WITH A MOVING SOURCE, SET VELOCITY TO GET DOPPLER EFFECT (SHOULD SOUND LIKE BEEP MOVES LEFT TO RIGHT)");

                    // # move listener to 0,0,0
                    client.SetListenerPosition(Vector3.zero);

                    // # create a repeating beep that moves from left to right with a velocity of 1 unit for every 0.1 seconds
                    pos = new Vector3(-10, 0, 0);
                    client.CreateSource(10, "beep-02.wav", pos, true);

                    // # move it from left to right 
                    while (pos.x < 10.0f)
                    {
                        client.SetSourcePosition(10, pos);
                        pos.x += 1.0f;
                        Thread.Sleep(100);
                    }


                    // # stop the source
                    client.StopSource(10);

                    // # delete the source (will also stop it, so prev command is not technically necessary here)
                    client.DeleteSource(10);


                    Debug.Log("# EXAMPLE 4: CHANGE SOURCE PARAMETERS, LIKE PITCH");

                    // # create a repeating beep
                    client.CreateSource(12, "beep-01.wav");

                    // # change pitch
                    float p = 0.5f;
                    for (int i = 0; i < 10; i++)
                    {
                        client.SetSourcePitch(12, p);
                        Thread.Sleep(100);
                        p += 0.1f;
                    }

                    // # stop the source
                    client.StopSource(12);

                    // # delete the source (will also stop it, so prev command is not technically necessary here)
                    client.DeleteSource(12);

                    Debug.Log("Finished testing audio");
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e);
                }
            });
        }
    }


}