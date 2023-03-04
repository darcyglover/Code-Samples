using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    //Rigidbody of Player
    [SerializeField] Rigidbody _rb;

    //Movement Speed and Turn Speed Floats
    [SerializeField] float _speed = 5;
    [SerializeField] float _turnSpeed = 360;

    //Jump Floats 
    [SerializeField] float _jumpHeight = 3;
    [SerializeField] float _gravityIntensity = -10;

    //Animator
    [SerializeField] Animator _animator;

    //Gun Stuff
    public GameObject projectilePrefab;
    GameObject projectile;

    ProjectileBehaviour projectileBehaviour;

    GameManager gameMan;

    MenuMaster menuMaster;

    Vector3 target;

    [SerializeField] Transform pistol;

    //Camera
    [SerializeField] Camera _playerCamera;

    bool cameraTurnedOff;

    //LayerMask
    [SerializeField] LayerMask groundMask;

    //Network Stuff
    NetworkObject netObj;

    //Ready Manager Stuff
    public bool playersReady;

    static bool isGrounded;

    Vector3 _input;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();

        netObj = GetComponentInParent<NetworkObject>();

        gameMan = FindObjectOfType<GameManager>();

        menuMaster = FindObjectOfType<MenuMaster>();
    }

    void Update()
    {
        if (!netObj.IsOwner)
        {
            return;
        }

        if (!playersReady)
        {
            return;
        }

        if (!_playerCamera.enabled)
        {
            _playerCamera.enabled = true;
        }

        if (!cameraTurnedOff && FindObjectsOfType<Camera>().Length > 1)
        {
            Camera[] cams = FindObjectsOfType<Camera>();

            foreach (Camera cam in cams)
            {
                if (!cam.GetComponentInParent<NetworkObject>().IsOwner)
                {
                    cam.gameObject.SetActive(false);
                    cameraTurnedOff = true;
                }
            }
        }

        GatherInput();
        Jump();
        Aim();

        if (Input.GetMouseButtonDown(0))
        {
            ShootProjectile();
        }
    }

    void FixedUpdate()
    {
        if (!netObj.IsOwner)
        {
            return;
        }

        if (!playersReady)
        {
            return;
        }

        Move();
    }

    void GatherInput()
    {
        _input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
    }

    (bool success, Vector3 position) GetMousePosition()
    {
        var ray = _playerCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, groundMask))
        {
            return (success: true, position: hitInfo.point);
        }
        else
        {
            return (success: false, position: Vector3.zero);
        }
    }

    void Aim()
    {
        if (!netObj.IsOwner)
        {
            return;
        }

        var (success, position) = GetMousePosition();
        if (success)
        {
            var direction = position - transform.position;

            direction.y = 0;

            transform.forward = direction;
        }
    }

    void Move()
    {
        //Movement
        _rb.MovePosition(transform.position + transform.forward * _input.normalized.magnitude * _speed * Time.deltaTime);

        if (_input != Vector3.zero)
        {
            _animator.SetBool("IsRunning", true);
            _animator.SetBool("NotRunning", false);
        }
        else
        {
            _animator.SetBool("IsRunning", false);
            _animator.SetBool("NotRunning", true);
        }

        if (_input != Vector3.zero && Input.GetKeyDown(KeyCode.Space))
        {
            _animator.SetBool("IsRunning", false);
            _animator.SetBool("IsJumping", true);
        }

        if (!IsHost)
        {
            MoveServerRpc(_input);
        }
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {

            //Animator
            _animator.SetBool("IsGrounded", false);
            _animator.SetBool("IsJumping", true);

            //Jump
            float jumpingVelocity = Mathf.Sqrt(-2 * _gravityIntensity * _jumpHeight);
            Vector3 playerVelocity = _input;
            playerVelocity.y = jumpingVelocity;
            _rb.velocity = playerVelocity;
            isGrounded = !isGrounded;
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            isGrounded = !isGrounded;
            _animator.SetBool("IsJumping", false);
            _animator.SetBool("IsGrounded", true);
        }
    }

    public void ShootProjectile()
    {
        target = transform.forward;

        if (!IsHost)
        {
            ShootProjectileServerRpc(target);
        }
        else
        {
            projectile = Instantiate(projectilePrefab, pistol.position, Quaternion.identity);

            projectileBehaviour = projectile.GetComponent<ProjectileBehaviour>();

            projectileBehaviour.Shoot(target);
        }
    }

    public void GameFinished(bool victory)
    {
        playersReady = false;

        _playerCamera.enabled = false;

        menuMaster.playing = false;

        if (victory)
        {
            menuMaster.PlayerWon();
        }
        else
        {
            menuMaster.PlayerLost();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ShootProjectileServerRpc(Vector3 sentTarget)
    {
        projectile = Instantiate(projectilePrefab, pistol.position, Quaternion.identity);

        projectile.GetComponent<ProjectileBehaviour>().Shoot(sentTarget);
    }

    [ServerRpc]
    public void MoveServerRpc(Vector3 _input)
    {
        _animator = GetComponent<Animator>();

        //Animation
        if (_input != Vector3.zero)
        {
            _animator.SetBool("IsRunning", true);
            _animator.SetBool("NotRunning", false);
        }
        else
        {
            _animator.SetBool("IsRunning", false);
            _animator.SetBool("NotRunning", true);
        }

        if (_input != Vector3.zero && Input.GetKeyDown(KeyCode.Space))
        {
            _animator.SetBool("IsRunning", false);
            _animator.SetBool("IsJumping", true);
        }
    }
}



public static class Helpers
{
    private static Matrix4x4 _isoMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, 90, 0));
    //private static Matrix4x4 _isoMatrix = Matrix4x4.Rotate(Quaternion.identity);
    public static Vector3 ToIso(this Vector3 input) => _isoMatrix.MultiplyPoint3x4(input);
}
