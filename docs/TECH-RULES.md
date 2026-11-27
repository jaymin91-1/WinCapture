# WinCapture 기술 규칙 (TECH-RULES)

> coder와 qa가 반드시 지키는 기술 규칙이다. 기능 요구사항은 `docs/SPEC.md`, 세션 계획은 `docs/SESSIONS.md`를 따른다.
> 이 문서와 SPEC이 충돌하면 SPEC이 우선하며, 충돌을 보고서에 적는다.

## 프로젝트 개요
Windows 트레이 상주형 화면 캡처 도구.
- 전역 단축키(조합키)로 캡처 영역을 지정하면 영역 바깥에 **빨간 테두리**가 항상 위에 표시된다.
- 단축키로 해당 영역의 스크린샷(PNG), 동영상(MP4), GIF를 지정 폴더에 저장한다.
- **테두리와 앱의 모든 오버레이 UI는 캡처 결과물에 절대 포함되지 않아야 한다.** (최우선 요구사항)
- 개인용 프로젝트이며 GitHub 공개 저장소로 관리한다.

## 기술 스택
- **.NET 10 (LTS)**, C#, **WPF**
  - .NET 8은 2026-11 지원 종료이므로 사용하지 않는다.
- UI: **WPF-UI (lepoco/wpfui)** — 컨트롤 기반으로 사용. 시스템 라이트/다크 테마 자동 추종. 색·배경은 SPEC 3장의 흰색/파랑 팔레트(Mica 사용 안 함)
- 트레이: **H.NotifyIcon.Wpf**
- MVVM: **CommunityToolkit.Mvvm**
- DI/호스팅: **Microsoft.Extensions.Hosting**
- 로깅: **Serilog** (파일 싱크, `%LocalAppData%\WinCapture\logs`)
- 테스트: **xUnit** + FluentAssertions
- 인코딩: **FFmpeg 외부 프로세스 호출** (바이너리는 저장소에 커밋하지 않음)
- NuGet 패키지는 작업 시점의 최신 안정 버전을 사용한다.

## 솔루션 구조
```
WinCapture.sln
src/
  WinCapture.App/          # net10.0-windows, WPF (View, ViewModel, 트레이, 오버레이 창)
  WinCapture.Core/         # net10.0 (플랫폼 무관): 설정 모델, 파일명 생성, 좌표 계산, FFmpeg 인자 빌더, 단축키 모델
  WinCapture.Native/       # net10.0-windows: Win32 P/Invoke 래퍼 (RegisterHotKey, SetWindowDisplayAffinity, BitBlt, DPI)
tests/
  WinCapture.Core.Tests/   # net10.0, xUnit. Core만 참조한다 (맥에서도 실행 가능해야 함)
scripts/
  get-ffmpeg.ps1           # FFmpeg 설치 안내/다운로드 스크립트 (Windows용)
.github/workflows/
  ci.yml                   # windows-latest에서 build + test
docs/
  SPEC.md  SESSIONS.md  TECH-RULES.md
```
- 순수 로직은 `Core`에 두어 맥에서도 단위 테스트가 돌게 한다. `Core`는 WPF, Win32, `System.Drawing`을 참조하지 않는다.
- `App`에 비즈니스 로직을 넣지 않는다. 계산은 `Core`에서 하고 App은 호출만 한다.
- P/Invoke 선언은 `Native`에만 둔다. 가능하면 `LibraryImport` 소스 생성기를 사용한다.
- 모든 오버레이 창은 공통 기반 클래스 `OverlayWindowBase`를 상속하고, 캡처 제외와 창 스타일 적용은 이 클래스 한 곳에서만 한다.

## 반드시 지켜야 할 기술 제약
1. **캡처 제외**: 테두리 창, 영역 선택 오버레이, 녹화 타이머 등 앱이 띄우는 모든 오버레이 창에 `SetWindowDisplayAffinity(hwnd, WDA_EXCLUDEFROMCAPTURE)`를 적용한다. 추가로 테두리는 **캡처 영역 바깥쪽**에 그린다(이중 안전장치).
2. **오버레이 창 스타일**: `WS_EX_LAYERED | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE`, Topmost. 포커스 탈취 금지, 작업표시줄/Alt+Tab 미노출.
   - 테두리 창은 **영역 내부만 클릭 통과**시키고(`WM_NCHITTEST`에서 `HTTRANSPARENT`), **테두리 띠(히트 영역 약 8px)와 우하단 리사이즈 그립**은 마우스 입력을 받는다. 테두리 띠 드래그 = 영역 이동, 우하단 그립 드래그 = 크기 조절.
   - 테두리 띠와 그립은 캡처 영역 **바깥**에 있으므로 조작해도 캡처 결과에 영향을 주지 않는다.
   - **녹화 중에는 이동/리사이즈를 잠근다**(FFmpeg 캡처 좌표가 고정되어 있으므로).
