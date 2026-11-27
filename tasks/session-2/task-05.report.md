# task-05 작업 보고서
## 한 일
- Core: `RegionCoordinator` (선택 결과 적용/복원/ClearRegion 규칙, 설정 저장, 크기 라벨 `800 × 600`) 추가. `src/WinCapture.Core/Geometry/RegionCoordinator.cs`
- App: `SelectionOverlayWindow : OverlayWindowBase`(가상 화면 전체 딤 40% + 선택부 구멍 + 흰 외곽선 + 크기 라벨 + 크로스헤어), `SelectionController`(트레이에서 호출, 중복 열기 방지)
- `OverlayWindowBase`에 생성자 옵션 `allowActivation`(기본 false) 추가
- App.xaml.cs: DI 등록, 시작 시 복원(`Restore`), 트레이 "영역 지정" 항목 활성화
- 테스트 `RegionCoordinatorTests` 14개 추가

## 완료 기준별 대응
- 1 [정적]: `SelectionOverlayWindow.cs` 클래스 선언이 `OverlayWindowBase` 상속. 캡처 제외는 `OverlayWindowBase.OnSourceInitialized`에서 항상 적용(옵션과 무관).
- 2 [정적]: 정규화 `RegionMath.Normalize`(OnMouseUp), 좌표 변환 `DpiConverter.ToPhysical/ToDip/ScaleFromDpi`(ToScreen, ToDipRect), 유효성 `RegionMath.IsValidRegion`은 `RegionCoordinator.CommitSelection`. 라벨 문자열도 Core(`FormatSizeLabel`). App에 계산식 없음(가상 화면 원점 더하기/빼기 제외).
- 3 [정적]: 저장 `RegionCoordinator.SaveLastRegion`(RestoreLastRegion 조건, 예외는 log 콜백 -> App.xaml.cs DI에서 Serilog Warning). 복원 `RegionCoordinator.Restore`, 호출 `App.xaml.cs` OnStartup의 "Restore the last region" 블록(try/catch + Log.Error).
- 4 [자동]: `RegionCoordinatorTests` — Commit_Null_Esc_KeepsPreviousRegionAndSettings, Commit_TooSmall_KeepsPreviousRegion, Commit_Valid_ReplacesAndSaves, Commit_RestoreOff_DoesNotSave, Restore_OffscreenRegion_IsClampedIntoVirtualScreen, Restore_ShrunkBelowMinimumByVirtualScreen_IsNotRestored, Clear_RemovesRegionAndEmptiesSavedLastRegion_ThenNoRestore 등.
- 5 [CI]: 아래 참조.
- 6 [수동]: 아래 "Windows 확인 필요".

## 내린 판단과 이유
- Esc 입력을 받으려면 포커스가 필요해서 선택 오버레이만 `WS_EX_NOACTIVATE`를 제외(`allowActivation: true`: NOACTIVATE 미적용, ShowActivated/Focusable true, WM_MOUSEACTIVATE 기본 처리). LAYERED/TOOLWINDOW/Topmost/캡처 제외는 유지. 예외 지점은 기반 클래스 한 곳. SPEC F-06-2 "NOACTIVATE" 문구의 의도적 예외이므로 QA 확인 바람.
- 레이어드 창에서 알파 0 영역은 마우스가 통과하므로 루트 Grid 배경을 alpha 1/255로 두어 드래그 입력을 받는다.
- 혼합 DPI: 창 DPI 단일 배율로 DIP<->물리 변환(창은 `SetBounds`로 가상 화면 물리 크기에 맞추고 DPI 변경 시 재적용).
- ClearRegion은 사용자 결정대로 `RestoreLastRegion` 설정과 무관하게 lastRegion을 0,0,0,0으로 저장. `Clear()`는 Core에 구현했으나 트레이/단축키 연결은 범위 밖(후속 작업).
- 복원은 ClampToVirtualScreen 후 IsValidRegion 검사(task-02 권고). 복원된 보정값은 설정에 다시 쓰지 않음.
- 소형 드래그/Esc 시 선택 오버레이만 닫히고 이전 영역·테두리는 그대로.

## 테스트 결과
- 맥 build: 성공 (경고 0, 오류 0, --no-incremental)
- 맥 test: 91개 통과
- CI: 푸시 후 확인(아래 갱신)

## Windows 확인 필요
- F-04-1 [수동]: 트레이 "영역 지정" 클릭 -> 전 모니터에 딤, 드래그 영역이 원본 밝기, 크기 라벨 `800 × 600` 표시, 크로스헤어 커서. 음수 좌표 모니터/150% 모니터에서 영역과 테두리가 일치하는지.
- F-04-2 [수동]: 영역이 있을 때 다시 지정 후 Esc, 또는 32px 미만 드래그 -> 이전 영역/테두리 유지.
- 선택 오버레이가 Win+Shift+S 캡처에 안 찍히는지, Esc가 즉시 먹는지(포커스 획득).
- 영역 지정 후 앱 재시작 시 테두리 복원.

## 못 한 것 / 질문
- 없음 (ClearRegion 호출 경로는 후속 작업)
