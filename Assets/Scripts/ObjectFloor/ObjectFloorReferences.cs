using Unity.VisualScripting;
using UnityEngine;

public class ObjectFloorReferences : MonoBehaviour
{
    [Header("GameObject")]
    [SerializeField] GameObject _ObjectFloorRoot;
    [SerializeField] GameObject _ObjectFloorModel;




    public GameObject ObjectFloorRoot { get => _ObjectFloorRoot; }
    public GameObject ObjectFloorModel { get => _ObjectFloorModel; }
}
