# task-01 작업 보고서
## 한 일
- Core에 `Storage/ISaveFileSystem.cs`(FileExists, GetFileNames 주입 인터페이스)와 `Storage/CaptureFileNamer.cs`(BuildPath, Sanitize, NextCounter) 추가.
- 테스트 `tests/WinCapture.Core.Tests/CaptureFileNamerTests.cs` 10개 추가.
## 완료 기준별 대응
- F-09-1 [자동]: `DateTimePattern_ProducesSpecName`
- F-09-2 중복 [자동]: `ExistingName_GetsSuffixBeforeExtension` (_1, _2)
- F-09-2 토큰 [자동]: `WidthHeightTypeTokens_AreReplaced` (shot_400x300_png.png), 카운터는 `Counter_*` 5개 (없으면 0001, 최대+1, 다른 패턴/확장자 무시, 유형별 하위 폴더 범위, 9999 이후 10000, 다른 토큰과 함께)
- F-09-2 금지 문자 [자동]: `InvalidChars_BecomeUnderscore`
- F-09-3 [자동]: `TypeSubfolders_ControlsPath`, 환경변수 확장 주입은 `SaveFolder_UsesInjectedEnvironmentExpansion`
- [정적]: `CaptureFileNamer.cs`는 DateTime.Now, File.Exists, 환경변수를 호출하지 않고 now, ISaveFileSystem, expandEnvironment 매개변수로 받음.
- [CI]: 성공
## 내린 판단과 이유
- 경로 구분자는 Windows 기준 `\` 고정(맥 테스트에서도 같은 결과). 저장 폴더 끝의 `\`,`/`는 제거.
- `{counter}` 일치 판정: 다른 토큰({date},{time},{w},{h},{type})은 현재 값으로 치환한 결과와 같은 이름만 "같은 패턴"으로 본다. 따라서 `{time}`이 들어간 패턴은 사실상 매번 0001이 된다(SPEC 문구를 문자 그대로 해석). 날짜가 바뀌어도 이어 가려면 날짜 토큰을 숫자 와일드카드로 보도록 바꿔야 하며, 필요하면 알려 달라.
- 중복 접미사 `_N`이 붙은 파일(`shot_0003_1.png`)도 번호 3으로 계산. 비교는 대소문자 무시. 4자리를 넘으면 자릿수가 늘어남(10000).
- 패턴에 `{counter}`가 여러 번이면 모두 같은 값으로 치환, 번호 산출은 첫 번째 위치 기준.
## 테스트 결과
- 맥 build: 성공 (경고 0, 오류 0)
- 맥 test: 191개 통과 (재작업 후 192개)
- CI: 성공 (run 45951933066, 커밋 38de58c)
## Windows 확인 필요
- 없음 (이 작업은 순수 로직. 실제 저장 연결은 이후 작업)
## 못 한 것 / 질문
- 위 `{counter}` 일치 판정 범위(날짜/시간 토큰 포함 여부)가 의도와 맞는지 확인 요청.
- 푸시 성공.

## 사용자 결정 반영 (재작업)
- `{counter}` 같은 패턴 판정을 SPEC 갱신대로 변경: `{date}`(8자리) `{time}`(6자리) `{w}` `{h}`(숫자)는 어떤 값이든 일치, `{type}`과 고정 문자는 일치해야 함. `NextCounter`가 원본 패턴으로 정규식을 만든다(`CaptureFileNamer.cs`).
- 테스트: `Counter_WithOtherTokens_ResolvesThemFirst`를 `Counter_OtherTokensMatchAnyValue`(날짜가 다른 WinCapture_20261126_081500_0003.png 있으면 다음 0004)와 `Counter_SizeTokensMatchAnyValue_TypeMustMatch`로 교체. 위 "판단" 중 현재 값 치환 설명과 "질문"은 해소됨.
- 테스트 192개 통과, 경고 0. CI와 커밋 해시는 푸시 후 아래에 기록.
