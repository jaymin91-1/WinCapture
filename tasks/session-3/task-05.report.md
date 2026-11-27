# task-05 작업 보고서
## 한 일
- 실패 알림 문구/묶기 규칙을 Core로 분리: `src/WinCapture.Core/Presentation/HotkeyFailureMessage.cs` (실패 동작 목록 -> 제목/본문, 중복 제거, enum 순서, 실패 없으면 null).
- `HotkeyService.Reapply()`가 실패 동작 전체를 한 번에 모아 토스트 1개로 알림 (`src/WinCapture.App/HotkeyService.cs:79`). 시작 시와 설정 변경 후 재등록 모두 같은 경로.
- `App.xaml.cs:109`의 풍선(`_trayIcon?.ShowNotification`)을 `IToastService.Show(ToastKind.Warning, ...)`로 교체. 풍선 호출은 소스에 더 이상 없음 (grep 확인). 트레이 아이콘은 그대로.
- 정리(task-04 QA 권고): 호출처가 없는 `RegionInteraction.Overlaps`(Core)와 `ScreenshotService.CaptureToFileAsync`(App) 제거.
## 완료 기준별 대응
- 1 [자동]: `HotkeyFailureMessageTests` (No_failures_gives_no_message, Single_failure_names_action_and_points_to_settings_hotkey_tab, Several_failures_are_grouped_into_one_message_in_enum_order)
- 2 [정적]: `App.xaml.cs:109-110` IToastService 경유, `ShowNotification` 잔존 없음
- 3 [정적]: 본문에 "설정 › 단축키 탭에서 변경해 주세요" 포함 (`HotkeyFailureMessage.cs`)
- 4 [정적]: `Statuses`, `StatusesChanged`, 상태 계산 코드 미변경. 기존 테스트 유지
- 5 [CI]: 성공 (아래)
- 6 [수동]: 아래 Windows 확인 항목
## 내린 판단과 이유
- 묶기 방식: 여러 동작이 실패해도 토스트 1개로 합침 (본문에 동작 이름 나열). 기존 풍선도 한 번에 묶던 방식이라 유지.
- 동작 이름을 enum 영문명 대신 한국어 라벨로 표시(HotkeyRows와 같은 이름). 본문에 "다른 앱이 사용 중일 수 있습니다" 한 문장 추가.
## 제거 목록
- `RegionInteraction.Overlaps` + 테스트 `Overlaps_SpecExamples`: 앱 창 자동 숨김 제거(task-04)로 사용처 소멸. 호출처는 해당 테스트뿐이었음. (`Rect.Intersects`는 유지)
- `ScreenshotService.CaptureToFileAsync`: ScreenshotWorkflow가 `CaptureAsync` + 저장 경로 로직을 직접 써서 호출처 없음.
## 테스트 결과
- 맥 build: 성공 (경고 0, 오류 0)
- 맥 test: 217개 통과
- 커밋: 519051c, 푸시 성공
- CI: 성공 (run 45953775864, 커밋 519051c)
## Windows 확인 필요
- F-03-5 [수동]: 다른 앱이 쓰는 조합(예: 같은 조합을 먼저 등록한 앱)을 가진 상태로 실행하면 우하단에 경고 토스트("단축키 등록 실패 ... 설정 › 단축키 탭에서 변경")가 1개 뜨고, 단축키 팝업에 빨간 테두리·`사용 중인 조합`이 보이는지 확인.
## 못 한 것 / 질문
- 없음
