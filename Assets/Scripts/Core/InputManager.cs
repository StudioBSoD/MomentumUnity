using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager local;
    public Vector3 move;
    public Vector2 look;
    public bool jump;
    public bool jumpOnce { get; private set; }
    public bool sprint;
    public bool crouch;
    public bool walk;

    private void Awake()
    {
        if (local != null) { Destroy(gameObject); return; }
        local = this;
    }

    private void Update()
    {
        if (Game.local.dontControl) return;
        move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        look = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        jump = Input.GetButton("Jump");
        jumpOnce = Input.GetButtonDown("Jump");
        sprint = Input.GetButton("Sprint");
        crouch = Input.GetButton("Crouch");
        walk = Input.GetButton("Walk");
    }
}
