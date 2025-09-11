using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ApplicationSettings;

/// <summary>
/// This class is a representation of the settings of one scenario.
/// </summary>
public class ScenarioSettingsContainer
{
    public string testSubjectID;
    public string comment;
    public PreferredInterventionSide interventionSide;
    public Scenario scenario;
    public SceneDetailLevel sceneDetailLevel;
    public LaserAngles.AngleNames laserAngle;
    public NeedleMarkerPositions.MarkerPositions insertionMarkerDepth;
}

