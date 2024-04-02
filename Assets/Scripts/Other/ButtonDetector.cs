using UnityEngine;
using Fragsurf.Movement;

public class ButtonDetector : MonoBehaviour
{
    private Button currentButton;
    public LayerMask buttonMask;
    bool found;

    private void Update()
    {
        found = Physics.Raycast(SurfCharacter.local.viewTransform.position, SurfCharacter.local.viewTransform.forward, out RaycastHit hit, 30f, buttonMask);
        if (Input.GetButtonDown("Interact") && currentButton != null)
            currentButton.Push();
        if (found && currentButton == null)
        {
            currentButton = hit.transform.GetComponent<Button>();
            Game.local.btnTooltip.SetActive(true);
        }
        else if (!found && currentButton != null)
        {
            currentButton = null;
            Game.local.btnTooltip.SetActive(false);
        }
    }
}
