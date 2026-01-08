using System.Collections.Generic;
using UnityEngine;

namespace BackGroundScript
{
    public class BackGroundControl : MonoBehaviour
    {
        // Start is called before the first frame update

        public Transform player;
        [Header("背景1")]
        public Transform first;
        public Transform second;
        public Transform third;
        public Transform fourth;
        [Header("背景2")]
        public Transform second2;
        public Transform third2;
        public Transform fourth2;

        [Header("Parallax factors (X,Y) - how much each layer follows the player/camera)")]
        public Vector2 firstFactor = new Vector2(1f, 1f);
        public Vector2 secondFactor = new Vector2(0.8f, 0.3f);
        public Vector2 thirdFactor = new Vector2(0.6f, 0.2f);
        public Vector2 fourthFactor = new Vector2(0.4f, 0.1f);

        // For the second set reuse same factors (can be customized if desired)
        public Vector2 second2Factor = new Vector2(0.8f, 0.3f);
        public Vector2 third2Factor = new Vector2(0.6f, 0.2f);
        public Vector2 fourth2Factor = new Vector2(0.4f, 0.1f);

        [Header("Smoothing (higher = snappier)")]
        [Tooltip("How quickly background layers reach their target parallax positions.")]
        public float smooth = 10f;

        // stored starting positions
        private Vector3 _playerStartPos;
        private Dictionary<Transform, Vector3> _layerStartPos = new Dictionary<Transform, Vector3>();
        private Dictionary<Transform, Vector2> _layerFactor = new Dictionary<Transform, Vector2>();

        void Awake()
        {
            if (player == null)
            {
                Debug.LogWarning("BackGroundControl: player reference is null.");
            }

            _playerStartPos = player != null ? player.position : Vector3.zero;

            // register layers and their start positions/factors (skip nulls)
            RegisterLayer(first, firstFactor);
            RegisterLayer(second, secondFactor);
            RegisterLayer(third, thirdFactor);
            RegisterLayer(fourth, fourthFactor);

            RegisterLayer(second2, second2Factor);
            RegisterLayer(third2, third2Factor);
            RegisterLayer(fourth2, fourth2Factor);
        }

        private void RegisterLayer(Transform layer, Vector2 factor)
        {
            if (layer == null) return;
            if (!_layerStartPos.ContainsKey(layer))
                _layerStartPos[layer] = layer.position;
            _layerFactor[layer] = factor;
        }

        // Use LateUpdate so the camera/player has moved this frame already
        void LateUpdate()
        {
            if (player == null) return;
            UpdateParallax();
        }

        private void UpdateParallax()
        {
            Vector3 delta = player.position - _playerStartPos; // absolute movement from start

            // Move each registered layer to its target startPos + delta * factor
            foreach (var kv in _layerStartPos)
            {
                Transform layer = kv.Key;
                Vector3 startPos = kv.Value;
                Vector2 factor = _layerFactor.ContainsKey(layer) ? _layerFactor[layer] : Vector2.one;

                Vector3 target = startPos + new Vector3(delta.x * factor.x, delta.y * factor.y, 0f);

                // Smoothly move towards the target position
                layer.position = Vector3.Lerp(layer.position, target, Mathf.Clamp01(Time.deltaTime * smooth));
            }
        }

        // If you need to re-center parallax (for example when teleporting player), call this
        public void ResetParallaxOrigin()
        {
            _playerStartPos = player != null ? player.position : Vector3.zero;
            List<Transform> keys = new List<Transform>(_layerStartPos.Keys);
            foreach (var layer in keys)
            {
                _layerStartPos[layer] = layer.position;
            }
        }
    }
}
