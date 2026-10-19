using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class PlayerController2D : MonoBehaviour
{
    public float speed = 4f;
    public float walkFps = 10f;
    public float idleFps = 4f;

    public Sprite[] walkDown, walkUp, walkLeft, walkRight;
    public Sprite[] idleDown, idleUp, idleLeft, idleRight;

    Rigidbody2D rb;
    SpriteRenderer sr;
    Vector2 input;
    Vector2 facing = Vector2.down;
    float timer;
    bool wasMoving;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    Vector2 ReadInput()
    {
#if ENABLE_INPUT_SYSTEM
        var k = Keyboard.current;
        if (k == null) return Vector2.zero;
        float x = ((k.dKey.isPressed || k.rightArrowKey.isPressed) ? 1 : 0)
                - ((k.aKey.isPressed || k.leftArrowKey.isPressed) ? 1 : 0);
        float y = ((k.wKey.isPressed || k.upArrowKey.isPressed) ? 1 : 0)
                - ((k.sKey.isPressed || k.downArrowKey.isPressed) ? 1 : 0);
        return new Vector2(x, y);
#else
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#endif
    }

    void Update()
    {
        input = ReadInput();
        if (input.sqrMagnitude > 1f) input.Normalize();
        bool moving = input.sqrMagnitude > 0.01f;

        if (moving)
            facing = Mathf.Abs(input.x) > Mathf.Abs(input.y)
                ? new Vector2(Mathf.Sign(input.x), 0)
                : new Vector2(0, Mathf.Sign(input.y));

        if (moving != wasMoving) { timer = 0f; wasMoving = moving; }

        Sprite[] frames = Pick(moving);
        if (frames == null || frames.Length == 0) return;

        timer += Time.deltaTime * (moving ? walkFps : idleFps);
        sr.sprite = frames[(int)timer % frames.Length];
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + input * speed * Time.fixedDeltaTime);
    }

    Sprite[] Pick(bool moving)
    {
        if (facing.y < 0) return moving ? walkDown : idleDown;
        if (facing.y > 0) return moving ? walkUp : idleUp;
        if (facing.x < 0) return moving ? walkLeft : idleLeft;
        return moving ? walkRight : idleRight;
    }
}
