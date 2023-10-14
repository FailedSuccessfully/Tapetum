using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppManager : MonoBehaviour
{
    public TapetumController tapetum;
    private void Start()
    {
        Utility.Settings settings = new Utility.Settings() { 
            CheckRadius = tapetum.radius,
            TargetSize = tapetum.size,
            BeamDistance = tapetum.range,
            LockDistance = tapetum.dist,
        };
        Utility.InitSettings(settings);

    }
}
