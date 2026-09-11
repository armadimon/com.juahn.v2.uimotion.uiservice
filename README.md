# UI Motion for UiService V2

UiService의 수명·입력·값 변화를 MotionPlayer에 연결한다. 패키지 의존성은 `com.juahn.v2.uiservice`, `com.juahn.v2.uimotion`이며 소비자가 UniTask를 설치한다. 어셈블리는 `juahn.v2.UiMotion.UiService`, namespace는 `Juahn.UiMotion.UiService`다.

| 기능 | 배치와 동작 |
|---|---|
| UiLifecycleMotionFeature | Presenter의 MotionPlayer 옆에 배치. 레이아웃 확정 후 기준 자세를 저장하고 Start/Loop/End를 소유한다. End는 유한해야 한다. |
| UiInteractionMotionFeature | UiButton과 MotionPlayer 옆에 배치. Press/Release/선택적 Click/Denied를 재생하며 최신 입력이 이전 연출을 취소한다. |
| UiValueMotionFeature | MotionValue와 MotionPlayer 옆에 배치. 현재 표시값부터 실행별 Target/Duration으로 이어간다. UiProgressBar가 있으면 표시값과 요청을 자동 연결한다. |
| UiTransientMotionFeature | 일시 피드백 UiWidget에 배치. PlayAndHide로 시작하고 해당 실행이 정상 완료된 경우에만 숨긴다. 재요청·닫기·취소 후 과거 완료가 새 표시를 숨기지 않는다. |

기능은 트리거 소유권을 선언해 MotionPlayer의 자동 OnEnable 재생과 중복되지 않는다. 표시 종료 시 입력·값 구독을 해제하고 플레이어를 정리한다. 닫는 중에는 End 연출이 보여야 하므로 입력 구독 종료와 플레이어 비활성화 시점을 혼동하지 않는다.

전환과 버튼 피드백이 같은 Transform을 동시에 변경하지 않게 별도 시각 자식/플레이어를 사용한다. `StopAll()`은 실행 중지이며 `ResetToBasePose()`는 중지와 기준 자세 복구다. 그래프 완료로 구매·보상·게임 단계 전환을 결정하지 않는다.

기존 `MotionGraphFeature`는 MonoScript GUID 호환을 위해 `UiLifecycleMotionFeature`를 상속한다. 새 프리팹은 새 이름을 사용한다. 소비자 에셋 참조를 이행하고 0건으로 확인하기 전까지 호환 타입을 삭제하지 않는다.

검증:

```sh
REF_PROJECT=/path/to/UnityProject ./Tools~/compile-check/run.sh
```

로컬 소비자가 빌드한 `juahn.v2.UiService.dll`과 `UniTask.dll` 및 Unity 참조 DLL로 컴파일한다. IdleMine PlayMode 검증은 열기·닫기·다시 열기, 모달 입력, 공유 그래프, timeScale=0, 게이지와 Animator 샘플링을 포함한다.
