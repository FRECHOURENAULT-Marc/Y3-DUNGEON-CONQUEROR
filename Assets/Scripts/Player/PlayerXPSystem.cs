using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerXPSystem : MonoBehaviour
{
    [SerializeField] PlayerReferences _PReferences;

    [Space]
    [SerializeField] Slider _XPSlider;
    [SerializeField] TextMeshProUGUI _levelDisplay;

    [Header("Setting")]
    [SerializeField] int _maxLevel;
    [SerializeField] float _maxXP;
    

    [Header("Debug")]
    [SerializeField] int _currentLevel;
    [SerializeField] float _currentXP;

    public int Level { get => _currentLevel; }


    void Start()
    {
        _PReferences.PlayerAttackSystem.KillProps += HandleKillProps;

        _currentXP = 0;
        _currentLevel = 0;

        _XPSlider.maxValue = _maxXP;
        _XPSlider.value = _currentXP;
        _levelDisplay.text = $"{_currentLevel}";
    }


    [ContextMenu("sim xp")]
    void HandleKillProps()
    {
        if ( _currentLevel >= _maxLevel ) return;

        _currentXP += 10;

        while ( _currentXP >= _maxXP )
        {
            _currentXP -= _maxXP;
            _currentLevel++;

            if ( _currentLevel > _maxLevel )
                _currentLevel = _maxLevel;
        }

        // Update UI
        _XPSlider.value = _currentXP;
        _levelDisplay.text = $"{_currentLevel}";
    }
}