3. **앱 창이 캡처 영역을 가리지 않아야 한다**: 설정 창 등 앱의 일반 창은 단축키로 숨김/표시를 토글할 수 있어야 하고, 캡처/녹화 시작 시 앱 창이 영역과 겹치면 자동으로 숨기는 옵션을 제공한다.
4. **DPI**: 앱 매니페스트에 **Per-Monitor V2** DPI 인식을 선언한다. 내부 좌표는 항상 **물리 픽셀(가상 화면 좌표)**로 통일하고, WPF DIP와의 변환은 한 곳(`Core`의 변환 유틸)에서만 한다.
5. **멀티 모니터**: 가상 화면 좌표는 음수가 될 수 있다. 영역이 여러 모니터에 걸쳐도 동작해야 한다.
6. **짝수 해상도**: 동영상 인코딩 시 폭/높이를 짝수로 보정한다(yuv420p 요구사항).
7. **단축키**: `RegisterHotKey`(조합키)만 사용한다. 저수준 키보드 훅(`WH_KEYBOARD_LL`)은 사용하지 않는다. 등록 실패(다른 앱이 선점) 시 사용자에게 알리고 설정에서 변경하도록 안내한다. **모든 전역 단축키는 설정 창의 '단축키' 탭에서 변경할 수 있어야 하며, 저장 즉시 재등록되어 재시작 없이 반영된다.**
8. **FFmpeg**: 녹화 종료는 프로세스 kill이 아니라 **stdin에 `q`를 써서 정상 종료**시킨다(파일 손상 방지). 경로는 설정값 → 앱 폴더 `ffmpeg\ffmpeg.exe` → PATH 순으로 탐색한다.
9. **UI 스레드**: 캡처/인코딩은 백그라운드에서 수행하고 UI를 블로킹하지 않는다.

## 저장소/라이선스 규칙 (GitHub 공개)
- 앱 소스 라이선스는 **MIT**.
- **FFmpeg 바이너리, 빌드 산출물, 캡처 결과물, 개인 설정 파일을 커밋하지 않는다.** `.gitignore`에 `bin/ obj/ ffmpeg/ *.mp4 *.gif *.png(테스트 리소스 제외) publish/`를 포함한다.
- README에 FFmpeg는 별도 설치가 필요하다는 점(`winget install Gyan.FFmpeg` 또는 `scripts/get-ffmpeg.ps1`)과 FFmpeg 라이선스(LGPL/GPL)는 FFmpeg 측에 귀속된다는 점을 명시한다.
- 비밀값, 개인 경로, 사용자명을 코드나 문서에 하드코딩하지 않는다.

## 빌드 / 실행 / 테스트
맥 미니(에이전트)에서:
```bash
dotnet build WinCapture.sln -c Debug      # WPF 프로젝트는 EnableWindowsTargeting=true로 빌드
dotnet test tests/WinCapture.Core.Tests
```
Windows(사용자 PC, CI)에서:
```powershell
dotnet build WinCapture.sln -c Debug
dotnet test
dotnet run --project src/WinCapture.App
dotnet publish src/WinCapture.App -c Release -r win-x64 --self-contained false -o publish
```
- App과 Native 프로젝트에 `<EnableWindowsTargeting>true</EnableWindowsTargeting>`를 설정한다.
- 맥에서는 앱을 **실행하지 않는다**(실행 불가). 맥에서 WPF 빌드가 도구 문제로 실패하면 임의로 우회하지 말고 보고서에 적는다. 이때는 Windows CI 결과가 빌드 판정 기준이 된다.
- **판정 기준이 되는 빌드는 GitHub Actions(windows-latest) CI다.** push 후 `gh run watch` 또는 `gh run list --limit 3`으로 결과를 확인한다.
- 오류 0개. 경고는 새로 늘리지 않는다. Nullable 참조 형식을 활성화한다(`<Nullable>enable</Nullable>`).

## 코딩 컨벤션
- C# 최신 언어 버전, file-scoped namespace, `async`/`await` 사용(`.Result`/`.Wait()` 금지).
- ViewModel은 `[ObservableProperty]`, `[RelayCommand]` 소스 생성기를 사용한다.
- Win32 핸들/GDI 리소스는 반드시 해제한다(`SafeHandle` 또는 `using`).
- 예외는 삼키지 말고 로그를 남기고, 사용자에게 필요한 것만 알린다(토스트/트레이 알림).
- 코드 주석은 영어로 쓴다. 커밋 메시지는 루트 CLAUDE.md 형식을 따른다.

## 작업 방식
- 작업 분할, 구현, 검증 절차는 루트 `CLAUDE.md`(진행자 규칙)와 `.claude/agents/`의 coder·qa 규칙을 따른다.
- UI 동작(오버레이, 캡처 제외, DPI)은 맥에서 실행할 수 없으므로, SPEC 완료 기준의 [수동] 항목은 세션 종료 시 사용자가 Windows에서 확인한다.
- 요구사항이 모호하면 추측하지 말고 보고서의 "질문"에 적는다.
