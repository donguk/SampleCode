using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleMain : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SampleCode.Debug.Log("<color=green>SampleMain</color> Start()...");

        SampleCode.Debug.LogLua("<color=green>SampleMain</color> print log lua");

        SampleCode.Debug.LogError("<color=green>SampleMain</color> print log Error!");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
