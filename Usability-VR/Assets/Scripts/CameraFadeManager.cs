using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.SceneManagement;
using static ApplicationSettings;
using static CameraFade;

public class CameraFadeManager : MonoBehaviour
{
    public EventManager eventManager;
    public GameObject blockerImagePrefab;

    public SceneController sceneController;
    public VoiceControl voiceControl;
    public CameraRigController cameraRigController;

    const float defaultFadeInDuration = 0.8f;
    const float defaultOpaqueDuration = 0.5f;
    const float defaultFadeOutDuration = 0.8f;
    readonly Color _defaultFadeColor = new Color(1, 1, 1, 0.0f);

    public EventHandler OnAllOpaqueFadesDestroyed;

    private void Start()
    {
        sceneController.OnSceneReloadKeyPressed += FadeOutToReloadScene;
        voiceControl.OnKeywordRecognized += FadeToChangeCameraPosition;
        eventManager.OnScenarioPreparedAndStarted += DestroyAllOpaqueFades;
        eventManager.OnScenarioNotApplied += DestroyAllOpaqueFades; // allow users to look around in scene
        StayOpaque(doWhenDestroyed: () => { FadeOutAtSceneStart(); });
    }

    void SetOffFade(FadeType fadeType, Action action, Color fadeColor,
        float fadeInDuration = defaultFadeInDuration, float opaqueDuration = defaultOpaqueDuration, float fadeOutDuration = defaultFadeOutDuration)
    {
        var fadeCamera = Instantiate(blockerImagePrefab, transform).GetComponent<CameraFade>(); // instantiate fade object and grab its settings component
        fadeCamera.ConfigureFade(this, fadeType, action, fadeColor, fadeInDuration, opaqueDuration, fadeOutDuration);
        fadeCamera.InitiateFade();
    }

    public void StayOpaque(Action doWhenDestroyed = null)
    {
        //Debug.Log("Fading - StayOpaque");

        var fadeType = FadeType.stayOpaque;
        SetOffFade(fadeType, doWhenDestroyed, _defaultFadeColor);
    }

    public void DestroyAllOpaqueFades(object sender, EventArgs e)
    {
        //Debug.Log("Destroying all opaque fades");

        OnAllOpaqueFadesDestroyed?.Invoke(this, null);
    }

    void FadeOutAtSceneStart(object o = null, EventArgs e = null)
    {
        //Debug.Log("Fading - Scene Start");

        SetOffFade(FadeType.fadeOut, null, _defaultFadeColor);
    }

    public void FadeToSignalInjectionKeywordDetected()
    {
        //Debug.Log("Fading - Manual Injection");

        var fadeInDur = 0.1f;
        var opaqueDur = 0.1f;
        var fadeOutDur = 0.2f;

        SetOffFade(FadeType.fadeInOut, null, Color.green, fadeInDur, opaqueDur, fadeOutDur);
    }

    void FadeOutToReloadScene()
    {
        //Debug.Log("Fading - To Reload Scene");

        var type = FadeType.fadeIn;
        Action action = () => { StayOpaque(); SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name); };

        SetOffFade(type, action, _defaultFadeColor);
    }

    void FadeToChangeCameraPosition(string args)
    {
        // check whether keyword is valid
        if (!(args.Equals(VoiceControlKeywords.camera_reset.ToString()) ||
        args.Equals(VoiceControlKeywords.camera_to_intervention.ToString()) ||
        args.Equals(VoiceControlKeywords.camera_to_control.ToString()) ||
        args.Equals(VoiceControlKeywords.camera_to_head.ToString()))) return;

        //Debug.Log("Fading - Change Camera Position");

        var type = FadeType.fadeInOut;
        Action action = () =>
        {
            cameraRigController.ChangeCameraTo(args);
        };

        SetOffFade(type, action, _defaultFadeColor);
    }
}
