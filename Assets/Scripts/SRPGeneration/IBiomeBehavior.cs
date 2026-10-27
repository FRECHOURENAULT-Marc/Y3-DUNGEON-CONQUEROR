using UnityEngine;

public class IBiomeBehavior : MonoBehaviour
{
    public GameObject roomRoot;
    
    public virtual bool IsRoomCompleted() => false; // override if needed

    /// <summary>
    /// Call method "OpenDoor" of all doors of the room
    /// </summary>
    public virtual void CompleteRoom() // override if needed
    {
        roomRoot.GetComponent<SRPRoom>().Finish();
    }
}
