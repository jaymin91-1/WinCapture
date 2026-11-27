# 세션 2 전체 검증

## 판정: 통과

## 실행 결과
- 맥 build (`dotnet build WinCapture.sln`): 성공, 경고 0, 오류 0
- 맥 test: 105/105 통과 (skip 0)
- CI: 코드 최종 커밋 239222f (run 45942065065) 성공, 보고서 커밋 3e29a28 (run 45942168452) 성공. windows-latest 로그에서 경고 0, 테스트 105/105 확인. 이후 문서 전용 커밋 2개(QA review 1, PROGRESS 기록)의 run(45942235423, 45942264135)은 검증 시점에 진행 중이다. 코드 변경이 없어 판정에 영향 없음. 진행자가 세션 종료 푸시 후 최종 run을 다시 확인할 것.

## 공통 완료 기준
1. 맥 test 전체 통과: 충족 (105/105)
2. 맥 build: 충족 (경고 0)
3. 최신 코드 커밋 CI: 충족 (위 참고, 문서 커밋 run은 진행 중)
4. 이전 세션 테스트 유지: 충족 (세션 1 테스트 포함 105개 통과)
5. 테스트 삭제/skip/기대값 변경 없음: 충족. `git diff 6ad8878 HEAD -- tests`는 신규 7개 파일 추가 703줄, 삭제 0줄
6. [자동] 테스트 존재, [정적] 근거 기록: 충족 (task-01~07 리뷰에 테스트명과 파일:줄 기록)
7. 새 경고 없음, TECH-RULES 위반 없음: 충족 (아래 추가 확인)
8. PROGRESS 기록과 푸시: 현재 PROGRESS.md에 task-01~07, 주요 결정(ClearRegion) 기록됨. 상태는 "진행 중"이므로 진행자가 "사용자 승인 대기"로 갱신 후 푸시할 것. 작업트리 clean, 푸시 완료 상태
9. 사용자 확인 항목 나열: 충족 (아래 장)

## 세션 완료 기준 (F-03~F-08 [자동][정적])
- F-03: 파싱/직렬화/충돌/기본값 [자동] 충족 (HotkeyTests, HotkeyPlanTests). RegisterHotKey 전용, 재등록·실패 알림·종료 해제 [정적] 충족 (HotkeyService, App.xaml.cs OnExit). WH_KEYBOARD_LL 없음 (grep)
- F-04: 정규화/최소 32/Esc·취소 유지 [자동] 충족 (RegionTests, RegionCoordinatorTests). 선택 오버레이 [정적] 충족 (SelectionOverlayWindow : OverlayWindowBase)
- F-05: DPI 100/125/150%, 음수 좌표, 복원/가상 화면 보정, 테두리 사각형 [자동] 충족 (RegionTests, RegionCoordinatorTests). PerMonitorV2 [정적] 충족 (app.manifest:12-13)
- F-06: 캡처 제외·창 스타일 [정적] 충족 (OverlayWindowBase.cs:48, :58-59). 내부 HTTRANSPARENT 충족 (BorderWindow.TryHitTest). 테두리 사각형 [자동] 충족 (BorderLayoutTests)
- F-07: 히트 테스트, 이동/리사이즈 제한, 최소 크기, 잠금 [자동] 충족 (RegionInteractionTests, RegionDragTests). UI 연결 [정적] 충족 (BorderWindow)
- F-08: 겹침 판정 [자동] 충족 (Overlaps_SpecExamples, AppWindowPolicyTests). 토글 경로 [정적] 충족 (App.xaml.cs ToggleAppWindow)

## 추가 확인 결과
- 오버레이 상속: 오버레이 창 3개(BorderWindow, SelectionOverlayWindow, DragTooltipWindow)가 모두 `OverlayWindowBase` 상속. Window 직접 상속은 OverlayWindowBase와 SettingsWindow(FluentWindow, 오버레이 아님)뿐이다.
- 캡처 제외·창 스타일 단일 지점: `SetWindowDisplayAffinity`와 GWL_EXSTYLE 설정 호출은 OverlayWindowBase.cs(:48, :58)가 Native 헬퍼(WindowInterop.cs:9-15)를 통해 부르는 한 곳뿐이다. 다른 오버레이/컨트롤러에 중복 호출 없음.
- 선택 오버레이만 `allowActivation: true`(NOACTIVATE 제외)이지만 캡처 제외·LAYERED·TOOLWINDOW·Topmost는 유지한다. 문자상 SPEC F-06-2와 어긋나므로 사용자에게 알리는 것을 권고(아래 후속 권고).
- ClearRegion 사용자 결정 반영: 충족. `RegionCoordinator.Clear()`(RegionCoordinator.cs:89-93)가 상태를 해제하고 RestoreLastRegion 설정과 무관하게 lastRegion을 0,0,0,0으로 저장. 테스트 Clear_RemovesRegionAndEmptiesSavedLastRegion_ThenNoRestore가 저장값과 재시작 후 복원 안 됨을 검증. App.xaml.cs:201 ClearRegion 단축키가 이 경로로 연결. SPEC F-06 118-119행과 일치.
- WH_KEYBOARD_LL, FFmpeg kill: src에 없음. 커밋된 bin/obj/publish, FFmpeg 바이너리 없음.

## 차단 이슈
없음

## 사용자 확인 항목 (Windows PC)
준비
```powershell
git pull
dotnet build WinCapture.sln -c Debug
dotnet test
dotnet run --project src/WinCapture.App
```

1. 앱 실행
   - 트레이 아이콘이 뜨고 앱이 정상 시작되는가. (세션 1 기능 유지)
