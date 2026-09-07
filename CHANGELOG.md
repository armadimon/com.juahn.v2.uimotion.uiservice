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
