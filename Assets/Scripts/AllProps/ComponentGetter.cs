using Unity.VisualScripting;
using UnityEngine;

public class ComponentGetter : MonoBehaviour
{
    [SerializeField] HealthController _healthController;


    public HealthController HealthController { get => _healthController; }

}
