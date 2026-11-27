# 세션 3 전체 검증 (스크린샷)

## 판정: 통과

## 실행 결과
- 맥 build (`dotnet build WinCapture.sln`): 성공, 경고 0, 오류 0
- 맥 test (`dotnet test tests/WinCapture.Core.Tests`): 217/217 통과, 실패 0, 건너뜀 0
- CI: 성공 (최신 커밋 e5cd4dd, run 45954045809, windows-latest). 직전 커밋 d7b108b, 519051c(코드 최종)도 success

## 공통 완료 기준
1. 맥 test 통과: 충족 (217/217)
2. 맥 build: 충족 (경고 0)
3. 최신 커밋 CI: 충족 (e5cd4dd success)
4. 이전 세션 테스트 통과: 충족. 217개 전부 통과, 제거·변경된 이전 테스트는 아래 5번에서 근거 확인
5. 테스트 삭제·skip·기대값 변경 금지: 충족. `git diff 6790df3 HEAD -- tests` 전체 확인 (Skip/Ignore 추가 없음). 제거·변경은 모두 SPEC 변경(2ca6603 자동 숨김 삭제, 0c1486b {counter} 규칙)에 근거:
   - AppWindowPolicyTests.cs 삭제(4개), RegionInteractionTests의 `Overlaps_SpecExamples` 삭제(3 케이스): SPEC F-08 기준 1(겹침 판정)·3(자동 숨김) 삭제(SPEC.md:173)에 따른 것. 대응 코드(AppWindowPolicy, Overlaps)도 제거됨
   - SettingsStoreTests: `AutoHideAppWindow`, `ReshowAppWindowAfterRecording` 기본값 단언·설정 제거: SPEC 7장 키 삭제(SPEC.md:255)에 따른 것
   - CaptureModeTests: 옛 설정 키 케이스를 `autoHideAppWindow`+`reshowAppWindowAfterRecording` 포함으로 확장, "깨진 captureMode 값" 테스트의 단언을 삭제된 속성 대신 `SaveFolder`로 교체(동일한 의도 유지), `Removed_auto_hide_keys_in_an_old_file_are_ignored_without_warnings` 신규 추가(SPEC.md:255 "기존 파일에 있어도 무시"를 검증). 약화 없음
   - 나머지는 모두 신규 추가(CaptureFileNamer, CapturePlan, ScreenshotPlan, ToastLayout, HotkeyFailureMessage, CaptureAvailability)
   - HotkeyPlanTests -25줄: 알림 문구 로직이 HotkeyFailureMessage(Core)로 이동하고 HotkeyFailureMessageTests로 옮겨진 것(task-05 리뷰에서 확인된 범위). 삭제로 인한 커버리지 손실 없음
6. SPEC [자동] 테스트 존재, [정적] 근거 기재: 충족 (아래 F-09, 각 task 리뷰에 파일:줄 근거)
7. 새 경고 없음, TECH-RULES 위반 없음: 충족 (아래 추가 확인)
8. PROGRESS.md 기록·커밋·푸시: 현재 상태 확인만. 세션 3 task-01~05 통과, 주요 결정이 기록됨. 상태는 "진행 중"이며 세션 통과 기록과 "사용자 승인 대기" 전환은 진행자가 처리. 작업 트리 clean, 원격과 동기화됨
9. [수동] 항목 나열: 충족 (아래 "Windows 확인 항목")

## 세션 완료 기준: F-09 [자동] 전부
- 기준 1 (`WinCapture_20261127_093005.png`): 충족 (CaptureFileNamerTests.cs:28)
- 기준 2 (`_1`, `_2` 접미, 토큰 {w}{h}{type}{counter}, 금지 문자 `_` 치환): 충족 (CaptureFileNamerTests.cs:35-38 중복 접미, :51 `\/:*?"<>|` -> `_________`, :70-113 counter 규칙: 파일 없음 0001, 최대+1, 유형 폴더 범위, 4자리 초과, 날짜·크기 와일드카드, {type}은 일치 필요)
- 기준 3 (저장 경로, 유형별 하위 폴더 켬/끔): 충족 (CaptureFileNamerTests.TypeSubfolders_ControlsPath, :55)
- 부수 로직(CapturePlan.Clip, ScreenshotPlan, ToastLayout, 설정 키 무시)도 테스트 존재

