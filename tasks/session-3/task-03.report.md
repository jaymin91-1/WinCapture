# task-03 작업 보고서
## 한 일
- Core: `ToastKind`(Success/Warning/Error), `ToastLayout.SlotPosition`(우하단 기준 위로 쌓기), `ToastLayout.Truncate`(제목 40자/본문 90자 줄임) — src/WinCapture.Core/Presentation/ToastLayout.cs
- Native: `Win32ScreenInfo.GetWorkAreaAtCursor()`(커서 모니터 작업 영역, rcWork), `MONITOR_DEFAULTTONEAREST` 상수 추가. `IScreenInfo` 인터페이스는 바꾸지 않음
- App: `Toast/ToastWindow.cs`(OverlayWindowBase 상속 카드), `Toast/IToastService.cs`, `Toast/ToastService.cs`(스택 관리, UI 스레드 마샬링), `App.xaml.cs` DI 등록(`Win32ScreenInfo` 단일 인스턴스 + `IScreenInfo` 별칭 + `IToastService`)
- 테스트 `tests/WinCapture.Core.Tests/ToastLayoutTests.cs` 추가
## 완료 기준별 대응
- 1 [정적]: `ToastWindow : OverlayWindowBase` — src/WinCapture.App/Toast/ToastWindow.cs:16. 캡처 제외는 기반 클래스 SourceInitialized에서 적용되며 이 창은 우회하지 않음
- 2 [정적]: `allowActivation` 기본값(false) 사용 → 기반 클래스가 NOACTIVATE|TOOLWINDOW 적용, ShowActivated=false, ShowInTaskbar=false (OverlayWindowBase.cs). Alt+Tab 미노출
- 3 [자동]: `ToastLayoutTests.SlotPosition_StacksUpwardFromBottomRight`(0번 (1544,944), 1번 (1544,856), 2번 (1544,768)), `SlotPosition_WorksWithNegativeWorkArea`(음수 작업 영역)
- 4 [정적]: ToastWindow.cs:87 `MouseLeftButtonUp` → `Click()`(:117 `_onClick?.Invoke()` 후 :124 `CloseToast()`)
- 5 [정적]: csproj 변경 없음, 새 NuGet 없음
- 6 [CI]: 아래 참조
- 7 [수동]: Windows 확인 필요 항목 참조
## 내린 판단과 이유
- 클릭 수신에 `allowActivation`을 쓰지 않았다. 기반 클래스는 WM_MOUSEACTIVATE에 MA_NOACTIVATE를 돌려주므로 창이 활성화되지 않아도 WPF가 마우스 클릭 이벤트는 받는다. 따라서 포커스 탈취가 전혀 없다(NOACTIVATE 유지).
- 위치/크기는 `SetWindowPos`(물리 픽셀)로 지정하고, 크기는 창의 DPI로 DIP(360x80, 여백 16, 간격 8)를 변환했다(DpiConverter 사용). 작업 영역은 첫 토스트가 뜰 때 커서 모니터로 정하고, 토스트가 모두 사라질 때까지 고정해 스택이 흩어지지 않게 했다.
- 색: 성공 점 `#2F7CF6`, 경고 `#FF7A85`, 오류 `#F04452`. SPEC에 오류색이 없어 경고색 계열의 진한 빨강으로 정했다(바꾸려면 ToastWindow.cs 한 곳).
- 마우스를 올리면 타이머를 멈추고 벗어나면 5초부터 다시 시작(선택 사항 구현). 클릭 동작이 없어도 클릭하면 닫힌다.
- 토스트가 많이 쌓여 작업 영역 위로 넘치는 경우의 상한은 SPEC에 없어 두지 않았다.
## 테스트 결과
- 맥 build: 성공 (경고 0, 오류 0)
- 맥 test: 206개 통과
- 커밋: 6f3b5a1, 푸시 성공
- CI: 성공 (run 45952922974, 커밋 6f3b5a1)
## Windows 확인 필요
- F-06 / task 기준 7 [수동]: 아직 어디서도 토스트를 호출하지 않는다(task-04에서 연결). task-04 이후 우하단 표시, 5초 후 소멸, 클릭 시 동작 실행·닫힘, 다른 앱의 포커스 유지, 캡처 결과에 토스트가 없는지, 고DPI/보조 모니터에서 크기·위치를 확인.
## 못 한 것 / 질문
- 없음
