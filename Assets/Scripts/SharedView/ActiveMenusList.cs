using System;
using System.Collections;
using Adventure.Settings.ViewModel;
using UniRx;
using UnityEngine;
using Zenject;

namespace SharedView
{
    public class ActiveMenusList
    {
    }

    public abstract class ModalPanelViewBase<TViewModel> : MonoBehaviour
        where TViewModel : class, IActiveMenu
    {
        protected TViewModel ViewModel { get; private set; }
        protected CompositeDisposable Bindings { get; private set; }

        private bool _initialized;

        [Inject]
        public virtual void Construct(TViewModel viewModel)
        {
            ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            TryInitialize();
        }

        protected virtual void Start()
        {
            TryInitialize();
        }

        protected virtual void OnDestroy()
        {
            Bindings?.Dispose();
        }

        public virtual void TryInitialize()
        {
            if (_initialized || ViewModel == null)
                return;

            _initialized = true;
            Bindings = new CompositeDisposable();

            ViewModel.IsOpen
                .Subscribe(OnMenuOpenStateChanged)
                .AddTo(Bindings);

            OnMenuOpenStateChanged(ViewModel.IsOpen.Value);
            OnInitialized();
        }

        protected virtual void OnInitialized()
        {
        }

        protected abstract void OnMenuOpenStateChanged(bool isOpen);
    }

    [RequireComponent(typeof(CanvasGroup))]
    public abstract class CanvasGroupPanelViewBase<TViewModel> : ModalPanelViewBase<TViewModel>
        where TViewModel : class, IActiveMenu
    {
        [Header("Visibility")]
        private CanvasGroup canvasGroup;
        [SerializeField, Min(0f)] private float fadeDuration = 0.15f;

        private Coroutine _fadeCoroutine;
        private bool _hasAppliedInitialState;

        protected virtual void Awake()
        {
            if (!TryCacheCanvasGroup())
            {
                Debug.LogError($"[{GetType().Name}] CanvasGroup is required on the same object.", this);
                enabled = false;
                return;
            }

            ApplyState(canvasGroup, 0f, false);
        }

        protected virtual void Reset()
        {
            SyncCanvasGroupReference();
        }

        protected virtual void OnValidate()
        {
            SyncCanvasGroupReference();
        }

        protected override void OnMenuOpenStateChanged(bool isOpen)
        {
            if (!TryCacheCanvasGroup())
                return;

            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }

            var targetAlpha = isOpen ? 1f : 0f;
            if (!_hasAppliedInitialState || fadeDuration <= 0f || !isActiveAndEnabled)
            {
                _hasAppliedInitialState = true;
                ApplyState(canvasGroup, targetAlpha, isOpen);
                OnVisibilityChanged(isOpen);
                return;
            }

            _fadeCoroutine = StartCoroutine(FadeRoutine(canvasGroup, targetAlpha, isOpen));
        }

        protected virtual void OnVisibilityChanged(bool isOpen)
        {
        }

        protected override void OnDestroy()
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }

            base.OnDestroy();
        }

        private bool TryCacheCanvasGroup()
        {
            if (canvasGroup == null || canvasGroup.gameObject != gameObject)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            return canvasGroup != null;
        }

        private void SyncCanvasGroupReference()
        {
            var localCanvasGroup = GetComponent<CanvasGroup>();
            if (localCanvasGroup != null)
            {
                canvasGroup = localCanvasGroup;
            }
        }

        private IEnumerator FadeRoutine(CanvasGroup group, float targetAlpha, bool isOpen)
        {
            group.interactable = false;
            group.blocksRaycasts = false;

            var startAlpha = group.alpha;
            var elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                var progress = Mathf.Clamp01(elapsed / fadeDuration);
                group.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
                yield return null;
            }

            ApplyState(group, targetAlpha, isOpen);
            _fadeCoroutine = null;
            OnVisibilityChanged(isOpen);
        }

        private static void ApplyState(CanvasGroup group, float alpha, bool isOpen)
        {
            group.alpha = alpha;
            group.interactable = isOpen;
            group.blocksRaycasts = isOpen;
        }
    }

    [RequireComponent(typeof(CanvasGroup))]
    public abstract class CanvasGroupVisibilityPanelBase : MonoBehaviour
    {
        [Header("Visibility")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField, Min(0f)] private float fadeDuration = 0.15f;

        private Coroutine _fadeCoroutine;

        protected virtual bool VisibleOnAwake => false;

        protected virtual void Awake()
        {
            if (!TryCacheCanvasGroup())
            {
                Debug.LogError($"[{GetType().Name}] CanvasGroup is required on the same object.", this);
                enabled = false;
                return;
            }

            ApplyState(canvasGroup, VisibleOnAwake ? 1f : 0f, VisibleOnAwake);
        }

        protected virtual void Reset()
        {
            SyncCanvasGroupReference();
        }

        protected virtual void OnValidate()
        {
            SyncCanvasGroupReference();
        }

        protected virtual void OnDestroy()
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }
        }

        protected void ShowPanel()
        {
            SetPanelVisible(true);
        }

        protected void HidePanel()
        {
            SetPanelVisible(false);
        }

        protected void ShowPanelImmediate()
        {
            SetPanelVisible(true, true);
        }

        protected void HidePanelImmediate()
        {
            SetPanelVisible(false, true);
        }

        protected virtual void OnPanelVisibilityChanged(bool isVisible)
        {
        }

        private void SetPanelVisible(bool isVisible, bool immediate = false)
        {
            if (!TryCacheCanvasGroup())
                return;

            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }

            var targetAlpha = isVisible ? 1f : 0f;
            if (immediate || fadeDuration <= 0f || !isActiveAndEnabled)
            {
                ApplyState(canvasGroup, targetAlpha, isVisible);
                OnPanelVisibilityChanged(isVisible);
                return;
            }

            _fadeCoroutine = StartCoroutine(FadeRoutine(targetAlpha, isVisible));
        }

        private bool TryCacheCanvasGroup()
        {
            if (canvasGroup == null || canvasGroup.gameObject != gameObject)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            return canvasGroup != null;
        }

        private void SyncCanvasGroupReference()
        {
            var localCanvasGroup = GetComponent<CanvasGroup>();
            if (localCanvasGroup != null)
            {
                canvasGroup = localCanvasGroup;
            }
        }

        private IEnumerator FadeRoutine(float targetAlpha, bool isVisible)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            var startAlpha = canvasGroup.alpha;
            var elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                var progress = Mathf.Clamp01(elapsed / fadeDuration);
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
                yield return null;
            }

            ApplyState(canvasGroup, targetAlpha, isVisible);
            _fadeCoroutine = null;
            OnPanelVisibilityChanged(isVisible);
        }

        private static void ApplyState(CanvasGroup group, float alpha, bool isVisible)
        {
            group.alpha = alpha;
            group.interactable = isVisible;
            group.blocksRaycasts = isVisible;
        }
    }
}
