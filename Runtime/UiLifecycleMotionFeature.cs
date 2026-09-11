using System;
using Cysharp.Threading.Tasks;
using Juahn.V2.UiService;
using UnityEngine;

namespace Juahn.UiMotion.UiService
{
    [DisallowMultipleComponent, RequireComponent(typeof(MotionPlayer))]
    public class UiLifecycleMotionFeature : PresenterFeatureBase, ITransitionFeature
    {
        [SerializeField] private MotionPlayer _player;
        [SerializeField] private bool _waitForStart = true;
        [SerializeField] private bool _waitForEnd = true;
        private UniTask _open, _close;
        public MotionPlayer Player => _player;
        public UniTask OpenTransitionTask => _open;
        public UniTask CloseTransitionTask => _close;
        protected override void OnInit()
        {
            if (_player == null) _player = GetComponent<MotionPlayer>();
            if (_player != null) _player.ClaimTriggerOwnership();
        }
        public override void OnPresenterOpened()
        {
            if (_player == null) return;
            _player.CaptureBasePose();
            _open = PlayTransition(MotionRuntime.StartTrigger, _waitForStart);
        }
        public override void OnPresenterClosing()
        {
            if (_player == null) return;
            _player.StopAll();
            if (!MotionGraphValidator.IsFinite(_player.Graph, MotionRuntime.EndTrigger))
            {
                _close = UniTask.FromException(new InvalidOperationException("UI End motion must be finite: " + _player.Graph.name));
                return;
            }
            _close = PlayTransition(MotionRuntime.EndTrigger, _waitForEnd);
        }
        private UniTask PlayTransition(string trigger, bool wait)
        {
            var playback = _player.Play(trigger);
            if (!wait) return UniTask.CompletedTask;
            var completion = new UniTaskCompletionSource();
            playback.WhenCompleted(result =>
            {
                if (result.Outcome == MotionOutcome.Failed) completion.TrySetException(result.Error);
                else if (result.Outcome == MotionOutcome.Canceled) completion.TrySetCanceled();
                else completion.TrySetResult();
            });
            return completion.Task;
        }
        public void Fire(string trigger) { if (_player != null) _player.Fire(trigger); }
        public void Stop(string trigger) { if (_player != null) _player.Stop(trigger); }
        protected override void OnDisable()
        {
            base.OnDisable();
            if (_player != null) _player.ResetToBasePose();
        }
    }
}
