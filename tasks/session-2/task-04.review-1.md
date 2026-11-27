# task-04 리뷰 1회차
## 판정: 통과
## 실행 결과
- 맥 build: 성공 (경고 0, 오류 0)
- 맥 test: 76/76 통과
- CI: 코드 커밋 274ed13 성공 (run 45940936607). 보고서 커밋 7aa9122 run 45941075355는 검토 시점에 진행 중(문서만 변경)
## 기준별 판정
- 1 [정적] 충족: OverlayWindowBase.cs:29 SourceInitialized 구독, :40 ExcludeFromCapture(WDA_EXCLUDEFROMCAPTURE, WindowInterop.cs:10), 실패 시 :42 Log.Error(Win32 오류 포함)
- 2 [정적] 충족: OverlayWindowBase.cs:48-50 LAYERED|TOOLWINDOW|NOACTIVATE, :26 Topmost, :24 ShowInTaskbar=false, :69 WM_MOUSEACTIVATE->MA_NOACTIVATE. 이 호출들은 소스 전체에서 이 파일(과 Native 헬퍼)에만 있음(grep 확인)
- 3 [정적] 충족: BorderWindow.cs:16 OverlayWindowBase 상속, 스타일/캡처 제외 코드 중복 없음. BorderWindow.cs:57,60 BorderLayout.GetWindowRect/RegionMath.GetBorderOuterRect 호출, BorderVisual.cs:41-43에서 바깥 사각형 - 영역(Exclude)으로 링을 그려 영역 안쪽 미도색. 그립은 영역 바깥(BorderLayout.cs:20, 테스트 GetGripRect_Is_12x12_Outside_Region)
- 4 [정적] 충족: DllImport/LibraryImport는 NativeMethods.cs에만 있고 전부 LibraryImport. WH_KEYBOARD_LL 없음
- 5 [정적] 충족: OverlayWindowBase.cs:74-83 WM_NCHITTEST -> BorderWindow.cs:71-79, 영역 내부(Passthrough)와 Outside는 HTTRANSPARENT, 띠/그립은 HTCLIENT (RegionInteraction.HitTest 근거)
- 6 [CI] 충족: windows-latest 성공(274ed13), 경고 0
- 7 [수동]: 아래로 이관
- 테스트: 기존 테스트 삭제/변경 없음(tests diff는 추가 55줄뿐). 새 테스트는 SPEC 숫자 (98,98,404,304 기반)를 사용. 그립 포함 창 (98,98,414,314)는 coder 판단값.
## 차단 이슈
없음
## coder 판단 검토
- 창을 그립까지 확장: 작업 범위의 "우하단 그립을 영역 바깥에 그린다"를 충족하려면 필요. 확장부는 투명이고 영역 안쪽은 칠하지 않으며 HTTRANSPARENT 처리됨. 범위 내.
- 8px 띠 히트 영역을 task-06으로 미룸: 작업 파일이 "띠·그립 히트 처리는 task-06, 이번엔 통과/비통과 구조만"이라고 명시. 적합. 단, task-06에서 창 사각형을 8px 띠까지 넓히거나 띠 영역을 거의 투명하게 칠해야 하므로 task-06 작업 파일에 반영 필요.
- x64 전용 GetWindowLongPtrW: SPEC/배포가 win-x64이므로 문제 없음.
## Windows 확인 항목 (사용자에게 넘김)
- F-06-5 [수동]: 임시로 App.xaml.cs OnStartup에서 BorderController 생성 뒤 `_host.Services.GetRequiredService<RegionState>().SetRegion(new WinCapture.Core.Geometry.Rect(100,100,400,300));` 추가 후 실행(커밋 금지). 영역 안쪽을 클릭해 아래 앱에 전달되고 포커스가 바뀌지 않는지 확인
- F-05-5 [수동]: 빨간(#FF3B30) 2px 테두리가 영역 바깥에 있고 우하단에 12px 삼각형이 있는지, 125%/150% 배율에서 두께가 물리 2px인지
- F-06-1 [수동]: Win+Shift+S 등 캡처에 테두리가 나타나지 않는지, 작업표시줄/Alt+Tab에 안 보이는지
## 권고 사항
- GetWindowRect가 그립 확장 결과를 쓰므로 task-06에서 band 8px 확장 시 BorderLayout 테스트 기대값 변경이 생기면 SPEC 숫자(98,98,404,304) 테스트는 유지할 것.
- OnSourceInitialized에서 SetWindowLongPtr 실패 반환을 검사하지 않음(경미).
