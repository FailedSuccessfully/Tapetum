using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AppManager : MonoBehaviour
{
    public UnityEvent toInitiallize;
    public TapetumController tapetum;
    private void Start()
    {
        ProjectUtility.Settings settings = new ProjectUtility.Settings() { 
            CheckRadius = tapetum.radius,
            TargetSize = tapetum.size,
            BeamDistance = tapetum.range,
            LockDistance = tapetum.dist,
        };
        ProjectUtility.InitSettings(settings);

    }


    public void QuitApplication() => Application.Quit();

    private void Initialize()
    {
        toInitiallize.Invoke();
    }
}
