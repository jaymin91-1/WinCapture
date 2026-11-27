# task-04: 스크린샷 서비스 연결 (버튼·트레이·단축키 단일 경로, 저장, 클립보드, 토스트, 자동 숨김 제거)
## 목표
컨트롤 바 [캡처](사진 모드), 트레이 "스크린샷", Ctrl+Alt+S가 같은 경로로 `RegionState.CaptureRect`를 캡처해 PNG로 저장하고 토스트를 띄운다.
## 관련 SPEC
F-09 전체(1~6), F-08 (2026-11-27 SPEC 변경: 자동 숨김 삭제, 새 [수동] F-08-4, 커밋 2ca6603), 4장 컨트롤 바([캡처])·영역 상태, 7장(`capture.copyToClipboard`; `autoHideAppWindow`·`reshowAppWindowAfterRecording` 삭제됨), TECH-RULES 제약 1·3·9
## 범위
- `CaptureAvailability`에 `CaptureMode.Screenshot`을 구현됨으로 추가(한 곳만 수정). 사진 모드의 [캡처] 버튼 활성화와 문구 `스크린샷 찍기`, 툴팁 갱신. 동영상·GIF는 계속 비활성
- 앱 단일 진입점 `ScreenshotService.CaptureAsync()`(이름 자유). `OnHotkey(Screenshot)`, 컨트롤 바 [캡처](Screenshot 모드), 트레이 "스크린샷"(활성화, 단축키 표시 유지)이 모두 이 메서드만 호출한다
- 순서
  1. `RegionState.CaptureRect`를 읽는다(지정 영역이면 그 사각형, 전체 화면이면 커서 모니터 전체). null이면 오류 토스트
  2. **앱 창을 자동으로 숨기지 않는다.** 메인 창(컨트롤 바)·설정 창이 보이는 상태로 캡처하면 결과물에 그대로 나온다(SPEC F-08). 캡처 직전 숨김 호출도, 렌더 대기도 넣지 않는다
  3. task-02의 캡처 → PNG. 테두리·선택 오버레이·토스트는 캡처 제외 + 테두리는 영역 바깥이라 결과에 들어가지 않는다. 별도 처리를 추가하지 않는다
  4. task-01의 파일명·경로로 저장 폴더(없으면 생성)에 저장. 폴더 생성/쓰기 실패는 예외를 삼키지 않고 로그 + 오류 토스트
  5. `capture.copyToClipboard`가 켜져 있으면 이미지를 클립보드에도 넣는다(UI 스레드에서, 실패 시 로그 + 경고 토스트만)
  6. 성공 토스트(저장 완료, 파일명 표시). 클릭하면 `explorer.exe /select,"<경로>"`로 파일이 선택된 탐색기를 연다(경로에 공백·특수문자 안전하게 인용)
- 캡처 중 중복 호출(연타)은 무시하거나 직렬화한다(동시에 두 번 BitBlt/저장하지 않는다). 캡처/인코딩/저장은 백그라운드, UI는 멈추지 않는다
- **자동 숨김 관련 코드·테스트 제거/교체** (SPEC 변경 근거: 사용자 지시, 커밋 2ca6603)
  - `App.HideAppWindowIfOverlapping` 및 그 호출부 제거
  - Core `AppWindowPolicy`의 겹침 판정 로직 제거(수동 토글 ToggleAppWindow 관련 로직은 유지)
  - 설정 키 `autoHideAppWindow`, `reshowAppWindowAfterRecording`을 설정 모델·기본값에서 제거. 기존 설정 파일에 이 키가 있어도 오류 없이 무시된다(로드가 실패하거나 경고 폭주하지 않음)
  - 위 코드에 묶인 기존 테스트(겹침 판정, 두 설정 키 기본값/직렬화 등)는 제거하거나 새 동작(키 무시, 수동 토글 유지)을 검증하는 테스트로 교체한다. 공통 기준 5(기존 테스트 삭제 금지)의 예외이며 근거는 SPEC 변경이다. 이 범위를 벗어난 기존 테스트는 건드리지 않는다
  - 보고서에 제거·교체한 코드/테스트 목록(파일, 이름, 제거 또는 교체 사유)을 남긴다
- Core에 둘 수 있는 판단(캡처 가능 여부, 오류 메시지 선택 등)은 Core로 빼 테스트한다
## 범위 밖
- 동영상·GIF, 설정 창 캡처 탭의 UI(세션 6), 영역 선택 오버레이 변경, 단축키 등록 실패 토스트(task-05)
## 완료 기준
1. [자동] `CaptureAvailability.IsImplemented(Screenshot)`는 true, Video/Gif는 false. 기존 `CaptureAvailabilityTests`의 기대값을 이 변경에 맞게 갱신하되 Video/Gif 검증은 유지(기존 테스트 삭제·skip 금지, 변경 이유를 보고서에 적는다)
2. [자동] Core로 뺀 파이프라인 판단(예: CaptureRect null → 실패, 클립보드 옵션 on/off에 따른 단계 결정)에 단위 테스트가 있다
3. [정적] 컨트롤 바 [캡처], 트레이 "스크린샷", `HotkeyAction.Screenshot`이 모두 같은 서비스 메서드를 호출한다. `App.xaml.cs`의 "not implemented yet" 분기에서 Screenshot이 빠졌다 (파일:줄)
4. [정적] 캡처 대상이 `RegionState.CaptureRect`이고, 스크린샷 경로에 앱 창 숨김 호출과 렌더 대기가 없다 (파일:줄)
4-1. [정적] `HideAppWindowIfOverlapping`, `AppWindowPolicy` 겹침 판정, `autoHideAppWindow`, `reshowAppWindowAfterRecording`이 src·tests 전체에서 grep으로 더 나오지 않는다. 기존 설정 파일의 해당 키는 무시된다(이를 검증하는 [자동] 테스트 존재). 수동 토글(ToggleAppWindow, 트레이 "창 표시/숨김")은 그대로 동작하는 코드가 남아 있다
4-2. [자동] 교체·제거된 테스트 목록이 보고서에 있고, 제거 사유가 SPEC 변경에 한정된다. 그 외 기존 테스트의 삭제·skip·기대값 변경은 없다
5. [정적] 저장 경로·파일명이 task-01 Core 함수로 만들어지고 App에 중복 로직이 없다. `saveFolder`의 `%USERPROFILE%` 확장이 적용된다
6. [정적] 클립보드 복사가 `copyToClipboard` 설정에 따라 조건부이고, 토스트 클릭이 탐색기 `/select`를 실행한다 (F-09-5·6)
7. [정적] 캡처·저장 예외가 로그에 남고 사용자에게 오류 토스트로 알려진다. 빈 catch 없음
8. [정적] 새 오버레이 창을 만들었다면 `OverlayWindowBase` 상속, `WH_KEYBOARD_LL`·FFmpeg 코드 없음
9. [CI] windows-latest build/test 성공, 새 경고 없음
10. [수동] F-09-4(400×300 영역 → PNG 정확히 400×300, 테두리 없음), F-09-5(토스트 클릭 → 탐색기에서 파일 선택), F-09-6(클립보드), F-08-4(메인 창이 캡처 영역 안에 보이는 상태로 찍으면 결과에 나오고, ToggleAppWindow로 숨긴 뒤 찍으면 나오지 않음), 전체 화면 상태에서 [캡처]·Ctrl+Alt+S가 커서 모니터 전체를 저장하는지 → Windows 확인 항목
## 검증 방법
qa가 코드 읽기 + 맥 build/test + CI 확인. 세 진입점이 한 메서드로 합쳐지는지 grep으로 확인.
