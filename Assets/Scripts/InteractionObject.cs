using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractionObject : MonoBehaviour
{
    public abstract void SetInteraction(InteractionManager manager, ServerMessage message);

    public abstract void StopInteraction(InteractionManager manager);

    public abstract void CheckTime(InteractionManager manager);

    public abstract string GetInteractionType(InteractionManager manager);

    public abstract void UpdateObject(InteractionManager manager);

}
