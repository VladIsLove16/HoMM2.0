using System.Collections;
using UnityEngine;

namespace Adventure.Infrastructure.Inventory
{
    [RequireComponent(typeof(MushroomCollectible))]
    public sealed class MushroomGrowthAnimator : MonoBehaviour
    {
        [SerializeField] private float duration = 0.45f;
        [SerializeField] private AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        private Vector3 _initialScale;
        private Coroutine _routine;

        private void Awake()
        {
            _initialScale = transform.localScale;
        }

        private void OnEnable()
        {
            Play();
        }

        public void Play()
        {
            if (!isActiveAndEnabled)
                return;

            if (_routine != null)
            {
                StopCoroutine(_routine);
            }

            _routine = StartCoroutine(GrowRoutine());
        }

        private IEnumerator GrowRoutine()
        {
            var targetScale = _initialScale;
            transform.localScale = Vector3.zero;

            float time = 0f;
            float durationSafe = Mathf.Max(0.01f, duration);
            while (time < durationSafe)
            {
                time += Time.deltaTime;
                var t = Mathf.Clamp01(time / durationSafe);
                var ease = curve.Evaluate(t);
                transform.localScale = targetScale * ease;
                yield return null;
            }

            transform.localScale = targetScale;
            _routine = null;
        }
    }
}
