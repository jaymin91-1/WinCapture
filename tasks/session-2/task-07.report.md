# task-07 작업 보고서
## 한 일
- Native: `RegisterHotKey`/`UnregisterHotKey`/`GetWindowRect` LibraryImport 추가 (NativeMethods.cs), `WindowInterop.RegisterHotkey/UnregisterHotkey/GetWindowBounds` 래퍼.
- Core: `HotkeyPlan.Build`(항목별 파싱, 유효하지 않은/충돌 조합 제외, `HotkeyStatus` enum), `AppWindowPolicy.ShouldAutoHide`(겹침 판정, task-03 `RegionInteraction.Overlaps` 사용).
- App: `HotkeyService`(HWND_MESSAGE 메시지 전용 HwndSource, `Reapply()`, `UnregisterAll()`, `Statuses`/`StatusesChanged`, 실패 시 로그 + 트레이 풍선 알림). `App.xaml.cs`에서 `OnHotkey` 하나로 단축키/트레이 메뉴 디스패치, 종료 시 Dispose로 전체 해제.
- 연결: SelectRegion, ToggleBorder(RegionState.ToggleBorder), ClearRegion(`RegionCoordinator.Clear()` - lastRegion도 비움, 사용자 결정 반영), ToggleAppWindow. Screenshot/RecordVideo/RecordGif는 등록만, 로그만 남김.
- 트레이 "영역 지정"/"창 표시/숨김" 활성화, 같은 `OnHotkey` 경로 사용.
- `App.HideAppWindowIfOverlapping()` 헬퍼(autoHideAppWindow 설정 읽음, 세션 3에서 스크린샷이 호출).
- 정리(task-06 권고): RegionState.cs 중복 summary 제거, 드래그 종료 시 영역이 시작과 같으면 저장 생략(BorderWindow `_dragStartRegion`).
## 완료 기준별 대응
- 1 [정적]: 등록은 `NativeMethods.RegisterHotKey`만 사용. `SetWindowsHook`/`WH_KEYBOARD` grep 결과 없음.
- 2 [정적]: `src/WinCapture.App/HotkeyService.cs` `Reapply()`, `UnregisterAll()`, 로그/`_notifyUser`, `Statuses`; 종료 해제는 `App.OnExit`의 `_hotkeys.Dispose()`.
- 3 [정적]: `App.xaml.cs` `OnHotkey`->`ToggleAppWindow()`; 트레이 항목도 같은 `OnHotkey`. 설정 창 Hide만 수행(트레이/테두리 무관).
- 4 [정적]: `App.HideAppWindowIfOverlapping()`가 `AppWindowPolicy.ShouldAutoHide`(내부 `RegionInteraction.Overlaps`) 사용, `AutoHideAppWindow` 설정 읽음.
- 5 [자동]: 기존 테스트 변경 없음. 신규 `HotkeyPlanTests`(Defaults_AllSevenRegister, Invalid_..., Conflict_...), `AppWindowPolicyTests` 4개.
- 6 [CI]: 아래 참조.
- 7 [수동]: 아래.
## 내린 판단과 이유
- 충돌 조합은 그룹 전체를 등록하지 않음(어느 쪽이 우선인지 SPEC에 없어 안전하게 둘 다 제외).
- ToggleAppWindow는 설정 창이 보이면 숨기고, 아니면 기존 `ShowSettingsWindow` 경로로 표시.
- 알림은 `TaskbarIcon.ShowNotification` 풍선(최소 구현).
## 테스트 결과
- 맥 build: 성공 (경고 0, 오류 0)
- 맥 test: 105개 통과
- CI: 성공 (run 45942065065, 커밋 239222f). 푸시 성공
## Windows 확인 필요
- F-03-5: 다른 앱이 쓰는 조합으로 설정을 바꿔 시작하면 풍선 알림이 뜨는지, 나머지 단축키는 동작하는지.
- F-08-2: Ctrl+Alt+W와 트레이 "창 표시/숨김"으로 설정 창이 토글되고 트레이 아이콘/테두리는 그대로인지.
- Ctrl+Alt+R(영역 지정), Ctrl+Alt+H(테두리 토글), Ctrl+Alt+X(해제, 재시작 후 복원 안 됨) 동작.
- F-06-4: 테두리를 띄운 채 Win+Shift+S로 찍어 테두리가 안 나오는지. F-04-1~2, F-06-5~6도 함께.
- 종료 후 같은 단축키가 해제되어 다른 앱에서 쓸 수 있는지.
## 못 한 것 / 질문
- 없음
