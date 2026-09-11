# Changelog

## [Unreleased] — IdleMine integration

- Migrate to V2 UiService dependency; add lifecycle, interaction, value and transient features; preserve MotionGraphFeature GUID adapter; update compile-check.

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

문서.

- README에 설치 순서(런타임 패키지 · UiService · UniTask), 붙이는 법, 그래프가
  선언해야 하는 트리거, `_waitForStart`/`_waitForEnd`를 끄면 무엇이 달라지는지,
  트리거 소유권, 검증 명령을 넣었다
- README에 **왜 `OnPresenterOpening`이 아니라 `OnPresenterOpened`인지**와
  **왜 닫힘 완료원을 `OnPresenterClosed`에서 풀면 안 되는지**를 남겼다.
  나중에 누가 "더 일찍 하는 게 낫지 않나"로 되돌리는 것을 막는다
- `docs/unity-verification.md` — 컴파일 게이트가 지키지 못하는 동작 확인 목록.
  실패의 모양 둘("팝업이 박제된다" · "End가 안 보인다")과 각각의 원인을 함께 적었다
- 런타임 패키지의 설계 스펙 6절을 이 브릿지의 실제 구현에 맞췄다. 스펙 초안은 닫힘
  완료원을 `OnPresenterClosed`에서 풀라고 읽혔지만 그것은 버그다 — 여기에는 그
  오버라이드가 없다. 스펙 3.3의 참조 목록(`juahn.v2.UiMotion.Core` · `UniTask`)과
  "UiService를 `package.json`에 적지 않는다"도 스펙에 반영됐다.
  런타임 패키지 README의 패키지 지도에 이 브릿지가 실렸다
