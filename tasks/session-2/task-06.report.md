# task-06 작업 보고서
## 한 일
- Core: `RegionDrag`(드래그 시작 커서·시작 영역 기준 델타로 Move/Resize 호출), `RegionInteraction.LockedMessage`, `BorderLayout.GetInputWindowRect/GetBandRect`, `IRegionLock`(RegionState가 구현, `IsLocked/SetLocked`), `RegionCoordinator.UpdateRegion(region, persist)` (잠금이면 거부, 저장은 restoreLastRegion 조건).
- Native: `GetCursorPos`, `WindowInterop.GetCursorPosition/MoveTo`(SWP_NOSIZE).
- App: `BorderWindow`에 WPF 마우스 이벤트 기반 드래그(CaptureMouse), 커서(띠 SizeAll, 그립 SizeNWSE), `DragTooltipWindow : OverlayWindowBase`(X, Y, W × H / 잠금 문구). `BorderController`가 coordinator를 BorderWindow에 전달.
- 드래그 중에는 상태만 갱신(persist 없음), 마우스를 놓을 때 한 번 lastRegion 저장.
## 완료 기준별 대응
- 1 [정적]: `BorderWindow.cs` OnMouseMove/OnMouseLeftButtonDown은 `RegionInteraction.HitTest`, `RegionDrag.Compute`(→ `RegionInteraction.Move/Resize`, locked 인자), `RegionState.IsLocked`만 호출. App에 계산 없음.
- 2 [정적]: `DragTooltipWindow.cs` `: OverlayWindowBase`.
- 3 [정적]: `BorderWindow.OnMouseMove`의 `state.IsLocked` 분기: 영역 변경 없이 `LockedMessage` 툴팁만 표시(`OnMouseLeftButtonDown`에서도 표시). 추가로 `UpdateRegion`이 잠금 시 거부.
- 4 [자동]: `RegionDragTests`(7개): 델타 기준, 최소 32, 잠금 시 원본, 입력 창 사각형(92,92,420,320), 문구, UpdateRegion 잠금/저장 조건/restore off. task-03 테스트 무변경.
- 5 [CI]: 아래.
- 6 [수동]: 아래.
## 내린 판단과 이유
- 8px 띠 히트 영역(task-04 권고): 창 사각형을 띠까지 넓히고(`GetInputWindowRect`: 영역±8px와 기존 창(테두리+그립)의 합집합) 띠 영역(영역 바깥만)을 알파 1로 칠했다. 둘 다 필요하다: 창이 좁으면 띠가 창 밖이고, 레이어드 창에서 완전 투명 픽셀은 입력이 통과하기 때문. 영역 내부는 칠하지 않고 HTTRANSPARENT 유지, 테두리/띠는 영역 바깥 원칙 유지. 기존 `GetWindowRect`와 그 테스트는 그대로 두고 새 함수를 추가했다.
- task-03 권고(델타 일관성): 드래그 시작 시 `GetCursorPos` 물리 픽셀과 시작 영역을 저장, 이동 시마다 "현재 커서 − 시작 커서"를 시작 영역에 적용(창이 움직여도 누적 오차 없음).
- 드래그 중 저장은 하지 않고 종료 시 1회 저장(디스크 쓰기 과다 방지).
- 툴팁 위치는 커서 +16,+16 물리 픽셀. 모니터 경계에서 화면 밖으로 나가는 보정은 하지 않았다.
- 잠금 확인용 임시 수단 제안(릴리스 기능 아님): 임시로 `App.OnStartup`에서 `RegionState.SetLocked(true)`를 호출해 빌드해 확인 후 되돌리기.
## 테스트 결과
- 맥 build: 성공 (경고 0, 오류 0)
- 맥 test: 98개 통과
- CI: 성공 (run 45941716624, 커밋 9c43027, 푸시 성공)
## Windows 확인 필요
- F-07-5: 테두리 띠(영역 바깥 8px, 테두리보다 넓음) 위에서 이동 커서, 드래그 시 영역 이동과 `X, Y, W × H` 툴팁. 그립 위에서 ↘ 커서, 드래그로 크기 변경(좌상단 고정, 최소 32). 영역 내부 클릭이 아래 창으로 통과하는지, 포커스를 가져가지 않는지. 재시작 후 위치 복원.
- F-07-6: 위 임시 수단으로 잠금 시 드래그하면 "녹화 중에는 영역을 변경할 수 없습니다" 툴팁이 뜨고 영역 불변.
- 알파 1 띠가 8px 전체에서 마우스를 받는지(받지 않으면 알려주세요).
## 못 한 것 / 질문
- 없음
