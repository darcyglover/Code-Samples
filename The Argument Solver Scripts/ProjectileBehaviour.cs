using System.Collections;
using UnityEngine;
using Unity.Netcode;
using DG.Tweening;

public class ProjectileBehaviour : NetworkBehaviour
{
    bool projectileMoving;

    Vector3 target;

    string hitPlayerTag;

    [SerializeField] GameObject playerToIgnore;

    NetworkObject netObj;

    public float projectileSpeed, lifeTime, rotationSpeed;

    Animator animator;

    void Awake()
    {
        netObj = GetComponent<NetworkObject>();

        playerToIgnore = playerToIgnore.GetComponentInChildren<PlayerController>().gameObject;

        playerToIgnore = GameObject.FindGameObjectWithTag(playerToIgnore.tag);
    }

    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        if (projectileMoving)
        {
            StartCoroutine(MoveProjectile());
        }
    }

    public void Throw(Vector3 sentTarget)
    {
        netObj.SpawnAsPlayerObject(OwnerClientId);

        target = sentTarget;

        projectileMoving = true;

        StartCoroutine(ProjectileLifetime());
    }

    void DeathAnimation()
    {
        PlayerController[] players = FindObjectsOfType<PlayerController>();

        foreach (var p in players)
        {
            if (hitPlayerTag == p.tag)
            {
                if (p.CompareTag("Client"))
                {
                    animator = p.GetComponent<Animator>();
                }
                else
                {
                    animator = p.GetComponent<Animator>();
                }
            }
        }

        animator.Play("Death");

        if (!IsHost)
        {
            PlayAnimationServerRpc("Death");
        }

        StartCoroutine(WaitForDeathAnimation());
    }

    void DespawnAfterHitting()
    {
        if (!IsServer)
        {
            return;
        }

        if (!IsOwner)
        {
            return;
        }

        PlayerController[] players = FindObjectsOfType<PlayerController>();

        foreach(var p in players)
        {
            if(hitPlayerTag == p.tag)
            {
                if (p.CompareTag("Client"))
                {
                    GameFinishedClientRpc(false);
                }
                else
                {
                    p.GetComponent<PlayerController>().GameFinished(false);
                }
            }
            else
            {
                if (p.CompareTag("Client"))
                {
                    GameFinishedClientRpc(true);
                }
                else
                {
                    p.GetComponent<PlayerController>().GameFinished(true);
                }
            }
        }

        hitPlayerTag = null;

        netObj.Despawn();
    }

    void DespawnAfterTime()
    {
        netObj.Despawn();
    }

    IEnumerator WaitForDeathAnimation()
    {
        yield return new WaitForSeconds(2.3f);

        DespawnServerRpc(true);
    }

    IEnumerator MoveProjectile()
    {
        if (!IsOwner)
        {
            yield return null;
        }

        transform.Translate(target * Time.deltaTime * projectileSpeed, Space.World);

        //transform.RotateAround(transform.GetChild(0).position, new Vector3(target.x, 0, 0), rotationSpeed * Time.deltaTime);
        transform.DORotate(new Vector3(360, 0, 0), rotationSpeed, RotateMode.LocalAxisAdd).SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear);

        yield return null;
    }

    IEnumerator ProjectileLifetime() //destroys the projectile after x seconds if it doesn't hit a player
    {
        yield return new WaitForSeconds(lifeTime);

        DespawnServerRpc(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsOwner)
        {
            return;
        }

        if(other.CompareTag("Client") || other.CompareTag("Host"))
        {
            if (playerToIgnore == other.gameObject)
            {
                return;
            }

            hitPlayerTag = other.tag;

            if (!IsServer)
            {
                UpdateHitPlayerTagServerRpc(hitPlayerTag);
            }

            DeathAnimation();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayAnimationServerRpc(string animationName)
    {
        animator = GetComponent<Animator>();

        animator.Play(animationName);
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateHitPlayerTagServerRpc(string tag)
    {
        hitPlayerTag = tag;
    }

    [ServerRpc(RequireOwnership = false)]
    public void DespawnServerRpc(bool hitPlayer)
    {
        if (hitPlayer)
        {
            DespawnAfterHitting();
        }
        else
        {
            DespawnAfterTime();
        }
    }

    [ClientRpc]
    public void GameFinishedClientRpc(bool victory)
    {
        PlayerController[] players = FindObjectsOfType<PlayerController>();

        foreach (var p in players)
        {
            if (p.CompareTag("Client") && !IsHost)
            {
                p.GameFinished(victory);
            }
        }
    }
}
