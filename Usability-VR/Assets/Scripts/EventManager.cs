using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;
using static ApplicationSettings;

/// <summary>
/// Event manager for the whole application.
/// </summary>
public class EventManager : MonoBehaviour
{
    public VoiceControl voiceControl;
    public SoundEffectsManager soundEffectsManager;

    public delegate void ScenarioRead(StudyControl studyControl);
    public event ScenarioRead OnScenarioSettingsApplied;
    /// <summary>
    /// When the application is started, it will search for scenario settings.
    /// If valid settings are found and applied, tell all listeners.
    /// </summary>
    /// <param name="studyControl">The StudyControl which contains all necessary parameters for other scripts to adapt their functionality.</param>
    public void RaiseOnScenarioSettingsApplied(StudyControl studyControl)
    {
        OnScenarioSettingsApplied?.Invoke(studyControl);
    }

    public event EventHandler OnScenarioNotApplied;
    public void RaiseOnScenarioNotApplied()
    {
        OnScenarioNotApplied?.Invoke(this, null);
    }

    public event EventHandler OnScenarioPreparedAndStarted;
    public void RaiseOnScenarioPreparedAndStarted()
    {
        OnScenarioPreparedAndStarted?.Invoke(this, null);
    }

    public event EventHandler OnScenarioStopped;
    public void RaiseOnScenarioStopped(object sender, EventArgs e)
    {
        OnScenarioStopped?.Invoke(sender, e);
    }

    public delegate void SceneDetailLevelChange(SceneDetailLevel detailLevel);
    public event SceneDetailLevelChange OnSceneDetailLevelChange;
    public void RaiseOnSceneDetailLevelChange(SceneDetailLevel detailLevel)
    {
        OnSceneDetailLevelChange?.Invoke(detailLevel);
    }

    public delegate void ToolTablePlacementFinished();
    public event ToolTablePlacementFinished OnToolTablePlacementFinished;
    public void RaiseOnToolTablePlacementFinished()
    {
        OnToolTablePlacementFinished?.Invoke();
    }


    public delegate void ManualInjection(string keyword);
    public event ManualInjection OnManualInjection;
    public void RaiseOnManualInjection(string keyword)
    {
        OnManualInjection?.Invoke(keyword);
    }

    public delegate void SoundEffectPlay(AudioClip clip, Vector3 location, float volume);
    public event SoundEffectPlay OnSoundEffectPlayed;

    public void RaiseOnSoundEffectPlayed(AudioClip clip, Vector3 location = new Vector3(), float volume = 1.0f)
    {
        OnSoundEffectPlayed?.Invoke(clip, location, volume);
    }
}
