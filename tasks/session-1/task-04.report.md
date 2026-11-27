# task-04 작업 보고서
## 한 일
- WPF-UI 4.3.0 추가(TECH-RULES 기술 스택 범위). App.xaml에 ThemesDictionary/ControlsDictionary 병합.
- SettingsWindow를 `FluentWindow`로 교체: Mica, ExtendsContentIntoTitleBar + TitleBar, 좌측 NavigationView, 탭 6개(껍데기 Page).
- 닫기(X)는 취소 후 Hide. 앱 종료 시에만 `AllowClose()`로 실제 닫힘.
- README 초안 작성.
## 완료 기준별 대응
- 기준 1 [정적]: src/WinCapture.App/SettingsWindow.xaml 1행 `ui:FluentWindow`, 10행 `WindowBackdropType="Mica"`, 11행 ExtendsContentIntoTitleBar. 시스템 테마 추종: SettingsWindow.xaml.cs `SystemThemeWatcher.Watch(this)`(생성자)와 `OnLoaded`의 `ApplicationThemeManager.ApplySystemTheme()`. 탭 순서 일반/단축키/캡처/녹화/FFmpeg/정보: SettingsWindow.xaml 28~58행 근처 MenuItems. 페이지: src/WinCapture.App/Pages/PlaceholderPage.cs.
- 기준 2 [정적]: SettingsWindow.xaml.cs `OnClosing`(Cancel + Hide). App.xaml.cs `ShowSettingsWindow()` 하나를 트레이 "설정" Click과 `_guard.StartListening` 콜백이 공유. 최소화 시 복원, Topmost 토글 + Activate. 종료 시 `ExitApplication`/`OnExit`에서 AllowClose.
- 기준 3 [정적]: README.md (기능, 빌드, `winget install Gyan.FFmpeg`, scripts/get-ffmpeg.ps1 언급, FFmpeg 라이선스 귀속, 단축키 7개 표, MIT).
- 기준 4 [정적]: 폰트 번들 없음(시스템 폰트), 타 프로그램 이름/로고 없음.
- 기준 5 [CI]: 아래 참조.
- 기준 6, 7 [수동]: 아래 Windows 확인 필요.
## 내린 판단과 이유
- 탭 페이지는 TargetPageType 요구 때문에 6개 타입으로 나누되, XAML 없이 `PlaceholderPage` 기반 코드 클래스로 최소 구현.
- 앱 기본 테마는 App.xaml에 Light로 두고 창 로드 시 시스템 테마를 적용한다.
- 아이콘 심볼(Settings24 등)은 맥 XAML 컴파일은 통과했으나 런타임 표시는 Windows에서 확인 필요.
## 테스트 결과
- 맥 build: 성공 (--no-incremental, 경고 0, 오류 0)
- 맥 test: 11개 통과
- 커밋: 6ad8878, 푸시 성공
- CI: 성공 (run 45920526256, 6ad8878)
## Windows 확인 필요
- F-13 기준 4 / task 기준 6 [수동]: 트레이 "설정"으로 창이 Mica 배경으로 뜨고, 시스템 테마를 다크로 바꾸면 창이 다크로 바뀌는지(창을 연 채/닫았다 다시 열어서 모두). 좌측 탭 6개 순서와 아이콘 표시, 첫 탭이 자동 선택되는지.
- 기준 7 [수동]: X로 닫아도 트레이 아이콘이 남고, 다시 "설정"을 누르면 창이 뜬다. 최소화 상태에서 앱을 다시 실행해도 창이 복원되어 앞으로 온다. 트레이 "종료"로 정상 종료된다.
## 못 한 것 / 질문
- 없음
