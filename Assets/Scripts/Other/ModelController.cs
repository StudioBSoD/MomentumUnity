using Fragsurf.Movement;
using UnityEngine;

public class ModelController : MonoBehaviour
{
    public Animator animator;
    public Transform spine;
    private Quaternion defSpineRot;
    int xHash;
    int yHash;
    int groundedHash;
    int walkHash;
    int crouchHash;

    private void Awake()
    {
        xHash = Animator.StringToHash("X");
        yHash = Animator.StringToHash("Y");
        groundedHash = Animator.StringToHash("Grounded");
        walkHash = Animator.StringToHash("Walk");
        crouchHash = Animator.StringToHash("Crouch");
        defSpineRot = spine.rotation;
    }

    private void LateUpdate()
    {
        if (InputManager.local.jump && SurfCharacter.local.groundObject == null && !IsInvoking(nameof(UnGround))) UnGround();
        if (SurfCharacter.local.groundObject == null && !IsInvoking(nameof(UnGround))) Invoke(nameof(UnGround), .1f);
        else if (IsInvoking(nameof(UnGround)) && SurfCharacter.local.groundObject != null) CancelInvoke(nameof(UnGround));
        if (SurfCharacter.local.groundObject != null) animator.SetBool(groundedHash, true);
    }

    private void Update()
    {
        animator.SetFloat(xHash, Vector3.Dot(SurfCharacter.local.controller.playerTransform.right, SurfCharacter.local.moveData.velocity));
        animator.SetFloat(yHash, Vector3.Dot(SurfCharacter.local.controller.playerTransform.forward, SurfCharacter.local.moveData.velocity));
        animator.SetBool(walkHash, InputManager.local.walk && !InputManager.local.sprint);
        animator.SetBool(crouchHash, SurfCharacter.local.controller.crouching);
        //spine.localRotation = Quaternion.Euler(defSpineRot.x, (-SurfCharacter.local.controller.camera.localRotation.x - .1f) * 70f, defSpineRot.z);
    }

    private void UnGround()
    {
        animator.SetBool(groundedHash, false);
    }
}
