using System.Collections;
using UnityEngine;

public class CheckWatchButtonAnimationController : MonoBehaviour
{
    [SerializeField] private float timeSpan = 0.5f;
    [SerializeField] private float animSpeed = 1.0f;
    [SerializeField] private AnimationClip animClip;

    private Animation _animation;
    private Coroutine _coroutine;
    
    void Start()
    {
        _animation = GetComponent<Animation>();
        
        if (_animation == null)
        {
            Debug.LogError("Animation component not found!");
            return;
        }
        
        _animation.AddClip(animClip, animClip.name);
        _animation[animClip.name].speed = animSpeed;
        
        _coroutine = StartCoroutine(PlayAnimationLoop());
    }

    private void OnEnable()
    {
        if(_coroutine == null)
            _coroutine = StartCoroutine(PlayAnimationLoop());
    }

    private void OnDisable()
    {
        StopCoroutine(_coroutine);
        _coroutine = null;
    }

    private IEnumerator PlayAnimationLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(timeSpan);
            _animation.Play(animClip.name);
        }
    }
}
