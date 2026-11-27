# task-03 작업 보고서
## 한 일
- App에 Generic Host(DI) + Serilog 파일 로그 + H.NotifyIcon 트레이 + 이름 있는 Mutex 단일 인스턴스 + Per-Monitor V2 매니페스트 구현.
- 기존 MainWindow를 삭제하고 빈 `SettingsWindow`(task-04가 내용 구현)로 대체. StartupUri 제거, `ShutdownMode=OnExplicitShutdown`.
- 새 패키지(모두 TECH-RULES 스택 범위): H.NotifyIcon.Wpf 2.4.1, Microsoft.Extensions.Hosting 11.0.0, Serilog 4.4.0, Serilog.Sinks.File 7.0.0, Serilog.Extensions.Hosting 11.0.0 (Host와 Serilog 연결용).
- task-02 QA 권고 반영: `JsonSettingsStore`의 log 콜백에 Serilog(`Log.Warning`) 주입.
## 완료 기준별 대응
- 1 [정적]: `src/WinCapture.App/SingleInstanceGuard.cs` 9행 MutexName, `Acquire()`의 `new Mutex(true, MutexName, out createdNew)`. App.xaml.cs OnStartup에서 호출.
- 2 [정적]: `src/WinCapture.App/app.manifest`(dpiAwareness `PerMonitorV2, PerMonitor`), `WinCapture.App.csproj`의 `<ApplicationManifest>app.manifest</ApplicationManifest>`.
- 3 [정적]: `App.xaml.cs` `CreateTrayIcon()` — 영역 지정/스크린샷/동영상 녹화 시작·정지/GIF 녹화 시작·정지/창 표시·숨김/저장 폴더 열기(모두 IsEnabled=false), 설정(Click -> ShowSettingsWindow), 종료(Click -> ExitApplication). 동영상/GIF/창 항목은 시작·정지, 표시·숨김을 한 항목에 "/"로 표기(상태 전환 구현 전).
- 4 [정적]: `App.xaml.cs` `ConfigureLogging()` — LocalApplicationData\WinCapture\logs, RollingInterval.Day, retainedFileCountLimit 7. `ConfigureServices`에서 `AddSingleton<ISettingsStore>`(경로 `JsonSettingsStore.GetDefaultPath()` = %AppData%\WinCapture\settings.json). 하드코딩 경로 없음. 시작 시 `Load()` 호출. 처리되지 않은 예외(Dispatcher/AppDomain/TaskScheduler)는 로그.
- 5 [CI]: run 45920240799 성공 (커밋 a8fdd22).
- 6, 7 [수동]: 아래 참조.
## 내린 판단과 이유
- 두 번째 실행 신호 방식: 이름 있는 EventWaitHandle(`Local\WinCapture.SingleInstance.Activate`, AutoReset). 파이프보다 코드가 짧고 서버/포트가 필요 없으며, 두 인스턴스 모두 생성-또는-열기 생성자로 경쟁 상태 없이 동작한다. 첫 인스턴스는 백그라운드 스레드가 이벤트를 기다리다 Dispatcher로 설정 창을 연다. 종료 시 stop 이벤트로 스레드 정리.
- 트레이 아이콘은 이진 파일 없이 XAML DrawingImage(어두운 둥근 사각형 + 흰 모서리 괄호 + 빨간 점)로 새로 그린 중립 아이콘. 트레이 메뉴는 좌/우클릭 모두 열림(`LeftOrRightClick`).
- 메뉴는 코드로 구성(XAML 리소스 방식의 핸들러 연결 위험 회피).
- 설정 창은 닫으면 파괴되고 다시 열 때 새로 만든다(트레이 상주는 ShutdownMode로 유지).
## 테스트 결과
- 맥 build: 성공 (오류 0, 경고 0)
- 맥 test: 11개 통과
- CI: 성공 (run 45920240799, 커밋 a8fdd22)
- 커밋 a8fdd22, 푸시 성공
## Windows 확인 필요
- F-01 1 [수동]: 실행(`dotnet run --project src/WinCapture.App`)하면 트레이 아이콘이 보이고, 우클릭 메뉴 순서가 SPEC과 같으며 "종료"로 프로세스(작업 관리자)가 사라진다. "설정"을 누르면 빈 설정 창이 열리고 창을 닫아도 앱은 유지된다.
- F-01 2 [수동]: 앱 실행 중 다시 실행하면 새 프로세스가 즉시 끝나고 기존 설정 창이 앞으로 나온다(창이 숨겨/닫혀 있어도 새로 열림).
- 로그: `%LocalAppData%\WinCapture\logs\wincapture-날짜.log` 생성 확인. DPI: 다중/고DPI 모니터에서 흐려지지 않는지(후속 작업에서 본격 확인).
## 못 한 것 / 질문
- 없음
