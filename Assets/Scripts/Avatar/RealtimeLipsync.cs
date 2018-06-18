using UnityEngine;
using System.Collections;

public class RealtimeLipsync : MonoBehaviour {

    //SEE: https://laboratoriesx86.wordpress.com/2016/05/15/unity-realtime-lipsync/

    //LIPSYNC CORE
    [Header("Lipsync")]
	public AudioSource audioSource;
	public int mouthBlendShape;
	private float[] _samples = new float[64];
	private float clampedLipsync = 0f;

	//PRIVATES
	private SkinnedMeshRenderer blendMesh;

	void Awake(){
		blendMesh = GetComponent<SkinnedMeshRenderer>();
	}

	void LateUpdate(){
		if(audioSource) DoLipsync();
	}

	void DoLipsync(){
		if(audioSource.isPlaying){
			audioSource.GetSpectrumData(_samples,0,FFTWindow.BlackmanHarris);

			clampedLipsync = Mathf.Clamp(_samples[2]*3000,0,100);

			blendMesh.SetBlendShapeWeight(mouthBlendShape,clampedLipsync);
		}
	}
}
