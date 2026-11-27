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

## 세션 2 (상태: 진행 중)
- task-01 통과 (반려 0회): Core 단축키 모델(파싱·직렬화·충돌 검사), CI actions checkout v7 / setup-dotnet v6 (Node 20 경고 해소)
- task-02 통과 (반려 0회): Core Rect·DPI 변환·드래그 정규화·테두리 사각형·lastRegion 복원과 가상 화면 보정(최소 이동/초과 시 축소)
- task-03 통과 (반려 0회): Core 히트 테스트(그립 우선)·이동/리사이즈 제한·녹화 중 잠금·창-영역 겹침·툴팁 형식
- task-04 통과 (반려 0회): Native LibraryImport 래퍼, OverlayWindowBase(캡처 제외·창 스타일 단일 지점), BorderWindow(영역 바깥 테두리·그립, 내부 HTTRANSPARENT)
- task-05 통과 (반려 0회): SelectionOverlayWindow(딤·크기 라벨·Esc 취소), RegionCoordinator(선택 적용·복원·ClearRegion), 트레이 '영역 지정' 연결. 선택 오버레이만 NOACTIVATE 예외(allowActivation, 캡처 제외 유지)
- 주요 결정: ClearRegion은 저장된 lastRegion도 비운다 (사용자 결정 A, 2026-11-27)

## 수정 이력
- 2026-11-27 설정 창·트레이 메뉴 스타일을 흰색/파랑으로 변경하기로 결정 (목업 승인, SPEC 3장·TECH-RULES 갱신)
- 2026-11-27 [change/20261127-01] 설정 창·트레이 메뉴 흰색/파랑 스타일 적용, Mica 제거, 트레이 단축키 표시 (반려 0회)
- 2026-11-27 [change/20261127-02] 트레이 아이콘(파랑 둥근 사각형+흰 점)과 설정 창 탭 선 아이콘 6개 교체 (반려 0회)
