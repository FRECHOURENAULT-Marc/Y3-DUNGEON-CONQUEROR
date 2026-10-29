using System;
using UnityEngine;

public enum WEAPON_SELECTED
{
    Sword,
    Axe,
    dagger,
}

[CreateAssetMenu(fileName = "GenerationMenuParametersSO", menuName = "Scriptable Objects/GenerationMenuParametersSO")]
public class GenerationMenuParameters : ScriptableObject
{
    public int generationEffort = 500;
    public int maxRooms = 5;
    public int seed = 0;
    public bool rdmSeed = false;
    public WEAPON_SELECTED weapon = WEAPON_SELECTED.Sword;
    
    public void SetEffort(int effort) => generationEffort = Math.Clamp(effort, 0, int.MaxValue);
    public void SetEffort(string effort) => SetEffort(int.Parse(effort));
    public void SetTarget(int target) => maxRooms = Math.Clamp(target, 0, int.MaxValue);
    public void SetTarget(string target) => SetTarget(int.Parse(target));
    public void SetSeed(int s) => seed = s;
    public void SetSeed(string s) => SetSeed(int.Parse(s));
    public void SetRandomSeed(bool rdm) => rdmSeed = rdm;
    
    public void SetWeapon(int w) => weapon = (WEAPON_SELECTED)w;
}
