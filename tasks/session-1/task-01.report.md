# task-01 작업 보고서
## 한 일
- WinCapture.sln, src/WinCapture.Core(net10.0), Native(net10.0-windows), App(net10.0-windows, WPF 기본 껍데기), tests/WinCapture.Core.Tests(xUnit + FluentAssertions 8.11.0) 생성
- 참조: App->Core,Native / Native->Core / Tests->Core. Directory.Build.props로 Nullable, ImplicitUsings, LangVersion 설정
- Core에 AppInfo(상수 클래스) 추가, 스모크 테스트가 이를 참조
- .github/workflows/ci.yml, MIT LICENSE 추가. .gitignore는 기존 파일이 이미 기준을 만족하여 변경 없음
## 완료 기준별 대응
- 기준 1 [자동]: SmokeTests.AppInfo_Name_IsWinCapture (Core의 AppInfo 참조), 맥에서 1개 통과
- 기준 2 [정적]: src/WinCapture.Core/WinCapture.Core.csproj (WPF/windows TFM 없음), tests csproj는 Core만 ProjectReference. Native/App csproj에 EnableWindowsTargeting=true, Nullable=enable
- 기준 3 [정적]: /.gitignore (bin/ obj/ publish/ ffmpeg/ *.mp4 *.gif *.png + 테스트 리소스 예외), /LICENSE MIT. 산출물 미커밋 확인
- 기준 4 [CI]: run 45919712822 성공 (커밋 aa9a0f1)
- 기준 5 [정적]: .github/workflows/ci.yml (windows-latest, push/pull_request, build + test, setup-dotnet 10.0.x)
## 내린 판단과 이유
- FluentAssertions는 지시대로 최신 안정(8.11.0) 사용.
- 테스트 단계는 CI에서 Core.Tests만 실행(TECH-RULES 명령과 동일).
## 테스트 결과
- 맥 build: 성공 (경고 0, 오류 0). 맥 WPF 빌드 도구 문제 없음
- 맥 test: 1개 통과
- CI: 성공 (run 45919712822, 커밋 aa9a0f1)
- 푸시: 성공 (origin/main)
## Windows 확인 필요
- 없음
## 못 한 것 / 질문
- 없음
