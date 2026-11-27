# task-01 리뷰 1회차
## 판정: 통과
## 실행 결과
- 맥 build: 성공 (경고 0, 오류 0)
- 맥 test: 33/33 통과
- CI: 성공 (코드 커밋 b2c5b54 run 45939928530, 보고서 커밋 861c131 run 45940055105 모두 success). 로그에 "Node.js 20"/deprecat 문자열 0건
## 기준별 판정
- 기준 1 [자동]: 충족 (Parse_CtrlAltR_RoundTrips, Parse_ModifierOrderIsNormalized)
- 기준 2 [자동]: 충족 (Invalid_Rejected: "R", "Ctrl+Alt", "Ctrl+A+B", 빈 문자열 등 / Valid_Accepted: F13, F24)
- 기준 3 [자동]: 충족 (Conflict_SameComboReturnsBothActions)
- 기준 4 [자동]: 충족 (Defaults_ConvertAndHaveNoConflict, 기본값 7개 AppSettings.cs:77~95)
- 기준 5 [정적]: 충족 (Core/Hotkeys/*.cs는 System 네임스페이스만 사용, WH_KEYBOARD_LL/WPF/Win32/System.Drawing 참조 없음)
- 기준 6 [정적]: 충족 (.github/workflows/ci.yml 11~12행 checkout v4->v7, setup-dotnet v4->v6만 변경, build/test 단계 동일)
- 기준 7 [CI]: 충족
- 기존 테스트 변경 없음 (diff는 신규 파일 4개 추가뿐), 설정 스키마 변경 없음, 범위 밖 구현 없음
## 차단 이슈
없음
## Windows 확인 항목 (사용자에게 넘김)
- 없음 (수동 기준 없음)
## 권고 사항
- default(KeyCombo)의 Key가 null이라 ToString이 빈 문자열을 반환함(방어 처리됨). 이후 Dictionary에 default 값이 들어가지 않도록만 주의.
- VirtualKey는 유효하지 않은 default 값에서 예외 가능성이 있으므로 사용 측(세션 2 후속 작업)에서 TryParse 성공 값만 쓸 것.
