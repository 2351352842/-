using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public AssetManagerConfigScirptableObject config;
    // Start is called before the first frame update
    void Start()
    {
        config=ScriptableObject.CreateInstance<AssetManagerConfigScirptableObject>();
        Debug.Log(config);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
