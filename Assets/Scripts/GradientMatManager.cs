using System;
using UnityEngine;
using UnityEngine.UI;

public class GradientMatManager : MonoBehaviour
{
    [SerializeField] private Slider syncTargetSlider;

    private Material _gradientMat;
    private Image _image;
    private Material _material;

    private static readonly int ValueID = Shader.PropertyToID("_Value");

    private void Start()
    {
        GetComponent<Image>();
        _material = _image.material;

        SyncMatValueToSlider(syncTargetSlider.value);
        syncTargetSlider.onValueChanged.AddListener(SyncMatValueToSlider);
    }

    private void OnDestroy()
    {
        if (_gradientMat != null)
        {
            Destroy(_gradientMat);
        }
    }

    private void SyncMatValueToSlider(float sliderValue)
    {
        OnDestroy();
        _material.SetFloat(ValueID, sliderValue);
        _gradientMat = Instantiate(_material);
        _image.material = _gradientMat;
    }
}