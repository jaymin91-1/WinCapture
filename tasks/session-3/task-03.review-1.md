# task-03 리뷰 1회차
## 판정: 통과
## 실행 결과
- 맥 build: 성공 (경고 0, 오류 0)
- 맥 test: 206/206 통과
- CI: 구현 커밋 6f3b5a1 성공 (run 45952922974). 보고서 커밋 f88edd1 실행은 확인 시점에 진행 중(코드 변경 없음)
## 기준별 판정
- 기준 1 [정적]: 충족. src/WinCapture.App/Toast/ToastWindow.cs:16 `ToastWindow : OverlayWindowBase`, 캡처 제외는 기반 클래스 SourceInitialized에서 적용(우회 없음).
- 기준 2 [정적]: 충족. 생성자 인자 없이 기반 호출 -> allowActivation=false. OverlayWindowBase.cs:33 ShowActivated=false, :56-59 WS_EX_NOACTIVATE|TOOLWINDOW, :78 WM_MOUSEACTIVATE -> MA_NOACTIVATE. ShowInTaskbar 미노출.
- 기준 3 [자동]: 충족. ToastLayoutTests.SlotPosition_StacksUpwardFromBottomRight (0번 (1544,944), 1번 (1544,856)), SlotPosition_WorksWithNegativeWorkArea. Truncate 테스트도 존재.
- 기준 4 [정적]: 충족. ToastWindow.cs:87 MouseLeftButtonUp -> Click(), :117 콜백 호출(예외는 로그) 후 CloseToast(). 클릭 수신: 기반 TryHitTest 기본값은 처리 안 함(HTCLIENT 동작)이고 콘텐츠 Border가 흰 배경이라 히트 테스트됨. MA_NOACTIVATE여도 마우스 메시지는 전달되므로 NOACTIVATE 유지한 채 클릭 수신 가능하다는 설명은 타당.
- 기준 5 [정적]: 충족. 커밋 6f3b5a1에 csproj 변경 없음.
- 기준 6 [CI]: 충족 (6f3b5a1 성공, 새 경고 없음).
- 추가: UI 스레드 마샬링 ToastService.cs Show()의 CheckAccess/BeginInvoke 충족. 기존 테스트 삭제/변경 없음. Core는 WPF/Win32 참조 없음(Core의 ToastLayout은 순수). 비밀 값/바이너리 커밋 없음.
## 차단 이슈
없음
## Windows 확인 항목 (사용자에게 넘김)
- 기준 7 [수동]: task-04 연결 후, 토스트가 우하단에 뜨고 약 5초 뒤 사라지며, 클릭하면 연결 동작이 실행되고 닫히는지.
- 토스트가 뜰 때 다른 앱의 포커스/입력이 유지되는지, 작업표시줄·Alt+Tab에 안 나타나는지.
- 캡처 결과(스크린샷/녹화)에 토스트가 찍히지 않는지.
- 여러 개가 위로 쌓이는지, 마우스를 올리면 유지되는지, 고DPI/보조 모니터에서 크기·위치가 맞는지.
## 권고 사항 (반려 사유 아님)
- 오류색 #F04452는 SPEC 3장에 없는 값이다. 라이트 경고색도 SPEC에는 다크 팔레트의 #FF7A85만 있다(작업 파일이 라이트에서 "경고 #FF7A85 계열"로 지정). 오류를 진한 빨강으로 구분한 것은 팔레트 톤과 어긋나지 않으나, 사용자 확정이 필요하면 SPEC 3장에 라이트 경고/오류색을 추가할 것.
- 쌓인 토스트가 작업 영역 위로 넘칠 때의 상한이 없음(SPEC 미정, 현재 문제 없음).
- ToastLayout.Truncate의 maxLength<1 분기 중복 조건은 정리 가능.
