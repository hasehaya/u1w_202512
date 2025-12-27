using UnityEngine;
using UnityEngine.UI;

public class TestBackSleep : MonoBehaviour
{
    [SerializeField] private SleepPhaseController sleepPhaseController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sleepPhaseController.OnPhaseEnter();
    }
}
