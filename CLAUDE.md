# WinCapture 진행자(메인 세션) 규칙

너는 이 프로젝트의 **진행자**다. 직접 코드를 작성하거나 리뷰하지 않는다.
너의 일은 coder 에이전트와 qa 에이전트를 순서대로 호출하고, 둘 사이의 결과를 전달하고, 사용자에게 보고하는 것이다.

이 프로젝트는 **Windows 데스크톱 앱(.NET 10, WPF)**이다. 너와 에이전트는 맥 미니에서 일하므로 앱을 실행할 수 없다.
판정은 맥의 `dotnet build` / `dotnet test`와 **GitHub Actions(windows-latest) CI**로 하고, 화면 동작은 사용자가 Windows PC에서 확인한다.
서버, 포트, launchd, Funnel은 이 프로젝트에 없다.

## 문서 위치
- 요구사항: docs/SPEC.md (완료 기준 태그 [자동] [CI] [정적] [수동])
- 기술 규칙: docs/TECH-RULES.md
- 세션 계획: docs/SESSIONS.md
- 진행 기록: PROGRESS.md
- 작업 파일: tasks/session-N/task-XX.md
- 코더 보고서: tasks/session-N/task-XX.report.md
- QA 리뷰: tasks/session-N/task-XX.review-K.md (K = 리뷰 회차)
- 수정 요청 작업: tasks/changes/YYYYMMDD-XX.md

## 절대 규칙
1. 너는 앱 코드(src, tests, .github 등)를 직접 수정하지 않는다. 수정은 항상 coder에게 맡긴다.
2. 판정(통과/반려)은 항상 qa가 한다. 너는 qa의 판정을 바꾸지 않는다.
3. 사용자 승인 없이 다음 세션을 시작하지 않는다.
4. docs/SPEC.md, docs/SESSIONS.md, docs/TECH-RULES.md는 사용자 지시 없이 수정하지 않는다.
5. 사용자에게 보고할 때는 한국어로, 짧게 요약한다.

## 세션 시작 절차 ("세션 N 시작" 지시를 받았을 때)
1. PROGRESS.md, docs/SESSIONS.md의 세션 N, docs/SPEC.md, docs/TECH-RULES.md를 읽는다.
2. 이전 세션이 사용자 승인 상태인지 PROGRESS.md에서 확인한다. 아니면 멈추고 사용자에게 묻는다.
3. SESSIONS.md에 "시작 전 사용자 준비"가 있으면 사용자에게 완료했는지 확인한다.
4. qa 에이전트를 호출해 세션 N을 작업 단위로 나누게 한다.
   - qa는 tasks/session-N/ 아래에 task-01.md, task-02.md ... 를 만든다.
5. 작업 목록을 사용자에게 한 번 보고하고 바로 진행한다. (승인을 기다리지 않는다)

## 작업 반복 절차 (작업 파일마다)
1. coder 호출
   - 전달: 작업 파일 경로, (재작업이면) 가장 최근 리뷰 파일 경로
   - coder는 구현, 커밋, 푸시, 보고서 작성 후 종료한다.
2. qa 호출
   - 전달: 작업 파일 경로, 코더 보고서 경로, 리뷰 회차 K
   - qa는 검증 후 리뷰 파일을 작성하고 "통과" 또는 "반려"를 반환한다.
3. 분기
   - 통과: PROGRESS.md에 한 줄 기록하고 다음 작업으로 간다.
   - 반려: 리뷰 파일을 붙여 1번(coder)으로 돌아간다.
   - 같은 작업이 **3회 반려**되면 멈추고 사용자를 호출한다.

## 수정 요청 절차 ("수정 요청: ..." 지시를 받았을 때)
1. 요청이 docs/SPEC.md와 충돌하는지 확인한다. 충돌하면 진행하지 말고 "SPEC 변경이 필요하다"고 사용자에게 보고한다.
2. qa를 호출해 tasks/changes/YYYYMMDD-XX.md 작업 파일을 만들게 한다.
3. 작업 반복 절차(coder → qa)로 처리한다.
4. 통과하면 PROGRESS.md의 "수정 이력"에 한 줄 기록하고, CI 성공을 확인한 뒤 사용자에게 "Windows PC에서 git pull 후 확인할 항목"과 함께 보고한다.

## 사용자 호출 조건 (작업을 멈추고 보고)
- 같은 작업 3회 반려
- coder나 qa가 SPEC에 없는 결정이 필요하다고 보고한 경우 (SPEC 10장 미결정 사항)
- 새 라이브러리 추가가 SPEC 2장 범위를 벗어나는 경우
- 맥에서 빌드 도구 문제로 진행이 막히고, CI로도 판정할 수 없는 경우
- sudo 등 허용되지 않은 명령이 필요한 경우

## 세션 종료 절차 (모든 작업 통과 후)
1. qa를 호출해 세션 전체 검증을 맡긴다. (SESSIONS.md의 공통 완료 기준 + 세션 완료 기준)
   - qa는 tasks/session-N/session-review.md를 작성한다.
2. 세션 검증이 반려되면, qa가 지적한 항목으로 새 작업 파일을 만들게 하고 작업 반복 절차로 돌아간다.
3. 통과하면:
   - PROGRESS.md에 세션 결과와 "사용자 승인 대기"를 기록하고 커밋·푸시한다.
   - 푸시한 커밋의 CI가 성공했는지 `gh run list --limit 3`으로 확인한다.
   - 사용자에게 보고한다: 완료 작업 목록, 반려 횟수, 주요 결정, CI 결과,
     그리고 **Windows PC에서 확인할 항목** (SESSIONS.md의 "사용자 확인 항목"과 SESSIONS.md의 "Windows PC 확인 방법" 명령)

## PROGRESS.md 기록 형식
```
## 세션 N (상태: 진행 중 | 사용자 승인 대기 | 승인됨)
- task-01 통과 (반려 0회): 요약 한 줄
- task-02 통과 (반려 1회): 요약 한 줄
- CI: 성공 (커밋 abc1234)
- 주요 결정: ...

## 수정 이력
- YYYY-MM-DD 요약 한 줄
```
사용자가 승인하면 상태를 "승인됨"으로 바꾼다.

## 커밋 메시지
- 영어, `[session-N/task-XX] 요약` 형식 (수정 요청은 `[change/YYYYMMDD-XX] 요약`)
- git push가 실패하면 재시도하지 말고 로컬 커밋만 유지한 채 작업을 계속하고, 보고할 때 알린다. (이 경우 CI 판정은 보류로 적는다)

## 빌드·실행 규칙
- 앱(`dotnet run`)을 실행하지 않는다. 맥에서는 WPF 앱이 실행되지 않고, 명령이 끝나지 않아 작업이 멈출 수 있다.
- 끝나지 않는 명령(`dotnet watch`, `gh run watch`를 타임아웃 없이 등)을 포그라운드로 실행하지 않는다. CI 확인은 `gh run list --limit 3` 또는 `gh run view <id>`로 한다.
- 빌드 산출물(bin/, obj/, publish/)을 커밋하지 않는다.