## 추가 확인
- 오버레이 상속: 오버레이 계열 창 4개(ToastWindow, DragTooltipWindow, BorderWindow, SelectionOverlayWindow)가 모두 `OverlayWindowBase` 상속 (Toast/ToastWindow.cs:16, Overlay/DragTooltipWindow.cs:10, Overlay/BorderWindow.cs:18, Overlay/SelectionOverlayWindow.cs:17). 나머지 `FluentWindow` 3개(Settings, ControlBar, HotkeyPopup)는 일반 앱 창으로 오버레이가 아님
- P/Invoke: `DllImport`/`LibraryImport`는 `src/WinCapture.Native/Interop/NativeMethods.cs`에만 있음. App 등 다른 프로젝트에 없음
- `WH_KEYBOARD_LL` 사용 없음, `Process.Kill` 없음(녹화는 세션 4 범위)
- task-05 권고(Reapply 호출 스레드): 현재 `Reapply()` 호출처는 `App.xaml.cs:111`(OnStartup, UI 스레드) 하나뿐. HotkeyService는 UI 스레드 전용으로 문서화(HotkeyService.cs:12). 토스트는 `ToastService.Show`가 `Dispatcher.CheckAccess()`로 UI 스레드가 아니면 마샬링(Toast/ToastService.cs:24-40). 설정 변경 시 재등록(세션 6)이 추가되어도 안전. 문제 없음
- 자동 숨김 코드·설정 키·테스트는 SPEC 지시대로 제거됨, 범위 밖 기능 없음

## 차단 이슈
없음

## 후속 세션 권고 (반려 사유 아님)
- 세션 4에서 녹화 영상에도 토스트·테두리가 찍히지 않는지(오버레이 캡처 제외) 같은 방식으로 확인
- 세션 6에서 단축키 변경 후 `Reapply()`를 UI 스레드에서만 호출하도록 유지(ViewModel 비동기 경로에서 호출 시 주의)
- 상태가 `RegisterFailed` 외인 경우도 토스트 문구가 "다른 앱이 사용 중일 수 있습니다"로 묶인다. 필요하면 세션 6에서 원인별 문구 검토

## Windows 확인 항목 (사용자 확인, 따라 하기 쉬운 순서)
PC에서 준비: `git pull` -> `dotnet build WinCapture.sln -c Debug` -> `dotnet test` -> `dotnet run --project src/WinCapture.App`

### 1. 앱 실행
- 앱이 예외 없이 뜨고 트레이 아이콘, 메인 창(컨트롤 바)이 나타난다. (change 05 EnumDisplayMonitors 콜백 포함: 시작 시 오류 없음)
- 옛 설정 파일(`%AppData%\WinCapture\settings.json`에 `"autoHideAppWindow": true`가 들어 있는 파일)이어도 정상 시작된다.
- 트레이 우클릭 메뉴가 즉시 뜨고(첫 클릭 포함), 좌클릭하면 메인 창이 앞으로 나온다. 트레이 "종료"로 종료된다.

### 2. 메인 창과 아이콘
- 작업표시줄, 메인 창·설정 창 제목줄, exe 파일 아이콘이 파랑 둥근 사각형+뷰파인더로 보인다. 안 보이면 `%LocalAppData%\WinCapture\logs`의 `Setting the window icon` 경고를 전달.
- 메인 창이 목업과 같은 구성이다(메뉴바, 상태 카드, 사진/동영상/GIF 모드, [영역 지정][영역 해제], 캡처 버튼). 라이트 테마이며 Windows 다크 모드에서도 라이트다. 사진 모드에서만 캡처 버튼이 켜지고 동영상·GIF는 비활성이다.
- 창 폭이 "스크린샷 찍기" 버튼 폭 + 좌우 8이고, 줄들의 좌우 폭이 같으며, 모드 변경 시 폭이 변하지 않는다. 눈에 띄게 넓으면 `%LocalAppData%\WinCapture\logs`의 `bounds:` 줄을 전달. 상하 여백 8, 빈 공간 없음.
- 네 변·네 모서리 8곳에서 포인터가 화살표 그대로이고 드래그해도 크기가 변하지 않는다. 더블클릭·Alt+Space·Win+Up·화면 가장자리 드래그로 최대화되지 않는다. 메뉴바 드래그 이동, 최소화, X(숨김)는 정상이다. 맨 위 가장자리에서 닫기/최소화 버튼이 클릭된다.
- 100%/150% 배율에서 잘림 없이 여백이 균일하다. 상태 카드에 긴 좌표가 있으면 말줄임(...)과 마우스 오버 툴팁이 나온다.
- 메뉴 [단축키] 팝업: 메인 창 위에 뜨고 7개 키캡 행이 보이며, 가장자리 드래그·더블클릭으로 크기가 변하지 않는다. [설정 열기]는 설정 창(단축키 탭), [닫기]는 팝업을 닫는다.
- 설정 창이 예외 없이 열리고 탭 6개 아이콘이 얇은 선으로 보인다. 메뉴 [설정], 트레이 "설정"으로 열린다.

