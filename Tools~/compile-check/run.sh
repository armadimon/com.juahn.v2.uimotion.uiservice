#!/usr/bin/env bash
#
# UiService 브릿지 컴파일 게이트.
#
#   ./Tools~/compile-check/run.sh
#   REF_PROJECT=/path/to/UnityProject ./Tools~/compile-check/run.sh
#
# REF_PROJECT는 UiService와 UniTask가 이미 컴파일돼 있는 Unity 프로젝트여야 한다.
# 그 어셈블리는 재배포할 수 없어 이 저장소에 넣을 수 없다.
#
set -euo pipefail

HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PACKAGE_ROOT="$(cd "${HERE}/../.." && pwd)"

if [ -z "${RUNTIME_PACKAGE:-}" ]; then
  RUNTIME_PACKAGE="$(cd "${PACKAGE_ROOT}/../com.juahn.v2.uimotion" 2>/dev/null && pwd || true)"
fi

if [ -z "${RUNTIME_PACKAGE}" ] || [ ! -d "${RUNTIME_PACKAGE}/Runtime/Core" ]; then
  echo "런타임 패키지를 찾지 못했습니다. RUNTIME_PACKAGE를 지정하세요." >&2
  echo "  예: RUNTIME_PACKAGE=/path/to/com.juahn.v2.uimotion $0" >&2
  exit 2
fi

# --- UiService와 UniTask가 컴파일된 프로젝트를 찾는다 -------------------
if [ -z "${REF_PROJECT:-}" ]; then
  for candidate in "${PACKAGE_ROOT}"/../../*/ ; do
    if [ -f "${candidate}Library/ScriptAssemblies/juahn.UiService.dll" ] \
       && [ -f "${candidate}Library/ScriptAssemblies/UniTask.dll" ]; then
      REF_PROJECT="$(cd "${candidate}" && pwd)"
      break
    fi
  done
fi

if [ -z "${REF_PROJECT:-}" ]; then
  echo "UiService와 UniTask가 컴파일된 Unity 프로젝트를 찾지 못했습니다." >&2
  echo "그 프로젝트를 한 번 열어 컴파일한 뒤 REF_PROJECT로 지정하세요." >&2
  echo "  예: REF_PROJECT=/Users/me/UnityProject/2026Template $0" >&2
  exit 2
fi

# --- Unity 설치 ---------------------------------------------------------
if [ -z "${UNITY_ROOT:-}" ]; then
  UNITY_ROOT="$(ls -d /Applications/Unity/Hub/Editor/*/ 2>/dev/null | sort -V | tail -1 || true)"
fi

if [ -z "${UNITY_ROOT}" ] || [ ! -d "${UNITY_ROOT}" ]; then
  echo "Unity 설치를 찾지 못했습니다. UNITY_ROOT를 지정하세요." >&2
  exit 2
fi

UNITY_MANAGED="${UNITY_ROOT}/Unity.app/Contents/Resources/Scripting/Managed/UnityEngine"
if [ ! -d "${UNITY_MANAGED}" ]; then
  echo "Unity 매니지드 폴더를 찾지 못했습니다: ${UNITY_MANAGED}" >&2
  echo "이 스크립트는 macOS 레이아웃을 가정합니다." >&2
  exit 2
fi

if [ -z "${UNITY_UGUI:-}" ]; then
  UNITY_UGUI="$(ls "${UNITY_ROOT}"/Unity.app/Contents/Resources/PackageManager/ProjectTemplates/libcache/*/ScriptAssemblies/UnityEngine.UI.dll 2>/dev/null | head -1 || true)"
fi

if [ -z "${UNITY_UGUI}" ] || [ ! -f "${UNITY_UGUI}" ]; then
  echo "UnityEngine.UI.dll을 찾지 못했습니다. UNITY_UGUI로 지정하세요." >&2
  exit 2
fi

echo "Unity:    ${UNITY_ROOT}"
echo "런타임:   ${RUNTIME_PACKAGE}"
echo "참조 or:  ${REF_PROJECT}"
echo

dotnet build "${HERE}/UiMotion.UiService.Compile.csproj" \
  -p:UnityManaged="${UNITY_MANAGED}" \
  -p:UnityUgui="${UNITY_UGUI}" \
  -p:RuntimePackage="${RUNTIME_PACKAGE}" \
  -p:RefProject="${REF_PROJECT}" \
  -v quiet --nologo

echo
echo "브릿지 컴파일 통과."
