using UnityEngine;
using UnityEngine.Animations;

public class ArmeToCam : MonoBehaviour
{
    [SerializeField] PlayerReferences _Pref;
    [SerializeField] ParentConstraint _constrainte;

    void Start()
    {
        ConstraintSource source = new ConstraintSource() { sourceTransform = _Pref.PlayerCamera.transform, weight = 1 };
        _constrainte.AddSource(source);
    }
}
