using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkEyes : MonoBehaviour {
    //Blink eyes
    [Header("BlinkEyes")]
    private SkinnedMeshRenderer skinnedMeshRenderer;
    public int EyesBlink_L_BlendShape;
    public int EyesBlink_R_BlendShape;
    bool infiniteBlinking = true;
    float blink = 100.0f;
    public float EyeOpenSpeed = 15.0f;
    public float EyeCloseSpeed = 10.0f;
    bool eyesClosed = false;

    void Awake()
    {
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
    }

    void Start()
    {
      //StartCoroutine(waiter());
    }

    void LateUpdate()
    {
        //Debug.Log("eyesClosed value is " + eyesClosed);
        //Debug.Log("blink value is " + blink);
        BlinkEye();
        //StartCoroutine(waiter());
    }

    private void BlinkEye()
    {
        {
            if (eyesClosed == true && blink <= 100.0f)
            {
                Debug.Log("Close");
                blink += EyeOpenSpeed; //...increase weight
                skinnedMeshRenderer.SetBlendShapeWeight(EyesBlink_R_BlendShape, blink);
                skinnedMeshRenderer.SetBlendShapeWeight(EyesBlink_L_BlendShape, blink);
            }
            if (eyesClosed == false && blink >= 0.0f)
            {
                blink -= EyeCloseSpeed; //...decrease weight
                skinnedMeshRenderer.SetBlendShapeWeight(EyesBlink_R_BlendShape, blink);
                skinnedMeshRenderer.SetBlendShapeWeight(EyesBlink_L_BlendShape, blink);
                //eyesClosed = false;
                Debug.Log("Open");
            }
            if (blink >= 100)
            {
                    eyesClosed = false;
            }
            if (blink <= 0)
            {
                eyesClosed = true;
            }
        }
    }
}
