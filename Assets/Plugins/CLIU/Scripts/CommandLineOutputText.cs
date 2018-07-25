using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This script manages the output text for the CLIU window.
/// </summary>
public class CommandLineOutputText : MonoBehaviour {

    public RectTransform output;
    //float initPoxY;

	void Awake () {
        //initPoxY = output.offsetMax.y;
	}

    private void Update()
    {
        ChangePosition();
    }

    public void ChangePosition()
    {
        //if (initPoxY == output.offsetMax.y)
        //{
        //    output.offsetMax += new Vector2(0, offset);
        //}
        if (output.offsetMin.y >= 0)
        {
            output.offsetMin = new Vector2(output.offsetMin.x, 4);
        }
    }
}
