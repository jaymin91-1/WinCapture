# task-04 리뷰 1회차
## 판정: 통과
## 실행 결과
- 맥 build: 성공 (경고 0, 오류 0)
- 맥 test: 11/11 통과 (tests 디렉터리 변경 없음, 기존 테스트 조작 없음)
- CI: 성공 (구현 커밋 6ad8878 run 45920526256, 보고서 커밋 6edaeb5 run 45920654443 모두 success)
## 기준별 판정
- 기준 1 [정적]: 충족. SettingsWindow.xaml:1 `ui:FluentWindow`, :9 `WindowBackdropType="Mica"`, :10 ExtendsContentIntoTitleBar, :18 TitleBar. 시스템 테마 추종: SettingsWindow.xaml.cs:16 `SystemThemeWatcher.Watch(this)`, :38 `ApplySystemTheme()`. 탭 6개 순서 일반/단축키/캡처/녹화/FFmpeg/정보: SettingsWindow.xaml:27,32,37,42,47,52 (SPEC 4장과 일치).
- 기준 2 [정적]: 충족. SettingsWindow.xaml.cs:23-34 OnClosing에서 Cancel + Hide (`_allowClose` 시에만 실제 닫힘). App.xaml.cs:117 트레이 "설정" Click과 :73 두 번째 실행 신호 콜백이 모두 `ShowSettingsWindow()`(:135)를 사용. 최소화 복원, Activate 포함. 종료 시 :78, :158에서 AllowClose.
- 기준 3 [정적]: 충족. README.md에 기능, 빌드, `winget install Gyan.FFmpeg`, 단축키 7개 표(SPEC 3장 표와 일치), MIT 라이선스 항목 있음.
- 기준 4 [정적]: 충족. 폰트 파일 커밋 없음(git ls-files 확인), 타 프로그램 이름/로고 없음, 비밀값/개인 경로 없음, bin/obj 미커밋.
- 기준 5 [CI]: 충족. 위 CI 결과 참조, 경고 0.
- 기준 6, 7 [수동]: 판정하지 않음. 아래 Windows 확인 항목.
## 차단 이슈
없음
## Windows 확인 항목 (사용자에게 넘김)
- 기준 6 [수동]: 트레이 "설정"으로 창을 열어 Mica 배경인지 확인하고, Windows 테마를 다크로 바꿨을 때 창이 다크로 바뀌는지(창을 연 채로, 그리고 닫았다 다시 열어서) 확인.
- 좌측 탭 6개의 순서와 아이콘이 표시되고 첫 탭(일반)이 자동 선택되는지 확인 (아이콘 심볼은 Windows 런타임에서만 확인 가능).
- 기준 7 [수동]: 창 X로 닫아도 트레이 아이콘이 남고, 다시 "설정"을 누르면 창이 뜨는지 확인.
- 창을 최소화한 상태에서 앱을 다시 실행하면 창이 복원되어 앞으로 오는지, 트레이 "종료"로 정상 종료되는지 확인.
## 권고 사항 (반려 사유 아님)
- README 빌드 절차의 `dotnet run`은 사용자용 문서이므로 문제 없음.
- App.xaml 기본 테마가 Light이고 창 Loaded 시점에 시스템 테마를 적용하므로, 로드 직전 한 프레임 깜빡임 가능성이 있다. Windows에서 거슬리면 후속 세션에서 개선 검토.
