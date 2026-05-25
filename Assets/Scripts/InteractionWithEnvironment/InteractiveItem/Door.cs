using DoorControl;
using Player.Input;
using UnityEngine;
using YG;

public sealed class Door : MonoBehaviour, IOpenable
{
    [Header("Input")]
    [SerializeField] private IInputProvider _inputProvider;

    [Header("References")]
    public SpriteRenderer spriteRenderer;
    public Sprite spriteOpened;
    public Sprite spriteClosed;

    [Header("Interaction")]
    
    public Vector2 triggerSize = new Vector2(1.5f, 1.5f);
    public LayerMask playerLayer;

    [Header("Key Settings")]
    public bool requiresKey = false;
    public KeyColor requiredKeyColor = KeyColor.WhiteColor;

    [Header("Sounds")]
    public AudioClip openSound;
    public AudioClip closeSound;

    [Header("Sound Settings")]
    public float soundRange = 5f;
    public bool checkPlayerDistance = true;

    private bool isOpened;
    private Transform player;
    private Animator animator;
    private KeyCollection playerKeyCollection;
    private AudioController audioController;

    public bool IsClosed => !isOpened;

    void IOpenable.Open() => Open();

    private void Awake()
    {
        animator = GetComponent<Animator>();

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        audioController = FindFirstObjectByType<AudioController>();

        if (player != null)
        {
            playerKeyCollection = player.GetComponent<KeyCollection>();

            if (playerKeyCollection == null)
            {
                playerKeyCollection = player.GetComponentInParent<KeyCollection>();
            }
        }
    }

    private void Start()
    {
        if (animator != null)
        {
            animator.Play(isOpened ? "Opened" : "Closed");
        }

        ApplyVisualState(isOpened);
        FindInputProvider();
    }

    private void Update()
    {
        if (_inputProvider.IsOpenShopOrChestPressed &&
            Physics2D.OverlapBox(transform.position, triggerSize, 0, playerLayer))
        {
            TryToggle();
        }
    }

    private void TryToggle()
    {
        if (isOpened)
        {
            Close();
        }
        else
        {
            TryOpen();
        }
    }

    private void TryOpen()
    {
        if (requiresKey)
        {
            if (playerKeyCollection != null && playerKeyCollection.HasKey(requiredKeyColor))
            {
                Open();
            }
        }
        else
        {
            Open();
        }
    }

    public void Open()
    {
        isOpened = true;

        if (animator != null)
        {
            animator.SetBool("IsOpened", true);
        }

        Collider2D collider = GetComponent<Collider2D>();

        if (collider != null)
        {
            collider.enabled = false;
        }

        ApplyVisualState(true);

        PlayOpenSound();
    }

    public void Close()
    {
        isOpened = false;

        if (animator != null)
        {
            animator.SetBool("IsOpened", false);
        }

        Collider2D collider = GetComponent<Collider2D>();

        if (collider != null)
        {
            collider.enabled = true;
        }

        ApplyVisualState(false);
        PlayCloseSound();
    }

    private void FindInputProvider()
    {
        if (_inputProvider == null)
        {
            if (YG2.envir.isDesktop)
            {
                _inputProvider = FindObjectOfType<OldInputProvider>();
            }
            else if (YG2.envir.isMobile)
            {
                _inputProvider = FindObjectOfType<JoystickInput>();
            }
        }

        if (_inputProvider == null)
            Debug.LogWarning("IInputProvider not found! Map toggle won't work with key.");
    }
    private void ApplyVisualState(bool opened)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = opened ? spriteOpened : spriteClosed;
        }
    }

    private void PlayOpenSound()
    {
        if (openSound != null)
        {
            if (!checkPlayerDistance || IsPlayerInRange())
            {
                if (audioController != null)
                {
                    audioController.PlayOneShot(openSound);
                }
                else
                {
                    AudioSource.PlayClipAtPoint(openSound, transform.position);
                }
            }
        }
    }

    private void PlayCloseSound()
    {
        if (closeSound != null)
        {
            if (!checkPlayerDistance || IsPlayerInRange())
            {
                if (audioController != null)
                {
                    audioController.PlayOneShot(closeSound);
                }
                else
                {
                    AudioSource.PlayClipAtPoint(closeSound, transform.position);
                }
            }
        }
    }

    private bool IsPlayerInRange()
    {
        if (player == null)
        {
            return true;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        return distance <= soundRange;
    }
}