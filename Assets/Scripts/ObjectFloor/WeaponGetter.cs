using Unity.VisualScripting;
using UnityEngine;

public class WeaponGetter : MonoBehaviour
{
    [SerializeField] ObjectBrain _brain;

    public ObjectBrain GetBrain()
    {
        return _brain;
    }
}
