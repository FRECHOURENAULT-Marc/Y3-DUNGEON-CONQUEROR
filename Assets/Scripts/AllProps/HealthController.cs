using System;
using UnityEngine;
using UnityEngine.UI;

public class HealthController : MonoBehaviour
{
    [SerializeField] GameObject _propsRoot;
    [SerializeField] EnnemiBrain _ennemiBrain;

    [Header("Health Setting")]
    [SerializeField] float _maxHealth;
    [SerializeField] float _currentHealth;

    [Header("Referances")]
    [SerializeField] Slider _healthBarSlider;

    public event Action PropsDeath;


    public float CurrentHealth { get => _currentHealth; }

    public event Action GetDamage;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Set min max on slider
        _healthBarSlider.maxValue = _maxHealth;
        _healthBarSlider.minValue = 0;

        // Set the current health value
        _currentHealth = _maxHealth;
        _healthBarSlider.value = _currentHealth;
    }


    public void GetAttacked(float dmg)
    {
        // Debug.Log($"Getting attacked (-{dmg} health)");
        GetDamage?.Invoke();

        if ( dmg < 0 )
            return;

        if ( _currentHealth - dmg <= 0 )
        {
            Death();
            _currentHealth = 0;
        }
        else
            _currentHealth -= dmg;
        
        _healthBarSlider.value = _currentHealth;
    }

    void Death()
    {
        Debug.Log($"{gameObject} is dead");
        PropsDeath?.Invoke();

        if ( _ennemiBrain != null )
            _ennemiBrain.IsAlive = false;


        if ( _propsRoot != null )
            Destroy(_propsRoot);
    }
}
