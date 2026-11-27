# task-03 리뷰 1회차
## 판정: 통과
## 실행 결과
- 맥 build: 성공 (경고 0, 오류 0)
- 맥 test: 73/73 통과
- CI: 성공 (run 45940554582, 코드 커밋 6f46b41). 보고서 커밋 baf678d의 run 45940699176은 확인 시점에 진행 중이었으나 문서만 변경된 커밋이다.
## 기준별 판정
- 1 [자동] F-07-1: 충족 (HitTest_SpecExamples, 4개 SPEC 점 그대로)
- 2 [자동] F-07-2: 충족 (Move_ClampsToVirtualScreen, X=1700 -> 1520, Y=-50 -> 0, 음수 가상 화면 좌측 경계 -1920 포함)
- 3 [자동] F-07-3: 충족 (Resize_EnforcesMinimumSize 10x10 -> 32x32 환산, Resize_StopsAtVirtualScreenEdge)
- 4 [자동] F-07-4: 충족 (Locked_ReturnsOriginalRegion, 이동과 리사이즈 모두)
- 5 [자동] F-08-1: 충족 (Overlaps_SpecExamples, 3개 SPEC 값 그대로)
- 6 [자동] 그립/띠 겹침: 충족 (HitTest_GripWinsWhereItOverlapsBand)
- 7 [정적]: 충족. RegionInteraction.cs는 System.Globalization만 사용, WPF/Win32/System.Drawing 참조 없음. Rect, PixelPoint 재사용 (RegionInteraction.cs:25-46 등)
- 8 [CI]: 충족 (코드 커밋 6f46b41 success)
- 기존 테스트 변경: 없음 (커밋 6f46b41은 신규 파일 2개와 보고서만 추가)
## coder 판단 3건 검토
1. 그립 12px가 띠(8px) 밖까지 리사이즈로 판정: SPEC F-07 "그립은 꼭짓점에서 오른쪽·아래로 12px, 겹치는 부분은 그립 우선"과 일치. 사용자 결정 불필요.
2. 경계에 붙어 32px가 불가하면 최소 크기 우선(RegionInteraction.cs:ClampSize): SPEC "최소 32x32"와 "경계에서 멈춘다"가 충돌하는 극단 경우의 해석이다. 정상 흐름(영역 생성 시 경계 안에 위치)에서는 발생하기 어렵고 SPEC 위반이 아니다. 사용자 결정 불필요. 다만 이 경우 영역이 가상 화면을 최대 몇 px 벗어날 수 있다.
3. 영역이 가상 화면보다 크면 좌상단 정렬(ClampPosition): SPEC에 규정 없는 극단 경우이며 합리적 기본 동작. 사용자 결정 불필요.
## 차단 이슈
없음
## Windows 확인 항목 (사용자에게 넘김)
- 이번 작업은 Core 순수 함수만 있어 직접 확인 항목 없음. F-07 기준 5, 6 [수동](커서, 툴팁, 녹화 중 잠금 안내)은 task-06 이후 UI 연결 시 확인.
## 권고 사항
- Resize는 델타 입력이라 task-06에서 마우스 시작점 기준 델타를 일관되게 넘겨야 한다.
- 최소 크기 우선으로 경계를 벗어나는 극단 경우에 대한 테스트를 추가하면 동작이 고정된다.
- Move의 Y 음수 경계, Resize 잠금 외 경계 케이스는 현재 SPEC 예시 위주라 보강 여지가 있다.
