# task-02 작업 보고서
## 한 일
- `src/WinCapture.Core/Settings/AppSettings.cs`: SPEC 7장 전체 키·기본값 모델(record, 하위 모델 포함, 핫키는 문자열)
- `src/WinCapture.Core/Settings/ISettingsStore.cs`: `ISettingsStore`, `IClock`, `SystemClock`
- `src/WinCapture.Core/Settings/JsonSettingsStore.cs`: System.Text.Json 기반 저장소. 경로·시계·로그 콜백을 생성자로 주입, `GetDefaultPath()`(%AppData%\WinCapture\settings.json) 헬퍼
- `tests/WinCapture.Core.Tests/SettingsStoreTests.cs`: 테스트 10개 (임시 폴더 사용, Dispose에서 정리)
## 완료 기준별 대응
- 기준 1 [자동]: `RoundTrip_Defaults_AreIdentical`, `RoundTrip_ChangedValues_AreIdentical`
- 기준 2 [자동]: `Load_CorruptJson_ReturnsDefaultsAndKeepsBackup` (`{"saveFolder":`, 고정 시각 -> `settings.json.bak-20260304050607`, 원본 내용 검증)
- 기준 3 [자동]: `Load_UnknownKeys_AreIgnoredAndKnownKeysRead`
- 기준 4 [자동]: `Load_PartialFile_FillsRestWithDefaults`, `Path_IsInjected_AndSaveCreatesMissingFolder`, `Load_MissingFile_ReturnsDefaults`
- 기준 5 [정적]: `AppSettings.cs` 각 프로퍼티 초기값 + `Defaults_MatchSpecSection7` 테스트
- 기준 6 [정적]: Core csproj는 참조 없음(순수 net10.0). 예외 처리는 `JsonSettingsStore.Load`/`Backup`에서 log 콜백 호출 + 원본 .bak 보존
- 기준 7 [CI]: 아래 참조
## 내린 판단과 이유
- 핫키 키 이름은 SPEC 7장대로 PascalCase(`SelectRegion` 등), 나머지는 camelCase (`JsonPropertyName`으로 명시)
- 읽기는 대소문자 무시. `"border": null` 같은 명시적 null은 기본값으로 대체
- 타입이 틀린 값(예: thickness가 문자열)도 JsonException이므로 깨진 파일과 동일하게 백업 후 기본값
- 쓰기는 임시 파일 후 Move(덮어쓰기)로 원자적 저장. 로깅은 Core에 Serilog 의존을 넣지 않고 `Action<string, Exception?>` 콜백으로 주입
- 파일 읽기 IO 오류(잠금 등)는 백업 없이 기본값 반환 + 로그 (파일 자체는 손상이 아니므로)
## 테스트 결과
- 맥 build: 성공 (경고 0, 오류 0)
- 맥 test: 11개 통과 (기존 스모크 1 + 신규 10)
- CI: 성공 (run 45919940425, 커밋 ae504c6)
- 푸시: 성공
## Windows 확인 필요
- 없음 (수동 기준 없음)
## 못 한 것 / 질문
- 없음
