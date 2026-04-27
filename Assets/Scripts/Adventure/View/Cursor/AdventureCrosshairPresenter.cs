using System;
using UnityEngine;
using UnityEngine.UI;

namespace Adventure.Presentation.Cursor
{
    public sealed class AdventureCrosshairPresenter : IDisposable
    {
        private readonly IInputModeVM _inputModeViewModel;
        private readonly RawImage _crosshairImage;

        public AdventureCrosshairPresenter(IInputModeVM inputModeViewModel, RawImage crosshairImage)
        {
            _inputModeViewModel = inputModeViewModel ?? throw new ArgumentNullException(nameof(inputModeViewModel));
            _crosshairImage = crosshairImage;

            if (_crosshairImage == null)
            {
                Debug.LogWarning("[AdventureCrosshairPresenter] Crosshair image is not assigned.");
                return;
            }

            _crosshairImage.raycastTarget = false;
            ApplyVisibility();
            _inputModeViewModel.OnModeChanged += OnInputModeChanged;
        }

        public void Dispose()
        {
            _inputModeViewModel.OnModeChanged -= OnInputModeChanged;
        }

        private void OnInputModeChanged(InputMode _)
        {
            ApplyVisibility();
        }

        private void ApplyVisibility()
        {
            if (_crosshairImage == null)
                return;

            _crosshairImage.enabled = _inputModeViewModel.CanLook && !_inputModeViewModel.IsCursorVisible;
            _crosshairImage.raycastTarget = false;
        }
    }
}
