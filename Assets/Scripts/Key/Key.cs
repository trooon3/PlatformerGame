using Cainos.LucidEditor;
using UnityEngine;

namespace DoorControl
{
    public class Key : MonoBehaviour
    {
        private const float EffectDestroyDelay = 2f;
        private const float RotationSpeed = 360f;
        private const float ScaleMultiplier = 0.2f;

        [FoldoutGroup("Key Settings")]
        public KeyColor keyColor = KeyColor.WhiteColor;

        [FoldoutGroup("Collection Effects")]
        public AudioClip collectSound;

        [FoldoutGroup("Collection Effects")]
        public float collectSoundVolume = 0.5f;

        private bool _isCollected = false;
        private SpriteRenderer _spriteRenderer;
        private Collider2D _collider;

        public KeyColor Color => keyColor;

        private void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<Collider2D>();

            if (_collider != null)
            {
                _collider.isTrigger = true;
            }

            ApplyKeyColor();
        }

        private void ApplyKeyColor()
        {
            if (_spriteRenderer != null)
            {
                switch (keyColor)
                {
                    case KeyColor.WhiteColor:
                        _spriteRenderer.color = UnityEngine.Color.white;

                        break;

                    case KeyColor.BlackColor:
                        _spriteRenderer.color = UnityEngine.Color.black;

                        break;

                    case KeyColor.YellowColor:
                        _spriteRenderer.color = UnityEngine.Color.yellow;

                        break;

                    case KeyColor.BlueColor:
                        _spriteRenderer.color = UnityEngine.Color.blue;

                        break;
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isCollected)
            {
                return;
            }

            if (other.CompareTag("Player"))
            {
                CollectKey(other.gameObject);
            }
        }

        private void CollectKey(GameObject player)
        {
            _isCollected = true;

            if (_collider != null)
            {
                _collider.enabled = false;
            }

            AddKeyToPlayer(player);
            StartCoroutine(CollectAnimation());
        }

        private void AddKeyToPlayer(GameObject player)
        {
            KeyCollection keyCollection = player.GetComponent<KeyCollection>();

            if (keyCollection == null)
            {
                keyCollection = player.GetComponentInParent<KeyCollection>();
            }

            if (keyCollection != null)
            {
                keyCollection.AddKey(keyColor);
            }
        }

        private System.Collections.IEnumerator CollectAnimation()
        {
            PlayCollectEffects();

            float duration = 0.5f;
            float elapsed = 0f;

            Vector3 originalScale = transform.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                float progress = elapsed / duration;

                transform.Rotate(0, 0, RotationSpeed * Time.deltaTime);
                transform.localScale = originalScale * (1 + progress * ScaleMultiplier);

                if (_spriteRenderer != null)
                {
                    Color color = _spriteRenderer.color;

                    color.a = 1 - progress;
                    _spriteRenderer.color = color;
                }

                yield return null;
            }

            Destroy(gameObject);
        }

        private void PlayCollectEffects()
        {
            if (collectSound != null)
            {
                AudioSource.PlayClipAtPoint(collectSound, transform.position, collectSoundVolume);
            }
        }
    }
}