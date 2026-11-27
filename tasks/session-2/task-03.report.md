# task-03 작업 보고서
## 한 일
- `src/WinCapture.Core/Geometry/RegionInteraction.cs`: RegionHit 열거형, HitTest, Move, Resize(잠금 플래그 지원), Overlaps, FormatTooltip
- `tests/WinCapture.Core.Tests/RegionInteractionTests.cs`: 테스트 추가 (기존 테스트 무변경)
## 완료 기준별 대응
- F-07-1 [자동]: HitTest_SpecExamples (4개 InlineData)
- F-07-2 [자동]: Move_ClampsToVirtualScreen (X=1520, Y=0, 음수 좌표 경계)
- F-07-3 [자동]: Resize_EnforcesMinimumSize, Resize_StopsAtVirtualScreenEdge
- F-07-4 [자동]: Locked_ReturnsOriginalRegion
- F-08-1 [자동]: Overlaps_SpecExamples (3개)
- 그립/띠 겹침 [자동]: HitTest_GripWinsWhereItOverlapsBand
- 툴팁 [자동]: FormatTooltip_UsesInvariantFormat (ar-SA 문화권에서도 동일)
- [정적]: Core 내부 파일, 외부 참조 없음, task-02의 Rect/PixelPoint 재사용
## 내린 판단과 이유
- 띠 = 영역 바깥 8px 전체(사각 고리), 그립 = 우하단 꼭짓점 기준 오른쪽/아래 12px 정사각형(띠를 벗어나는 부분 포함), 그립 우선.
- 리사이즈는 델타(dw, dh) 입력. 시작 위치가 이미 가상 화면 경계에 너무 가까워 32px도 못 들어가는 경우 최소 크기 32를 우선한다.
- 이동 시 영역이 가상 화면보다 크면 가상 화면 좌상단에 맞춘다.
## 테스트 결과
- 맥 build: 성공 (경고 0, 오류 0)
- 맥 test: 73개 통과
- 커밋: 6f46b41, 푸시 성공
- CI: 성공 (run 45940554582, 커밋 6f46b41)
## Windows 확인 필요
- 없음 (이번 작업은 [자동]/[정적]/[CI]만)
## 못 한 것 / 질문
- 없음
