# Changelog

## [0.1.0] - 미출시

### 추가

브릿지 패키지 뼈대.

- `juahn.v2.UiMotion.UiService` 어셈블리 (런타임). `juahn.v2.UiMotion` ·
  `juahn.v2.UiMotion.Core` · `juahn.UiService` · `UniTask`를 참조한다
- `com.juahn.uiservice`를 `package.json`의 의존성에 **적지 않는다** —
  소비자가 따로 설치하는 외부 패키지이고, 여기에 적으면 버전이 어긋났을 때
  UPM이 해석에 실패한다. asmdef가 이름으로 참조한다
- `MotionUiServiceLog` — 브릿지의 진단. `[UiMotion/UiService]` 접두어를 붙인다
- 컴파일 게이트 (`Tools~/compile-check`). 설치된 Unity의 매니지드 DLL과 참조
  프로젝트의 `juahn.UiService.dll` · `UniTask.dll`을 참조해 런타임 패키지의
  `Runtime` 전체와 이 브릿지를 `dotnet build`로 컴파일한다. 그 어셈블리들은
  재배포할 수 없어 CI가 아니라 로컬이다
- CI 게이트 — 매니페스트 형식 · UiService를 의존성에 적지 않았는지 ·
  런타임 asmdef와 그 참조 · `Runtime` 폴더 존재 · `.meta` 누락 · GUID 중복

브릿지 본체.

- `MotionGraphFeature : PresenterFeatureBase, ITransitionFeature` — `UiPresenter`의
  열기·닫기 전이를 `MotionPlayer`의 그래프에 연결한다. 파일 하나가 병합의 전부다.
  `UiPresenter`가 이미 `ITransitionFeature`를 `await`한 뒤에 비활성화·파괴하므로
  별도의 숨기기 계약이 필요 없다
- `OnPresenterInitialized`에서 `ClaimTriggerOwnership()` — 이것이 없으면
  `PlayOnEnable`이 `SetActive(true)` 순간에 `Start`를 한 번 더 발사한다. 프리팹이
  활성 상태로 인스턴스화돼 `OnEnable`이 먼저 돌았더라도 이미 시작된 것을 걷어낸다
- **`OnPresenterOpening`이 아니라 `OnPresenterOpened`에서 `Start`를 발사한다.**
  Opening 시점에는 GameObject가 비활성이라 펌프에 등록되지 않았고
  (`MotionPlayer.Fire`가 비활성 플레이어에서 아무 일도 하지 않는다),
  완료원은 프리젠터가 `await`하기 전에 만들어져야 하기 때문이다
- `OnPresenterClosing`에서 `End`를 발사하고 기다린다. 프리젠터는 이 콜백 뒤에
  `CloseTransitionTask`를 `await`하고 그것이 끝나야 `SetActive(false)`를 하므로
  End 연출이 화면에 보이는 채로 끝까지 돈다
- 완료원이 영영 안 풀리는 경로를 `OnDisable`·`OnDestroy`가 막는다. `MotionPlayer`
  쪽도 `OnDisable`·`OnDestroy`에서 `StopAll`을 돌려 대기자를 푼다
- `_waitForStart` / `_waitForEnd`를 끄면 트리거는 그대로 발사하되 기다리지 않는다
- `Fire(string)` / `Stop(string)` — 프리젠터 코드가 `MotionPlayer`를 직접 알지 않고도
  `Click`·`Reward` 같은 임의 연출을 부를 수 있게 하는 통로
