using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BeatMaster : MonoBehaviour {
	
	public int primaryDelay = 0;
	public int minTimeBetweenBeats;
	private int minTimeBetweenBeatsInSamples;
	public string parserMethodName;
	
	
	private List<int> beatBand = new List<int>();
	private List<float[]> beatMagnitudes = new List<float[]>();
	private List<int> beatTime = new List<int>();
	private List<bool> beatTriggered = new List<bool>();
	private int visualDelay = 130;
	[HideInInspector]
	public int audioFrequency;
	[HideInInspector]
	public int sampleSize;
	[HideInInspector]
	public float sampleRate;
	private int totalBeats = 0;
	
	private float[] power;
	private int powerTimeIncrement;
	private int currentPowerIndex;
	private Texture2D tex;
	[HideInInspector]
	public GUIStyle gui;
	
	private bool checkForGameOver = false;
	
	private Texture2D tex2;
	[HideInInspector]
	public GUIStyle gui2;
	
	private bool loweringVolume = false;
	// Use this for initialization
	IEnumerator Start () {
		tex = new Texture2D (1, 1);
		tex.SetPixel (0, 0, new Color(0f, 0f, 1f, .6f));
		tex.Apply ();
		gui.normal.background = tex;
		
		tex2 = new Texture2D (1, 1);
		tex2.SetPixel (0, 0, new Color(0f, 1f, 0f, .8f));
		tex2.Apply ();
		gui2.normal.background = tex2;
		yield return new WaitForSeconds(5f);
		checkForGameOver = true;
		yield return new WaitForSeconds(200f);
		loweringVolume = true;
		yield return new WaitForSeconds(4f);
		Application.LoadLevel(3);
	}
	
	// Update is called once per frame
	void Update () {
		if(loweringVolume) {
			GetComponent<AudioSource>().volume *= .978f;
		}
		CheckForBeats ();
		//currentPowerIndex = audio.timeSamples / powerTimeIncrement;
		if (Input.GetKeyDown (KeyCode.UpArrow)) {
			visualDelay += 5;
		} else if (Input.GetKeyDown (KeyCode.RightArrow)) {
			visualDelay ++;	
		} else if (Input.GetKeyDown (KeyCode.DownArrow)) {
			visualDelay -= 5;
		} else if (Input.GetKeyDown (KeyCode.LeftArrow)) {
			visualDelay --;	
		}
		
		if(Input.GetKeyDown(KeyCode.F)) {
			CheckBeatsTriggered();
		}
		
		if(checkForGameOver && !GetComponent<AudioSource>().isPlaying) {
			Application.LoadLevel(3);
		}
	}
	
	private void CheckBeatsTriggered() {
		int currentTime = GetComponent<AudioSource>().timeSamples;
		int totalBeats = 0;
		int totalBeatsTriggered = 0;
		for(int i = 0; i < beatTime.Count; i++) {
			if(beatTime[i] < currentTime) {
				totalBeats ++;
				if(beatTriggered[i]) {
					totalBeatsTriggered ++;
				}
			}
		}
		Debug.Log("Beats triggered: " + totalBeatsTriggered + " out of " + totalBeats);
	}
	
	public void RemoveBeatsThatAreTooClose() {
		int lastTime = 0;
		int minTime = ConvertMSToSamples(minTimeBetweenBeats);
		for (int i = 0; i < beatTime.Count; i++) {
			if(beatTime[i] < lastTime + minTime) {
				beatTriggered[i] = true;
			} else {
				lastTime = beatTime[i]; 
			}
		}
	}
	
	public void SetBeatDeltas() {
		int latestActiveTime = 0;
		for(int i = 0; i < beatTime.Count; i++) {
			if(!beatTriggered[i]) {
				beatMagnitudes[i][8] = beatTime[i] - latestActiveTime;
				latestActiveTime = beatTime[i];
				beatMagnitudes[i][9] = GetTimeToNextBeat(i);
			}
		}
	}
	
	float GetTimeToNextBeat(int beatIndex) {
		int counter = 1;
		while(counter < 30) {
			if(beatIndex + counter < beatTriggered.Count) {
				if(!beatTriggered[beatIndex + counter]) {
					return beatTime[beatIndex + counter] - beatTime[beatIndex];
				}
			}
			counter ++;
		}
		return 44100f;
	}
	
	void SetBeatDelays() {
		int samplesDelay = ConvertMSToSamples (primaryDelay);
		for (int i = 0; i < beatTime.Count; i++) {
			beatTime[i] = beatTime[i] - samplesDelay;
			if(beatTime[i] < 0) {
				beatTriggered[i] = true;
			}
		}
		
	}
	
	int ConvertMSToSamples(int samples) {
		return (int)(((float)audioFrequency) * (float)samples / 1000f);
	}
	
	
	void CheckForBeats() {
		int currentTime = GetComponent<AudioSource>().timeSamples;
		for (int i = 0; i < totalBeats; i++) {
			if( !beatTriggered[i] && beatTime[i] < currentTime + 10000) {
				beatTriggered[i] = true;
				StartCoroutine(TriggerBeat(currentTime, i));
			}
		}
	}
	
	IEnumerator TriggerBeat(int currentTime, int beatIndex) {
		yield return new WaitForSeconds ((sampleRate) * (beatTime [beatIndex] - currentTime) + (visualDelay * 0.001f));
		gameObject.SendMessage (parserMethodName, beatMagnitudes[beatIndex]);
	}
	
	// Time is starting time in samples, length is length of the beat in samples
	public void CreateBeat(int time, double[] m) {
		beatTime.Add (time);
		beatTriggered.Add (false);
		beatMagnitudes.Add(new float[10]{ (float)m[0], (float)m[1], (float)m[2], (float)m[3], (float)m[4], (float)m[5], (float)m[6], (float)m[7], 0f, 0f});
		
		totalBeats ++;
	}
	
	public void CalculatePower(int numberOfSamples, int sampleIncrement, int sampleRange) {
		SetBeatDelays ();
		power = new float[numberOfSamples / sampleIncrement];
		powerTimeIncrement = sampleIncrement;
		int bTime = 0;
		float bMag = 0f;
		int pIndex = 0;
		for (int i = 0; i < beatTime.Count; i++) {
			bTime = beatTime[i];
			bMag = 0f;
			pIndex = bTime / sampleIncrement;
			for(int g = pIndex - sampleRange/2; g < pIndex + sampleRange; g++) {
				if(g > 0 && g < power.Length) {
					power[g] += bMag;
				}
			}
		}
		CalculatePowerAverage ();
	}
	
	void CalculatePowerAverage() {
		float average = 0f;
		float max = 0f;
		for (int i = 0; i < power.Length; i++) {
			average += power[i];
			if(power[i] > max) {
				max = power[i];
			}
		}
		average = average / power.Length;
		average = Mathf.Pow (average, 1f / 5f);
		for (int i = 0; i < power.Length; i++) {
			power[i] = power[i] / max;
			power[i] = power[i] * average;
		}
	}
	
	void OnGUI() {
		//GUI.Box (new Rect (10, Screen.height - 40, 200, 50), "Visual delay: " + visualDelay + "ms");
		//		int xPos = Screen.width - 300;
		//		int xPosOrig = xPos;
		//		int yPos = Screen.height;
		//		if (power.Length > 0) {
		//			for (int i = currentPowerIndex; i < currentPowerIndex + 300; i++) {
		//				GUI.Box(new Rect(xPos++, yPos, 1, -1 * power[i] * 100), "", gui);
		//			}
		//			GUI.Box(new Rect(xPosOrig, yPos, 2, -1 * 20), "", gui2);
		//		}
	} 
}
