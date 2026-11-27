# task-02 리뷰 1회차
## 판정: 통과
## 실행 결과
- 맥 build: 성공 (경고 0, 오류 0)
- 맥 test: 201/201 통과
- CI: 구현 커밋 fdf2e21 성공 (run 45952495263). 보고서 커밋 93cd8db run 45952694363은 확인 시점에 진행 중(코드 변경 없는 문서 커밋)
## 기준별 판정
- 기준 1 [자동]: 충족 (CapturePlanTests.Region_inside_virtual_screen_is_unchanged, SPEC 숫자 그대로)
- 기준 2 [자동]: 충족 (Region_overhanging_right_bottom_is_clipped -> (1800,900,120,180); Outside_or_empty_region_fails 5건: 완전 밖, 음수 쪽 밖, W/H 0)
- 기준 3 [자동]: 충족 (Negative_coordinates_are_clipped_correctly -> (-1920,10,120,100)). 홀수 크기 유지도 Odd_sizes_are_kept로 검증
- 기준 4 [정적]: 충족. ScreenCapturer.cs:39-112 try/finally에서 SelectObject 복원(93-96), DeleteObject(dib)(98-101), DeleteDC(103-106), ReleaseDC(108-111)를 모든 반환/예외 경로에서 역순 해제. 커서 아이콘 hbmMask/hbmColor는 :135-151 finally에서 해제. (HCURSOR는 GetCursorInfo의 공유 핸들이라 해제 불필요)
- 기준 5 [정적]: 충족. P/Invoke는 NativeMethods.cs(Native)에만, LibraryImport 사용. CapturePlan.cs는 정수 연산만, Core에 WPF/Win32/System.Drawing 참조 없음
- 기준 6 [정적]: 충족. ScreenshotService.cs:78 BitmapSource.Create(..., 96, 96, Bgr32, ...), 크기는 clipped 사각형 W×H
- 기준 7 [정적]: 충족. ScreenshotService.cs:23-24 Task.Run, CaptureToFileAsync는 await 사용. .Result/.Wait() 없음
- 기준 8 [CI]: 구현 커밋 기준 성공, 새 경고 없음
- 기준 9 [수동]: Windows 확인 항목으로 이관
- 추가 점검: 기존 테스트 변경 없음(테스트 diff는 신규 파일 추가뿐). 자동 숨김 처리 코드 없음. WH_KEYBOARD_LL/FFmpeg 해당 없음. 비밀값/산출물 커밋 없음. CAPTUREBLT(0x40000000)는 SRCCOPY와 함께 사용되어 레이어드 창 포함(ScreenCapturer.cs:69-70). 커서 합성은 CURSOR_SHOWING + 영역 내부일 때 핫스팟 보정해 DrawIconEx(:122-155). 실패는 CaptureResult/ScreenshotResult.Error로 반환하고 Serilog 로그
## 차단 이슈
없음
## Windows 확인 항목 (사용자에게 넘김)
- F-09-4 [수동]: task-04 연결 후 400×300 영역 캡처 시 PNG가 정확히 400×300이고 테두리·오버레이가 없는지 확인
- WDA_EXCLUDEFROMCAPTURE 창(테두리/오버레이)이 BitBlt|CAPTUREBLT 캡처 결과에서 제외되는지 확인 (코드로 증명 불가, 문서·커뮤니티 근거만 존재)
- includeCursor 켜짐/꺼짐에 따라 커서가 합성/미합성되는지, 커서 위치(핫스팟)가 맞는지
- 125%/150% DPI 및 음수 좌표 모니터에서 캡처 영역이 선택 영역과 일치하는지
## 권고 사항
- 오른쪽/아래 모니터 경계의 가상 화면 밖 걸침은 CapturePlan이 자르지만 캡처된 PNG가 선택 영역보다 작아질 수 있으니 UI에서 이 경우를 알리는 것을 고려
- CapturePlan.Clip의 Right/Bottom 오버플로 경계(int 최대치)는 실사용 범위 밖이라 무시 가능
- ScreenCapturer.Capture에서 BitBlt 실패 시 GetLastError가 후속 호출로 덮이지 않도록 Fail 호출 직후 사용 구조는 현재 적절함
