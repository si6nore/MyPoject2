using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 5f;
    public float gravity = -20f;
    public float jumpHeight = 1.5f;

    [Header("마우스 설정")]
    public float sensitivity = 2f;
    public float minPitch = -10f; // 사용자의 인스펙터 값을 기본값으로 설정
    public float maxPitch = 20f;  // 사용자의 인스펙터 값을 기본값으로 설정

    [Header("레퍼런스")]
    public Transform cameraHolder;

    [Tooltip("Size를 2로 설정하고\nElement 0: L Clavicle\nElement 1: R Clavicle을 넣으세요.")]
    public Transform[] armBones;

    public Animator animator;

    // 내부 상태
    private CharacterController _cc;
    private float _velocityY = 0f;
    private float _pitch = 0f;

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
        if (cameraHolder == null) Debug.LogError("FPSController: CameraHolder가 연결되지 않았습니다!");
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        LockCursor(true);
    }

    void Update()
    {
        HandleLook();
        HandleMove();
        HandleGravityJump();
        HandleAnimation();
        HandleCursor();
    }

    // 애니메이터가 뼈를 움직인 직후에 실행하여 회전값을 보정함
    void LateUpdate()
    {
        // 뼈가 배열에 두 개 미만으로 들어있다면 작동하지 않음 (오류 방지)
        if (armBones == null || armBones.Length < 2) return;

        // ★★★ 중요: 양쪽 팔의 회전 방향을 일치시키기 위해 루프를 돌지 않고 직접 지정합니다. ★★★

        // Element 0 (왼쪽 팔) 제어
        if (armBones[0] != null)
        {
            // 왼쪽 팔은 원래 Pitch 값으로 회전 (방향이 이상하면 이 축을 변경하세요)
            armBones[0].localRotation *= Quaternion.Euler(_pitch, 0f, 0f);
        }

        // Element 1 (오른쪽 팔) 제어
        if (armBones[1] != null)
        {
            // ★★★ 오른쪽 팔은 Pitch 값을 반전(-_pitch)시켜서 회전합니다. ★★★
            // 이렇게 하면 대칭 좌표축 문제가 해결되어 양쪽 팔이 같은 방향으로 움직입니다.
            armBones[1].localRotation *= Quaternion.Euler(-_pitch, 0f, 0f);
        }
    }

    void HandleLook()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        float mouseX = Input.GetAxisRaw("Mouse X") * sensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensitivity;

        transform.Rotate(Vector3.up, mouseX);

        _pitch -= mouseY;
        _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

        if (cameraHolder != null)
            cameraHolder.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }

    void HandleMove()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = transform.right * h + transform.forward * v;
        if (move.magnitude > 1f) move.Normalize();
        _cc.Move(move * moveSpeed * Time.deltaTime);
    }

    void HandleGravityJump()
    {
        if (_cc.isGrounded)
        {
            _velocityY = -2f;
            if (Input.GetButtonDown("Jump"))
                _velocityY = Mathf.Sqrt(2f * Mathf.Abs(gravity) * jumpHeight);
        }
        else
        {
            _velocityY += gravity * Time.deltaTime;
        }
        _cc.Move(new Vector3(0f, _velocityY * Time.deltaTime, 0f));
    }

    void HandleAnimation()
    {
        if (animator == null) return;
        bool isMoving = Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.1f;
        animator.SetBool("IsRunning", isMoving);
    }

    void HandleCursor()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) LockCursor(false);
        if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked) LockCursor(true);
    }

    void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    // WeaponSystem에서 찾는 함수 (CS1061 에러 방지)
    public void TriggerShoot()
    {
        if (animator != null)
            animator.SetTrigger("Shoot");
    }
}