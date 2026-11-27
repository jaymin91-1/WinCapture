# task-01: 솔루션, 프로젝트 4개, 저장소 기본 파일, CI
## 목표
TECH-RULES 구조의 솔루션과 4개 프로젝트를 만들고, Windows CI와 저장소 기본 파일(.gitignore, LICENSE)을 갖춘다.
## 관련 SPEC
F-01 완료 기준 4, F-14 완료 기준 2 (TECH-RULES 솔루션 구조, 빌드 규칙)
## 범위
- `WinCapture.sln`, `src/WinCapture.Core`(net10.0), `src/WinCapture.Native`(net10.0-windows), `src/WinCapture.App`(net10.0-windows, WPF), `tests/WinCapture.Core.Tests`(net10.0, xUnit + FluentAssertions, Core만 참조)
- 프로젝트 참조: App -> Core, Native / Native -> Core(필요 시만) / Tests -> Core. Core는 WPF, Win32, System.Drawing 미참조
- App, Native에 `EnableWindowsTargeting=true`, 모든 프로젝트 `Nullable=enable`, 경고를 새로 늘리지 않는 설정(필요하면 Directory.Build.props 사용)
- Core.Tests에 최소 1개의 스모크 테스트 (CI test 단계가 실제로 테스트를 실행하는지 확인용)
- `.github/workflows/ci.yml`: windows-latest, push와 pull_request 시 `dotnet build WinCapture.sln` + `dotnet test`, .NET 10 SDK 설정
- `.gitignore`(.NET용, bin/ obj/ ffmpeg/ publish/ *.mp4 *.gif *.png(테스트 리소스 제외)), MIT `LICENSE`
## 범위 밖
- DI, 트레이, 설정 모델, 설정 창, README, 릴리스 워크플로(F-14-3, 후속 세션)
- 앱 기능 코드 일체 (App은 빈 창/앱 껍데기 수준이면 충분)
## 완료 기준
1. [자동] `dotnet test tests/WinCapture.Core.Tests`가 맥에서 통과한다 (스모크 테스트 1개 이상, 항상 통과하는 빈 테스트가 아니라 Core의 실제 타입을 참조할 것)
2. [정적] `Core.csproj`에 WPF/Windows 참조나 `-windows` TFM이 없고, `Core.Tests`는 Core만 참조한다. App, Native에 `EnableWindowsTargeting=true`와 Nullable이 있다
3. [정적] `.gitignore`가 bin/, obj/, publish/, ffmpeg/, *.mp4, *.gif, *.png(테스트 리소스 예외)를 제외하고, `LICENSE`가 MIT이다. 빌드 산출물이 커밋에 없다
4. [CI] 푸시한 최신 커밋의 windows-latest build와 test가 성공한다
5. [정적] ci.yml이 windows-latest에서 push/pull_request로 build와 test를 실행한다
## 검증 방법
qa가 맥에서 `dotnet build WinCapture.sln`, `dotnet test tests/WinCapture.Core.Tests`를 직접 실행하고 `gh run list`로 CI를 확인한다. 맥 WPF 빌드가 도구 문제로만 실패하면 CI 결과를 기준으로 한다.
