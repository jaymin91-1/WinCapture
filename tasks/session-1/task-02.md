# task-02: Core 설정 모델과 저장소 (F-02)
## 목표
SPEC 7장 구조의 설정 모델과, 왕복·복구·알 수 없는 키 무시·경로 주입을 지원하는 설정 저장소를 Core에 구현하고 테스트한다.
## 관련 SPEC
F-02 완료 기준 1~4, SPEC 7장 (데이터 구조)
## 범위
- Core: `AppSettings`(7장의 모든 키와 기본값, `version` 1), 하위 모델(lastRegion, hotkeys, border, capture, recording, gif)
- Core: 설정 저장소 (`ISettingsStore` 등): Load / Save, 설정 파일 경로는 생성자로 주입 (기본 경로 `%AppData%\WinCapture\settings.json` 계산은 App 쪽 또는 별도 헬퍼에서 하고, 테스트는 임시 폴더 사용)
- System.Text.Json 사용 (키 이름은 7장의 camelCase 그대로). 파일이 없으면 기본값 반환
- 깨진 JSON이면 예외 없이 기본값 반환 + 원본을 `settings.json.bak-yyyyMMddHHmmss`로 보존. 시각 소스는 주입 가능하게 해서 테스트에서 고정
- 7장에 없는 키는 무시, 파일에 없는 키는 기본값으로 채움 (기존 값은 지우지 않음)
- 핫키 값은 이 작업에서는 문자열로만 보관 (파싱 모델은 세션 2)
- 테스트는 임시 폴더를 쓰고 끝나면 정리한다
## 범위 밖
- 단축키 파싱/충돌 검사, 설정 UI, DI 등록, 앱에서의 호출 (task-03)
- `%USERPROFILE%` 환경변수 확장 로직은 저장값을 문자열 그대로 유지하고, 확장은 사용 시점(후속 세션)에 한다. 이 작업에서 확장 결과를 파일에 쓰지 않는다
## 완료 기준
1. [자동] 기본 설정을 저장한 뒤 다시 읽으면 모든 값이 같다 (왕복 동일). 기본값 외의 값을 바꾼 설정으로도 왕복 테스트가 있다
2. [자동] `{"saveFolder":` 를 읽으면 예외 없이 기본값을 반환하고, `settings.json.bak-yyyyMMddHHmmss` 파일(고정 시각 기준 이름 검증)에 원본 내용이 남는다
3. [자동] 7장에 없는 키가 들어 있어도 읽기에 성공하고, 알려진 키 값은 정상으로 읽힌다
4. [자동] 설정 파일 경로가 주입 가능하고 테스트는 임시 폴더를 쓴다. 일부 키만 있는 파일을 읽으면 나머지는 기본값이다
5. [정적] 기본값이 SPEC 7장 JSON과 일치한다 (hotkeys 7개 기본 문자열, border #FF3B30/2, fps 30, gif 12/800 등)
6. [정적] Core가 WPF/Win32/System.Drawing을 참조하지 않는다. 예외를 삼키는 경우 로그 또는 보존 동작이 있다
7. [CI] 최신 커밋의 windows-latest build와 test가 성공한다
## 검증 방법
qa가 맥에서 `dotnet test tests/WinCapture.Core.Tests`를 실행하고, 위 SPEC 문자열(`{"saveFolder":`)이 테스트에 그대로 쓰였는지 확인한다. 기존 task-01 스모크 테스트가 삭제/변경되지 않았는지 git diff로 확인한다.
