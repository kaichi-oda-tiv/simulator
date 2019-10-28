using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverloadHelloBundleScript : HelloBundleScript
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log($"{this.name}.{this.GetHashCode().ToString("x")}");
    }

    // Update is called once per frame
    void Update()
    {

    }
}
