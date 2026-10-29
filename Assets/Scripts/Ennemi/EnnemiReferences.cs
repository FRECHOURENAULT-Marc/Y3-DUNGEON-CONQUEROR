using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnnemiReferences : MonoBehaviour
{
    [Header("GameObject")]
    [SerializeField] GameObject _playerOBJ;
    [SerializeField] GameObject _ennemiRoot;

    [Header("Component")]
    [SerializeField] NavMeshAgent _navMeshAgent;
    [SerializeField] Animator _ennemiAnimator;
    [SerializeField] SphereCollider _targetAttackSphere;

    [Header("Script")]
    [SerializeField] EnnemiBrain _EBrain;
    [SerializeField] EnnemiMovement _Emovement;
    [SerializeField] listCollidObject _EnnemiColliderTarget;


    // Getter Gameobject
    public GameObject Player { get => _playerOBJ; set => _playerOBJ = value; }
    public GameObject EnnemiRoot { get => _ennemiRoot; }
    
    // Getter Component
    public NavMeshAgent NavMeshAgent { get => _navMeshAgent; }
    public Animator EnnemiAnimator { get => _ennemiAnimator; }
    public SphereCollider EnnemiAttackSphere { get => _targetAttackSphere; }

    // Getter Script
    public EnnemiBrain EnnemiBrain { get => _EBrain; }
    public EnnemiMovement EnnemiMovement { get => _Emovement; }
    public listCollidObject EnnemiColliderTarget { get => _EnnemiColliderTarget; }
}
