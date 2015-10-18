using UnityEngine;
using System.Collections;
using MathNet.Numerics.IntegralTransforms;
using SpeedTest;

public class Core : MonoBehaviour {
	public AudioClip audioClip;
	public GameObject beatMasterGameObject;
	private int sampleSizeFactorOf2 = 11;
	private int numberOfBands = 8;
	private int welchSegments = 4;
	[HideInInspector]
	private float[] bandPercents = new float[8]{0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.60f, 0.80f, 1f};
	private float[] bandPercentsLinear = new float[8]{0.125f, 0.25f, 0.375f, 0.50f, 0.625f, 0.75f, 0.875f, 1f};
	[HideInInspector]
	public int sampleSizeForFFT;
	private BeatMaster beatMaster;
	private double[][] condensedValues;
	public double[][] deltas;
	private float[] audioModifiers = new float[8]{1f,1f,1f,1f,1f,1f,1f,1f};
	private float[] averages;
	private float songTotalAverage = 1f;
	private int[] beatCount;
	public GameObject[] objectsToSendPlayMessage;
	// Use this for initialization
	void Start () {
		beatCount = new int[numberOfBands];
		sampleSizeForFFT = (int)Mathf.Pow (2f, (float)sampleSizeFactorOf2);
		beatMaster = beatMasterGameObject.GetComponent<BeatMaster> ();
		beatMaster.audioFrequency = audioClip.frequency;
		beatMaster.sampleSize = sampleSizeForFFT;
		beatMaster.sampleRate = sampleSizeForFFT / audioClip.frequency;
		//Camera.main.transform.position = new Vector3 (2f * numberOfBands, 2.5f * numberOfBands, -2f);
		//Debug.Log (audioClip.samples);
		//Debug.Log (audioClip.channels);
		//Debug.Log (audioClip.frequency);
		DoManyIterations (audioClip.samples/sampleSizeForFFT, sampleSizeForFFT, true);
		// StartCoroutine (DoManyIterations (100, sampleSizeForFFT, false));
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void DoManyIterations (int numberOfIterations, int sampleSize, bool timeToSong) {
		float refreshRate = 1f / (audioClip.frequency / sampleSizeForFFT);
		condensedValues = new double[(int)audioClip.samples/sampleSizeForFFT][];
		deltas = new double[(int)audioClip.samples/sampleSizeForFFT][];
		averages = new float[numberOfBands];
		Debug.Log ("Refresh Rate Peroid: " + refreshRate.ToString ());
		if (!timeToSong) {	
			refreshRate = .05f;
		}
		
		for(int i = 0; i < numberOfIterations; i++) {
			//if(timeToSong)	yield return new WaitForSeconds(refreshRate);
			DoOneIteration(sampleSize, i);
		}
		ComputeDeltas(condensedValues);
		Debug.Log("Finished gathering data");
		ComputeAverages(deltas);
		ComputeBeats (deltas);
		//beatMaster.CalculatePower (audioClip.samples, 1024, 40);
		beatMaster.RemoveBeatsThatAreTooClose();
		beatMaster.SetBeatDeltas();
		beatMaster.gameObject.GetComponent<AudioSource>().clip = audioClip;
		beatMaster.gameObject.GetComponent<AudioSource>().Play ();
		for(int i  = 0; i < objectsToSendPlayMessage.Length; i++) {
			objectsToSendPlayMessage[i].SendMessage("MusicStarting");
		}
	}
	
	void DoOneIteration(int numberOfSamples, int offset) {
		float[] samples = new float[numberOfSamples];
		audioClip.GetData (samples, offset * numberOfSamples);
		
		int numSegments = welchSegments * 2 - 1;
		int segmentLength = numberOfSamples / welchSegments;
		int stepAmount = segmentLength / 2;
		int counter = 0;
		if (offset == 10) {
			Debug.Log("Number of segments " + numSegments.ToString());
			Debug.Log("Segment Length " + segmentLength.ToString());
			Debug.Log("Step amount " + stepAmount.ToString());
		}
		double[][] welchTotal = new double[numSegments][];
		
		for(int i = 0; i < numSegments; i++) {
			welchTotal[i] = DoFFT(ApplyBlackmanHarris(GrabSamples(samples, counter, segmentLength)));
			counter += stepAmount;
		}
		condensedValues [offset] = Condense ((WelchAverage(welchTotal)), numberOfBands, true);
		//condensedValues [offset] = Condense ((DoFFT(ApplyBlackmanHarris(samples))), numberOfBands, false);
	}
	
	double[] WelchAverage(double[][] values) {
		double[] results = new double[values[0].Length];
		for (int i = 0; i < values.Length; i++) {
			//values[i] = FindMagnitudes(values[i]);
			for(int g = 0; g < values[i].Length; g++) {
				results[g] += values[i][g];
			}		
		}
		
		for (int i = 0; i < results.Length; i++) {
			results[i] = results[i]/values.Length;		
		}
		return results;
	}
	
	double[] GrabSamples(float[] values, int startingIndex, int numSamples) {
		double[] samples = new double[numSamples];
		for (int i = 0; i < numSamples; i++) {
			samples[i] = values[startingIndex + i];		
		}
		return samples;
	}
	
	double[] DoFFT(double[] values) {
		double[] real = new double[values.Length];
		double[] imaginary = new double[values.Length];
		for (int i = 0; i < values.Length; i++) {
			imaginary[i] = 0f;
			real[i] = values[i];
		}
		FFT2 fft = new FFT2();
		fft.init((uint)Mathf.Log ((float)values.Length));
		fft.run (real, imaginary, false);
		return real;
	}
	
	double[] FindMagnitudes(double[] values) {
		for (int i = 0; i < values.Length; i++) {
			values[i] = (values.Length - i) * (double)Mathf.Abs((float)values[i]);
		}
		return values;
	}
	
	double[] Condense(double[] values, int numBands, bool average) {
		double[] vals = new double[numBands];
		int[] cutoffs = new int[numBands];
		int[] cutoffsAverage = new int[numBands];
		int currentCutoff = 0;
		for(int g = 0; g < numBands - 1; g++) {
			float f = bandPercents[g] / 2f;
			cutoffs[g] = (int)(f * (float)values.Length);
		}
		for (int i = 0; i < values.Length / 2; i++) {
			if(i == cutoffs[currentCutoff]) {
				currentCutoff ++;
			}
			vals[currentCutoff] += values[i];
			cutoffsAverage[currentCutoff] += 1;
		}
		if (average) {
			for(int g = 0; g < numBands; g++) {
				vals[g] = vals[g] / cutoffsAverage[g];
			}	
		}
		return vals;
	}
	
	void ComputeDeltas(double[][] values) {
		for (int i = 0; i < values.Length - 1; i++) {
			deltas[i] = new double[values[i].Length];
			for(int g = 0; g < values[i].Length; g++) {
				deltas[i][g] = values[i + 1][g] - values[i][g];
			}		
		}
		deltas[values.Length - 1] = new double[values[values.Length - 1].Length];
	}
	
	void ComputeBeats(double[][] values) {
		for (int i = 0; i < values.Length - 1; i++) {
			SingleBandBeatDetection(i, values);
		}
	}
	
	void ComputeAverages(double[][] values) {
		int[] counters = new int[averages.Length];
		int totalCounter = 0;
		float total = 0f;
		for (int i = 0; i < values.Length - 1; i++) {
			for(int g = 0; g < values[i].Length; g++) {
				if(values[i][g] >= 0) {
					total += (float)values[i][g];
					totalCounter ++;
					averages[g] += (float)values[i][g];
					counters[g] += 1;
				}
				
			}
		}
		songTotalAverage = total / (float)totalCounter;
		for(int g = 0; g < averages.Length; g++) {
			//Debug.Log("Totals for band " + g.ToString() + " : " + averages[g].ToString());
			averages[g] = averages[g]/counters[g];
			//Debug.Log("Averages for band " + g.ToString() + " : " + averages[g].ToString());
			averages[g] *= 1f;
		}
		
		
	}
	
	void SingleBandBeatDetection(int index, double[][] values) {
		int range = 8;
		double zeroMag = 1d;
		double minMag = 26f;
		double maxMag = 120f;
		if(index < range) {
			minMag = 60f;
			range = index;
		}
		double[] averageVals = new double[8];
		for(int g = 0; g < 8; g++) {
			for(int r = index - range; r < index + range; r++) {
				if(r < values.Length && r != index) {
					if(values[r][g] > 0) {
						averageVals[g] += values[r][g];
					} else {
						averageVals[g] *= .95f;
					}
				}
				
			}
			averageVals[g] = averageVals[g] / ((range * 2f) - 1f);
		}
		
		double[] beatPersonality = new double[8];
		double totalMag = 0d;
		for(int g = 0; g < 8; g++) {
			if(averages[g] != 0d) {
				beatPersonality[g] = audioModifiers[g] * values[index][g] / (averageVals[g]);
				if(beatPersonality[g] <= zeroMag * audioModifiers[g]) {
					beatPersonality[g] = 0d;
				} else if(beatPersonality[g] > maxMag) {
					beatPersonality[g] = maxMag;
				}
			} else {
				beatPersonality[g] = 0d;
			}
			
			totalMag += beatPersonality[g];
		} 
		
		if (totalMag >= minMag) {
			// Passed Beat Test
			beatMaster.CreateBeat(index * sampleSizeForFFT, beatPersonality);
		}
	}
	
	float[] ApplyHamming(float[] values) {
		for (int n = 0; n < values.Length; n++) {
			values[n] = 0.54f - (0.46f * Mathf.Cos( (2f * Mathf.PI * values[n]) / (values.Length - 1) ));	
		}
		return values;
	}
	
	double[] ApplyBlackmanHarris(double[] values) {
		for (int n = 0; n < values.Length; n++) {
			double val = values[n];
			values[n] = 0.35875f - (0.48829f * System.Math.Cos((2f * System.Math.PI * val) / (values.Length - 1)));
			values[n] += 0.14128f * System.Math.Cos ((4f * System.Math.PI * val) / (values.Length - 1));
			values[n] -= 0.01168f * System.Math.Cos((6f * System.Math.PI * val) / (values.Length - 1));
		}
		return values;
	}
}
