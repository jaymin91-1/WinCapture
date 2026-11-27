# task-04 리뷰 1회차
## 판정: 통과
## 실행 결과
- 맥 build: 성공 (경고 0, 오류 0)
- 맥 test: 217/217 통과
- CI: 코드 커밋 bcc7574 성공 (run 45953362794). 보고서 커밋 3f80960 run 45953581053은 검증 시점에 진행 중(코드 변경 없는 문서 커밋)
## 기준별 판정
- 1 [자동]: 충족 (CaptureAvailabilityTests.Only_screenshot_is_implemented: Screenshot true, Video/Gif false. 기존 테스트는 추가만, 수정 없음)
- 2 [자동]: 충족 (ScreenshotPlanTests: null/빈 영역 실패, 클립보드 on/off, 메시지, 탐색기 인자, SingleFlightGate)
- 3 [정적]: 충족. 트레이 App.xaml.cs:157 -> OnHotkey(Screenshot), 컨트롤 바 App.xaml.cs:300 -> ActionFor(mode)=Screenshot -> OnHotkey, 단축키 -> OnHotkey. 모두 App.xaml.cs:250-253 case Screenshot -> ScreenshotWorkflow.RunAsync 한 곳. default 분기(257-)에는 RecordVideo/RecordGif만 남음
- 4 [정적]: 충족. ScreenshotWorkflow.RunCoreAsync가 region.CaptureRect를 ScreenshotPlan.Decide로 전달. 숨김 호출·렌더 대기(Delay/Sleep) 없음
- 4-1 [정적/자동]: 충족. grep 결과 HideAppWindowIfOverlapping, AppWindowPolicy, AutoHideAppWindow 식별자 0건. 문자열 키 autoHideAppWindow/reshowAppWindowAfterRecording은 CaptureModeTests.cs:48, :67에만 남음. 이는 "옛 키 무시" 검증 [자동] 테스트(Removed_auto_hide_keys_in_an_old_file_are_ignored_without_warnings)가 요구하는 것이라 허용 범위. 수동 토글 ToggleAppWindow(App.xaml.cs:247, 265~), 트레이 "창 표시/숨김"(163) 유지
- 4-2 [자동]: 충족. 보고서에 제거·교체 목록 있음. 제거/교체는 AppWindowPolicyTests 4개, 설정 키 assert 대입, AutoHide 기반 assert 교체뿐이며 모두 SPEC 변경(2ca6603)에 한정. 그 외 기존 테스트 삭제·skip·기대값 변경 없음(git diff 확인)
- 5 [정적]: 충족. ScreenshotWorkflow.Save가 CaptureFileNamer.BuildPath 사용, Environment.ExpandEnvironmentVariables 주입으로 %USERPROFILE% 확장, 폴더 생성 포함. App에 이름 로직 중복 없음
- 6 [정적]: 충족. plan.CopyToClipboard일 때만 TryCopyToClipboard(UI 스레드), 실패 시 경고 토스트. 토스트 클릭 -> RevealInExplorer -> explorer.exe /select,"경로"(인용은 ScreenshotMessages.ExplorerSelectArguments, 단위 테스트 있음)
- 7 [정적]: 충족. 캡처 실패/저장 실패/예기치 못한 예외 모두 Log + 오류 토스트. 빈 catch 없음 (explorer 실행 실패는 Log.Warning)
- 8 [정적]: 충족. 새 오버레이 창 없음, WH_KEYBOARD_LL/FFmpeg 코드 없음(grep). Core는 WPF/Win32 참조 없음
- 9 [CI]: 충족 (bcc7574 성공, 새 경고 없음 - 맥 빌드 경고 0)
## 차단 이슈
없음
## 특별 확인 답변
- 연타 "무시" 구현: 작업 파일 범위에 "무시하거나 직렬화한다"로 둘 다 허용되어 있어 기준 위반 아님. SingleFlightGate는 Interlocked로 동시 진입을 막고 finally에서 해제, 단위 테스트 있음. 사용자 관점에서 한 장 저장 중 두 번째 호출이 버려지는 것은 중복 방지 동작으로 합리적이며, 캡처가 수십~수백 ms라 연속 두 장을 기대하는 상황은 드묾. SPEC에 연타 동작 규정이 없어 SPEC 변경 필요 없음.
## Windows 확인 항목 (사용자에게 넘김)
- F-09-4 [수동]: 400×300 영역 지정 후 [캡처]/Ctrl+Alt+S -> PNG가 정확히 400×300이고 테두리가 없는지
- F-09-5 [수동]: 성공 토스트 클릭 -> 탐색기에서 파일이 선택되는지(공백 포함 경로 포함)
- F-09-6 [수동]: settings.json의 capture.copyToClipboard를 true로 두고 촬영 -> 그림판 붙여넣기로 확인, false면 클립보드 변화 없음
- F-08-4 [수동]: 메인 창이 영역 안에 보이는 채 촬영하면 결과에 나오고, ToggleAppWindow로 숨긴 뒤 촬영하면 안 나오는지
- 전체 화면 상태에서 [캡처]·Ctrl+Alt+S·트레이 "스크린샷"이 커서 모니터 전체를 저장하는지
- Ctrl+Alt+S 연타 시 UI가 멈추지 않고 파일이 중복 생성되지 않는지, 저장 폴더가 없을 때 자동 생성되는지
- 영역이 화면 밖으로 걸칠 때 토스트의 W×H가 실제 저장 크기인지
- 옛 설정 파일(autoHideAppWindow 키 포함)로 실행해도 정상 시작되는지
## 권고 사항
- RegionInteraction.Overlaps는 프로덕션 호출처가 없어졌을 가능성이 있음. 별도 정리 시 자체 테스트와 함께 처리 검토
- ScreenshotService.CaptureToFileAsync(task-02)가 미사용. 추후 정리 검토
- 성공 토스트에서 클립보드 성공 여부는 알리지 않음(실패 시에만 경고). 필요하면 SPEC 확인
