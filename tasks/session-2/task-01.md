# task-01: Core 단축키 모델과 충돌 검사 (F-03) + CI Node.js 20 경고 해소
## 목표
단축키 문자열 파싱/직렬화/유효성/충돌 검사를 Core에 구현하고 테스트한다. 세션 1 리뷰 권고인 CI 액션 deprecation 경고도 함께 해소한다.
## 관련 SPEC
F-03 완료 기준 1~4, 7장(hotkeys 기본값), TECH-RULES 7번
## 범위
- Core: `HotkeyAction` 열거(SPEC 동작 ID 7개: SelectRegion, Screenshot, RecordVideo, RecordGif, ToggleBorder, ClearRegion, ToggleAppWindow), `Modifiers`(Ctrl/Alt/Shift/Win 플래그), `KeyCombo`(수정자 + 키)
- 파싱(`TryParse`)과 문자열화(수정자 순서 항상 Ctrl, Alt, Shift, Win), 유효성(수정자 1개 이상 + 일반 키 정확히 1개, 키는 A–Z, 0–9, F1–F24), 대소문자·공백 관용
- 충돌 검사: 동작→조합 맵을 받아 같은 조합을 쓰는 동작 ID 그룹을 모두 반환
- 기본값 7개를 기존 `AppSettings` 기본 hotkeys 문자열에서 모델로 변환할 수 있어야 한다(설정 파일 형식은 문자열 유지, 변경 금지)
- Win32 가상 키 코드/RegisterHotKey 수정자 비트로의 매핑 값은 Core의 순수 값 계산으로만 제공 가능(P/Invoke 금지). 선택 사항
- CI: `.github/workflows/ci.yml`의 actions/checkout, actions/setup-dotnet을 Node 24 런타임을 쓰는 최신 메이저 버전으로 올려 Node.js 20 deprecation 경고를 없앤다(해당 시점의 최신 안정 메이저 확인). 워크플로 나머지 동작은 바꾸지 않는다
## 범위 밖
- RegisterHotKey 호출, 서비스, 설정 UI(세션 6)
- 설정 JSON 스키마 변경
## 완료 기준
1. [자동] `"Ctrl+Alt+R"` 파싱 → 수정자 {Ctrl, Alt}, 키 R, 다시 문자열화하면 `"Ctrl+Alt+R"`. `"Alt+Ctrl+R"` 입력 → `"Ctrl+Alt+R"` 출력 (F-03-1)
2. [자동] `"R"`, `"Ctrl+Alt"`, `"Ctrl+A+B"`는 유효하지 않다고 판정. 추가로 빈 문자열, F13/F24 같은 허용 키는 유효 (F-03-2)
3. [자동] 두 동작에 같은 조합을 지정하면 충돌 검사가 두 동작 ID를 모두 반환 (F-03-3)
4. [자동] 기본값 7개를 모델로 변환해 충돌 검사하면 충돌 없음 (F-03-4)
5. [정적] Core가 WPF/Win32/System.Drawing을 참조하지 않고 `WH_KEYBOARD_LL` 관련 코드가 없다
6. [정적] ci.yml에서 checkout/setup-dotnet 버전이 올라가 있고 나머지 단계(build, test)는 동일하다
7. [CI] 최신 커밋의 windows-latest build/test 성공, 실행 로그에 Node.js 20 deprecation 경고가 더 이상 없다 (`gh run view --log` 또는 annotations 확인)
## 검증 방법
qa가 맥에서 `dotnet test tests/WinCapture.Core.Tests` 실행, SPEC 문자열이 테스트에 그대로 쓰였는지 확인, `gh run view <id>`로 경고 여부 확인. 기존 테스트가 변경되지 않았는지 git diff 확인.
