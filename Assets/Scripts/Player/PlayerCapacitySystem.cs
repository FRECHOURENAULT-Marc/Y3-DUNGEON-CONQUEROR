using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerCapacitySystem : MonoBehaviour
{
    [SerializeField] PlayerReferences _PReferences;

    [Header("Setting")]
    [SerializeField] List<SOCapacity> _capacityList;

    [SerializeField] List<int> _levelStep;
    List<SOCapacity> _capacityActive;

    int _currentStep;

    void Start()
    {
        _currentStep = 0;
        _capacityActive = new List<SOCapacity>();
    }

    void Update()
    {
        int playerLevel = _PReferences.PlayerXPSystem.Level;

        if ( playerLevel == _levelStep[_currentStep] )
        {
            Debug.Log($"New capacity can be unlock");
            UnlockCapacity();

            _currentStep++;
        }
    }

    [ContextMenu("Unlock capacity")]
    void UnlockCapacity()
    {
        SOCapacity cap1 = _capacityList[Random.Range(0, _capacityList.Count - 1)];
        // SOCapacity cap2 = _capacityList[Random.Range(0, _capacityList.Count) - 1];

        _capacityActive.Add(cap1);
        _capacityList.Remove(cap1);

        ActivateCapacity(cap1);
    }

    void ActivateCapacity(SOCapacity cap)
    {
        string name = cap.CapacityName;

        Debug.Log($"You have unlock {cap.CapacityName} ({cap.description})");

        if (name == "DoubleJump") _PReferences.PlayerMovement.AsUnlockDoubleJump = true;

        else if (name == "Dash") _PReferences.PlayerMovement.AsUnlockDash = true;

        else if (name == "Attack Distance Multiplier") _PReferences.PlayerAttackSystem.AsUnlockDistanceMultiplier = true;

        else if (name == "Speed Multiplier") _PReferences.PlayerMovement.AsUnlockSpeedMultiplier = true;

        else if (name == "Damage Multiplier") _PReferences.PlayerAttackSystem.AsUnlockDamageMultiplier = true;
    }


}
