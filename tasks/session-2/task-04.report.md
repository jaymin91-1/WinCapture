# task-04 작업 보고서
## 한 일
- Native: `src/WinCapture.Native/Interop/NativeMethods.cs`(LibraryImport: SetWindowDisplayAffinity, Get/SetWindowLongPtrW, SetWindowPos, GetSystemMetrics, GetDpiForWindow + 상수), `WindowInterop.cs`(Core 타입을 쓰는 얇은 헬퍼, 가상 화면 조회 포함). Native csproj에 `AllowUnsafeBlocks` 추가(LibraryImport 생성기 요구).
- App: `Overlay/OverlayWindowBase.cs`, `BorderWindow.cs`, `BorderVisual.cs`, `BorderController.cs`. `App.xaml.cs`에서 `RegionState`/`BorderController`를 DI 등록하고 시작 시 생성(단축키 연결은 task-05/07).
- Core: `BorderLayout`(창 사각형, 그립 사각형), `RegionState`(영역 유무, 테두리 표시 여부, Set/Clear/Toggle, Changed 이벤트). 테스트 3개 추가.
## 완료 기준별 대응
- 1 [정적]: OverlayWindowBase.cs의 `OnSourceInitialized`(SourceInitialized 핸들러)에서 `WindowInterop.ExcludeFromCapture(hwnd)`(= SetWindowDisplayAffinity WDA_EXCLUDEFROMCAPTURE) 호출, 실패 시 `Log.Error`(Win32 오류 코드 포함).
- 2 [정적]: 같은 파일 `OnSourceInitialized`에서 `AddExtendedStyles(LAYERED|TOOLWINDOW|NOACTIVATE)`, 생성자에서 `Topmost = true`, ShowInTaskbar=false, ShowActivated=false, WS_NOACTIVATE 외에 `WM_MOUSEACTIVATE -> MA_NOACTIVATE`도 처리.
- 3 [정적]: `BorderWindow : OverlayWindowBase`. 스타일/캡처 제외 코드는 BorderWindow에 없음. `Apply()`가 `BorderLayout.GetWindowRect`(내부에서 `RegionMath.GetBorderOuterRect` 사용)와 `RegionMath.GetBorderOuterRect`를 호출. `BorderVisual`은 바깥 사각형에서 영역 사각형을 `GeometryCombineMode.Exclude`로 빼서 영역 안쪽을 칠하지 않는다. DIP 변환은 `DpiConverter`만 사용.
- 4 [정적]: DllImport/LibraryImport는 `NativeMethods.cs`에만 있고 모두 LibraryImport. WH_KEYBOARD_LL 없음.
- 5 [정적]: `BorderWindow.TryHitTest`가 `RegionInteraction.HitTest` 결과로 Passthrough/Outside는 `HTTRANSPARENT`, Move/Resize는 `HTCLIENT`를 반환. `OverlayWindowBase.WndProc`의 WM_NCHITTEST에서 호출.
- 6 [CI]: 성공 (아래).
- 7 [수동]: 아래 Windows 확인 필요.
## 내린 판단과 이유
- 창 사각형: 완료 기준 F-06-3의 (98,98,404,304)는 `GetBorderOuterRect` 그대로이며 테스트됨. 다만 우하단 그립(영역 꼭짓점에서 오른쪽/아래 12px)이 테두리 바깥 사각형(2px)보다 크므로, 창 자체는 우/하 방향으로 그립까지 확장한 `BorderLayout.GetWindowRect`를 썼다(예: (98,98,414,314)). 확장 영역은 투명하고 영역 안쪽은 어디서도 칠하지 않는다.
- 8px 히트 띠는 아직 창 범위(2px)보다 넓다. 띠 히트 영역 확보(거의 투명한 픽셀 칠하기 등)와 드래그 처리는 task-06 범위라 하지 않았다. 따라서 이번에는 두께 2px 테두리와 그립 부분만 입력을 받을 수 있는 구조까지만 있다.
- HTTRANSPARENT는 다른 프로세스 창에는 효과가 제한적이라, 영역 안쪽은 칠하지 않아(알파 0) 클릭이 OS 수준에서도 통과한다. 이중 구조다.
- RegionState.SetRegion은 표시 여부를 바꾸지 않는다(기본 표시). ToggleBorder는 영역이 없으면 아무 일도 안 한다. ClearRegion의 lastRegion 저장 비우기는 호출자(task-05/07) 몫.
- `GetWindowLongPtrW`/`SetWindowLongPtrW`는 64비트 전용 export라 x64 빌드 전제(배포가 win-x64).
- DPI 변경 시 WPF가 창을 리사이즈하므로 `OnDpiChanged`에서 물리 좌표로 다시 배치한다.
## 테스트 결과
- 맥 build: 성공 (경고 0, 오류 0)
- 맥 test: 76개 통과
- CI: 성공 (run 45940936607, 커밋 274ed13). 푸시 성공.
## Windows 확인 필요
- F-06-5 [수동], F-05-5 [수동]: 아직 단축키/선택 UI가 없어 임시 확인 경로가 없다. 확인하려면 `App.xaml.cs` OnStartup의 BorderController 생성 뒤에 임시로 `_host.Services.GetRequiredService<RegionState>().SetRegion(new WinCapture.Core.Geometry.Rect(100, 100, 400, 300));` 한 줄을 넣고 실행한다(커밋하지 말 것). 확인: 빨간 2px 테두리가 영역 바깥에 있고 우하단에 12px 삼각형이 있는지, 안쪽 클릭이 아래 앱으로 가고 포커스가 안 바뀌는지, Win+Shift+S 캡처에 테두리가 안 나오는지, 125%/150% 배율에서 두께가 물리 2px인지.
## 못 한 것 / 질문
- 없음
