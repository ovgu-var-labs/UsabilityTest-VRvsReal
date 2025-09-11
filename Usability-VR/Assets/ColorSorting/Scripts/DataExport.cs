using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Valve.VR;

public class DataExport : MonoBehaviour
{
    [Header("File Settings")]
    [Tooltip("Directory in which to save the CSV file")]
    public string folderPath = "/Users/robertklank/Unity";
    [Tooltip("Identifier for the current user")]
    //public string userID = "test01";
    private StreamWriter writer;

    void Awake()
    {
        InitializeFile();  
    }
    
    // open or create csv
    private void InitializeFile()
    {
        // Ensure output directory exists
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string filePath = Path.Combine(folderPath, "results.csv");
        bool fileExists = File.Exists(filePath);

        writer = new StreamWriter(filePath, append: true);
        if (!fileExists)
        {
            // Write CSV header
            writer.WriteLine("Date;Start time;User ID;Environment;GUI Mode;Task Nr;Sort array;Phase;Phase time;Gestures-FalseNeg;False negatives;Dist. to 1st target;Dist. to 2nd;Dist. to 3rd target;Dist. to 4th target");  // add headers
            writer.Flush();     //added fields to header
        }
    }

    public void LogTaskEnd
    (
        DateTime    dateTime,
        string      userID,
        Enum        mode,
        int         taskNr,
        string      currentSortOrderString,
        string      phase,
        float       phaseTime,
        int         gestures,
        int         falseNegatives
        )
    {
        // Format fields
        string dateField      = dateTime.ToString("yyyy-MM-dd");
        string timeField      = dateTime.ToString("HH:mm:ss");
        string errorsField = mode.ToString();

        // Build CSV line
        string line = string.Join(";", new string[]
        {
            dateField,
            timeField,
            userID,
            "VR", // Change if VR session!
            errorsField,
            taskNr.ToString(),
            currentSortOrderString.ToString(),
            phase,
            phaseTime.ToString("F2"),
            gestures.ToString(),
            falseNegatives.ToString()
        });

        // Write and flush
        writer.WriteLine(line);
        writer.Flush();
        Debug.Log($"[DataExport] Saved: {line}");
    }

public void LogTaskEndA  //New method specific to phase A (Since its adds the proximity check)
    (
        DateTime    dateTime,
        string      userID,
        Enum        mode,
        int         taskNr,
        string      currentSortOrderString,
        string      phase,
        float       phaseTime,
        int         gestures,
        int         falseNegatives,
        List<float> phaseADistances                          // Distances in cm between the target color slide (middle) and the user target
        )
    {
        // Format fields
        string dateField      = dateTime.ToString("yyyy-MM-dd");
        string timeField      = dateTime.ToString("HH:mm:ss");
        string errorsField = mode.ToString();

        List<float> distanceInCm = phaseADistances.Select(n => n * 100f).ToList();     // Distance formating
        string distance1 = distanceInCm[0].ToString("F2");
        string distance2 = distanceInCm[1].ToString("F2");
        string distance3 = distanceInCm[2].ToString("F2");
        string distance4 = distanceInCm[3].ToString("F2");

        // Build CSV line
        string line = string.Join(";", new string[]
        {
            dateField,
            timeField,
            userID,
            "VR", // Change if VR session!
            errorsField,
            taskNr.ToString(),
            currentSortOrderString.ToString(),
            phase,
            phaseTime.ToString("F2"),
            gestures.ToString(),
            falseNegatives.ToString(),
            distance1,   //Added distances here too!
            distance2,
            distance3,
            distance4
        });

        // Write and flush
        writer.WriteLine(line);
        writer.Flush();
        Debug.Log($"[DataExport] Saved: {line}");
    }

    void OnDestroy()
    {
        // Close the writer to release the file handle
        if (writer != null)
        {
            writer.Close();
            writer = null;
        }
    }
}
