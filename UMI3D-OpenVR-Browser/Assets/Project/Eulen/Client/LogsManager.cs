using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogsManager : MonoBehaviour
{
    [HideInInspector] public int LogMedia;
    /// <summary>
    /// Full time log performing/viewing the exercises
    /// </summary>
    [HideInInspector] public int LogPerform;

    /// <summary>
    /// Number of attempts performed in the exercises log
    /// </summary>
    [HideInInspector] public int[] LogAttemptsAll;
    /// <summary>
    /// Number of views of the exercises log
    /// </summary>
    [HideInInspector] public int[] LogViewsAll = new int[3];

    /// <summary>
    /// Results history of the 1st exercise
    /// </summary>
    [HideInInspector] public List<string> LogResultA = new();
    /// <summary>
    /// Results history of the 2nd exercise
    /// </summary>
    [HideInInspector] public List<string> LogResultB = new();
    /// <summary>
    /// Results history of the 3rd exercise
    /// </summary>
    [HideInInspector] public List<string> LogResultC = new();
    /// <summary>
    /// Take note if the user has done the exercise well
    /// </summary>
    [HideInInspector] public bool[] IsWellDone;

    /// <summary>
    /// Has the user seen all the media content
    /// </summary>
    [HideInInspector] public bool IsAllMediaViewedLog = false;

    public void UpdateResultLogs(int logType, string log)
    {
        Debug.LogError("To remove");
    }
}
