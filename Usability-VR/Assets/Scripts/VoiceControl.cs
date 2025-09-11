using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;
using System.Linq;
using System;
using static ApplicationSettings;

public class VoiceControl : MonoBehaviour
{
    public EventManager eventManager;

    KeywordRecognizer _keywordRecognizer;
    Dictionary<string, System.Action> keywords;

    public delegate void KeywordRecognizerDelegate(string keyword);
    public event KeywordRecognizerDelegate OnKeywordRecognized;

    [Tooltip("Set the keyword which will detect a manual injection. " +
        "If no valid keyword is set, the application will use the default keyword defined in the Application Settings.")]
    public string manualInjectionKeyword;

    private void Start()
    {
        eventManager.OnScenarioSettingsApplied += PrepareKeywordRecognizer;
        eventManager.OnScenarioStopped += ClearKeywordRecognizer;
    }

    /// <summary>
    /// Prepare all voice control commands.
    /// </summary>
    /// <param name="studyControl">Not used.</param>
    private void PrepareKeywordRecognizer(StudyControl studyControl)
    {
        // remove all non-letter characters from custom keyword
        manualInjectionKeyword = new string((from c in manualInjectionKeyword
                                             where char.IsLetter(c) || char.IsWhiteSpace(c)
                                             select c
        ).ToArray());

        // If no custom keyword was set then use the default one.
        if (manualInjectionKeyword.Equals("")) { manualInjectionKeyword = VoiceControlKeywords.stop.ToString().Replace('_', ' '); }

        Debug.Log("Voice control manual injection keyword is \"" + manualInjectionKeyword + "\"."
            + "\nKeyCode for manual injection is \"" + ApplicationSettings.manualInjectionKeyCode + "\".");

        //Create keywords for keyword recognizer
        keywords = new Dictionary<string, System.Action>();
        keywords.Add(manualInjectionKeyword, () =>
        {
            OnKeywordRecognized?.Invoke(manualInjectionKeyword);
        });
        keywords.Add(VoiceControlKeywords.camera_reset.ToString().Replace('_', ' '), () =>
        {
            OnKeywordRecognized?.Invoke(VoiceControlKeywords.camera_reset.ToString().Replace('_', ' '));
        });
        keywords.Add(VoiceControlKeywords.camera_to_control.ToString().Replace('_', ' '), () =>
        {
            OnKeywordRecognized?.Invoke(VoiceControlKeywords.camera_to_control.ToString().Replace('_', ' '));
        });
        keywords.Add(VoiceControlKeywords.camera_to_head.ToString().Replace('_', ' '), () =>
        {
            OnKeywordRecognized?.Invoke(VoiceControlKeywords.camera_to_head.ToString().Replace('_', ' '));
        });
        keywords.Add(VoiceControlKeywords.camera_to_intervention.ToString().Replace('_', ' '), () =>
        {
            OnKeywordRecognized?.Invoke(VoiceControlKeywords.camera_to_intervention.ToString().Replace('_', ' '));
        });

        _keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());
        _keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
        _keywordRecognizer.Start();
    }

    /// <summary>
    /// Clear the current keyword recognizer when it's not needed anymore.
    /// </summary>
    /// <param name="sender">Not used.</param>
    /// <param name="e">Not used.</param>
    private void ClearKeywordRecognizer(object sender, EventArgs e)
    {
        _keywordRecognizer?.Stop();
        _keywordRecognizer?.Dispose();
    }

    private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        Action keywordAction;
        // if the keyword recognized is in our dictionary, call that Action.
        if (keywords.TryGetValue(args.text, out keywordAction))
        {
            Debug.Log("Voice control keyword " + args.text + " recognised");
            keywordAction.Invoke();
        }
    }
}
