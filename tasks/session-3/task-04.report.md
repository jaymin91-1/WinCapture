# task-04 작업 보고서
## 한 일
- Core `WinCapture.Core/Capture/ScreenshotPlan.cs`: `ScreenshotPlan.Decide`(CaptureRect null/빈 영역 → 오류, 클립보드 단계 결정), `ScreenshotMessages`(토스트 문구, 탐색기 `/select,"경로"` 인자, 파일명 추출), `SingleFlightGate`(연타 무시).
- App `Capture/ScreenshotWorkflow.cs`: 단일 진입점 `RunAsync()`. CaptureRect 읽기 → 백그라운드 BitBlt/PNG(`ScreenshotService.CaptureAsync`) → 백그라운드에서 `CaptureFileNamer.BuildPath` + 폴더 생성 + 저장 → (옵션) UI 스레드 클립보드 → 성공 토스트(클릭 시 explorer /select). `Capture/SaveFileSystem.cs`는 `ISaveFileSystem` 실제 구현.
- `CaptureAvailability`: Screenshot을 구현됨으로 추가(한 곳), 툴팁 `현재 영역을 스크린샷으로 저장합니다`. 컨트롤 바는 기존 로직으로 [캡처] 활성화(문구 `스크린샷 찍기`).
- `App.xaml.cs`: `OnHotkey`에 `case Screenshot` 추가(workflow 호출), 트레이 "스크린샷" 활성화(단축키 표시 유지), DI 등록, `HideAppWindowIfOverlapping` 제거.
- 자동 숨김 제거: `AppWindowPolicy.cs` 삭제, `AppSettings`에서 `AutoHideAppWindow`/`ReshowAppWindowAfterRecording` 제거.
- task-02 QA 권고: 성공 토스트 본문에 실제 저장 크기(`result.Area` = 클리핑 후 크기) `W×H` 표시.

## 완료 기준별 대응
- 1 [자동]: `CaptureAvailabilityTests.Only_screenshot_is_implemented`(Screenshot true, Video/Gif false). 기존 테스트는 수정하지 않고 추가만 함(기존에 IsImplemented 기대값 테스트 없었음).
- 2 [자동]: `ScreenshotPlanTests` (Null_capture_rect_fails_with_message, Empty_capture_rect_fails, Clipboard_step_follows_the_option, Success_message..., Error_message..., Explorer_arguments_quote_paths..., FileNameOf..., Gate_refuses_overlapping_runs_until_exit).
- 3 [정적]: 세 진입점 모두 `OnHotkey(HotkeyAction.Screenshot)` → `ScreenshotWorkflow.RunAsync`. 트레이 App.xaml.cs:157, 컨트롤 바 App.xaml.cs:300(`ActionFor(mode)`→Screenshot), 단축키 HotkeyService→OnHotkey, 분기 App.xaml.cs:250. default 분기 주석은 RecordVideo/RecordGif만 남김.
- 4 [정적]: `ScreenshotWorkflow.cs` RunCoreAsync에서 `region.CaptureRect` 사용. 숨김 호출·렌더 대기 없음.
- 4-1 [정적/자동]: src·tests grep 결과 `HideAppWindowIfOverlapping`, `AppWindowPolicy`, `AutoHideAppWindow`, `ReshowAppWindow...`(코드 식별자)는 0건. 문자열 키 `autoHideAppWindow`/`reshowAppWindowAfterRecording`은 "옛 설정 파일 키 무시" 검증 테스트에만 남음(`CaptureModeTests.Removed_auto_hide_keys_in_an_old_file_are_ignored_without_warnings`, `Unknown_value_or_missing_key_restores_Screenshot` 한 케이스). 이 테스트가 요구되어 grep 0건 기준과 충돌하므로 의도된 잔존임. 수동 토글 `App.ToggleAppWindow`, 트레이 "창 표시/숨김" 유지.
- 4-2: 아래 교체/제거 목록.
- 5 [정적]: `ScreenshotWorkflow.Save`가 `CaptureFileNamer.BuildPath`(저장 폴더 `Environment.ExpandEnvironmentVariables`로 %USERPROFILE% 확장) 사용. App에 파일명 로직 중복 없음.
- 6 [정적]: `TryCopyToClipboard`는 `plan.CopyToClipboard`(설정 `capture.copyToClipboard`)일 때만 호출. 토스트 클릭 → `RevealInExplorer` (`explorer.exe`, `/select,"경로"`, UseShellExecute=false).
- 7 [정적]: 캡처 실패(ScreenshotService가 로그), 저장 실패/예기치 못한 예외 모두 `Log.Error`/`Log.Warning` + 오류 토스트. 빈 catch 없음.
- 8 [정적]: 새 오버레이 창 없음, WH_KEYBOARD_LL/FFmpeg 없음.
- 9 [CI]: 성공 (아래).