2. 영역 지정 (F-04 [수동] 1-2, F-03 [수동] 5의 시작 단계)
   - 트레이 "영역 지정"(또는 Ctrl+Alt+R) 클릭: 모든 모니터가 40% 검정 딤, 드래그 영역은 원본 밝기, 크기 라벨 `800 x 600` 형식, 십자 커서.
   - 영역이 있는 상태에서 다시 지정 후 Esc, 또는 32px 미만 드래그: 이전 영역과 테두리가 유지되는가.
   - Esc가 즉시 먹는가(오버레이가 포커스를 받는가), 확정 후 이전 앱으로 포커스가 돌아오는가.
3. 테두리 (F-05 [수동] 5, F-06 [수동] 4-6)
   - 빨간 2px 테두리가 영역 바깥에 그려지고 우하단에 12px 삼각형 그립이 있는가. 작업표시줄/Alt+Tab에 안 보이는가.
   - 100%/125%/150% 모니터(가능한 만큼)에서 테두리가 선택 영역과 픽셀 단위로 일치하고 두께가 물리 2px인가. 음수 좌표 모니터/혼합 DPI 포함.
   - **F-06-4 (가장 중요)**: 테두리를 띄운 채 Win+Shift+S로 찍어서 결과물에 테두리가 안 나오는가. 선택 오버레이(딤), 드래그 툴팁도 찍히지 않는지 함께 확인.
   - 영역 안쪽 클릭이 아래 앱으로 통과하고 포커스가 바뀌지 않는가.
4. 이동/리사이즈 (F-07 [수동] 5)
   - 영역 바깥 8px 띠 전체에서 이동 커서(사방)가 뜨고, 드래그하면 영역이 따라오며 `X, Y, W x H` 툴팁이 뜨는가. 띠가 8px 전체에서 마우스를 받는지 확인(안 받으면 알려줄 것).
   - 우하단 그립에서 대각선 커서, 드래그로 크기 변경(좌상단 고정, 최소 32px, 화면 경계에서 멈춤).
   - 드래그 중 포커스를 뺏지 않는가.
5. 단축키 (F-03 [수동] 5)
   - Ctrl+Alt+R 영역 지정, Ctrl+Alt+H 테두리 숨김/표시 토글.
   - 다른 앱이 쓰는 조합(예: 설정 파일의 hotkeys 값을 다른 앱과 겹치게 수정)으로 시작: 풍선 알림이 뜨고 나머지 단축키는 동작하는가.
6. 앱 창 토글 (F-08 [수동] 2)
   - Ctrl+Alt+W와 트레이 "창 표시/숨김" 모두 설정 창을 토글하는가. 트레이 아이콘과 테두리는 그대로인가.
7. 재시작 복원/해제
   - 영역 지정 후 앱을 종료하고 다시 실행: 테두리가 같은 위치에 복원되는가. (이동/리사이즈 후 위치도 복원)
   - 설정에서 restoreLastRegion을 false로 바꾸면 복원되지 않는가.
   - Ctrl+Alt+X(영역 해제): 테두리가 사라지고, 앱 재시작 후에도 복원되지 않는가. (restoreLastRegion이 true여도)
   - 앱 종료 후 같은 단축키를 다른 앱에서 쓸 수 있는가(등록 해제).
8. [선택] 녹화 중 잠금 확인 (F-07 [수동] 6 중 잠금 안내) 
   - 코드 임시 수정 필요: App.xaml.cs OnStartup에서 `RegionState.SetLocked(true)` 추가해 빌드(커밋 금지, 확인 후 되돌림). 드래그 시 "녹화 중에는 영역을 변경할 수 없습니다" 툴팁이 뜨고 영역이 변하지 않는가. 실제 녹화 연동 확인은 세션 4에서 함께 가능하므로 생략해도 된다.
9. [선택] 영역 내부 HTTRANSPARENT 단독 확인 (F-06 [수동] 5)
   - 위 3번의 클릭 통과 확인으로 대체 가능. 별도 코드 수정 불필요.

참고: F-08 [수동] 3(자동 숨김 후 캡처)은 세션 3, F-07 [수동] 6(녹화 중 잠금 실제 연동)은 세션 4 확인 항목이다.

## 후속 세션 권고 사항
- 선택 오버레이의 NOACTIVATE 예외(allowActivation)를 사용자에게 알리고, 승인 시 SPEC F-06-2/TECH-RULES에 "선택 오버레이는 예외" 한 줄 반영 (문서 수정은 사용자 지시 시에만).
- 단축키 충돌 시 "둘 다 제외" 정책을 세션 6 단축키 탭의 충돌 경고 UI와 함께 재확인. 실패 알림 문구를 원인별(Invalid/Conflict/RegisterFailed)로 분리(세션 3 토스트 전환 시).
- 혼합 DPI는 창 DPI 단일 배율 변환이라 모니터별 배율이 다르면 오차 가능. 위 수동 확인 결과에 따라 후속 수정.
- 최소 크기 우선으로 가상 화면을 벗어나는 극단 케이스 테스트 보강, 드래그 툴팁의 모니터 경계 밖 표시 처리.
- 클릭만 하고 놓을 때 불필요한 저장 비교(이미 변경 없으면 생략되도록 수정됨, 재확인 정도).
- 단축키 id가 1+(int)action으로 enum 값에 의존. enum 순서 변경 금지.
- 영역 모서리(그립과 띠 사이)가 알파 0이면 마우스가 통과할 수 있음. Windows 확인 결과 반영.
