# task-05 리뷰 1회차
## 판정: 통과
## 실행 결과
- 맥 build: 성공 (경고 0, 오류 0)
- 맥 test: 217/217 통과
- CI: 성공 (run 45953775864, 커밋 519051c). 보고서 커밋(run 45953922223)은 확인 시점에 진행 중이었으나 문서만 변경.
## 기준별 판정
- 기준 1 [자동]: 충족. HotkeyFailureMessageTests 3건(실패 없음 -> null, 단일 실패 -> 동작명+"설정 › 단축키", 복수·중복 -> enum 순서로 1개 메시지). Core 로직(src/WinCapture.Core/Presentation/HotkeyFailureMessage.cs).
- 기준 2 [정적]: 충족. src/WinCapture.App/App.xaml.cs:109-110 IToastService.Show(ToastKind.Warning, ...). grep으로 src/tests 전체에 ShowNotification 잔존 없음. 트레이 아이콘 자체는 미변경.
- 기준 3 [정적]: 충족. HotkeyFailureMessage.cs 본문 "설정 › 단축키 탭에서 변경해 주세요".
- 기준 4 [정적]: 충족. HotkeyService.cs diff는 알림 호출부 5줄뿐, Statuses/StatusesChanged 계산 미변경. 기존 HotkeyService 관련 테스트 유지.
- 재등록 경로: HotkeyService.cs:79가 Reapply() 내부이므로 시작 시와 설정 변경 후 모두 토스트. 묶기는 실패 동작 전체를 1개 토스트로 합침(보고서 명시).
- 기준 5 [CI]: 충족. 새 경고 없음(맥 빌드 경고 0).
## 제거 항목 판단
- RegionInteraction.Overlaps와 테스트 Overlaps_SpecExamples: 이 테스트는 구 SPEC F-08 기준 1(겹침 판정, 0,0,500,500 등 숫자 예시)에 대응하던 것이다. 커밋 2ca6603에서 사용자 지시로 기준 1·3이 SPEC에서 삭제되었고, SPEC이 "관련 코드·테스트는 제거·교체할 수 있다(목록을 보고서에 남긴다)"고 명시했다. 보고서에 제거 목록이 있다. 따라서 공통 기준 5(테스트 삭제 금지)의 정당한 예외이며 기대값 변경·skip이 아니다. 삭제된 것은 해당 테스트 1개(InlineData 3건)뿐이고, 테스트 수는 Overlaps 3건 감소·신규 3건 증가이다. Rect.Intersects는 유지됨.
- ScreenshotService.CaptureToFileAsync: src/tests 전체 grep에서 참조 0건, 빌드 성공 및 CI 성공으로 깨지는 호출처 없음.
## 차단 이슈
없음
## Windows 확인 항목 (사용자에게 넘김)
- F-03-5 기준 [수동]: 다른 앱이 쓰는 조합(같은 조합을 먼저 등록한 앱)을 가진 상태로 실행하면 우하단에 경고 토스트 1개("단축키 등록 실패 ... 설정 › 단축키 탭에서 변경")가 뜨고, 단축키 팝업에 빨간 테두리·`사용 중인 조합`이 보이는지 확인. 설정 변경 후 재등록 때도 같은 토스트가 뜨는지 확인.
## 권고 사항
- HotkeyService는 RegisterFailed 외 상태도 "실패"로 묶는다(기존 동작과 동일). 문구가 "다른 앱이 사용 중일 수 있습니다"라 다른 원인과 어긋날 수 있으나 "일 수 있습니다"로 완화되어 있어 문제 없음.
- Reapply가 UI 스레드 밖에서 호출될 경우 토스트 창 생성이 문제가 될 수 있으니 호출 스레드를 세션 종료 검증에서 한 번 더 확인하면 좋다.
