using UnityEngine;
using System.Collections;

public class BeatController : MonoBehaviour
{
    public BeatMaster beatMaster;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    // This method will get called a designated amount of time before the beat happens, so you could for instance spawn something in the distance
    // magnitudes is an array of size 10, I would try to stay away from using this data unless you have a good grasp of what it does
    // indexes 0-7 of the magnitueds array are the strength of the beat along the spectrum (the higher the number the more bass heavy). 
    // index 8 is the amount of time time in samples since the last beat
    // and index 9 is the amount of time in samples until the next beat
    public void StartBeat(float[] magnitudes)
    {
        StartCoroutine(BeginCountdown(magnitudes));
        //Debug.Log ("Beat happening soon");
        // In my game, this is where I created the targets that the player had to look at
    }

    IEnumerator BeginCountdown(float[] magnitudes)
    {
        yield return new WaitForSeconds(beatMaster.primaryDelay / 1000f);
        // This code will happen when the beat actually occurs
        Debug.Log("Beat happened");

        //for (int i = 0; i < 7; ++i)
        //{
        //	Debug.Log(magnitudes[i]);
        //	Vector3 start = new Vector3(i, 0, 0);
        //	Vector3 end = new Vector3(i, magnitudes[i] / 10, 0);
        //	Debug.DrawLine(start, end);
        //}


        // In my game, this is where I triggered the code to see if the player was actually looking at the target when the beat happened
    }
}
