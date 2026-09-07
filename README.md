# UI Motion for UiService

UI Motion을 juahn `UiService`의 팝업 생명주기에 붙이는 브릿지다.

프리팹에 `MotionPlayer`와 `MotionGraphFeature`를 함께 붙이면 `UiPresenter`가
`Start` 그래프가 끝난 뒤에 열림 완료를 보고하고, `End` 그래프가 끝난 뒤에
비활성화한다.

## 설치

`com.juahn.v2.uimotion`(런타임 패키지)과 `com.juahn.uiservice`, UniTask가 먼저
프로젝트에 있어야 한다. UiService는 이 패키지의 의존성에 적혀 있지 않다 —
소비자가 따로 설치하는 외부 패키지이기 때문이다.

## 검증

```bash
./Tools~/compile-check/run.sh
```

Unity 에디터를 열지 않고 런타임 패키지와 이 브릿지를 함께 컴파일한다.
`juahn.UiService.dll`과 `UniTask.dll`은 재배포할 수 없어 CI가 아니라 로컬 게이트다.
