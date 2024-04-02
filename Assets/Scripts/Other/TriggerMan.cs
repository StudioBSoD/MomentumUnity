using UnityEngine;
using UnityEngine.Events;

public class TriggerMan : MonoBehaviour
{
    public UnityEvent onTriggerEnter;
    public UnityEvent onTriggerExit;

    public void OnTriggerEnter(Collider other)
    {
        onTriggerEnter.Invoke();
    }

    public void OnTriggerExit(Collider other)
    {
        onTriggerExit.Invoke();
    }
}
