using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkingEyes : MonoBehaviour {

    Mesh thisMesh;
    SkinnedMeshRenderer smr;
    int lefteye;
    int righteye;

    private void Awake()
    {
        smr = this.GetComponent<SkinnedMeshRenderer>();
        thisMesh = smr.sharedMesh;
    }

    // Use this for initialization
  	void Start () {

          lefteye = thisMesh.GetBlendShapeIndex("EyeBlink_L");
          righteye = thisMesh.GetBlendShapeIndex("EyeBlink_R");
          Invoke("Blink", 1);
  	}

    void Blink()
    {
        smr.SetBlendShapeWeight(lefteye, 100);
        smr.SetBlendShapeWeight(righteye, 100);
        float nextBlink = Random.Range(0.5f, 4f);
        Invoke("Blink", nextBlink);
        Invoke("StopBlink", Random.Range(0.1f, 0.5f));
    }

    void StopBlink()
    {
        smr.SetBlendShapeWeight(lefteye, 0);
        smr.SetBlendShapeWeight(righteye, 0);
    }

}
