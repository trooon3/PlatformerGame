using GameLogic;
using UnityEngine;

public sealed class CoinFactory
{
    private readonly GameObject _coinPrefab;
    private readonly LayerMask _groundLayer;
    private readonly float _bounciness;
    private readonly float _verticalForce;
    private readonly float _horizontalForce;
    private readonly float _torqueForce;
    private readonly SfxPlayer _sfxPlayer;

    public CoinFactory(
        GameObject coinPrefab,
        LayerMask groundLayer,
        float bounciness,
        SfxPlayer sfxPlayer,
        float verticalForce = 1.5f,
        float horizontalForce = 2f,
        float torqueForce = 100f)
    {
        _coinPrefab = coinPrefab;
        _groundLayer = groundLayer;
        _bounciness = bounciness;
        _verticalForce = verticalForce;
        _horizontalForce = horizontalForce;
        _torqueForce = torqueForce;
    }

    public GameObject Create(Vector3 position)
    {
        if (_coinPrefab == null)
            return null;

        GameObject coin = Object.Instantiate(_coinPrefab, position, Quaternion.identity);

        if (coin.TryGetComponent(out Coin coinComponent))
        {
            coinComponent.SetSfxPlayer(_sfxPlayer);
        }

        SetupPhysics(coin);

        return coin;
    }

    private void SetupPhysics(GameObject coin)
    {
        Rigidbody2D rb = coin.GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            rb = coin.AddComponent<Rigidbody2D>();
            ConfigureRigidbody(rb);
        }

        rb.sharedMaterial = CreateMaterial();
        ApplyForces(rb);

        CoinPhysicsHandler handler = coin.GetComponent<CoinPhysicsHandler>();
        if (handler == null)
            handler = coin.AddComponent<CoinPhysicsHandler>();

        handler.Initialize(_groundLayer);
    }

    private void ConfigureRigidbody(Rigidbody2D rb)
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1.5f;
        rb.mass = 0.1f;
        rb.drag = 0.5f;
        rb.angularDrag = 0.05f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private PhysicsMaterial2D CreateMaterial()
    {
        return new PhysicsMaterial2D("BouncyCoin")
        {
            bounciness = _bounciness,
            friction = 0.1f
        };
    }

    private void ApplyForces(Rigidbody2D rb)
    {
        float horizontalDirection = Random.Range(0.1f, 0.1f);
        Vector2 force = new Vector2(horizontalDirection * 0.2f, 1f);
        rb.AddForce(force, ForceMode2D.Impulse);
    }
}