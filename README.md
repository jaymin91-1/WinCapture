# WinCapture

Windows 트레이 상주형 화면 캡처 도구입니다. 전역 단축키로 캡처 영역을 지정하면 영역 바깥에 빨간 테두리가 표시되고, 단축키 한 번으로 해당 영역의 스크린샷(PNG), 동영상(MP4), GIF를 저장합니다. 테두리와 앱의 모든 오버레이 UI는 캡처 결과물에 포함되지 않습니다.

> 개발 중입니다. 현재 구현된 기능은 [PROGRESS.md](PROGRESS.md)를 참고하세요.

## 기능
- 전역 단축키로 영역 지정, 스크린샷(PNG), 동영상(MP4) 및 GIF 녹화
- 영역 바깥 빨간 테두리 (캡처 결과에 포함되지 않음), 드래그로 이동/크기 조절
- 멀티 모니터와 Per-Monitor V2 DPI 지원
- 트레이 상주, Windows 11 Fluent 디자인(Mica, 시스템 라이트/다크 테마 추종) 설정 창

## 요구 사항
- Windows 10/11
- [.NET 10 SDK](https://dotnet.microsoft.com/download) (빌드 시), .NET 10 Desktop Runtime (실행 시)
- FFmpeg (동영상/GIF 녹화용, 별도 설치)

## 빌드
```powershell
dotnet build WinCapture.sln -c Debug
dotnet test
dotnet run --project src/WinCapture.App
dotnet publish src/WinCapture.App -c Release -r win-x64 --self-contained false -o publish
```

## FFmpeg 설치
동영상/GIF 녹화에는 FFmpeg가 필요하며 이 저장소에는 포함되어 있지 않습니다. 아래 중 하나로 설치하세요.

```powershell
winget install Gyan.FFmpeg
```

또는 `scripts/get-ffmpeg.ps1` 스크립트를 사용합니다(준비 중). FFmpeg의 라이선스(LGPL/GPL)는 FFmpeg 측에 귀속되며 WinCapture와는 별개입니다.

## 기본 단축키
설정 창의 '단축키' 탭에서 변경할 수 있습니다.

| 동작 | 기본값 |
|---|---|
| 영역 지정 시작 | Ctrl+Alt+R |
| 스크린샷 저장 | Ctrl+Alt+S |
| MP4 녹화 시작/정지 | Ctrl+Alt+V |
| GIF 녹화 시작/정지 | Ctrl+Alt+G |
| 테두리 표시/숨김 | Ctrl+Alt+H |
| 영역 해제 | Ctrl+Alt+X |
| 앱 창 숨김/표시 | Ctrl+Alt+W |

## 라이선스
MIT License. 자세한 내용은 [LICENSE](LICENSE)를 참고하세요.
