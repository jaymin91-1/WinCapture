# task-01 리뷰 1회차
## 판정: 통과
## 실행 결과
- 맥 build: 성공 (경고 0, 오류 0)
- 맥 test: 192/192 통과
- CI: 성공 (run 45952159219, 커밋 fd4577f = 마지막 코드 변경). 보고서만 바꾼 14e6e4c의 run 45952290532는 리뷰 시점에 진행 중이며 코드 변경이 없다.
## 기준별 판정
- 1 [자동] 충족 (DateTimePattern_ProducesSpecName: WinCapture_20261127_093005.png)
- 2 [자동] 충족 (ExistingName_GetsSuffixBeforeExtension: _1, _2)
- 3 [자동] 충족 (WidthHeightTypeTokens_AreReplaced: shot_400x300_png.png, Counter_* 6개)
- 4 [자동] 충족 (InvalidChars_BecomeUnderscore: a:b*c -> a_b_c.png, 금지 문자 9개 전부 검증)
- 5 [자동] 충족 (TypeSubfolders_ControlsPath, SaveFolder_UsesInjectedEnvironmentExpansion)
- 6 [정적] 충족: src/WinCapture.Core/Storage/CaptureFileNamer.cs 에서 DateTime.Now, File.Exists, 환경변수 직접 호출 없음. now, ISaveFileSystem(Storage/ISaveFileSystem.cs), expandEnvironment 매개변수로 주입.
- 7 [CI] 충족 (fd4577f 성공, 새 경고 없음)
- 사용자 결정 {counter}: 충족. NextCounter(CaptureFileNamer.cs)가 {date}(8자리) {time}(6자리) {w} {h}(숫자)를 와일드카드로, {type}과 고정 문자는 일치로 판정. 최대+1, D4, 없으면 0001, 유형별 하위 폴더 범위. 테스트 Counter_OtherTokensMatchAnyValue, Counter_SizeTokensMatchAnyValue_TypeMustMatch, Counter_ScopedToTypeSubfolder, Counter_PastFourDigits_KeepsGrowing 등이 검증.
## 차단 이슈
없음
- 기존 테스트 변경: 재작업 커밋에서 Counter_WithOtherTokens_ResolvesThemFirst를 사용자 결정에 맞게 교체한 것뿐이며 정당하다. skip/삭제 없음. 범위 밖 구현 없음. 비밀 값/산출물 커밋 없음.
## Windows 확인 항목 (사용자에게 넘김)
- 없음 (순수 로직. 실제 저장 연결은 이후 작업)
## 권고 사항
- 패턴에 {counter}가 있는데 {date} 등 와일드카드 자리에 자릿수가 다른 값이 들어간 파일은 같은 패턴으로 안 본다(예: {w}는 숫자 1자리 이상, {date}는 정확히 8자리). SPEC 의도와 일치하므로 참고만.
- 앱 창 자동 숨김 SPEC 삭제(2ca6603)는 이번 작업과 무관.
