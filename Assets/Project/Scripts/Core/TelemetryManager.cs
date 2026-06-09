using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;

[Serializable]
public class TelemetryEvent
{
    public string timestamp; 
    public string category;
    public string detail;
}

[Serializable]
public class GameMetrics
{
    public string sessionDate;
    public string finalOutcome; 
    public string totalTimePlayed; 

    [Header("Análisis Espacial (BSP)")]
    public int totalDoorsOpened;

    [Header("Análisis de IA (Director)")]
    public int totalEnemyTeleports;
    public int teleportsNearPlayer;
    public int timesChased;
    public string totalTimeInChase; 

    [Header("Línea de Tiempo (Timeline)")]
    public List<TelemetryEvent> timeline = new List<TelemetryEvent>();
}

public class TelemetryManager : MonoBehaviour
{
    public static TelemetryManager Instance { get; private set; }

    private GameMetrics metrics = new GameMetrics();
    private float startTime;

    
    private bool isCurrentlyChased = false;
    private float chaseStartTime;
    private float rawTotalTimeInChase = 0f; 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            startTime = Time.time;
            metrics.sessionDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            metrics.totalTimeInChase = "00:00:00";
            LogEvent("Sistema", "Generación BSP completada. Partida iniciada.");
        }
        else { Destroy(gameObject); }
    }

    
    private string FormatTime(float timeInSeconds)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(timeInSeconds);
        
        return string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
    }

    
    public void LogEvent(string category, string detail)
    {
        float currentTime = Time.time - startTime;
        metrics.timeline.Add(new TelemetryEvent
        {
            timestamp = FormatTime(currentTime), 
            category = category,
            detail = detail
        });
    }

    
    public void RegisterDoorOpened()
    {
        metrics.totalDoorsOpened++;
        LogEvent("Exploración", $"Puerta abierta. Total: {metrics.totalDoorsOpened}");
    }

    
    public void RegisterEnemyTeleport(bool nearPlayer)
    {
        metrics.totalEnemyTeleports++;
        if (nearPlayer)
        {
            metrics.teleportsNearPlayer++;
            LogEvent("IA_Director", "Teletransporte de Tensión (Cerca del jugador)");
        }
        else
        {
            LogEvent("IA_Director", "Teletransporte de Reposicionamiento (Lejos)");
        }
    }

    public void SetChaseState(bool isChasing)
    {
        if (isChasing == isCurrentlyChased) return;

        isCurrentlyChased = isChasing;

        if (isChasing)
        {
            metrics.timesChased++;
            chaseStartTime = Time.time;
            LogEvent("Combate", "Inicio de persecución");
        }
        else
        {
            float chaseDuration = Time.time - chaseStartTime;
            rawTotalTimeInChase += chaseDuration; 

            
            metrics.totalTimeInChase = FormatTime(rawTotalTimeInChase);
            LogEvent("Combate", $"Fin de persecución. Duración: {FormatTime(chaseDuration)}");
        }
    }

    
    public void ExportMetrics(string outcome)
    {
        if (isCurrentlyChased) SetChaseState(false);

        metrics.finalOutcome = outcome;

        
        metrics.totalTimePlayed = FormatTime(Time.time - startTime);

        LogEvent("Sistema", $"Fin de la partida: {outcome}");

        string json = JsonUtility.ToJson(metrics, true);
        string filename = $"/Telemetry_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        string path = Application.persistentDataPath + filename;

        File.WriteAllText(path, json);
        Debug.Log($"<color=cyan>Métricas (00:00:00) guardadas en: {path}</color>");
    }
}
