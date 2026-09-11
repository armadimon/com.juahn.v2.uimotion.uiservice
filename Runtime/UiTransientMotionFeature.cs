using System;
using Cysharp.Threading.Tasks;
using Juahn.V2.UiService;
using UnityEngine;

namespace Juahn.UiMotion.UiService
{
    /// <summary>유한 피드백 그래프가 끝나면 소유 위젯을 숨긴다. 게임 명령은 실행하지 않는다.</summary>
    [DisallowMultipleComponent, RequireComponent(typeof(MotionPlayer))]
    public sealed class UiTransientMotionFeature : UiFeature
    {
        [SerializeField] private string _trigger = MotionRuntime.StartTrigger;
        private MotionPlayer _player;
        private long _request;
        protected override void OnInit()
        {
            _player = GetComponent<MotionPlayer>();
            _player.ClaimTriggerOwnership();
        }
        public void PlayAndHide()
        {
            var widget = GetComponent<UiWidget>();
            if (widget == null) throw new InvalidOperationException("Transient feedback requires a UiWidget owner.");
            widget.Show();
            if (!widget.IsShown) return;
            _player.StopAll(); _player.ResetToBasePose();
            Canvas.ForceUpdateCanvases(); _player.CaptureBasePose();
            Complete(widget, ++_request).Forget();
        }
        private async UniTask Complete(UiWidget widget, long request)
        {
            try
            {
                var playback = _player.Play(_trigger);
                await UniTask.WaitUntil(() => playback.IsDone, cancellationToken: widget.LifetimeToken);
                if (request == _request && playback.Result.Outcome == MotionOutcome.Completed) widget.Hide();
            }
            catch (OperationCanceledException) { }
        }
        protected override void OnHide()
        {
            ++_request; _player.StopAll(); _player.ResetToBasePose();
        }
    }
}
