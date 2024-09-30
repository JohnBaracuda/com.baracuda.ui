using System;
using Baracuda.Utility.PlayerLoop;
using Baracuda.Utility.Timing;
using Baracuda.Utility.Utilities;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;
using Index = Baracuda.Utility.Types.Index;

namespace Baracuda.UI.Components
{
    [RequireComponent(typeof(Image))]
    public class UISpritesAnimation : MonoBehaviour
    {
        [SerializeField] [Required] private UISpriteAnimationData animationData;
        [SerializeField] private bool autoPlay = true;
        [SerializeField] private int framesPerSecond = 24;
        [SerializeField] private Color playColor;
        [SerializeField] private Color stopColor;
        [SerializeField] private float fadeInTime;
        [SerializeField] private float fadeOutTime;
        [SerializeField] private bool randomizeStart = true;
        [SerializeField] private Image image;

        private ScaledTimer _scaledTimer;
        private Index _index;
        private Action _update;
        private TweenCallback _onComplete;
        private float _updateInterval;
        private Color _playColor;
        private bool _isInitialized;

        public bool IsPlaying { get; private set; }

        private void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }
            _isInitialized = true;
            _playColor = playColor;
            _index = Index.Create(animationData.Sprites);
            if (randomizeStart)
            {
                _index.Value = RandomUtility.Int(animationData.Sprites.Length - 1);
            }

            _update = OnUpdate;
            _updateInterval = 1f / framesPerSecond;
            _onComplete = () => Gameloop.Update -= _update;
            if (autoPlay)
            {
                Play();
            }
        }

        private void Awake()
        {
            Initialize();
        }

        private void OnValidate()
        {
            image = GetComponent<Image>();
        }

        public void SetAnimationColor(Color color, bool animate = true)
        {
            _playColor = color;
            if (animate && IsPlaying)
            {
                image.DOComplete(true);
                image.DOColor(_playColor, fadeInTime).SetEase(Ease.InOutSine);
            }
        }

        public void Play()
        {
            if (IsPlaying || !enabled || Gameloop.IsQuitting)
            {
                return;
            }

            Initialize();
            IsPlaying = true;
            image.DOComplete(true);
            image.DOColor(_playColor, fadeInTime).SetEase(Ease.InOutSine);
            Gameloop.Update += _update;
        }

        public void Stop()
        {
            if (!enabled || Gameloop.IsQuitting)
            {
                return;
            }

            IsPlaying = false;
            if (Gameloop.IsQuitting)
            {
                return;
            }

            image.DOComplete(true);
            image.DOColor(stopColor, fadeOutTime).SetEase(Ease.InOutSine).OnComplete(_onComplete);
        }

        private void OnDestroy()
        {
            Stop();
            Gameloop.Update -= _update;
            if (Gameloop.IsQuitting)
            {
                return;
            }

            image.DOKill(true);
        }

        private void OnUpdate()
        {
            if (_scaledTimer.IsRunning)
            {
                return;
            }

            _scaledTimer = ScaledTimer.FromSeconds(_updateInterval);
            image.sprite = animationData.Sprites[_index++];
        }
    }
}