using Juahn.V2.UiService;
using UnityEngine;

namespace Juahn.UiMotion.UiService
{
    /// <summary>입력 한 번이 이전 입력 연출을 취소한다. 전환과 다른 시각 자식/플레이어를 사용한다.</summary>
    [DisallowMultipleComponent, RequireComponent(typeof(MotionPlayer), typeof(UiButton))]
    public class UiInteractionMotionFeature : UiFeature
    {
        [SerializeField] private MotionPlayer _player;
        [SerializeField] private string _pressTrigger = "Press";
        [SerializeField] private string _releaseTrigger = "Release";
        [SerializeField] private string _clickTrigger = "";
        private UiButton _button;
        public MotionPlayer Player => _player;
        protected override void OnInit()
        {
            _button = GetComponent<UiButton>();
            if (_player == null) _player = GetComponent<MotionPlayer>();
            if (_player != null) _player.ClaimTriggerOwnership();
        }
        protected override void OnShow()
        {
            if (_player != null) _player.CaptureBasePose();
            _button.Pressed += Press; _button.Released += Release; _button.Clicked += Click; _button.Canceled += Cancel;
        }
        protected override void OnHide()
        {
            _button.Pressed -= Press; _button.Released -= Release; _button.Clicked -= Click; _button.Canceled -= Cancel;
            Cancel();
        }
        private void Press() => Fire(_pressTrigger);
        private void Release() => Fire(_releaseTrigger);
        private void Click() => Fire(_clickTrigger);
        private void Cancel() { if (_player != null) _player.ResetToBasePose(); }
        public void Fire(string trigger)
        {
            if (_player == null || string.IsNullOrEmpty(trigger)) return;
            _player.StopAll(); _player.Play(trigger);
        }
    }
}
