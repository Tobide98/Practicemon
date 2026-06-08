using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;

    private bool btnUp, btnDown, btnLeft, btnRight;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    void FixedUpdate()
    {
        float x = 0f;
        float y = 0f;

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) || btnRight) { x += 1f; Debug.Log("Right"); }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)  || btnLeft)  { x -= 1f; Debug.Log("Left"); }
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)    || btnUp)    { y += 1f; Debug.Log("Up"); }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)  || btnDown)  { y -= 1f; Debug.Log("Down"); }

        Vector2 move = new Vector2(x, y).normalized * moveSpeed;
        rb.velocity = move;
    }

    public void SetUp(bool pressed)    { btnUp    = pressed; }
    public void SetDown(bool pressed)  { btnDown  = pressed; }
    public void SetLeft(bool pressed)  { btnLeft  = pressed; }
    public void SetRight(bool pressed) { btnRight = pressed; }
}
