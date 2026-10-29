using UnityEngine;

public class StartTask : IBiomeTask
{
    [SerializeField] GameObject weaponPrefab;
    [SerializeField] SOWeapon starterWeaponSO;
    
    public override bool Do()
    {
        if (base.Do() == false)
            return false;

        Vector3 startPos = room.root.transform.position - new Vector3(0f, room.global.transform.lossyScale.y/2f, 0f);
        
        GameObject pGo = DependenciesInjector.PDeps.Player;
        pGo.transform.position = startPos + new Vector3(0f, Utils.GetWorldSize(DependenciesInjector.PDeps.PCollider).y/2f, 0f);
        
        if (starterWeaponSO == null || weaponPrefab == null)
            return true;
        
        // Drop on Player
        GameObject weapon = Instantiate(weaponPrefab);
        ObjectBrain wBrain = weapon.GetComponent<ObjectBrain>();
        wBrain.Weapon = starterWeaponSO;
        wBrain.GenerateWeapon();
        weapon.transform.position = startPos + new Vector3(0f, 1f, 0f);
        
        // Give directly weapon to Player
        // pGo.GetComponentInChildren<PlayerWeaponSystem>().GiveWeapon(starterWeaponSO);
        
        return true;
    }
}
