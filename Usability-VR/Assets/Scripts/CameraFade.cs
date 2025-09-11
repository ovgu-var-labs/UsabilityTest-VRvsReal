using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Leap;
using static ApplicationSettings;


public class CameraFade : MonoBehaviour
{
    private CameraFadeManager _cameraFadeManager;
    private FadeType _fadeType;
    public enum FadeType
    {
        fadeIn,
        fadeOut,
        fadeInOut,
        stayOpaque
    }

    [HideInInspector] public bool enableFade;

    private float _currentFadeInDuration;
    private float _currentOpaqueDuration;
    private float _currentFadeOutDuration;
    private Color _currentFadeColor;

    private bool _wasStartTimeLogged;
    private float _startTime;

    private Action _fadeCompletedAction;

    public void ConfigureFade(CameraFadeManager manager, FadeType fadeType, Action action, Color fadeColor,
        float fadeInDuration, float opaqueDuration, float fadeOutDuration)
    {
        _cameraFadeManager = manager;
        _fadeType = fadeType;
        _fadeCompletedAction = action;
        _currentFadeColor =  new Color(fadeColor.r, fadeColor.g, fadeColor.b);
        _currentFadeInDuration = fadeInDuration;
        _currentOpaqueDuration = opaqueDuration;
        _currentFadeOutDuration = fadeOutDuration;

        if (_fadeType.Equals(FadeType.stayOpaque))
        {
            _cameraFadeManager.OnAllOpaqueFadesDestroyed += DoActionAndDisappear;
        }
    }

    public void InitiateFade()
    {
        enableFade = true;
    }

    void DoActionAndDisappear(object o = null, EventArgs e = null)
    {
        if (_fadeType.Equals(FadeType.stayOpaque))
        {
            _cameraFadeManager.OnAllOpaqueFadesDestroyed -= DoActionAndDisappear;
        }

        _fadeCompletedAction?.Invoke();
        Destroy(transform.gameObject);
    }

    void FixedUpdate()
    {
        if (enableFade)
        {
            float t = 1.0f; // interpolation factor at opaque level
            if (!_fadeType.Equals(FadeType.stayOpaque))
            {
                if (!_wasStartTimeLogged)
                {
                    _startTime = Time.time;
                    if (_fadeType.Equals(FadeType.fadeOut))
                    {
                        _startTime -= _currentFadeInDuration; // act as if fade in was already done
                    }

                    _wasStartTimeLogged = true;
                }

                if (Time.time >= _startTime + _currentFadeInDuration + _currentOpaqueDuration + _currentFadeOutDuration)
                {
                    // fadeInOut / fadeOut is complete
                    DoActionAndDisappear();
                }
                else if (Time.time < _startTime + _currentFadeInDuration)
                {
                    // fade in (from transparent to opaque)
                    t = (Time.time - _startTime) / _currentFadeInDuration;
                }
                else if (Time.time < _startTime + _currentFadeInDuration + _currentOpaqueDuration)
                {
                    // stay opaque
                    t = 1;

                    // fade-in is already complete at this point
                    if (_fadeType.Equals(FadeType.fadeIn))
                    {
                        DoActionAndDisappear();
                    }
                }
                else if (Time.time < _startTime + _currentFadeInDuration + _currentOpaqueDuration + _currentFadeOutDuration)
                {
                    // fade out (from opaque to transparent)
                    t = 1 - ((Time.time - _startTime - _currentFadeInDuration - _currentOpaqueDuration) / _currentFadeOutDuration);
                }
            }

            float alpha = Mathf.Lerp(0, 1, t);
            var colour = new Color(_currentFadeColor.r, _currentFadeColor.g, _currentFadeColor.b, alpha);
            // GetComponent<Renderer>().sharedMaterial.color = colour;
        }
    }
}
