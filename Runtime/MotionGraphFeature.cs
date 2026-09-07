using System;
using Cysharp.Threading.Tasks;
using Juahn.UiService;
using UnityEngine;

namespace Juahn.UiMotion.UiService
{
    /// <summary>
    /// UiPresenter의 열기·닫기 전이를 <see cref="MotionPlayer"/>의 그래프에 연결한다.
    ///
    /// <b>이 파일 하나가 "병합"의 전부다.</b> UiService의 <c>UiPresenter</c>는 이미
    /// <c>ITransitionFeature</c>를 <c>await</c>한 뒤에 비활성화·파괴하므로, 별도의
    /// 숨기기 계약을 새로 만들 필요가 없다.
    ///
    /// <see cref="MotionPlayer"/>와 같은 오브젝트에 붙인다.
    /// </summary>
    [AddComponentMenu("Juahn/UI Motion/Motion Graph Feature")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MotionPlayer))]
    public sealed class MotionGraphFeature : PresenterFeatureBase, ITransitionFeature
    {
        [SerializeField]
        [Tooltip("비워 두면 같은 오브젝트에서 찾는다.")]
        private MotionPlayer _player;

        [SerializeField]
        [Tooltip("켜면 Start 그래프가 끝날 때까지 프리젠터가 열림 완료를 미룬다.")]
        private bool _waitForStart = true;

        [SerializeField]
        [Tooltip("켜면 End 그래프가 끝날 때까지 프리젠터가 비활성화를 미룬다.")]
        private bool _waitForEnd = true;

        [NonSerialized] private UniTaskCompletionSource _openCompletion;
        [NonSerialized] private UniTaskCompletionSource _closeCompletion;

        /// <inheritdoc />
        public UniTask OpenTransitionTask => _openCompletion?.Task ?? UniTask.CompletedTask;

        /// <inheritdoc />
        public UniTask CloseTransitionTask => _closeCompletion?.Task ?? UniTask.CompletedTask;

        /// <summary>연결된 플레이어. 없으면 null.</summary>
        public MotionPlayer Player => _player;

        /// <summary>
        /// 임의 트리거를 발사한다. 프리젠터 코드가 <c>Click</c>·<c>Reward</c> 같은
        /// 연출을 부를 때 쓴다. 브릿지를 거치므로 프리젠터가 <c>MotionPlayer</c>를
        /// 직접 알 필요가 없다.
        /// </summary>
        public void Fire(string trigger)
        {
            if (_player != null)
            {
                _player.Fire(trigger);
            }
        }

        public void Stop(string trigger)
        {
            if (_player != null)
            {
                _player.Stop(trigger);
            }
        }

        private void Reset()
        {
            _player = GetComponent<MotionPlayer>();
        }

        private void OnValidate()
        {
            if (_player == null)
            {
                _player = GetComponent<MotionPlayer>();
            }
        }

        /// <inheritdoc />
        public override void OnPresenterInitialized(UiPresenter presenter)
        {
            base.OnPresenterInitialized(presenter);

            if (_player == null)
            {
                _player = GetComponent<MotionPlayer>();
            }

            if (_player == null)
            {
                MotionUiServiceLog.Warn(
                    "MotionGraphFeature에 MotionPlayer가 없습니다. 전이가 즉시 완료됩니다.", this);
                return;
            }

            // 트리거 발사를 이쪽이 전담한다고 선언한다. 이것이 없으면 PlayOnEnable이
            // SetActive(true) 순간에 Start를 한 번 더 발사한다. 재발사 정책이 Restart면
            // 우연히 무해하지만 Ignore나 Queue인 그래프에서는 실제 버그가 된다.
            //
            // 프리팹이 활성 상태로 인스턴스화돼 OnEnable이 여기보다 먼저 돌았더라도
            // 안전하다 — ClaimTriggerOwnership이 이미 시작된 것을 걷어낸다.
            _player.ClaimTriggerOwnership();
        }

        /// <inheritdoc />
        public override void OnPresenterOpened()
        {
            // Opening이 아니라 Opened에서 하는 이유가 둘이다.
            //
            // 하나, Opening 시점에는 GameObject가 아직 비활성이라 펌프에 등록되지 않았다.
            // MotionPlayer.Fire는 비활성 플레이어에서 아무 일도 하지 않는다 — 스코프를
            // 만들어 봤자 아무도 틱하지 않기 때문이다.
            //
            // 둘, 프리젠터는 이 콜백 직후에 전이 태스크를 await한다. 그때 이미 완료된
            // 태스크는 건너뛰므로, 완료원은 반드시 그 전에 만들어져야 한다.
            if (!_waitForStart || _player == null)
            {
                FireWithoutWaiting(MotionRuntime.StartTrigger);
                return;
            }

            _openCompletion = new UniTaskCompletionSource();

            _player.Fire(MotionRuntime.StartTrigger);
            _player.WaitFor(MotionRuntime.StartTrigger, CompleteOpen);
        }

        /// <inheritdoc />
        public override void OnPresenterClosing()
        {
            // 여기서 하는 이유 — 프리젠터는 이 콜백 다음에 CloseTransitionTask를 await하고,
            // 그것이 끝나야 비활성화한다. 그래서 End 연출이 화면에 보이는 채로 끝까지 돈다.
            //
            // OnPresenterClosed에서는 절대 완료시키지 않는다. UiPresenter의
            // InternalCloseProcessAsync는 NotifyFeaturesClosing() 바로 다음 줄에서
            // NotifyFeaturesClosed()를 부르고, await는 그보다 뒤에 온다. 거기서
            // 완료원을 풀면 WaitForCloseTransitionsAsync가 Succeeded를 보고 건너뛰어
            // End 연출이 한 프레임도 보이지 않는다.
            //
            // 대신 오브젝트가 사라지는 경로는 OnDisable / OnDestroy가 막는다.
            // MotionPlayer 쪽도 OnDisable·OnDestroy에서 StopAll을 돌려 대기자를 푼다.
            if (!_waitForEnd || _player == null)
            {
                FireWithoutWaiting(MotionRuntime.EndTrigger);
                return;
            }

            _closeCompletion = new UniTaskCompletionSource();

            // Fire("End")는 Loop를 먼저 멈춘다. 닫히는 중에 계속 떠다니면 안 되기 때문이다.
            _player.Fire(MotionRuntime.EndTrigger);
            _player.WaitFor(MotionRuntime.EndTrigger, CompleteClose);
        }

        private void OnDisable()
        {
            // 프리젠터가 아니라 다른 경로로 비활성화됐을 수 있다. 붙잡아 두지 않는다.
            // 정상적인 닫기에서는 프리젠터가 await를 끝낸 뒤에 SetActive(false)를 하므로
            // 여기 올 때는 이미 풀려 있다.
            CompleteOpen();
            CompleteClose();
        }

        private void OnDestroy()
        {
            CompleteOpen();
            CompleteClose();
        }

        private void FireWithoutWaiting(string trigger)
        {
            if (_player != null)
            {
                _player.Fire(trigger);
            }
        }

        private void CompleteOpen()
        {
            // TrySetResult라 두 번 불려도 안전하다.
            if (_openCompletion != null)
            {
                _openCompletion.TrySetResult();
            }
        }

        private void CompleteClose()
        {
            if (_closeCompletion != null)
            {
                _closeCompletion.TrySetResult();
            }
        }
    }
}