### 3. 영역 지정
- 시작 시 상태 카드가 `전체 화면`+좌표+`· 모니터 N`+`테두리 없음`이다.
- [영역 지정]으로 영역을 정하면 빨간 테두리가 나오고 상태 카드가 `지정 영역`+`테두리 표시 중`으로 바뀐다. 이동·리사이즈 후 좌표가 갱신된다.
- Ctrl+Alt+H로 테두리 토글(`테두리 숨김`). 전체 화면 상태에서는 테두리가 나타나지 않는다.
- [영역 해제] 또는 Ctrl+Alt+X로 테두리가 사라지고 커서가 있는 모니터 전체 화면 상태가 된다. 앱을 재시작하면 테두리 없는 전체 화면으로 시작한다.

### 4. 스크린샷 (F-09 수동 4)
- 400x300 영역을 지정하고 [캡처] 버튼(또는 Ctrl+Alt+S, 트레이 "스크린샷")을 누른다. 저장된 PNG가 정확히 400x300이고 빨간 테두리가 없다(파일 속성 > 자세히). 기본 저장 폴더는 `%USERPROFILE%\Pictures\WinCapture`이고 없으면 자동 생성된다.
- 전체 화면 상태에서 찍으면 커서가 있는 모니터 전체가 저장된다.
- 영역을 화면 밖으로 걸치게 하고 찍으면 토스트의 W x H가 저장된 파일의 실제 크기와 같다.
- 125%/150% 배율, 보조·음수 좌표 모니터에서도 캡처가 선택 영역과 일치한다.
- 커서가 영역 안에 있을 때 PNG에 커서가 찍히고(위치 정확), 아래 6번 설정으로 끄면 안 찍힌다.
- Ctrl+Alt+S를 연타해도 UI가 멈추지 않고, 진행 중인 촬영 중에는 중복 파일이 생기지 않는다.

### 5. 토스트와 탐색기 (F-09 수동 5)
- 저장 성공 토스트가 우하단에 나타나 약 5초 뒤 사라진다. 마우스를 올리면 유지된다. 여러 개가 연속으로 뜨면 위로 쌓인다.
- 토스트가 떠도 다른 앱의 포커스·입력이 유지되고 작업표시줄·Alt+Tab에 나타나지 않는다. 토스트와 테두리는 PNG에 찍히지 않는다.
- 토스트를 클릭하면 탐색기가 열리고 방금 저장된 파일이 선택되어 있다. 공백이 들어간 저장 경로에서도 동작한다.
- 고DPI·보조 모니터에서 토스트 크기와 위치가 맞다.

### 6. 클립보드 (F-09 수동 6, 설정 파일을 직접 고쳐야 함)
1. 앱을 종료한다(트레이 "종료").
2. 메모장으로 `%AppData%\WinCapture\settings.json`(탐색기 주소창에 `%AppData%\WinCapture` 입력)을 연다.
3. `"capture": { "includeCursor": true, "copyToClipboard": false }`에서 `copyToClipboard`를 `true`로 바꿔 저장한다. (커서 확인 시 `includeCursor`를 `false`로 바꿔 시험하고 되돌린다)
4. 앱을 다시 실행하고 스크린샷을 찍은 뒤 그림판에서 Ctrl+V로 붙여 넣으면 방금 이미지가 나온다.
5. `copyToClipboard`를 `false`로 되돌리고 다시 찍으면 클립보드가 바뀌지 않는다(다른 내용을 복사해 둔 뒤 확인).

### 7. 메인 창 찍힘/숨김 (F-08 수동 4)
- 메인 창이 캡처 영역 안에 보이는 상태로 찍으면 결과 PNG에 메인 창이 나온다(자동 숨김 없음).
- Ctrl+Alt+W(또는 트레이 "창 표시/숨김", 메뉴 [창 숨기기])로 메인 창·설정 창을 숨긴 뒤 찍으면 결과에 나오지 않는다. 다시 누르면 복원되고 트레이 아이콘과 테두리는 그대로다.

### 8. 단축키 등록 실패 토스트 (F-03 수동 5 / task-05)
- 앱을 종료하고, 같은 조합(예: Ctrl+Alt+S)을 먼저 등록한 다른 앱을 실행한 상태에서 WinCapture를 켠다.
- 우하단에 경고 토스트가 1개 뜬다("단축키 등록 실패 ... 설정 > 단축키 탭에서 변경"). 실패한 단축키가 여러 개여도 토스트는 하나다.
- 메인 창 [단축키] 팝업에서 해당 행에 빨간 테두리와 `사용 중인 조합`이 보인다. (설정 변경 후 재등록 시 토스트는 세션 6에서 확인 가능)

확인 후 폰에서 "세션 3 승인" 또는 "수정 요청: ..."으로 알려 주세요.
