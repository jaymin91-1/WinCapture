# task-06 리뷰 1회차
## 판정: 통과
## 실행 결과
- 맥 build: 성공 (경고 0, 오류 0)
- 맥 test: 98/98 통과
- CI: 성공 (run 45941716624, 커밋 9c43027). 문서 커밋 45941842727은 확인 시점에 진행 중(코드 변경 없음)
## 기준별 판정
- 기준 1 [정적]: 충족. BorderWindow.cs TryHitTest/OnMouseMove/OnMouseLeftButtonDown은 RegionInteraction.HitTest, RegionDrag.Compute(RegionInteraction.Move/Resize), IsLocked만 호출. App에 별도 계산 없음.
- 기준 2 [정적]: 충족. DragTooltipWindow.cs:9 `: OverlayWindowBase`.
- 기준 3 [정적]: 충족. BorderWindow.OnMouseMove의 IsLocked 분기(영역 변경 없이 LockedMessage 툴팁만 표시 후 return), OnMouseLeftButtonDown에서도 표시. 추가로 RegionCoordinator.UpdateRegion이 잠금 시 거부, Compute도 locked면 시작 영역 반환.
- 기준 4 [자동]: 충족. RegionDragTests 7개(델타 기준 Move/Resize, 최소 32, 잠금 시 원본, 입력 창 (92,92,420,320), 문구, UpdateRegion 잠금/저장/restore off). task-03 테스트 파일 변경 없음(git diff: 테스트는 RegionDragTests 추가만).
- 기준 5 [CI]: 충족. 새 경고 없음.
- 기준 6 [수동]: 아래 Windows 확인 항목으로 이관.
## 요청 확인 사항
1. 8px 띠: 충족. BorderVisual.cs의 띠는 (region±8) 사각형에서 region 사각형을 Exclude한 링이라 영역 내부 픽셀은 칠하지 않음. 테두리 링도 기존대로 영역 제외. 영역 내부는 HitTest가 Passthrough라 HTTRANSPARENT 유지(BorderWindow.TryHitTest). 띠는 알파 1 무채색(0,0,0)이고 영역 바깥에만 있어 캡처 결과에 영향 없음. 창이 OverlayWindowBase 상속이라 캡처 제외 유지. 창 크기는 GetInputWindowRect로 band/grip/border 합집합.
2. 델타: 충족. RegionDrag이 시작 커서와 시작 영역을 보관하고 (현재 커서 - 시작 커서)를 시작 영역에 적용. GetCursorPos 물리 픽셀 사용.
3. 잠금: 충족(위 기준 3). 테스트 UpdateRegion_Refused_While_Locked..., Locked_Returns_Start_Region.
4. DragTooltipWindow: OverlayWindowBase 상속 확인. IsHitTestVisible=false, 비활성 표시.
5. 저장 시점: 충족. 드래그 중 persist:false, EndDrag(마우스 업/캡처 상실)에서만 persist:true, restoreLastRegion 조건은 SaveLastRegion(onlyIfRestoreEnabled)로 처리.
## 차단 이슈
없음
## Windows 확인 항목 (사용자에게 넘김)
- F-07-5: 영역 바깥 8px 띠 전체에서 이동 커서(사방)가 뜨고, 드래그하면 영역이 따라 움직이며 `X, Y, W × H` 툴팁이 표시되는가. 알파 1 띠가 8px 전체에서 마우스를 받는지(안 받으면 알려줄 것).
- F-07-5: 우하단 그립에서 ↘ 커서, 드래그로 크기 변경(좌상단 고정, 최소 32).
- F-07-5: 영역 내부 클릭이 아래 창으로 통과하고, 드래그 중 포커스를 뺏지 않는가. 재시작 후 위치 복원.
- 캡처/녹화 결과에 띠·툴팁이 찍히지 않는가(후속 세션에서 캡처 구현 후 확인).
- F-07-6: 임시로 App.OnStartup에서 RegionState.SetLocked(true)로 빌드해 드래그하면 "녹화 중에는 영역을 변경할 수 없습니다"가 뜨고 영역이 불변인가(확인 후 되돌릴 것).
## 권고 사항 (반려 사유 아님)
- 클릭만 하고 이동 없이 놓아도 lastRegion을 저장한다(동일 값 쓰기). 변경 여부 비교 후 저장하면 불필요한 디스크 쓰기를 줄일 수 있다.
- 툴팁이 모니터 경계 밖으로 나갈 수 있음(보고서에 명시된 미처리 사항).
- 그립과 띠 사이 모서리의 미칠한 영역은 HitTest상 입력 대상이나 알파 0이라 실제로는 통과할 수 있음(Windows에서 확인).
- RegionState.cs의 IRegionLock 위에 기존 `/// <summary>`가 두 개 연속으로 붙어 있어 RegionState 문서 주석이 인터페이스 쪽으로 밀림. 정리 권장.
