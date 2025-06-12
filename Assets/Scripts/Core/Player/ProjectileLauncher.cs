using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class ProjectileLauncher : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] InputReader inputReader;
    [SerializeField] CoinWallet wallet;
    [SerializeField] GameObject serverProjectilePrefab;
    [SerializeField] GameObject clientProjectilePrefab;
    [SerializeField] Transform projectileSpawnPoint;
    [SerializeField] Collider2D tankCollider;
    [SerializeField] GameObject muzzleFlash;

    [Header("Settings")]
    [SerializeField] float projectileSpeed;
    [SerializeField] float projectileFireRate;
    [SerializeField] float muzzleFlashDuration;
    [SerializeField] int costToFire;



    bool canFire;
    float fireTimer;
    float muzzleFlashTimer;
   
    
    

    void Start()
    {
        fireTimer = projectileFireRate;
    }


    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        inputReader.PrimaryFireEvent += HandleFire;
    }
    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;

        inputReader.PrimaryFireEvent -= HandleFire;
    }


    void Update()
    {
        if (muzzleFlashTimer > 0f)
        {
            muzzleFlashTimer -= Time.deltaTime;

            if (muzzleFlashTimer <= 0f)
            {
                muzzleFlash.SetActive(false);
            }
        }

        fireTimer += Time.deltaTime;

        if (!canFire) return;
        if (!IsOwner) return;
        if (wallet.TotalCoins.Value < costToFire) return;

        if (HandleFireCheck()) return;

        FireServerRpc(projectileSpawnPoint.position, projectileSpawnPoint.up);

        SpawnDummyProjectile(clientProjectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.up);
    }

    void HandleFire(bool canFire)
    {
        this.canFire = canFire;
    }

    bool HandleFireCheck()
    {
        
        if (fireTimer < projectileFireRate)
        {
            return true;
        }

        fireTimer = 0f;
        return false;
    }

    void SpawnDummyProjectile(GameObject prefab, Vector3 spawnPosition, Vector3 direction)
    {
        muzzleFlash.SetActive(true);
        muzzleFlashTimer = muzzleFlashDuration;

        GameObject spawnedProjectile = Instantiate(prefab, spawnPosition, Quaternion.identity);
        spawnedProjectile.transform.up = direction;

        Collider2D projectileCollider = spawnedProjectile.GetComponent<Collider2D>();
        Physics2D.IgnoreCollision(tankCollider, projectileCollider);

        if (spawnedProjectile.TryGetComponent<DealDamageOnContact>(out DealDamageOnContact dealDamage))
        {
            dealDamage.SetOwner(OwnerClientId);
        }

        Rigidbody2D rb = spawnedProjectile.GetComponent<Rigidbody2D>();
        rb.linearVelocity = rb.transform.up * projectileSpeed;

        
    }

    [ServerRpc]
    void FireServerRpc(Vector3 spawnPosition, Vector3 direction)
    {
        if (wallet.TotalCoins.Value < costToFire) return;

        wallet.SpendCoins(costToFire);

        GameObject prefab = serverProjectilePrefab;
        SpawnDummyProjectile(prefab, spawnPosition, direction);

        FireClientRpc(spawnPosition, direction);
    }

    [ClientRpc]
    void FireClientRpc(Vector3 spawnPosition, Vector3 direction)
    {
        if (IsOwner) return;

        GameObject prefab = clientProjectilePrefab;
        SpawnDummyProjectile(prefab, spawnPosition, direction);
    }
}
