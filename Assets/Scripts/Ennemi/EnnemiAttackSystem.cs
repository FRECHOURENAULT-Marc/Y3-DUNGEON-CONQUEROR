using System.Collections.Generic;
using UnityEngine;

public class EnnemiAttackSystem : MonoBehaviour
{
    [SerializeField] EnnemiReferences _EReferences;

    [Header("Settings")]
    [SerializeField] float _attackDistance;
    [SerializeField] float _damage;
    [SerializeField] float _animationDuration;

    [Header("Debug")]
    [SerializeField] List<GameObject> _targetList;

    void Start()
    {
        _EReferences.EnnemiColliderTarget.CollidEnter += AddToList;
        _EReferences.EnnemiColliderTarget.CollidExit += RemoveToList;

        _EReferences.EnnemiAttackSphere.radius = _attackDistance;


        _EReferences.EnnemiBrain.OnAttack += HandleAttack;
    }

    void OnDestroy()
    {
        _EReferences.EnnemiColliderTarget.CollidEnter -= AddToList;
        _EReferences.EnnemiColliderTarget.CollidExit -= RemoveToList;

        _EReferences.EnnemiBrain.OnAttack -= HandleAttack;
    }




    void AddToList(GameObject obj)
    {
        _targetList.Add(obj);
    }

    void RemoveToList(GameObject obj)
    {
        _targetList.Remove(obj);
    }


    async void HandleAttack()
    {
        await Awaitable.WaitForSecondsAsync(_animationDuration / 2);

        foreach (GameObject ctx in _targetList)
        {
            if ( ctx.GetComponent<PlayerTag>() != null )
            {
                HealthController ctxHealth = ctx.GetComponent<ComponentGetter>().HealthController;

                ctxHealth.GetAttacked(_damage);
            }
        }
    }
}
