# task-02 작업 보고서
## 한 일
- `src/WinCapture.Core/Geometry/Rect.cs`: `PixelPoint`, `PixelSize`, `Rect`(정수, 물리 픽셀), `DipRect`(double, DIP 경계 전용)
- `src/WinCapture.Core/Geometry/DpiConverter.cs`: DIP<->물리 변환 단일 지점(AwayFromZero, X/Y/W/H 각각 독립 반올림)
- `src/WinCapture.Core/Geometry/RegionMath.cs`: 드래그 정규화, 32px 판정, 테두리 바깥 사각형, 설정<->Rect 변환, 복원값 제공, 가상 화면 보정
- `tests/WinCapture.Core.Tests/RegionTests.cs`: 테스트 추가
## 완료 기준별 대응
- 1 [자동]: `Normalize_AllFourDirections` (4방향, 기대값 100,100,400,300)
- 2 [자동]: `IsValidRegion_MinimumIs32` (31 무효, 32x32 유효)
- 3 [자동]: `Restore_On_ReturnsSavedRegion_AndSurvivesSaveLoad`, `Restore_Off_ReturnsNull`, 크기 0은 `Restore_ZeroSize_ReturnsNull`/`Restore_DefaultSettings_ReturnsNull`
- 4 [자동]: `Dpi_150Percent_ConvertsAndRoundTrips`
- 5 [자동]: `Dpi_125Percent_RoundsEachComponent`
- 6 [자동]: `Dpi_100Percent_NegativeCoordinates`
- 7 [정적]: `src/WinCapture.App/app.manifest:12-14` (`dpiAwareness` = `PerMonitorV2, PerMonitor`). 변경 없음.
- 8 [자동]: `BorderOuterRect_GrowsByThickness_AndDoesNotCoverRegion` (98,98,404,304; 안쪽 inset == 영역)
- 9 [정적]: Core csproj는 참조 없음, Geometry는 System만 사용(`WinCapture.Core.Settings`만 참조).
- 10 [CI]: 성공
- 11 [수동]: 아래 참조
## 내린 판단과 이유
- 변환은 X/Y/Width/Height를 각각 독립 반올림한다. SPEC 125% 예시(401*1.25=501.25 -> 501)와 일치시키기 위함이다.
- `Rect`는 `System.Windows`와 이름이 같지만 Core 네임스페이스(`WinCapture.Core.Geometry`)이며 App에서는 별칭이 필요할 수 있다.
- 크기 0(너비 또는 높이 <= 0) 영역은 "복원할 영역 없음"(null). 사용자 결정(ClearRegion이 0,0,0,0 저장)을 반영.
- 복원 시 32px 미만 유효성은 검사하지 않는다(SPEC에 없음). 필요하면 호출측에서 `IsValidRegion` 사용.
- default(KeyCombo)는 사용하지 않았다(이 작업과 무관).
### 가상 화면 보정 규칙 (`RegionMath.ClampToVirtualScreen`)
1. 너비/높이가 가상 화면보다 크면 가상 화면 크기로 줄인다.
2. 영역이 완전히 안에 있지 않으면(일부 벗어남, 전혀 안 겹침 포함) 최소 이동량으로 안쪽에 들어오게 평행 이동한다. 겹치지 않는 경우 가장 가까운 모서리로 붙는다.
3. 이미 안에 있으면 그대로 둔다.
- 가상 화면은 모니터 전체를 감싸는 하나의 사각형으로 취급한다(모니터 사이 빈 공간은 고려하지 않음). 모니터 열거는 task-05 범위.
## 테스트 결과
- 맥 build: 성공 (경고 0, 오류 0)
- 맥 test: 60개 통과
- 커밋: be5aad0, 푸시 성공
- CI: 성공 (run 45940260904, 커밋 be5aad0)
## Windows 확인 필요
- F-05 기준 5 [수동]: 100%/150% 모니터에서 테두리가 선택 영역과 픽셀 단위로 일치하는지 (계산부는 이 작업, 실제 창은 이후 task에서 연결)
## 못 한 것 / 질문
- 없음
