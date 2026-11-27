# task-01 작업 보고서
## 한 일
- Core에 `Hotkeys/HotkeyAction.cs`(7개 열거), `Hotkeys/KeyCombo.cs`(Modifiers 플래그, KeyCombo, TryParse/IsValid/ToString, VirtualKey), `Hotkeys/HotkeyConflicts.cs`(충돌 그룹 검색, HotkeySettings -> 모델 변환) 추가
- Modifiers 값은 RegisterHotKey 비트(Alt=1, Ctrl=2, Shift=4, Win=8)와 동일, VirtualKey는 순수 계산(P/Invoke 없음)
- `.github/workflows/ci.yml`: actions/checkout v4->v7, actions/setup-dotnet v4->v6 (둘 다 action.yml에서 node24 확인). 나머지 단계 동일
- 테스트 `tests/WinCapture.Core.Tests/HotkeyTests.cs` 추가 (기존 테스트 무변경)
## 완료 기준별 대응
- 1 [자동]: Parse_CtrlAltR_RoundTrips, Parse_ModifierOrderIsNormalized ("Alt+Ctrl+R" -> "Ctrl+Alt+R")
- 2 [자동]: Invalid_Rejected ("R", "Ctrl+Alt", "Ctrl+A+B", 빈 문자열 등), Valid_Accepted (F13, F24 등)
- 3 [자동]: Conflict_SameComboReturnsBothActions
- 4 [자동]: Defaults_ConvertAndHaveNoConflict
- 5 [정적]: Core/Hotkeys/*.cs는 System 네임스페이스만 사용, WH_KEYBOARD_LL 없음
- 6 [정적]: .github/workflows/ci.yml 9~10행 버전만 변경
- 7 [CI]: run 45939928530 success, 로그에 "Node.js 20" 문자열 0건, annotation 없음
## 내린 판단과 이유
- 허용 별칭: "Control", "Windows"도 수정자로 인정(관용). 출력은 항상 Ctrl/Alt/Shift/Win
- 중복 수정자("Ctrl+Ctrl+A"), "F01" 같은 0 패딩, Space 등 SPEC 밖 키는 무효 처리
- 충돌 그룹 내 동작 ID는 열거 순서로 정렬
## 테스트 결과
- 맥 build: 성공 (경고 0, 오류 0)
- 맥 test: 33개 통과
- 커밋: b2c5b54, 푸시 성공
- CI: 성공 (run 45939928530, 커밋 b2c5b54)
## Windows 확인 필요
- 없음 (수동 항목 없음)
## 못 한 것 / 질문
- 없음
