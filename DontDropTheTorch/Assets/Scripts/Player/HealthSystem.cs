using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    private HealthAttributes attributes;

    public void SetAttributes(PlayerAttributes playerAttributes)
    {
        attributes = playerAttributes.HealthAttributes;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
