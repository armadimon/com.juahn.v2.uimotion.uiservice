# Unity 확인 목록

컴파일 게이트는 이 브릿지가 **컴파일된다**는 것만 지킨다. 아래는 Unity에서
손으로 확인한다. `MotionPlayer`와 `MotionGraphFeature`를 붙인 팝업 프리팹과,
그것을 여닫는 `UiService` 호출부가 있으면 된다.

## 준비

1. `Start` · `Loop` · `End`를 전부 선언한 그래프를 하나 만든다.
   `Start`는 페이드 인 + 스케일, `Loop`는 은은한 `Float`, `End`는 페이드 아웃
   정도면 충분하다
2. 팝업 프리팹 루트에 `MotionPlayer`(그래프 꽂음) · `MotionGraphFeature` ·
   `UiPresenter` 파생 컴포넌트를 함께 붙인다
3. 프리젠터의 `OnOpenTransitionCompleted` / `OnCloseTransitionCompleted`에
   로그를 하나씩 심어 둔다. 타이밍은 이 두 줄로 읽는다

## 확인 항목

- [ ] 팝업이 열릴 때 `Start` 연출이 **끝난 뒤에** `OnOpenTransitionCompleted`가 온다.
      연출이 도는 도중에 로그가 뜨면 완료원이 너무 일찍 풀린 것이다

- [ ] `Start`가 끝나면 `Loop`가 자동으로 이어진다. 따로 발사하지 않는다 —
      런타임의 `AutoLoopAfterStart`가 한다

- [ ] 닫을 때 `End` 연출이 **화면에 보이는 채로** 끝까지 돌고 그 뒤에 사라진다.
      팝업이 즉시 사라지면 닫힘 완료원이 `await` 전에 풀린 것이다
      (README의 "왜 `OnPresenterOpening`이 아닌가" 마지막 문단 참조)

- [ ] `MotionPlayer`의 `PlayOnEnable`이 켜져 있어도 `Start`가 두 번 발사되지 않는다.
      연출이 처음에 한 번 튀거나 되감기면 소유권 주장이 듣지 않은 것이다.
      프리팹을 **활성 상태로** 인스턴스화하는 경로에서도 확인한다

- [ ] 그래프에 `End` 트리거가 없는 프리젠터도 정상적으로 닫힌다.
      경고가 콘솔에 한 번 뜨고, 팝업은 연출 없이 곧바로 사라진다.
      **닫히지 않고 화면에 남으면 심각한 문제다**

- [ ] `MotionPlayer`에 그래프가 아예 없어도 정상적으로 열리고 닫힌다

- [ ] `_waitForStart`를 끄면 `Start`가 재생되면서 프리젠터는 곧바로 열림을 보고한다

- [ ] `_waitForEnd`를 끄면 팝업이 `End`를 기다리지 않고 즉시 사라진다

- [ ] 같은 팝업을 빠르게 여러 번 열고 닫아도 걸리지 않는다.
      열 때마다 완료원이 새로 만들어지므로 이전 것이 남아 막지 않아야 한다

- [ ] 닫히는 도중에 씬을 바꿔도 예외가 나지 않는다.
      `MotionPlayer.OnDestroy`의 `StopAll`과 `MotionGraphFeature.OnDestroy`가
      대기자를 푼다

- [ ] 닫히는 도중에 팝업 오브젝트를 직접 `SetActive(false)`해도 걸리지 않는다.
      `MotionGraphFeature.OnDisable`이 대기자를 푼다

- [ ] 열린 적 없는 프리젠터를 닫아도 걸리지 않는다.
      비활성 플레이어의 `Fire`는 경고 한 번 후 no-op이고 `WaitFor`는 즉시 콜백한다

## 실패의 모양

이 브릿지가 잘못됐을 때의 증상은 둘뿐이다.

| 증상 | 원인 |
|---|---|
| **팝업이 닫히지 않고 화면에 박제된다** | 닫힘 완료원이 풀리지 않았다. `CloseTransitionTask`에서 `UiPresenter`가 멈춰 있다 |
| **`End` 연출이 한 프레임도 안 보인다** | 닫힘 완료원이 프리젠터의 `await`보다 먼저 풀렸다 |

전자가 더 심각하다. 하나라도 보이면 `MotionGraphFeature`의
`CompleteClose` 호출 지점을 전부 짚는다.
