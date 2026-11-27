# task-03 리뷰 1회차
## 판정: 통과
## 실행 결과
- 맥 build: 성공 (경고 0, 오류 0)
- 맥 test: 11/11 통과 (tests/ 변경 없음, 기존 테스트 삭제/skip/기대값 변경 없음)
- CI: 구현 커밋 a8fdd22 성공 (run 45920240799, windows-latest). 보고서 커밋 0193df1의 run 45920372645는 확인 시점에 진행 중이었으나 문서만 변경된 커밋이다.
## 기준별 판정
- 기준 1 [정적]: 충족. src/WinCapture.App/SingleInstanceGuard.cs:9 MutexName, :30 `new Mutex(true, MutexName, out var createdNew)`. App.xaml.cs:24-32에서 호출, 두 번째 실행은 즉시 Shutdown. 신호는 이름 있는 EventWaitHandle(:31-35, 수신 :48-55).
- 기준 2 [정적]: 충족. src/WinCapture.App/app.manifest:13 `PerMonitorV2, PerMonitor`, WinCapture.App.csproj:9 `<ApplicationManifest>app.manifest</ApplicationManifest>`.
- 기준 3 [정적]: 충족. App.xaml.cs:108-121 순서가 SPEC 4장과 동일(영역 지정/스크린샷/동영상/GIF/창 표시·숨김/저장 폴더 열기/설정/종료). 미구현 6개는 IsEnabled=false, 설정(:116)과 종료(:120)에 Click 핸들러 연결. ShutdownMode=OnExplicitShutdown (App.xaml:4), StartupUri 없음.
- 기준 4 [정적]: 충족. App.xaml.cs:90-99 LocalApplicationData\WinCapture\logs, RollingInterval.Day, retainedFileCountLimit 7. :52 `AddSingleton<ISettingsStore>` (JsonSettingsStore.GetDefaultPath()), :69 시작 시 Load. 하드코딩된 사용자명/개인 경로 없음. 미처리 예외 로그 :35-46.
- 기준 5 [CI]: 충족 (a8fdd22 성공). 경고 0.
- 기준 6, 7 [수동]: 판정 안 함, 아래로 이관.
## 차단 이슈
없음
## 추가 확인
- 새 패키지(H.NotifyIcon.Wpf, Microsoft.Extensions.Hosting, Serilog, Serilog.Sinks.File, Serilog.Extensions.Hosting)는 TECH-RULES 스택 범위 내. WH_KEYBOARD_LL, FFmpeg, 오버레이 관련 코드 없음, Core 변경 없음.
- bin/obj/바이너리 커밋 없음. 이진 아이콘 없이 XAML DrawingImage 사용(타 프로그램 로고 아님).
## Windows 확인 항목 (사용자에게 넘김)
- F-01 기준 1 [수동]: 앱을 실행해 트레이 아이콘이 나타나는지, 우클릭/좌클릭 메뉴 순서가 SPEC과 같고 미구현 항목이 회색인지, "종료"로 작업 관리자에서 프로세스가 사라지는지 확인.
- F-01 기준 2 [수동]: 실행 중 한 번 더 실행하면 새 프로세스가 바로 끝나고 기존 인스턴스의 설정 창이 앞으로 나오는지 확인(설정 창을 닫은 상태에서도 열리는지).
- "설정" 메뉴로 빈 창이 열리고 닫아도 앱이 트레이에 남는지 확인.
- %LocalAppData%\WinCapture\logs\wincapture-날짜.log 생성 확인.
## 권고 사항
- Mutex/이벤트 이름이 `Local\` 접두사라 세션 단위 단일 인스턴스다. 단일 사용자 도구이므로 문제 없음.
- 트레이 메뉴가 기본 WPF ContextMenu라 다크 테마와 어긋날 수 있다. task-04 이후 검토.
- 예외 로그에서 `Log.Fatal(args.ExceptionObject as Exception, ...)`는 null일 수 있으나 무해.
