# 세션 1 전체 검증

## 판정: 통과

## 실행 결과
- 맥 build (`dotnet build WinCapture.sln`): 성공, 경고 0, 오류 0
- 맥 test (`dotnet test tests/WinCapture.Core.Tests`): 11/11 통과 (실패 0, 건너뜀 0)
- CI: 성공 (run 45920931703, 커밋 ad817f1 "Record task-04 pass in PROGRESS", windows-latest). 직전 커밋 45920910552, 45920654443도 성공.

## 공통 완료 기준
1. 맥 test 통과: 충족 (11/11)
2. 맥 build 성공: 충족
3. 최신 커밋 CI 성공: 충족 (ad817f1)
4. 이전 세션 테스트 유지: 충족 (세션 0에는 테스트 없음. task-01 스모크 테스트는 그대로 통과)
5. 테스트 삭제/skip/기대값 변경 없음: 충족 (tests 변경 이력은 신규 파일 추가뿐, 삭제/수정 커밋 없음)
6. [자동] 테스트 존재, [정적] 근거 기재: 충족 (아래)
7. 새 경고 없음, TECH-RULES 위반 없음: 충족 (빌드 경고 0. Core는 net10.0 순수 프로젝트로 참조 없음. WH_KEYBOARD_LL 사용 없음. bin/obj/ffmpeg 커밋 없음)
8. PROGRESS 기록/푸시: 현재 task-01~04 통과가 기록되어 있고 워킹트리 clean. 세션 상태 전환("사용자 승인 대기")은 진행자가 세션 종료 시 처리.
9. [수동] 항목 나열: 충족 (아래 "사용자 확인 항목")

## 세션 완료 기준
### F-01
- 기준 3 [정적]: 충족. src/WinCapture.App/SingleInstanceGuard.cs:30 `new Mutex(initiallyOwned: true, MutexName, out var createdNew)`, 이름 상수 :9. App.xaml.cs:24-32에서 호출, 두 번째 인스턴스는 즉시 Shutdown하고 이름 있는 EventWaitHandle로 기존 인스턴스에 신호(task-03 리뷰 기준 1).
- 기준 4 [CI]: 충족. windows-latest build+test 성공 (run 45920931703).
- 기준 1, 2 [수동]: 판정 안 함. 사용자 확인 항목으로 이관.
### F-02 (전부 [자동])
- 기준 1 왕복 동일: 충족 (RoundTrip_Defaults_AreIdentical, RoundTrip_ChangedValues_AreIdentical)
- 기준 2 깨진 JSON 기본값+.bak: 충족 (Load_CorruptJson_ReturnsDefaultsAndKeepsBackup)
- 기준 3 알 수 없는 키 무시: 충족 (Load_UnknownKeys_AreIgnoredAndKnownKeysRead)
- 기준 4 경로 주입: 충족 (Path_IsInjected_AndSaveCreatesMissingFolder)
- 추가: Defaults_MatchSpecSection7, Load_MissingFile/PartialFile/NullSection, Save_WritesCamelCaseKeysAndUnexpandedEnvVar

## 차단 이슈
없음

## 사용자 확인 항목 (Windows PC)
명령: `git pull` -> `dotnet build WinCapture.sln -c Debug` -> `dotnet test` -> `dotnet run --project src/WinCapture.App`
1. F-01 [수동] 1: 실행하면 트레이 아이콘이 나타나는지, 우클릭/좌클릭 메뉴가 SPEC 순서(영역 지정, 스크린샷, 동영상, GIF, 창 표시/숨김, 저장 폴더 열기, 설정, 종료)이고 미구현 항목이 회색인지, "종료"로 작업 관리자에서 프로세스가 사라지는지 확인.
2. F-01 [수동] 2: 실행 중 한 번 더 실행하면 새 프로세스가 바로 끝나고 기존 인스턴스의 설정 창이 앞으로 나오는지 확인 (설정 창을 닫은 상태, 최소화한 상태 모두).
3. 설정 창 분위기: 트레이 "설정"으로 연 창이 Mica 배경인지, Windows 테마를 다크/라이트로 바꿨을 때(창을 연 채로, 닫았다 다시 열어서) 따라가는지 확인. 열릴 때 라이트가 한 프레임 깜빡이는지도 봐 주세요.
4. 설정 창 껍데기: 좌측 탭 6개의 순서와 아이콘이 표시되고 첫 탭(일반)이 자동 선택되는지 확인.
5. 설정 창 X로 닫아도 트레이 아이콘이 남고, 다시 "설정"을 누르면 창이 뜨는지 확인.
6. 로그 생성: `%LocalAppData%\WinCapture\logs\wincapture-날짜.log` 파일이 생기는지 확인.
7. 트레이 메뉴가 기본 WPF ContextMenu라 다크 테마와 어긋나 보이는지 확인 (거슬리면 수정 요청).

## 후속 세션 권고 (반려 사유 아님)
- CI 워크플로의 actions/checkout@v4, setup-dotnet@v4에서 Node.js 20 deprecation 경고. 후속에서 버전 상향 검토.
- 세션 6 설정 UI에서 JsonSettingsStore 로그 콜백(IO 오류 시 null이면 조용히 기본값 반환)에 Serilog를 주입했는지 재확인.
- 설정 창 시작 시 테마 적용이 Loaded 시점이라 첫 프레임 깜빡임 가능성. Windows에서 거슬리면 개선.
- 트레이 ContextMenu 다크 테마 정합성은 Windows 확인 후 판단.
- Mutex/이벤트가 `Local\` 접두사(세션 단위 단일 인스턴스). 단일 사용자 도구라 문제 없음.
- 세션 2부터 새 오버레이 창은 반드시 OverlayWindowBase 상속(TECH-RULES).
