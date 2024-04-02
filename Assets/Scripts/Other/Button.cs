using UnityEngine;
using UnityEngine.Events;

public class Button : MonoBehaviour
{
    [SerializeField] private UnityEvent pushedEvent;
    public void Push()
    {
        if (pushedEvent != null) pushedEvent.Invoke();
    }
}
