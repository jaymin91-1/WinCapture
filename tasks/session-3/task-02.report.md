# task-02 작업 보고서
## 한 일
- Core `CapturePlan.Clip(region, virtualScreen)` (src/WinCapture.Core/Geometry/CapturePlan.cs): 가상 화면과 교차 부분으로 자르고, 비었거나 밖이면 null. 홀수 크기 유지.
- Native `ScreenCapturer` (src/WinCapture.Native/Interop/ScreenCapturer.cs): GetDC → CreateCompatibleDC → CreateDIBSection(top-down 32bpp) → BitBlt(SRCCOPY | CAPTUREBLT) → 픽셀 복사(알파 255). 실패는 `CaptureResult.Error`로 반환. P/Invoke 선언은 NativeMethods.cs 하단에 추가(LibraryImport).
- 커서 합성: includeCursor가 true이고 GetCursorInfo가 CURSOR_SHOWING이며 커서 좌표가 영역 안일 때, GetIconInfo로 핫스팟을 구해 DrawIconEx(DI_NORMAL)로 (커서 - 영역 좌상단 - 핫스팟) 위치에 그린다. 현재 시스템 커서 이미지를 그대로 쓰며 세부 규정이 없어 이 정도로 한정.
- App `ScreenshotService` (src/WinCapture.App/Capture/ScreenshotService.cs): `CaptureAsync`/`CaptureToFileAsync`. 내부는 `Task.Run`, PNG는 WPF `PngBitmapEncoder`. 실패는 Serilog `Log`로 기록하고 `ScreenshotResult.Error`로 전달(예외 삼키지 않고 결과화). 아직 어디에도 연결하지 않음(task-04).
- 테스트 `CapturePlanTests` 추가.
## 완료 기준별 대응
- 1 [자동]: `Region_inside_virtual_screen_is_unchanged`
- 2 [자동]: `Region_overhanging_right_bottom_is_clipped`, `Outside_or_empty_region_fails`(Theory 5건)
- 3 [자동]: `Negative_coordinates_are_clipped_correctly`
- 4 [정적]: ScreenCapturer.cs `Capture` 의 try/finally(약 90행~). SelectObject 복원, DeleteObject(DIB), DeleteDC, ReleaseDC를 역순 해제. 커서 아이콘 비트맵(hbmMask/hbmColor)은 DrawCursor 내부 finally(약 140행)에서 DeleteObject.
- 5 [정적]: P/Invoke는 Native/Interop/NativeMethods.cs에만. Core의 CapturePlan은 순수 정수 연산.
- 6 [정적]: ScreenshotService.cs `EncodePng`의 `BitmapSource.Create(..., 96, 96, PixelFormats.Bgr32, ...)`(약 78행).
- 7 [정적]: ScreenshotService.cs `CaptureAsync`의 `Task.Run`(24행). `.Result`/`.Wait()` 없음.
- 8 [CI]: 성공 (아래)
- 9 [수동]: task-04 연결 후 확인.
## 내린 판단과 이유
- 비트맵은 CreateDIBSection으로 만들어 GetDIBits 없이 픽셀 포인터를 바로 복사. BitBlt이 알파를 채우지 않으므로 복사 시 알파 255, WPF 포맷은 Bgr32.
- ICONINFO.fIcon은 DisableRuntimeMarshalling 제약(SYSLIB1051) 때문에 bool 대신 int.
- Native는 로깅 의존이 없어 로그는 App 서비스에서 남김.
- 앱 창 자동 숨김 처리는 넣지 않음(지시대로).
- BitBlt과 WDA_EXCLUDEFROMCAPTURE: Microsoft 문서상 WDA_EXCLUDEFROMCAPTURE는 "창이 화면에 표시되지만 캡처(화면 복사 등)에서 제외"되는 옵션이고 Windows 10 2004(빌드 19041)+에서 지원된다. 이 제외는 DWM이 합성한 데스크톱 이미지 단계에서 해당 창을 빼는 방식이라, 데스크톱 DC(GetDC(NULL))를 BitBlt하는 GDI 캡처에도 적용되는 것으로 알려져 있다(Magnification/BitBlt/DXGI Desktop Duplication 모두 제외됨). 다만 이는 문서·커뮤니티 근거이며 맥에서 실행 확인은 불가. 이중 안전장치로 테두리는 영역 바깥에 있다. 2004 미만에서는 WDA_MONITOR(검정 표시)로 동작하는 문제가 있으나 대상 OS 밖. CAPTUREBLT는 레이어드 창까지 포함하라는 플래그이며, 제외 창은 이 플래그와 무관하게 제외되어야 함 - Windows 확인 필요.
## 테스트 결과
- 맥 build: 성공 (오류 0, 경고 0)
- 맥 test: 201개 통과
- CI: 성공 (run 45952495263, 커밋 fdf2e21)
- 푸시: 성공
## Windows 확인 필요
- 연결 전이라 단독 확인 불가. task-04 이후: 400x300 영역 캡처 시 PNG 400x300, 테두리·오버레이 없음, includeCursor 켜짐/꺼짐에 따른 커서 표시, 125%/150% DPI와 음수 좌표 모니터에서 영역 일치, 위 BitBlt+WDA_EXCLUDEFROMCAPTURE 제외 동작.
## 못 한 것 / 질문
- 없음
