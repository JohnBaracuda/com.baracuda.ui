/*
 * AddSingleton this component to a RawImage, to automatically adjust the
 * uvRect to make the image completely fill its bounds while
 * preserving its aspect ratio (by clipping the top/bottom or sides).
 */

using System;
using UnityEngine;
using UnityEngine.UI;

namespace Baracuda.UI
{
    [ExecuteInEditMode]
    public class RawImageAspectFill : MonoBehaviour
    {
        private RectTransform _rectTransform;
        private RawImage _image;
        private Rect _lastBounds;
        private Texture _lastTexture;

        private void Update()
        {
            if (_rectTransform == null)
            {
                _rectTransform = transform as RectTransform;
            }
            if (_image == null)
            {
                _image = GetComponent<RawImage>();
            }

            if ((_rectTransform != null && _rectTransform.rect != _lastBounds) ||
                (_image != null && _image.mainTexture != _lastTexture))
            {
                UpdateUV();
            }
        }

        public void UpdateUV()
        {
            if (_rectTransform == null || _image == null)
            {
                return;
            }
            _lastBounds = _rectTransform.rect;
            var frameAspect = _lastBounds.width / _lastBounds.height;

            _lastTexture = _image.mainTexture;
            var imageAspect = _lastTexture.width / (float) _lastTexture.height;

            if (Math.Abs(frameAspect - imageAspect) < float.Epsilon)
            {
                _image.uvRect = new Rect(0, 0, 1, 1);
            }
            else if (frameAspect < imageAspect)
            {
                var w = frameAspect / imageAspect;
                _image.uvRect = new Rect(0.5f - w * 0.5f, 0, w, 1);
            }
            else
            {
                var h = imageAspect / frameAspect;
                _image.uvRect = new Rect(0, 0.5f - h * 0.5f, 1, h);
            }
        }
    }
}