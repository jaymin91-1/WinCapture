# task-02 리뷰 1회차
## 판정: 통과
## 실행 결과
- 맥 build: 성공 (경고 0, 오류 0)
- 맥 test: 60/60 통과
- CI: 코드 커밋 be5aad0 성공 (run 45940260904). 보고서 커밋 c03e889 run은 문서만 변경, 확인 시점에 진행 중.
## 기준별 판정
- 1 [자동]: 충족 (RegionTests.Normalize_AllFourDirections, 4방향, 기대값 100,100,400,300)
- 2 [자동]: 충족 (IsValidRegion_MinimumIs32, 31 무효, 32x32 유효)
- 3 [자동]: 충족 (Restore_On_ReturnsSavedRegion_AndSurvivesSaveLoad는 JsonSettingsStore 저장/로드 후 비교, Restore_Off_ReturnsNull, Restore_ZeroSize_ReturnsNull, Restore_DefaultSettings_ReturnsNull)
- 4 [자동]: 충족 (Dpi_150Percent_ConvertsAndRoundTrips)
- 5 [자동]: 충족 (Dpi_125Percent_RoundsEachComponent, 126,126,501,376)
- 6 [자동]: 충족 (Dpi_100Percent_NegativeCoordinates)
- 7 [정적]: 충족 (src/WinCapture.App/app.manifest:12-13, PerMonitorV2)
- 8 [자동]: 충족 (BorderOuterRect_GrowsByThickness_AndDoesNotCoverRegion, 98,98,404,304, 안쪽 inset == 영역)
- 9 [정적]: 충족 (Core csproj에 참조 없음, Geometry는 System과 Core.Settings만 사용)
- 10 [CI]: 충족 (be5aad0)
- 11 [수동]: 판정 안 함, 아래로 이관
- 기존 테스트 변경 없음 (tests 변경은 RegionTests.cs 신규 추가만).
## 차단 이슈
없음
## 가상 화면 보정 규칙 타당성 (RegionMath.cs ClampToVirtualScreen)
- 타당함. 크기 초과 시 축소, 그 외 최소 이동 후 Math.Clamp로 안쪽 배치, 이미 안이면 불변. min<=max가 항상 보장되어 Clamp 예외 없음. 겹치지 않는 경우도 가장 가까운 모서리로 붙어 사용자가 영역을 잃지 않음. 테스트 4종(안, 비겹침, 부분 벗어남, 초과)이 이를 검증.
- 한계(허용 가능): 가상 화면을 하나의 외곽 사각형으로 취급하므로 모니터 사이 빈 공간에 걸친 영역은 보정되지 않음. 실제 모니터 열거는 task-05에서 다룸.
- 보정 후 32px 미만 검사는 하지 않음. 호출측(task-05)에서 IsValidRegion 사용 권장.
## Windows 확인 항목 (사용자에게 넘김)
- F-05 기준 5 [수동]: 100%/150% 모니터에서 테두리가 선택 영역과 픽셀 단위로 일치하는지 (실제 창 연결은 이후 task)
## 권고 사항
- Core의 Rect가 System.Windows.Rect와 이름이 같음. App에서 using 별칭 필요. 참고용.
- Dpi 테스트는 ToDip 역변환을 150%에서만 검증. 충분하나 125%에서 역변환 오차는 반올림 손실이 있음을 호출측이 인지할 것.
