using UnityEngine;

public class CrosshairActivator : MonoBehaviour
{
    [SerializeField] private GameObject centerDot;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1) && !centerDot.activeSelf)
            centerDot.SetActive(true);
        else if (Input.GetKeyUp(KeyCode.F1) && centerDot.activeSelf)
            centerDot.SetActive(false);
    }
}