## 제거·교체한 코드/테스트 (사유는 모두 SPEC 변경 2ca6603: 앱 창 자동 숨김 삭제)
- 코드 제거: `App.HideAppWindowIfOverlapping` (App.xaml.cs), `src/WinCapture.Core/Geometry/AppWindowPolicy.cs` 파일 전체, `AppSettings.AutoHideAppWindow`/`ReshowAppWindowAfterRecording`.
- 테스트 제거: `HotkeyPlanTests.cs`의 `AppWindowPolicyTests` 클래스 전체(Overlapping_AutoHideOn_Hides, Overlapping_AutoHideOff_DoesNotHide, NotOverlapping_DoesNotHide, HiddenWindowOrNoRegion_DoesNotHide) — 겹침 판정 로직 삭제로 대상 소멸.
- 테스트 수정: `SettingsStoreTests` 기본값 테스트에서 두 키 assert 2줄 제거, `RoundTrip_ChangedValues`에서 두 키 대입 2줄 제거.
- 테스트 교체: `CaptureModeTests.Other_settings_survive_an_unknown_captureMode`는 `AutoHideAppWindow` 대신 `SaveFolder` 보존을 검증하도록 교체. `Unknown_value_or_missing_key_restores_Screenshot`의 InlineData 하나는 두 옛 키를 함께 넣은 JSON으로 변경(여전히 Screenshot 복원·백업 없음 검증).
- 테스트 추가: `Removed_auto_hide_keys_in_an_old_file_are_ignored_without_warnings`(옛 키가 있어도 로드 성공, 경고 로그 0, 손상 백업 없음).
- 그 외 기존 테스트 변경 없음. `HotkeyPlanTests.cs`의 미사용 `using WinCapture.Core.Geometry` 제거.
- 남겨 둔 것(범위 밖): `RegionInteraction.Overlaps`(자체 테스트 있음), `WindowInterop.GetWindowBounds`(FixedSizeWindow가 사용). Overlaps는 이제 프로덕션 호출처가 없을 수 있음 — 필요하면 별도 정리.

## 내린 판단과 이유
- 연타는 "직렬화" 대신 "무시"(SingleFlightGate). 캡처 중 추가 호출은 로그만 남기고 버림.
- 기존 정적 `ScreenshotService`(task-02)는 그대로 두고, 파이프라인은 새 클래스 `ScreenshotWorkflow`로 둠(이름 충돌 회피). `CaptureToFileAsync`는 이제 미사용(삭제하지 않음).
- 토스트 문구: 제목 `스크린샷 저장됨`, 본문 `파일명 (W×H) · 클릭하면 폴더에서 보기`.
- 클립보드는 PNG 바이트를 UI 스레드에서 디코딩해 `Clipboard.SetImage`. 실패 시 경고 토스트(파일 저장은 성공 유지).

## 테스트 결과
- 맥 build: 성공 (오류 0, 경고 0, --no-incremental 포함)
- 맥 test: 217개 통과
- CI: 성공 (run 45953362794, 커밋 bcc7574)
- 커밋 bcc7574, 푸시 성공

## Windows 확인 필요
- F-09-4 [수동]: 400×300 영역 지정 후 [캡처]/Ctrl+Alt+S → PNG가 정확히 400×300이고 테두리 없음.
- F-09-5 [수동]: 성공 토스트 클릭 → 탐색기가 열리고 파일이 선택됨(공백 있는 경로 포함).
- F-09-6 [수동]: 설정 파일 `capture.copyToClipboard`를 true로 두고 촬영 → 클립보드에 이미지(그림판 붙여넣기). false면 변경 없음.
- F-08-4 [수동]: 메인 창이 영역 안에 보이는 채로 찍으면 결과에 창이 나오고, ToggleAppWindow로 숨긴 뒤 찍으면 안 나옴.
- 전체 화면 상태(테두리 없음)에서 [캡처]·Ctrl+Alt+S·트레이 "스크린샷"이 커서 모니터 전체를 저장하는지.
- 연타(Ctrl+Alt+S 빠르게 여러 번)해도 파일 1개만 생기고 UI가 멈추지 않는지. 저장 폴더가 없을 때 자동 생성되는지.
- 영역이 화면 밖으로 걸친 경우 토스트의 W×H가 실제 저장 크기인지.

## 못 한 것 / 질문
- 없음
