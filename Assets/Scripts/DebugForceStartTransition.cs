using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DebugForceStartTransition : MonoBehaviour
{
    [SerializeField] private GameState startState;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(DelayStart());
    }

    private IEnumerator DelayStart()
    {
        yield return new WaitForSeconds(0.5f);
        GameManager.Instance.RequestPhaseTransition(startState);
    }
}
