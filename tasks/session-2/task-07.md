# task-07: 전역 단축키 서비스와 앱 창 숨김/표시 연결 (F-03, F-08)
## 목표
RegisterHotKey 서비스로 7개 단축키를 등록·재등록하고, 영역 지정/테두리 토글/영역 해제/앱 창 토글에 연결한다.
## 관련 SPEC
F-03 완료 기준 5, F-08 완료 기준 2, 3, TECH-RULES 3, 7번
## 범위
- Native: `RegisterHotKey`/`UnregisterHotKey` 래퍼(`LibraryImport`), WM_HOTKEY 수신용 메시지 창(또는 HwndSource). `WH_KEYBOARD_LL` 사용 금지
- App: `HotkeyService`: 설정의 hotkeys를 task-01 모델로 파싱해 등록. 모델 변경 시 전부 해제 후 재등록할 수 있는 `Reapply()` 제공(설정 UI는 세션 6). 유효하지 않거나 충돌하는 값은 등록하지 않고 로그. 등록 실패 시 동작별 실패 상태를 노출(세션 6 단축키 탭 경고 표시용)하고 사용자에게 알린다(트레이 풍선 알림 등 최소 구현, 정식 토스트는 세션 3)
- 연결: SelectRegion(선택 오버레이), ToggleBorder, ClearRegion(영역·테두리 해제), ToggleAppWindow. Screenshot/RecordVideo/RecordGif는 등록만 하고 동작은 비어 있는 자리 표시(후속 세션)로 두며 SPEC 범위를 넘는 구현 금지
- 트레이 메뉴 "영역 지정", "창 표시/숨김"을 활성화해 같은 동작에 연결(현재 비활성 항목 정리). 창 숨김 시 트레이 아이콘과 테두리는 유지
- 겹침 판정 연결 준비: 앱 창과 영역 겹침을 Core 함수로 판단해 숨기는 헬퍼와 `autoHideAppWindow` 설정 사용 지점을 만든다(실제 스크린샷 호출은 세션 3)
- 앱 종료 시 모든 단축키 해제
## 범위 밖
- 단축키 설정 UI, 스크린샷/녹화/GIF 구현, 자동 재표시(녹화 후)
## 완료 기준
1. [정적] 단축키 등록이 `RegisterHotKey`만 사용하고 `WH_KEYBOARD_LL`/`SetWindowsHookEx` 계열이 없다 (TECH-RULES 7)
2. [정적] 설정 변경 후 재등록(`Reapply` 등)이 가능하고 종료 시 해제된다. 등록 실패 시 로그와 사용자 알림, 동작별 실패 상태 노출이 있다 (F-03-5 연결)
3. [정적] ToggleAppWindow와 트레이 "창 표시/숨김"이 같은 경로를 사용하고, 창 숨김이 트레이·테두리에 영향 없다 (F-08-2 연결)
4. [정적] 겹침 판정은 task-03 Core 함수를 사용하고 `autoHideAppWindow` 설정을 읽는다 (F-08-3 연결)
5. [자동] Core의 기존 테스트(F-03, F-07, F-08 숫자 예시) 전부 계속 통과하고 삭제·변경이 없다. 이 작업에서 Core에 추가한 로직(예: 유효·충돌 조합 필터링)이 있으면 테스트 추가
6. [CI] windows-latest build/test 성공, 새 경고 없음
7. [수동] F-03-5, F-08-2, F-08-3(세션 3), F-04-1~2, F-06-4~6 등 세션 2 사용자 확인 항목은 Windows 확인 항목으로 넘긴다. 특히 F-06-4: 테두리를 띄운 채 Win+Shift+S로 찍어 테두리가 안 나오는지
## 검증 방법
qa가 코드 읽기 + 맥 build/test + CI 확인. 세션 전체 검증에서 모든 오버레이 창이 OverlayWindowBase를 상속하는지 grep으로 확인한다.
