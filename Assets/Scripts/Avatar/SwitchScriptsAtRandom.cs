using UnityEngine;
using System.Collections;

public class SwitchScriptsAtRandom : MonoBehaviour
{

    public MonoBehaviour script;
    public float minTime = 1f;
    public float maxTime = 4f;


    // Use this for initialization
    void Start()
    {
        StartCoroutine(ConstantlySwitchScrtiptsState(minTime, maxTime, script));
    }

    /// <summary>
    /// Enables or Disables scripts depending on it's current state. Refreshes by random time.
    /// Don't turn off this the script itself if you don't want the switching to interrupt.
    /// You won't be able to enable it with itself.
    /// </summary>
    /// <param name="minWaitTime">Mininal random amount of time to wait until script changes it's state</param>
    /// <param name="maxWaitTime">Maximal random amount of time to wait until script changes it's state</param>
    /// <param name="scriptToSwitch">Reference to instance of the script</param>
    /// <returns>WaitForSeconds</returns>
    public static IEnumerator ConstantlySwitchScrtiptsState(float minWaitTime, float maxWaitTime, MonoBehaviour scriptToSwitch)
    {
        while (true)
        {
            /*
             * If you want to check if gameobject is enabled you can use - scriptToSwitch.gameObject.activeSelf; 
             * Also you could check by scriptToSwitch.isActiveAndEnabled;
             * https://docs.unity3d.com/ScriptReference/Behaviour-isActiveAndEnabled.html
             */
            scriptToSwitch.enabled = !scriptToSwitch.enabled;

            // Make sure that values are correct. You can check with "if" and "throw" an exception if you have to be sure that values the developer enter are definitely correct.
            minWaitTime = Mathf.Clamp(minWaitTime, 0, maxWaitTime);
            maxWaitTime = Mathf.Clamp(maxWaitTime, 0, maxWaitTime);

            yield return new WaitForSeconds(Random.Range(minWaitTime, maxWaitTime));
        }
    }

}
