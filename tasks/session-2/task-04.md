# task-04: Native P/Invoke 래퍼, OverlayWindowBase, 테두리 창 (F-06, F-05 정적)
## 목표
캡처 제외·창 스타일이 한 곳에 모인 `OverlayWindowBase`와, 영역 바깥에 그려지는 빨간 테두리 창을 만든다.
## 관련 SPEC
F-06 완료 기준 1, 2, 5(수동), 3장 디자인 원칙(테두리 `#FF3B30` 2px, 바깥쪽), TECH-RULES 1, 2, 4번
## 범위
- Native(`LibraryImport`): `SetWindowDisplayAffinity`(WDA_EXCLUDEFROMCAPTURE), `Get/SetWindowLongPtr`(확장 스타일), 모니터 DPI·가상 화면 조회, 필요한 상수. P/Invoke는 Native에만 둔다
- App: `OverlayWindowBase`(Window 상속). 창 핸들 생성 직후(`SourceInitialized`) `SetWindowDisplayAffinity(WDA_EXCLUDEFROMCAPTURE)` 호출, 실패 시 Serilog 로그. 확장 스타일 `WS_EX_LAYERED | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE`, Topmost, ShowInTaskbar=false, WindowStyle=None, AllowsTransparency, 포커스 탈취 금지. 이후 모든 오버레이 창은 이 클래스만 상속(서브클래스에서 캡처 제외·스타일을 따로 설정하지 않는다)
- App: `BorderWindow`: 영역(물리 픽셀) 입력 → task-02의 테두리 사각형 계산으로 배치(DIP↔물리 변환은 Core 유틸만 사용), 설정의 색·두께 사용, 영역 내부는 `WM_NCHITTEST`에서 `HTTRANSPARENT`(클릭 통과), 띠·그립 히트 처리는 task-06에서 하므로 이 작업은 통과/비통과 영역 구조만 준비
- 우하단 그립 12×12 삼각형을 영역 바깥에 그린다(테두리와 같은 색)
- 영역 설정/해제/표시 토글을 위한 간단한 서비스 또는 컨트롤러(상태: 영역 유무, 테두리 표시 여부). 아직 단축키/선택 UI와는 연결하지 않아도 된다(task-05, 07)
## 범위 밖
- 선택 오버레이, 이동/리사이즈 처리, 점멸·타이머 알약(세션 4), 단축키 연결
## 완료 기준
1. [정적] `OverlayWindowBase`가 `SourceInitialized`(또는 핸들 생성 직후)에서 `SetWindowDisplayAffinity(hwnd, WDA_EXCLUDEFROMCAPTURE)`를 호출하고 실패 시 로그를 남긴다. 파일:줄 근거 (F-06-1)
2. [정적] 확장 스타일 `WS_EX_LAYERED | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE`와 Topmost가 `OverlayWindowBase` 한 곳에 있다 (F-06-2)
3. [정적] `BorderWindow`가 `OverlayWindowBase`를 상속하고, SetWindowDisplayAffinity/스타일 설정 코드가 다른 곳에 중복되지 않는다. 테두리가 영역 바깥에 그려진다(Core 계산 호출 근거)
4. [정적] P/Invoke 선언이 Native에만 있고 `LibraryImport`를 쓴다. `WH_KEYBOARD_LL` 없음
5. [정적] WM_NCHITTEST에서 영역 내부가 `HTTRANSPARENT`로 처리된다
6. [CI] windows-latest build/test 성공, 새 경고 없음
7. [수동] F-06-5 (영역 안쪽 클릭이 아래 앱에 전달되고 포커스를 뺏지 않음), F-05-5 (픽셀 일치)는 Windows 확인 항목으로 넘긴다. 임시로 테두리를 띄워볼 방법이 필요하면 보고서에 적는다
## 검증 방법
qa가 코드를 읽어 정적 기준 근거를 파일:줄로 기록하고 맥 build + CI를 확인한다. 테두리 창이 OverlayWindowBase를 상속하지 않으면 반려.
