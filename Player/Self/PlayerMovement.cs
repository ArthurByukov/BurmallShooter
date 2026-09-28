using UnityEngine;
using UnityEngine.UI;
public class PlayerMovement : MonoBehaviour
{
    [Header("Движение")]
    public Text DashText;
    public string DashStr;
    public CharacterController controller;
    public float speed = 6f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    [Header("Дэш")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public int maxDashes = 3;
    public float dashRechargeTime = 3f;

    [Header("Текущая скорость (полный вектор)")]
    public Vector3 velocity;

    private bool isGrounded;
    private bool isDashing = false;
    private float dashTimer = 0f;
    private Vector3 dashDirection;
    private int currentDashes;
    private float dashRechargeTimer = 0f;

    void Start()
    {
        currentDashes = maxDashes;
    }

    void Update()
    {
        
        if (currentDashes < maxDashes)
        {
            if (dashRechargeTimer > 0)
                dashRechargeTimer -= Time.deltaTime;
            else
            {
                currentDashes++;
                if (currentDashes < maxDashes)
                    dashRechargeTimer = dashRechargeTime;
            }

        }

        
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
            {
                isDashing = false;
               
            }
            else
            {
               
                controller.Move(velocity * Time.deltaTime);
                return; 
            }
        }

        
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = 0f;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        if (move.magnitude > 1f)
            move.Normalize();

        
        velocity.x = move.x * speed;
        velocity.z = move.z * speed;

        
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            if (!isDashing && currentDashes > 0)
            {
                float inputX = Input.GetAxisRaw("Horizontal");
                float inputZ = Input.GetAxisRaw("Vertical");
                if (inputX != 0 || inputZ != 0)
                    dashDirection = (transform.right * inputX + transform.forward * inputZ).normalized;
                else
                    dashDirection = transform.forward;

                isDashing = true;
                dashTimer = dashDuration;

                
                velocity = new Vector3(dashDirection.x * dashSpeed, 0f, dashDirection.z * dashSpeed);

                currentDashes--;
                if (currentDashes < maxDashes && dashRechargeTimer <= 0f)
                    dashRechargeTimer = dashRechargeTime;
            }
        }

        
        velocity.y += gravity * Time.deltaTime;

        
        controller.Move(velocity * Time.deltaTime);

        updateUI();
    }

    public void Bounce(float force)
    {
        velocity.y = force;
    }

    public void updateUI()
    {
        DashText.text = DashStr + currentDashes + "/" + maxDashes;
    }
    public int GetCurrentDashes() => currentDashes;
}