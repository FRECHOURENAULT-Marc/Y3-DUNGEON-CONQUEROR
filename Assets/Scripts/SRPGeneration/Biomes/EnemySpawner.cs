using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class EnemyWaveData
{
    public GameObject enemyPrefab;
    public int count;
    public float startTime;
}

public class EnemySpawner : IBiomeTask
{
    [SerializeField] List<EnemyWaveData> waves;
    [HideInInspector] public int waveIndexToPlay;
    
    EnemyWaveData _actualWaveData;
    int _enemiesKilled;
    bool _isWavePlaying;
    
    public bool IsWaveCompleted
    {
        get
        {
            if(_actualWaveData == null)
                return true;
            return (_isWavePlaying == false) && (_enemiesKilled == _actualWaveData.count);
        } 
    }

    public int LastWaveIndex
    {
        get => waves.Count - 1;
    }

    public override void Reset()
    {
        base.Reset();
        _doMultipleTimes = false;
        
        waveIndexToPlay = 0;
        
        _actualWaveData = null;
        _enemiesKilled = 0;
        _isWavePlaying = false;
    }
    
    public override bool Do()
    {
        if(base.Do() == false)
            return false;
        
        return PlayWave(waveIndexToPlay);
    }
    
    public bool PlayWave(int index)
    {
        if (index < 0 || index >= waves.Count)
        {
            Debug.LogError("Wave " + index + " cannot be played because index is invalid (min: 0, max: " + LastWaveIndex + ")");
            return false;
        }
        
        Debug.Log("Starting wave " + index);
        StartWaveAsync(waves[index]);
        return true;
    }

    async Task StartWaveAsync(EnemyWaveData wave)
    {
        _isWavePlaying = true;
        _enemiesKilled = 0;
        _actualWaveData = wave;
        
        await Awaitable.WaitForSecondsAsync(wave.startTime, destroyCancellationToken);
        if (destroyCancellationToken.IsCancellationRequested)
            return;

        GameObject actionGo = wave.enemyPrefab.transform.GetChild(1).gameObject;
        float enemyHeight = Utils.GetWorldSize( actionGo.GetComponentInChildren<Collider>()).y;
        float localY = -room.global.transform.lossyScale.y/2f + enemyHeight/2f;
        
        for (int i = 0; i < wave.count; i++)
        {
            float localX = Random.Range(-room.global.transform.lossyScale.x/2f, room.global.transform.lossyScale.x/2);
            float localZ = Random.Range(-room.global.transform.lossyScale.z/2f, room.global.transform.lossyScale.z/2);
            Vector3 localPos = new Vector3(localX, localY, localZ);
            
            GameObject enemy = Instantiate(wave.enemyPrefab, room.root.transform);
            enemy.transform.localPosition = localPos;
            enemy.GetComponentInChildren<HealthController>().PropsDeath += () => _enemiesKilled++;
        }

        while (_enemiesKilled < wave.count)
        {
            await Awaitable.WaitForSecondsAsync(0.25f, destroyCancellationToken);
            if (destroyCancellationToken.IsCancellationRequested)
                return;
        }

        _isWavePlaying = false;

        if (waveIndexToPlay == LastWaveIndex)
        {
            _done = true;
            Debug.LogWarning("Last wave played: " + waveIndexToPlay);
        }
            
    }
}

// public class EnemyBiome : BiomeBehavior
// {
//     [SerializeField] List<EnemyWaveData> waves;
//     
//     bool _isWaveCompleted;
//     int _enemiesKilled;
//     int _lastIndex = 0;
//     
//     // Start is called once before the first execution of Update after the MonoBehaviour is created
//     void Start()
//     {
//         _isWaveCompleted = true;
//         _enemiesKilled = 0;
//         _lastIndex = 0;
//     }
//
//     public void PlayWave(int index)
//     {
//         if (index < 0 || index >= waves.Count)
//         {
//             Debug.LogError("Wave " + index + " cannot be played because index is invalid (min: 0, max: " + (waves.Count-1) + ")");
//             return;
//         }
//         if (_isWaveCompleted == false)
//         {
//             Debug.LogWarning("Wave " + index + " cannot be played because another one is currently playing.");
//             return;
//         }
//         StartWaveAsync(waves[index]);
//     }
//
//     async Task StartWaveAsync(EnemyWaveData wave)
//     {
//         _isWaveCompleted = false;
//         _enemiesKilled = 0;
//         
//         await Awaitable.WaitForSecondsAsync(wave.startTime, destroyCancellationToken);
//         if (destroyCancellationToken.IsCancellationRequested)
//             return;
//
//         float enemyHeight = Utils.GetWorldSize(wave.enemyPrefab.GetComponentInChildren<Collider>()).y;
//         float localY = -room.global.transform.lossyScale.y/2f + enemyHeight/2f;
//         
//         for (int i = 0; i < wave.count; i++)
//         {
//             float localX = Random.Range(-room.global.transform.lossyScale.x/2f, room.global.transform.lossyScale.x/2);
//             float localZ = Random.Range(-room.global.transform.lossyScale.z/2f, room.global.transform.lossyScale.z/2);
//             Vector3 localPos = new Vector3(localX, localY, localZ);
//             
//             GameObject enemy = Instantiate(wave.enemyPrefab, room.root.transform);
//             enemy.transform.localPosition = localPos;
//             enemy.GetComponentInChildren<HealthController>().PropsDeath += () => _enemiesKilled++;
//         }
//
//         while (_enemiesKilled < waves.Count)
//         {
//             await Awaitable.WaitForSecondsAsync(0.25f, destroyCancellationToken);
//             if (destroyCancellationToken.IsCancellationRequested)
//                 return;
//         }
//
//         _isWaveCompleted = true;
//     }
//
//     void Update()
//     {
//         if (_isWaveCompleted == false)
//             return;
//         
//         PlayWave(_lastIndex);
//         _lastIndex++;
//     }
// }
