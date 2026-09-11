using System.Collections.Generic;
using Juahn.V2.UiService;
using UnityEngine;

namespace Juahn.UiMotion.UiService
{
    [DisallowMultipleComponent, RequireComponent(typeof(MotionPlayer), typeof(MotionValue))]
    public sealed class UiValueMotionFeature : UiFeature
    {
        [SerializeField] private string _trigger = "Change";
        private MotionPlayer _player;
        private MotionValue _value;
        private UiProgressBar _bar;
        private readonly Dictionary<string, float> _parameters = new Dictionary<string, float>();
        protected override void OnInit()
        {
            _player = GetComponent<MotionPlayer>(); _value = GetComponent<MotionValue>(); _bar = GetComponent<UiProgressBar>();
            _player.ClaimTriggerOwnership();
        }
        protected override void OnShow()
        {
            if (_bar == null) return;
            _value.Value = _bar.DisplayedValue;
            _value.Changed += _bar.SetDisplayedValue;
            _bar.ValueRequested += SetValue;
        }
        protected override void OnHide()
        {
            _player.StopAll();
            if (_bar == null) return;
            _value.Changed -= _bar.SetDisplayedValue;
            _bar.ValueRequested -= SetValue;
        }
        public void SetValue(float target, float duration)
        {
            _player.StopAll();
            if (duration <= 0f || !isActiveAndEnabled)
            {
                _value.Value = target;
                if (_bar != null) _bar.SetDisplayedValue(target);
                return;
            }
            _parameters["Target"] = target; _parameters["Duration"] = duration;
            _player.Play(_trigger, _parameters);
        }
    }
}
