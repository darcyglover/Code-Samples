using System.Collections;
using System.Reflection;
using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    //Rigidbody of Player
    [SerializeField] Rigidbody _rb;

    //Movement Speed and Turn Speed Floats
    [SerializeField] float _speed = 5;

    //Jump Floats 
    [SerializeField] float _jumpHeight = 3;
    [SerializeField] float _gravityIntensity = -10;

    //Animator
    [SerializeField] Animator _animator;

    //Throwing Stuff
    public GameObject projectilePrefab;
    GameObject projectile;

    [SerializeField] float throwWaitTime;

    ProjectileBehaviour projectileBehaviour;

    GameManager gameMan;

    MenuManager menuMan;

    AmmunitionManager ammoMan;

    Vector3 target;

    [SerializeField] Transform projectileSpawnPoint;

    //Camera
    public Camera _playerCamera;

    bool cameraTurnedOff;

    //LayerMask
    [SerializeField] LayerMask groundMask;

    //Network Stuff
    NetworkObject netObj;

    //Ready Manager Stuff
    public bool playersReady;

    static bool isGrounded;

    static bool _sliding;

    static bool _noJumping;

    Vector3 _input;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();

        netObj = GetComponentInParent<NetworkObject>();

        gameMan = FindObjectOfType<GameManager>();

        menuMan = FindObjectOfType<MenuManager>();

        ammoMan = FindObjectOfType<AmmunitionManager>();
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

        GatherInput();
        Jump();
        Aim();
        Slide();

        if (Input.GetMouseButtonDown(0))
        {
            if (!ammoMan.ProjectileThrown(throwWaitTime))
            {
                return;
            }

            _animator.Play("Throwing");

            if (!IsHost)
            {
                PlayAnimationServerRpc("Throwing");
            }

            StartCoroutine(CallMethodAfterTime(throwWaitTime, "ThrowProjectile"));
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

    void Slide()
    {
        if (_input != Vector3.zero && Input.GetKeyDown(KeyCode.C))
        {
            if (isGrounded)
            {
                _animator.Play("Slide");

                PlayAnimationServerRpc("Slide");

                _noJumping = true;
            }  
        }
    }

    void SlideSpeed()
    {
        if (_sliding == false)
        {
            _speed = 10;
            _sliding = true;
        }
        else
        {
            _speed = 5;
            _sliding = false;
            _noJumping = false;
        }
    }

    void ResetSpeed()
    {
        _speed = 5;
        _sliding = false;
        _noJumping = false;
    }

    void Move()
    {
        //Movement
        _rb.MovePosition(transform.position + transform.forward * _input.normalized.magnitude * _speed * Time.deltaTime);

        if (_input != Vector3.zero)
        {
            _animator.SetBool("isRunning", true);
            _animator.SetBool("notRunning", false);
        }
        else
        {
            _animator.SetBool("isRunning", false);
            _animator.SetBool("notRunning", true);
        }

        if (!IsHost)
        {
            MoveServerRpc(_input);
        }
    }

    void Jump()
    {
        if (_input != Vector3.zero && Input.GetKeyDown(KeyCode.Space))
        {
            if (_noJumping == false && isGrounded)
            {
                _animator.Play("Jump");

                //Jump
                float jumpingVelocity = Mathf.Sqrt(-2 * _gravityIntensity * _jumpHeight);
                Vector3 playerVelocity = _input;
                playerVelocity.y = jumpingVelocity;
                _rb.velocity = playerVelocity;
                isGrounded = !isGrounded;
            }
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            if (_noJumping == false)
            {
                //Animator
                _animator.Play("Jump");

                //Jump
                float jumpingVelocity = Mathf.Sqrt(-2 * _gravityIntensity * _jumpHeight);
                Vector3 playerVelocity = _input;
                playerVelocity.y = jumpingVelocity;
                _rb.velocity = playerVelocity;
                isGrounded = !isGrounded;
            }
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            isGrounded = !isGrounded;
            _animator.SetBool("isJumping", false);
            _animator.SetBool("isGrounded", true);
        }
    }

    void ThrowProjectile()
    {
        target = transform.forward;

        if (!IsHost)
        {
            ShootProjectileServerRpc(target, projectileSpawnPoint.position);
        }
        else
        {
            projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);

            projectileBehaviour = projectile.GetComponent<ProjectileBehaviour>();

            projectileBehaviour.Throw(target);
        }
    }

    public void GameFinished(bool victory)
    {
        playersReady = false;

        _playerCamera = null;

        menuMan.playing = false;

        gameMan.joinCodeText.Text = "";

        if (victory)
        {
            menuMan.currentMenuSection = "Victory Screen";
            menuMan.PlayerWon();
        }
        else
        {
            menuMan.currentMenuSection = "Loss Screen";
            menuMan.PlayerLost();
        }
    }

    IEnumerator CallMethodAfterTime(float waitTime, string methodName) //this can only be used to call methods which don't take overloads for now. Will look into ways to make it more modular - Darcy
    {
        yield return new WaitForSeconds(waitTime);

        MethodInfo mi = GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);

        mi.Invoke(this, null);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ShootProjectileServerRpc(Vector3 sentTarget, Vector3 spawnPoint)
    {
        projectile = Instantiate(projectilePrefab, spawnPoint, Quaternion.identity);

        projectile.GetComponent<ProjectileBehaviour>().Throw(sentTarget);
    }

    [ServerRpc]
    public void MoveServerRpc(Vector3 _input)
    {
        _animator = GetComponent<Animator>();

        if (_input != Vector3.zero)
        {
            _animator.SetBool("isRunning", true);
            _animator.SetBool("notRunning", false);
        }
        else
        {
            _animator.SetBool("isRunning", false);
            _animator.SetBool("notRunning", true);
        }

        if (_input != Vector3.zero && Input.GetKeyDown(KeyCode.Space))
        {
            _animator.SetBool("isRunning", false);
            _animator.SetBool("isJumping", true);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayAnimationServerRpc(string animationName)
    {
        _animator = GetComponent<Animator>();

        _animator.Play(animationName);
    }
}



public static class Helpers
{
    private static Matrix4x4 _isoMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, 90, 0));
    //private static Matrix4x4 _isoMatrix = Matrix4x4.Rotate(Quaternion.identity);
    public static Vector3 ToIso(this Vector3 input) => _isoMatrix.MultiplyPoint3x4(input);
}
