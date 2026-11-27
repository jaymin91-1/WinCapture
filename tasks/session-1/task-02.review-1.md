# task-02 리뷰 1회차
## 판정: 통과
## 실행 결과
- 맥 build: 성공 (경고 0, 오류 0)
- 맥 test: 11/11 통과 (기존 스모크 1 + 신규 10)
- CI: 구현 커밋 ae504c6 run 45919940425 성공. 보고서 커밋 28a5eb7 run 45920060762는 확인 시점에 진행 중이나 문서만 변경된 커밋이다.
## 기준별 판정
- 기준 1 [자동]: 충족 (RoundTrip_Defaults_AreIdentical, RoundTrip_ChangedValues_AreIdentical. 모든 필드를 기본값 외로 바꿔 검증)
- 기준 2 [자동]: 충족 (Load_CorruptJson_ReturnsDefaultsAndKeepsBackup. `{"saveFolder":` 그대로 사용, settings.json.bak-20260304050607 이름과 원본 내용 검증)
- 기준 3 [자동]: 충족 (Load_UnknownKeys_AreIgnoredAndKnownKeysRead)
- 기준 4 [자동]: 충족 (Load_PartialFile_FillsRestWithDefaults, Path_IsInjected_AndSaveCreatesMissingFolder, 임시 폴더 사용 및 Dispose 정리)
- 기준 5 [정적]: 충족 (src/WinCapture.Core/Settings/AppSettings.cs 전체가 SPEC 7장과 일치, Defaults_MatchSpecSection7 테스트로도 검증)
- 기준 6 [정적]: 충족 (Core csproj에 참조 없음. JsonSettingsStore.cs Load의 catch에서 log 콜백 호출 및 Backup으로 원본 보존)
- 기준 7 [CI]: 충족 (구현 커밋 성공)
## 차단 이슈
없음
## Windows 확인 항목
- 없음 ([수동] 기준 없음)
## 권고 사항
- 파일 읽기 IO 오류 시 로그 콜백이 null이면 조용히 기본값을 반환한다. task-03 DI 연결 시 Serilog 콜백을 주입할 것.
- 기존 task-01 스모크 테스트는 변경되지 않았다(tests 변경은 신규 파일 추가뿐).
