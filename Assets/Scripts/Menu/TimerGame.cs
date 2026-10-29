using System;
using UnityEngine;

public class TimerGame : MonoBehaviour
{
    static DateTime _startTime;
    static DateTime _endTime;

    public static DateTime StartTime { get => _startTime; }
    public static DateTime EndTime { get => _endTime; }

    public static void StartTimer()
    {
        _startTime = DateTime.Now;
    }

    public static void StopTimer()
    {
        _endTime = DateTime.Now;
    }
}
