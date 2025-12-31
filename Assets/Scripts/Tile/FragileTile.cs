using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FragileTile : Tile
{
    private Collider2D _collider2D;
    private Tilemap _tilemapRenderer;
    private Coroutine _fadeCoroutine;

    [Header("FragileTile Settings")] [Tooltip("淡出的时长（秒）")]
    public float fadeDuration = 0.6f;

    private void Awake()
    {
        _collider2D = GetComponent<Collider2D>();
        _tilemapRenderer = GetComponent<Tilemap>();
    }

    // 当使用 isTrigger = true 的 collider 时，会调用此方法
    protected override void OnEnterTrigger(Collider2D other)
    {
        Debug.LogWarning("FragileTile triggered by " + other.gameObject.name);
        // 如果已经在淡出中，忽略重复触发
        if (_fadeCoroutine != null) return;

        // 先禁用碰撞体，防止再次触发或玩家掉落时再次触碰
        if (_collider2D != null)
            _collider2D.enabled = false;

        // 启动淡出协程：逐渐降低 TilemapRenderer 的 alpha，最后销毁对象
        _fadeCoroutine = StartCoroutine(FadeAndDestroy());
    }

    // 淡出并销毁的协程实现
    private IEnumerator FadeAndDestroy()
    {
        // Safety: ensure we have a Tilemap to operate on
        if (_tilemapRenderer == null)
        {
            // Nothing to fade; clear coroutine ref and exit
            _fadeCoroutine = null;
            yield break;
        }

        // 可在此处播放破碎音效或粒子（占位）
        // e.g., AudioSource.PlayClipAtPoint(breakClip, transform.position);

        // Record starting color and alpha
        Color startColor = _tilemapRenderer.color;
        float startAlpha = startColor.a;

        // If fadeDuration is zero or negative, snap to transparent immediately
        if (fadeDuration <= 0f)
        {
            Color snap = startColor;
            snap.a = 0f;
            _tilemapRenderer.color = snap;
            
            if (_tilemapRenderer != null)
                Destroy(_tilemapRenderer.gameObject);
            else
                Destroy(gameObject);

            _fadeCoroutine = null;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            Color c = startColor;
            c.a = Mathf.Lerp(startAlpha, 0f, t);
            _tilemapRenderer.color = c;
            yield return null;
        }

        // Ensure fully transparent at the end
        Color finalColor = startColor;
        finalColor.a = 0f;
        _tilemapRenderer.color = finalColor;

        // Optionally disable the renderer or the GameObject
        // Destroy the Tilemap's GameObject (preferred) to remove it from the scene
        if (_tilemapRenderer != null)
            Destroy(_tilemapRenderer.gameObject);
        else
            Destroy(gameObject);

        // Clear the coroutine reference so this tile can be reused if necessary
        _fadeCoroutine = null;
    }
}