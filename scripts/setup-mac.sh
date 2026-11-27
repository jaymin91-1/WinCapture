#!/bin/zsh
# WinCapture 맥(jay 계정) 개발 환경 준비 스크립트
# 사용법: zsh /Users/Shared/projects/WinCapture/scripts/setup-mac.sh
# 여러 번 실행해도 안전하다(이미 된 단계는 건너뜀).
set -e
PROJ=/Users/Shared/projects/WinCapture
step() { print -P "\n%F{cyan}==> $1%f"; }

# 1) PATH 설정 (claude, dotnet) — ~/.zshrc에 한 번만 추가
step "1/6 PATH 설정"
if ! grep -q '>>> WinCapture setup >>>' ~/.zshrc 2>/dev/null; then
  cat >> ~/.zshrc <<'EOF'

# >>> WinCapture setup >>>
export PATH="$HOME/.local/bin:$PATH"
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$DOTNET_ROOT:$DOTNET_ROOT/tools:$PATH"
# <<< WinCapture setup <<<
EOF
  echo "~/.zshrc에 추가했습니다."
else
  echo "이미 설정되어 있습니다."
fi
export PATH="$HOME/.local/bin:$PATH"
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$DOTNET_ROOT:$DOTNET_ROOT/tools:$PATH"

# 2) .NET 10 SDK (홈 폴더에만 설치, 관리자 권한 불필요, agent 계정 Homebrew와 무관)
step "2/6 .NET 10 SDK"
if command -v dotnet >/dev/null 2>&1 && dotnet --list-sdks | grep -q '^10\.'; then
  echo "이미 설치되어 있습니다."
else
  curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
  bash /tmp/dotnet-install.sh --channel 10.0
fi
dotnet --version

# 3) 하네스 설정 배치 (.claude)
step "3/6 하네스 설정(.claude) 배치"
cd "$PROJ"
if [ -d claude-config ]; then
  mkdir -p .claude
  cp -R claude-config/. .claude/
  rm -rf claude-config
  echo "claude-config → .claude 복사 완료"
else
  echo "이미 배치되어 있습니다."
fi
mkdir -p tasks && touch tasks/.gitkeep

# 4) git 초기화와 첫 커밋 (커밋 작성자는 GitHub noreply 주소, 이 저장소에만 적용)
step "4/6 git 초기화"
[ -d .git ] || git init -q -b main
if ! git config user.email >/dev/null 2>&1; then
  LOGIN=$(gh api user --jq .login)
  GHID=$(gh api user --jq .id)
  git config user.name "$LOGIN"
  git config user.email "${GHID}+${LOGIN}@users.noreply.github.com"
  echo "커밋 작성자: $LOGIN (noreply 이메일)"
fi
git add -A
if git diff --cached --quiet; then
  echo "커밋할 변경이 없습니다."
else
  git commit -q -m "init from harness template (WPF)"
  echo "첫 커밋 완료"
fi

# 5) GitHub 공개 저장소 생성과 푸시
step "5/6 GitHub 연결"
if git remote get-url origin >/dev/null 2>&1; then
  git push -u origin main
else
  gh repo create WinCapture --public --source=. --push
fi
git remote get-url origin

# 6) Claude Code 확인
step "6/6 Claude Code 확인"
if command -v claude >/dev/null 2>&1; then
  claude --version
else
  echo "claude를 찾을 수 없습니다. ~/.local/bin/claude 설치 여부를 확인하세요."
fi

print -P "\n%F{green}환경 준비 완료.%f"
echo "다음: 새 터미널 창을 열고 'claude'를 한 번 실행해 로그인한 뒤 /exit"
