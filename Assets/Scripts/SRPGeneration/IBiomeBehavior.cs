using UnityEngine;

public class IBiomeBehavior : MonoBehaviour
{
    public GameObject roomRoot;
    
    public virtual bool IsRoomCompleted() => false; // override

    public virtual void CompleteRoom() // override if needed
    {
        roomRoot.GetComponent<SRPRoom>().Finish();
    }
}
