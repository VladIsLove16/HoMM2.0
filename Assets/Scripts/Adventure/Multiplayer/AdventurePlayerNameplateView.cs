using TMPro;
using UnityEngine;

namespace Adventure.Multiplayer
{
    public sealed class AdventurePlayerNameplateView : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private GameObject root;

        private Camera _cachedCamera;

        private void LateUpdate()
        {
            if (root != null && !root.activeInHierarchy)
                return;

            if (_cachedCamera == null)
                _cachedCamera = Camera.main;

            if (_cachedCamera == null)
                return;

            var cameraTransform = _cachedCamera.transform;
            transform.forward = cameraTransform.forward;
        }

        public void SetDisplayName(string displayName)
        {
            if (nameText != null)
                nameText.text = displayName ?? string.Empty;
        }

        public void SetVisible(bool visible)
        {
            if (root != null)
                root.SetActive(visible);
            else
                gameObject.SetActive(visible);
        }
    }
}
