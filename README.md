# UI Motion for UiService

UI Motion을 juahn `UiService`의 팝업 생명주기에 붙이는 브릿지다. 런타임 코드
한 파일(`MotionGraphFeature`)이 전부다.

프리팹에 `MotionPlayer`와 `MotionGraphFeature`를 함께 붙이면 `UiPresenter`가
`Start` 그래프가 끝난 뒤에 열림 완료를 보고하고, `End` 그래프가 끝난 뒤에
비활성화한다.

## 설치

이 패키지보다 먼저 프로젝트에 있어야 하는 것:

1. `com.juahn.v2.uimotion` — UI Motion 런타임. 이 패키지의 UPM 의존성이다
2. `com.juahn.uiservice` — UiService
3. UniTask (`com.cysharp.unitask`) — UiService가 요구한다

**`com.juahn.uiservice`는 이 패키지의 `package.json`에 적혀 있지 않다.**
소비자가 따로 설치하는 외부 패키지이고, 여기에 적으면 버전이 어긋났을 때
UPM이 해석에 실패한다. asmdef가 어셈블리 이름으로 참조한다.

## 붙이는 법

1. 팝업 프리팹의 루트(또는 연출 대상이 있는 오브젝트)에 `MotionPlayer`를 붙이고
   그래프를 꽂는다
2. 같은 오브젝트에 `MotionGraphFeature`를 붙인다. `[RequireComponent]`가 걸려
   있으므로 `MotionPlayer`가 없으면 Unity가 함께 붙인다
3. 그 오브젝트에 `UiPresenter` 파생 컴포넌트가 있어야 한다. `UiPresenter`는
   `GetComponents<PresenterFeatureBase>()`로 같은 오브젝트의 피처만 모은다

`_player`를 비워 두면 같은 오브젝트에서 자동으로 찾는다.

## 그래프가 선언해야 하는 트리거

| 트리거 | 언제 | 필수 |
|---|---|---|
| `Start` | 팝업이 열릴 때 | 권장 |
| `Loop` | `Start`가 자연 완료하면 런타임이 자동으로 이어 붙인다 | 선택 |
| `End` | 팝업이 닫힐 때 | 권장 |

없는 트리거를 발사하면 경고가 한 번 뜨고 대기자는 즉시 풀린다. 그래서
`End`가 없는 그래프도 팝업이 정상적으로 닫힌다 — 연출 없이 바로 사라질 뿐이다.

`MotionPlayer`에 그래프가 아예 없어도 마찬가지로 즉시 열리고 닫힌다.

## `_waitForStart` / `_waitForEnd`

둘 다 기본값은 켜짐이다.

- **켜짐** — 트리거를 발사하고, 그것이 끝날 때까지 프리젠터의 전이 완료를 미룬다.
  `_waitForEnd`가 켜져 있어야 `End` 연출이 **화면에 보이는 채로** 끝까지 돈다
- **꺼짐** — 트리거는 그대로 발사하되 기다리지 않는다. 연출은 재생되지만
  프리젠터는 곧바로 다음 단계로 넘어간다. `_waitForEnd`를 끄면 `End`는
  발사되자마자 `SetActive(false)`에 잘려 사실상 보이지 않는다

## 왜 `OnPresenterOpening`이 아니라 `OnPresenterOpened`인가

`UiPresenter.InternalOpenProcessAsync`의 실제 순서는 이렇다.

```
_openTransitionCompletion = new
NotifyFeaturesOpening()        <- GameObject는 아직 비활성
gameObject.SetActive(true)
OnOpened() / NotifyFeaturesOpened()
await WaitForOpenTransitionsAsync()
```

`Opening` 시점에는 GameObject가 **비활성**이다. `MotionPlayer.Fire`는 비활성
플레이어에서 일부러 아무 일도 하지 않는다 — 스코프를 만들어 봤자 펌프가 틱하지
않아 `IsPlaying`이 true인 채로 굳고, 뒤이은 `WaitFor`가 영원히 풀리지 않기
때문이다. 그래서 `Opening`에서 발사하면 연출이 돌지 않는다.

그리고 `WaitForOpenTransitionsAsync`는 **이미 완료된 태스크를 건너뛴다.**
완료원은 프리젠터가 `await`하기 전에 만들어져야 하는데, 그 마지막 시점이
`OnPresenterOpened`다.

**"더 일찍 발사하는 게 낫지 않나"로 되돌리지 않는다.** 두 이유가 다 사라져야
그 변경이 맞는데, 둘 다 런타임과 UiService의 현재 계약이다.

같은 이유의 반대편도 있다. **닫힘 완료원은 `OnPresenterClosed`에서 풀면 안 된다.**
`InternalCloseProcessAsync`는 `NotifyFeaturesClosing()` 바로 다음 줄에서
`NotifyFeaturesClosed()`를 부르고 `await`는 그보다 뒤에 온다. 거기서 풀면
`WaitForCloseTransitionsAsync`가 `Succeeded`를 보고 건너뛰어 `End` 연출이
한 프레임도 보이지 않는다.

## 트리거 발사 소유권

`OnPresenterInitialized`에서 `MotionPlayer.ClaimTriggerOwnership()`을 부른다.
이것이 없으면 `PlayOnEnable`이 `SetActive(true)` 순간에 `Start`를 한 번 더
발사한다. 재발사 정책이 `Restart`면 우연히 무해하지만 `Ignore`나 `Queue`인
그래프에서는 실제 버그가 된다.

프리팹이 활성 상태로 인스턴스화돼 `OnEnable`이 `Init`보다 먼저 돌았더라도
안전하다 — `ClaimTriggerOwnership`이 이미 시작된 것을 걷어낸다.

## 임의 트리거

프리젠터 코드가 `MotionPlayer`를 직접 알 필요는 없다.

```csharp
[SerializeField] private MotionGraphFeature _motion;

private void OnRewardClaimed()
{
    _motion.Fire("Reward");
}
```

## 검증

```bash
./Tools~/compile-check/run.sh
```

Unity 에디터를 열지 않고 런타임 패키지(`Runtime/Core` · `Runtime/Unity`)와 이
브릿지를 함께 `dotnet build`로 컴파일한다. `juahn.UiService.dll`과
`UniTask.dll`은 소비자 프로젝트가 컴파일해 만든 결과물이라 재배포할 수 없어
CI가 아니라 로컬 게이트다.

참조 프로젝트를 자동으로 못 찾으면 지정한다.

```bash
REF_PROJECT=/path/to/UnityProject ./Tools~/compile-check/run.sh
```

동작 확인은 [docs/unity-verification.md](docs/unity-verification.md).
