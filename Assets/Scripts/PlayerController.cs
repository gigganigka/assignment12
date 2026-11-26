using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed;
    public float jumpForce;
    public Transform groundCheck;
    public float groundRadius;
    public LayerMask groundLayer;
    public AudioClip jumpSound;

    Rigidbody2D rb;
    SpriteRenderer sr;
    Animator anim;
    Health health;
    float keyboardMove, touchMove;
    bool keyboardDirectionHeld;
    bool touchActive;
    bool isGrounded;
    bool CanAct => isActiveAndEnabled && Time.timeScale > 0f &&
                   (health == null || health.CurrentHp > 0);
    float Move
    {
        get
        {
            // A keyboard key also overrides a joystick stuck at its centre.
            if(keyboardDirectionHeld) return keyboardMove;
            if(touchActive) return touchMove;
            Gamepad pad=Gamepad.current;
            if(pad==null) return 0;
            float stick=pad.leftStick.ReadValue().x;
            return Mathf.Clamp(stick!=0?stick:pad.dpad.ReadValue().x,-1,1);
        }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        PlayerStats stats=GameCatalog.Current.player;
        moveSpeed=stats.moveSpeed; jumpForce=stats.jumpVelocity; groundRadius=stats.groundRadius;
        rb.gravityScale=stats.gravityScale;
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        health = GetComponent<Health>();
    }
    void Update()
    {
        PollPhysicalInput();
        CheckGround();
        float move = CanAct ? Move : 0f;
        if (sr != null && Mathf.Abs(move) > 0.1f) sr.flipX = move < 0f;
        if (anim != null && anim.runtimeAnimatorController != null)
        {
            anim.SetFloat("Speed", Mathf.Abs(move));
            anim.SetBool("Grounded", isGrounded);
        }
    }
    void CheckGround()
    {
        // Passing upward through a one-way platform must not count as landing.
        isGrounded = rb != null && rb.linearVelocity.y <= GameCatalog.Current.player.groundedVelocityLimit && groundCheck != null &&
            Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer) != null;
    }
    void FixedUpdate()
    {
        if (rb != null)
            rb.linearVelocity = new Vector2((CanAct ? Move : 0f) * moveSpeed, rb.linearVelocity.y);
    }
    void PollPhysicalInput()
    {
        keyboardMove=0; keyboardDirectionHeld=false;
        if(!CanAct) return;
        Keyboard keyboard=Keyboard.current;
        if(keyboard!=null)
        {
            bool left=keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed;
            bool right=keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed;
            keyboardDirectionHeld=left || right;
            keyboardMove=(right?1f:0f)-(left?1f:0f);
            if(keyboard.spaceKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame ||
               keyboard.upArrowKey.wasPressedThisFrame) TryJump();
        }
        Gamepad pad=Gamepad.current;
        if(pad!=null && pad.buttonSouth.wasPressedThisFrame) TryJump();
    }
    // Retain serialized SendMessage targets, but poll physical devices once.
    // This avoids dependence on PlayerInput pairing and duplicate jump callbacks.
    public void OnMove(InputValue value) { }
    public void OnJump(InputValue value) { }
    public void SetTouchMove(float value, bool active)
    {
        touchMove = CanAct ? Mathf.Clamp(value, -1f, 1f) : 0f;
        touchActive = active && CanAct;
    }
    public void TryJump()
    {
        if (!CanAct || rb == null) return;
        CheckGround();
        if (!isGrounded) return;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        Sfx.Play(SoundEvent.Jump);
    }
    public void ResetInput()
    {
        keyboardMove = touchMove = 0f;
        touchActive = false;
        keyboardDirectionHeld=false;
        if (rb != null) rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }
    void OnDisable() { ResetInput(); }
    void OnApplicationFocus(bool focused) { if (!focused) ResetInput(); }
    void OnApplicationPause(bool paused) { if (paused) ResetInput(); }
}
