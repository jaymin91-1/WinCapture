# task-04: 스크린샷 서비스 연결 (버튼·트레이·단축키 단일 경로, 자동 숨김, 저장, 클립보드, 토스트)
## 목표
컨트롤 바 [캡처](사진 모드), 트레이 "스크린샷", Ctrl+Alt+S가 같은 경로로 `RegionState.CaptureRect`를 캡처해 PNG로 저장하고 토스트를 띄운다.
## 관련 SPEC
F-09 전체(1~6), F-08-3, 4장 컨트롤 바([캡처]·자동 숨김 컨트롤 바 포함)·영역 상태, 7장(`capture.copyToClipboard`, `autoHideAppWindow`), TECH-RULES 제약 1·3·9
## 범위
- `CaptureAvailability`에 `CaptureMode.Screenshot`을 구현됨으로 추가(한 곳만 수정). 사진 모드의 [캡처] 버튼 활성화와 문구 `스크린샷 찍기`, 툴팁 갱신. 동영상·GIF는 계속 비활성
- 앱 단일 진입점 `ScreenshotService.CaptureAsync()`(이름 자유). `OnHotkey(Screenshot)`, 컨트롤 바 [캡처](Screenshot 모드), 트레이 "스크린샷"(활성화, 단축키 표시 유지)이 모두 이 메서드만 호출한다
- 순서
  1. `RegionState.CaptureRect`를 읽는다(지정 영역이면 그 사각형, 전체 화면이면 커서 모니터 전체). null이면 오류 토스트
  2. 기존 `App.HideAppWindowIfOverlapping()`으로 설정 창·컨트롤 바 중 영역과 겹치는 창을 숨긴다(autoHideAppWindow 켜짐일 때만). 창이 실제로 화면에서 사라진 뒤 캡처하도록 짧은 렌더 대기(예: 한 번의 Dispatcher 렌더/약 100~150ms)를 둔다
  3. task-02의 캡처 → PNG. 테두리·선택 오버레이·토스트는 캡처 제외 + 테두리는 영역 바깥이라 결과에 들어가지 않는다. 별도 처리를 추가하지 않는다
  4. task-01의 파일명·경로로 저장 폴더(없으면 생성)에 저장. 폴더 생성/쓰기 실패는 예외를 삼키지 않고 로그 + 오류 토스트
  5. `capture.copyToClipboard`가 켜져 있으면 이미지를 클립보드에도 넣는다(UI 스레드에서, 실패 시 로그 + 경고 토스트만)
  6. 성공 토스트(저장 완료, 파일명 표시). 클릭하면 `explorer.exe /select,"<경로>"`로 파일이 선택된 탐색기를 연다(경로에 공백·특수문자 안전하게 인용)
- 캡처 중 중복 호출(연타)은 무시하거나 직렬화한다(동시에 두 번 BitBlt/저장하지 않는다). 캡처/인코딩/저장은 백그라운드, UI는 멈추지 않는다
- 자동 숨김으로 숨긴 창을 캡처 후 다시 보일지는 SPEC에 없다. 아래 "결정 필요"의 진행자 확인이 없으면 **숨긴 창을 그대로 둔다**(`reshowAppWindowAfterRecording`은 녹화용 설정이므로 쓰지 않는다)고 구현하고 보고서에 적는다
- Core에 둘 수 있는 판단(캡처 가능 여부, 오류 메시지 선택 등)은 Core로 빼 테스트한다
## 범위 밖
- 동영상·GIF, 설정 창 캡처 탭의 UI(세션 6), 영역 선택 오버레이 변경, 단축키 등록 실패 토스트(task-05)
## 완료 기준
1. [자동] `CaptureAvailability.IsImplemented(Screenshot)`는 true, Video/Gif는 false. 기존 `CaptureAvailabilityTests`의 기대값을 이 변경에 맞게 갱신하되 Video/Gif 검증은 유지(기존 테스트 삭제·skip 금지, 변경 이유를 보고서에 적는다)
2. [자동] Core로 뺀 파이프라인 판단(예: CaptureRect null → 실패, 클립보드 옵션 on/off에 따른 단계 결정)에 단위 테스트가 있다
3. [정적] 컨트롤 바 [캡처], 트레이 "스크린샷", `HotkeyAction.Screenshot`이 모두 같은 서비스 메서드를 호출한다. `App.xaml.cs`의 "not implemented yet" 분기에서 Screenshot이 빠졌다 (파일:줄)
4. [정적] 캡처 대상이 `RegionState.CaptureRect`이고, 캡처 직전 `HideAppWindowIfOverlapping()`이 호출되며 숨김 후 렌더 대기가 있다 (F-08-3) (파일:줄)
5. [정적] 저장 경로·파일명이 task-01 Core 함수로 만들어지고 App에 중복 로직이 없다. `saveFolder`의 `%USERPROFILE%` 확장이 적용된다
6. [정적] 클립보드 복사가 `copyToClipboard` 설정에 따라 조건부이고, 토스트 클릭이 탐색기 `/select`를 실행한다 (F-09-5·6)
7. [정적] 캡처·저장 예외가 로그에 남고 사용자에게 오류 토스트로 알려진다. 빈 catch 없음
8. [정적] 새 오버레이 창을 만들었다면 `OverlayWindowBase` 상속, `WH_KEYBOARD_LL`·FFmpeg 코드 없음
9. [CI] windows-latest build/test 성공, 새 경고 없음
10. [수동] F-09-4(400×300 영역 → PNG 정확히 400×300, 테두리 없음), F-09-5(토스트 클릭 → 탐색기에서 파일 선택), F-09-6(클립보드), F-08-3(설정 창·컨트롤 바가 영역과 겹친 채 찍어도 결과에 안 나옴), 전체 화면 상태에서 [캡처]·Ctrl+Alt+S가 커서 모니터 전체를 저장하는지 → Windows 확인 항목
## 검증 방법
qa가 코드 읽기 + 맥 build/test + CI 확인. 세 진입점이 한 메서드로 합쳐지는지 grep으로 확인.
