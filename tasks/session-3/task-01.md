# task-01: 스크린샷 파일명 생성과 저장 경로 (Core)
## 목표
Core에 파일명 패턴 치환, 중복 회피, 저장 경로 조합 로직을 만들고 SPEC 숫자 예시로 단위 테스트한다.
## 관련 SPEC
F-09 완료 기준 1~3, 7장(`saveFolder`, `useTypeSubfolders`, `fileNamePattern`), F-02(경로 주입 가능)
## 범위
- Core(플랫폼 무관, System.Drawing/Win32 금지): `CaptureFileNamer`(이름은 자유) 등
  - 패턴 토큰 치환: `{date}`=`yyyyMMdd`, `{time}`=`HHmmss`, `{w}`, `{h}`, `{type}`(png 등 확장자 문자열), `{counter}`
  - 파일명에 쓸 수 없는 문자(`\ / : * ? " < > |`)는 `_`로 치환(패턴 결과 전체에 적용)
  - 같은 이름이 있으면 `_1`, `_2` … (확장자 앞). 존재 여부 확인은 주입 가능한 함수/인터페이스로 받아 테스트가 실제 디스크 없이 돈다
  - 저장 경로: `useTypeSubfolders`가 켜지면 `<저장폴더>\Screenshots\파일명`, 꺼지면 `<저장폴더>\파일명`. `%USERPROFILE%` 등 환경변수 확장은 주입 가능한 함수로 처리(Core가 OS에 의존하지 않게)
  - 시각·저장 폴더 확장은 모두 매개변수로 받는다(`DateTime.Now` 직접 호출 금지)
- `{counter}`의 의미는 SPEC에 정의가 없다. 구현 전 아래 "결정 필요"를 확인하고, 사용자 결정이 없으면 **보고서의 질문에 적고 임시 정의(해당 저장 폴더에서 같은 패턴으로 이미 존재하는 파일 수 + 1이 아니라, 단순히 1부터 시작하는 중복 회피 번호와 별개로 항상 `1`)를 쓰지 말고** 진행자에게 확인을 요청한다.
## 범위 밖
- 실제 파일 저장, 캡처, UI, 동영상/GIF 하위 폴더 이름(Videos/Gifs는 세션 4·5)
## 완료 기준
1. [자동] 패턴 `WinCapture_{date}_{time}`, 시각 2026-11-27 09:30:05, 유형 png → `WinCapture_20261127_093005.png` (F-09-1)
2. [자동] 같은 이름이 이미 있으면 `_1`, 그것도 있으면 `_2`가 확장자 앞에 붙는다 (F-09-2)
3. [자동] `{w}`, `{h}`, `{type}`, `{counter}`가 치환된다. 예: 패턴 `shot_{w}x{h}_{type}` , 400×300, png → `shot_400x300_png.png`  (F-09-2)
4. [자동] 금지 문자 `\ / : * ? " < > |`가 모두 `_`로 바뀐다. 예: `a:b*c` → `a_b_c.png` (F-09-2)
5. [자동] `useTypeSubfolders=true`면 `<저장폴더>\Screenshots\파일명`, false면 `<저장폴더>\파일명` (F-09-3)
6. [정적] Core가 `DateTime.Now`, `File.Exists`, 환경변수를 직접 호출하지 않고 주입받는다 (TECH-RULES)
7. [CI] windows-latest build/test 성공, 새 경고 없음
## 검증 방법
qa가 테스트 존재·SPEC 숫자 사용 확인, 맥 build/test, CI 확인.
