using System;
using Baracuda.Bedrock.PlayerLoop;
using Baracuda.Bedrock.Timing;
using Baracuda.Bedrock.Types;
using Baracuda.Bedrock.Utilities;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Baracuda.UI
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
        private Loop _loop;
        private Action _update;
        private TweenCallback _onComplete;
        private float _updateInterval;
        private Color _playColor;

        public bool IsPlaying { get; private set; }

        private void Awake()
        {
            _playColor = playColor;
            _loop = Loop.Create(animationData.Sprites);
            if (randomizeStart)
            {
                _loop.Value = RandomUtility.Int(animationData.Sprites.Length - 1);
            }

            _update = OnUpdate;
            _updateInterval = 1f / framesPerSecond;
            _onComplete = () => Gameloop.Update -= _update;
            if (autoPlay)
            {
                Play();
            }
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
            image.sprite = animationData.Sprites[_loop++];
        }
    }
}