using Baracuda.Mediator.Callbacks;
using Baracuda.Utilities.Types;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Baracuda.UI
{
    [RequireComponent(typeof(Image))]
    public class UISpritesAnimation : MonoBehaviour
    {
        [SerializeField] private bool autoPlay = true;
        [SerializeField] private int framesPerSecond = 24;
        [SerializeField] private Sprite[] sprites;
        [SerializeField] private Color playColor;
        [SerializeField] private Color stopColor;

        private Image _image;
        private Timer _timer;
        private Loop _loop;
        private Action _update;
        private float _updateInterval;

        public bool IsPlaying { get; private set; }

        private void Awake()
        {
            _image = GetComponent<Image>();
            _loop = Loop.Create(sprites);
            _update = OnUpdate;
            _updateInterval = 1f / framesPerSecond;
            if (autoPlay)
            {
                Play();
            }
        }

        public void Play()
        {
            if (IsPlaying)
            {
                return;
            }
            IsPlaying = true;
            _image.color = playColor;
            Gameloop.Update += _update;
        }

        public void Stop()
        {
            IsPlaying = false;
            _image.color = stopColor;
            Gameloop.Update -= _update;
        }

        private void OnDestroy()
        {
            Stop();
        }

        private void OnUpdate()
        {
            if (_timer.IsRunning)
            {
                return;
            }
            _timer = Timer.FromSeconds(_updateInterval);
            _image.sprite = sprites[_loop++];
        }
    }
}