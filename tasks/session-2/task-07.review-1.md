# task-07 리뷰 1회차
## 판정: 통과
## 실행 결과
- 맥 build: 성공 (경고 0, 오류 0)
- 맥 test: 105/105 통과
- CI: 구현 커밋 239222f 성공 (run 45942065065). 보고서 갱신 커밋 3e29a28(run 45942168452)은 검토 시점에 진행 중이며 코드 변경이 없는 문서 커밋이다.
## 기준별 판정
- 기준 1 [정적]: 충족. 등록은 NativeMethods.RegisterHotKey만 사용 (src/WinCapture.Native/Interop/NativeMethods.cs, WindowInterop.cs RegisterHotkey). src 전체 grep에서 WH_KEYBOARD/SetWindowsHook 없음.
- 기준 2 [정적]: 충족. HotkeyService.Reapply()가 UnregisterAll 후 재등록, 종료 시 App.OnExit에서 Dispose -> UnregisterAll. 실패 시 Log.Warning(Win32 error 포함), _notifyUser(트레이 풍선, App.xaml.cs), Statuses/StatusesChanged 노출. Invalid/Conflict/RegisterFailed 모두 알림 대상.
- 기준 3 [정적]: 충족. 단축키와 트레이 "창 표시/숨김"이 모두 OnHotkey -> ToggleAppWindow 경로. 설정 창 Hide만 수행하고 트레이/BorderWindow는 건드리지 않음. 트레이 "영역 지정"도 OnHotkey(SelectRegion).
- 기준 4 [정적]: 충족. App.HideAppWindowIfOverlapping()이 AutoHideAppWindow 설정을 읽고 AppWindowPolicy.ShouldAutoHide(내부 RegionInteraction.Overlaps) 사용. 실제 호출은 세션 3(범위 밖 명시).
- 기준 5 [자동]: 충족. tests diff는 HotkeyPlanTests.cs 신규 추가만(삭제/변경 줄 없음). HotkeyPlanTests 3개, AppWindowPolicyTests 4개가 필터링·겹침 판정을 실제로 검증.
- 기준 6 [CI]: 충족 (구현 커밋 성공, 경고는 맥 빌드 기준 0).
- 기준 7 [수동]: 아래 Windows 확인 항목으로 이관.
## 확인 요청 사항별 소견
- 2) 충돌 시 둘 다 제외: SPEC F-03-3은 충돌 검사가 "두 동작 ID를 모두 반환"하라고만 규정하고 우선순위 규칙은 없다. 둘 다 제외는 이와 모순되지 않고 안전한 선택이며, 테스트(Conflict_RejectsAllMembersOfTheGroup)로 고정되어 있다. 사용자 결정이 필수인 수준은 아니다(세션 6 단축키 탭에서 충돌 경고 표시 방식과 함께 필요하면 재확인하면 됨).
- 3) ClearRegion -> RegionCoordinator.Clear()가 state 해제와 lastRegion 비움을 수행 (사용자 결정 반영).
- 4) Screenshot/RecordVideo/RecordGif는 등록만 하고 로그만 남김. 범위 이탈 없음.
- 5) RegionState summary 정리, BorderWindow 변경 없는 드래그 저장 생략(final != _dragStartRegion) 확인.
## 차단 이슈
없음
## Windows 확인 항목 (사용자에게 넘김)
- F-03 기준 5: 다른 앱이 쓰는 조합으로 설정을 바꿔 시작하면 풍선 알림이 뜨고 나머지 단축키는 동작하는지 (설정 탭 경고 표시는 세션 6).
- F-08 기준 2: Ctrl+Alt+W와 트레이 "창 표시/숨김"으로 설정 창이 토글되고 트레이 아이콘/테두리가 그대로인지.
- Ctrl+Alt+R(영역 지정), Ctrl+Alt+H(테두리 토글), Ctrl+Alt+X(해제 후 재시작해도 복원 안 됨) 동작, 트레이 "영역 지정" 동작.
- F-06 기준 4: 테두리를 띄운 채 Win+Shift+S로 찍어 테두리가 안 나오는지. F-04 기준 1~2, F-06 기준 5~6도 함께.
- 앱 종료 후 같은 단축키가 해제되어 다른 앱에서 쓸 수 있는지.
- F-08 기준 3은 세션 3에서 확인.
## 권고 사항
- 단축키 등록 id가 1+(int)action이라 enum 값에 의존한다. 후속 변경 시 주의.
- 실패 알림이 Invalid/Conflict/RegisterFailed를 한 문구로 묶는다. 세션 3 토스트 전환 시 원인별 문구 분리를 고려.
