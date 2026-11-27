# task-05 리뷰 1회차
## 판정: 통과
## 실행 결과
- 맥 build: 성공 (경고 0, 오류 0)
- 맥 test: 91/91 통과
- CI: 성공 (run 45941368488, 구현 커밋 162e2cc). 보고서 커밋 3093740의 run 45941476558은 확인 시점에 진행 중(문서만 변경)
## 기준별 판정
- 기준 1 [정적]: 충족. SelectionOverlayWindow.cs:16 `: OverlayWindowBase`, 생성자에서 base(allowActivation: true). 캡처 제외는 OverlayWindowBase.OnSourceInitialized에서 옵션과 무관하게 적용(diff상 해당 호출 미변경). LAYERED/TOOLWINDOW/Topmost도 유지.
- 기준 2 [정적]: 충족. Normalize(OnMouseUp), IsValidRegion(RegionCoordinator.CommitSelection), DpiConverter 변환, FormatSizeLabel 모두 Core 호출. App에는 가상 화면 원점 가감만 있음.
- 기준 3 [정적]: 충족. 저장 RegionCoordinator.SaveLastRegion(RestoreLastRegion 조건, 예외 시 log 콜백 -> Serilog Warning), 복원 RegionCoordinator.Restore + App.xaml.cs OnStartup 블록(try/catch, Log.Error).
- 기준 4 [자동]: 충족. RegionCoordinatorTests 14개: Commit_Null_Esc_KeepsPreviousRegionAndSettings, Commit_TooSmall_KeepsPreviousRegion, Commit_Valid_ReplacesAndSaves, Commit_RestoreOff_DoesNotSave, Commit_SaveFailure_DoesNotThrow_AndIsLogged, Restore_* 등. 기존 테스트 변경/삭제 없음(추가만).
- 기준 5 [CI]: 충족 (구현 커밋 기준 성공, 새 경고 없음).
- 기준 6 [수동]: Windows 확인 항목으로 이관.
## 특별 확인 사항
1. ClearRegion 사용자 결정: 충족. RegionCoordinator.Clear()가 State.ClearRegion() 후 RestoreLastRegion 설정과 무관하게 LastRegion을 0,0,0,0으로 저장. 테스트 Clear_RemovesRegionAndEmptiesSavedLastRegion_ThenNoRestore가 저장값 0,0,0,0과 재시작 후 Restore가 false임을 검증. (트레이/단축키 연결은 후속 작업이므로 범위 밖.)
2. 상속/캡처 제외: 위 기준 1과 동일, 우회 없음.
3. allowActivation 예외 판단: 양립 가능하다고 판단. SPEC F-06-2/TECH-RULES 2번의 NOACTIVATE는 테두리처럼 상주하는 오버레이가 사용자 작업 포커스를 빼앗지 않게 하려는 취지이고, 선택 오버레이는 사용자가 호출해 드래그가 끝날 때까지만 떠 있는 모달 성격이며 Esc 입력(F-04-2)에 키보드 입력이 필수다. 예외는 기반 클래스 한 곳, 기본값 false(다른 오버레이는 영향 없음), 캡처 제외·LAYERED·TOOLWINDOW·Topmost 유지, 사용 곳은 SelectionOverlayWindow 하나뿐이다. 다만 SPEC F-06-2의 문구는 "모든 오버레이"에 NOACTIVATE를 요구하는 것으로 읽힐 수 있어 문서와 코드가 문자상 어긋난다. 사용자 결정이 필수는 아니며 차단 사유가 아니다. 권고: 진행자가 사용자에게 "선택 오버레이는 NOACTIVATE 예외"를 알리고 원하면 SPEC/TECH-RULES에 한 줄 반영(문서 수정은 사용자 지시 시에만).
4. 복원 시 보정 후 32px 검사: 충족. Restore가 ClampToVirtualScreen 후 IsValidRegion을 검사(RegionCoordinator.cs). 테스트 Restore_OffscreenRegion_IsClampedIntoVirtualScreen, Restore_ShrunkBelowMinimumByVirtualScreen_IsNotRestored.
## 차단 이슈
없음
## Windows 확인 항목 (사용자에게 넘김)
- F-04-1: 트레이 "영역 지정" 클릭 -> 전 모니터 딤(검정 40%), 드래그 영역은 원본 밝기, 크기 라벨 `800 × 600`, 크로스헤어 커서.
- F-04-2: 영역이 있는 상태에서 다시 지정 후 Esc, 또는 32px 미만 드래그 -> 이전 영역/테두리 유지.
- 음수 좌표 모니터/150% 모니터 혼합에서 선택 영역과 확정 후 테두리가 일치하는지.
- 선택 오버레이가 Win+Shift+S 캡처에 찍히지 않는지, Esc가 즉시 먹는지(포커스 획득), 선택 후 이전 앱 포커스에 문제 없는지.
- 영역 지정 후 앱 재시작 시 테두리 복원, restoreLastRegion 끄면 복원 안 됨.
## 권고 사항
- 선택 오버레이를 닫은 뒤 이전 활성 창으로 포커스가 돌아오는지 Windows에서 확인 필요(문제 시 후속 작업).
- 혼합 DPI에서 창 DPI 단일 배율 변환 방식이라 모니터별 배율이 다르면 오차 가능성이 있어 위 수동 확인이 중요하다.
