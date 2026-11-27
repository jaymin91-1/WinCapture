# WinCapture 진행 기록

## 세션 0 (상태: 승인됨)
- 프로젝트 생성, SPEC·SESSIONS·TECH-RULES 작성 (하네스 템플릿을 Windows WPF용으로 변형)

## 세션 1 (상태: 승인됨)
- task-01 통과 (반려 0회): 솔루션·프로젝트 4개·스모크 테스트·Windows CI·LICENSE 구성
- task-02 통과 (반려 0회): Core 설정 모델(SPEC 7장)과 JSON 저장소, 깨진 파일 백업, 테스트 10개
- task-03 통과 (반려 0회): Generic Host·DI·Serilog, 트레이 메뉴 골격, Mutex 단일 인스턴스(재실행 신호: 이름 있는 EventWaitHandle), Per-Monitor V2 매니페스트
- task-04 통과 (반려 0회): WPF-UI FluentWindow 설정 창(Mica, 탭 6개 껍데기, X=숨김), README 초안
- 세션 검증: 통과 (tasks/session-1/session-review.md)
- CI: 성공 (코드 최종 커밋 6ad8878 및 이후 문서 커밋 ad817f1까지 success)
- 주요 결정: 재실행 신호는 이름 있는 EventWaitHandle, saveFolder는 %USERPROFILE% 문자열 저장 후 사용 시 확장, 트레이 토글 항목은 "/" 표기

## 수정 이력
- 2026-11-27 설정 창·트레이 메뉴 스타일을 흰색/파랑으로 변경하기로 결정 (목업 승인, SPEC 3장·TECH-RULES 갱신)
- 2026-11-27 [change/20261127-01] 설정 창·트레이 메뉴 흰색/파랑 스타일 적용, Mica 제거, 트레이 단축키 표시 (반려 0회)
