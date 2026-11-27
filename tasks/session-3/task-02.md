# task-02: 화면 캡처(BitBlt)와 PNG 인코딩, 캡처 사각형 보정
## 목표
물리 픽셀 사각형을 받아 화면을 BitBlt로 캡처하고 PNG 바이트/파일로 만드는 계층을 만든다. 영역 보정 규칙은 Core에서 테스트한다.
## 관련 SPEC
F-09-4(400×300 영역 → PNG 정확히 400×300), F-05(물리 픽셀·음수 좌표), F-06(테두리·오버레이는 결과에 없음), 영역 상태(`RegionState.CaptureRect`), 7장 `capture.includeCursor`, TECH-RULES 제약 1·4·5·9
## 범위
- Core: 캡처 사각형 정규화 `CapturePlan`(이름 자유) — 입력 `Rect`(=`RegionState.CaptureRect`)와 가상 화면 `Rect`. 가상 화면과 교차 부분으로 자르고, 교차가 비면 실패(null/결과 타입). 크기는 홀수도 그대로 유지(스크린샷은 짝수 보정 불필요)
- Native: `WinCapture.Native`에 GDI `BitBlt`(SRCCOPY | CAPTUREBLT) 기반 캡처 래퍼. `LibraryImport` 사용, DC·비트맵 핸들은 `SafeHandle`/`try-finally`로 해제. 가상 화면 좌표(음수 포함)로 호출
- `includeCursor`가 true면 커서를 캡처 비트맵에 합성(영역 안에 있을 때만, 좌표는 영역 좌상단 기준). SPEC에 세부 규정이 없으므로 기본 커서 아이콘 합성 정도로 하고 보고서에 방식을 적는다
- App 또는 Native: 비트맵 → PNG 인코딩(WPF `PngBitmapEncoder` 사용 가능, 새 라이브러리 금지). 결과 PNG의 픽셀 크기는 입력 사각형과 정확히 같다(DPI 메타데이터 때문에 크기가 바뀌지 않게 96 dpi 고정)
- 캡처는 UI 스레드를 막지 않는 `Task`/`async` API로 제공한다(TECH-RULES 9). 실패는 예외를 삼키지 않고 로그 후 호출자에 결과로 전달
## 범위 밖
- 파일 저장 경로 결정(task-01), 토스트, 단축키/버튼 연결(task-04), 동영상·GIF
## 완료 기준
1. [자동] `CapturePlan`: 가상 화면 (-1920, 0, 3840, 1080), 영역 (100, 100, 400, 300) → 그대로 (100, 100, 400, 300) 반환 (F-09-4 / F-05-3 연계)
2. [자동] 가상 화면 밖으로 걸친 영역은 교차 부분으로 잘린다. 예: 가상 화면 (0, 0, 1920, 1080), 영역 (1800, 900, 400, 300) → (1800, 900, 120, 180). 완전히 밖이거나 W/H가 0이면 실패 결과
3. [자동] 음수 좌표 가상 화면에서도 올바르다. 예: 가상 화면 (-1920, 0, 3840, 1080), 영역 (-2000, 10, 200, 100) → (-1920, 10, 120, 100)
4. [정적] BitBlt 호출에서 DC/비트맵 핸들이 모든 경로(예외 포함)에서 해제된다 (파일:줄)
5. [정적] P/Invoke 선언이 `WinCapture.Native`에만 있고 Core는 System.Drawing/WPF/Win32를 참조하지 않는다 (TECH-RULES)
6. [정적] PNG 인코딩 결과의 픽셀 크기가 입력 사각형 W×H와 같도록 DPI 96 고정 등이 코드에 드러난다
7. [정적] 캡처 호출이 UI 스레드를 블로킹하지 않는다(`Task.Run` 등, `.Result`/`.Wait()` 금지)
8. [CI] windows-latest build/test 성공, 새 경고 없음
9. [수동] F-09-4 (400×300 영역 → PNG 400×300, 테두리 없음)는 task-04에서 연결 후 Windows 확인 항목으로 넘긴다
## 검증 방법
qa가 코드 읽기 + 맥 build/test + CI 확인.
