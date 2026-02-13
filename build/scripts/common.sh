#!/bin/bash
# build/scripts/common.sh — Shared functions and configuration for AppCore build scripts
# Source this file from other scripts: source "$(dirname "$(readlink -f "$0")")/../scripts/common.sh"
#                                  or: source "$(dirname "$(readlink -f "$0")")/common.sh"

# ── Colors ──────────────────────────────────────────────────────────────────────
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# ── Print helpers ───────────────────────────────────────────────────────────────
print_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

print_section() {
    echo -e "${CYAN}===================================================================${NC}"
    echo -e "${CYAN}  $1${NC}"
    echo -e "${CYAN}===================================================================${NC}"
}

# ── Coverage configuration ──────────────────────────────────────────────────────
COVERAGE_THRESHOLD=80
COVERAGE_RUNSETTINGS="build/coverage/coverage.runsettings"
COVERAGE_FILE_FILTERS="-*.g.cs;-**/obj/**;-**/bin/**"
COVERAGE_CLASS_FILTERS="-System.Text.Json.SourceGeneration.*"
COVERAGE_REPORT_TYPES="Html;Cobertura;JsonSummary;Badges;TextSummary"

# ── Runtime detection ───────────────────────────────────────────────────────────
detect_runtime_id() {
    local os arch
    case "$(uname -s)" in
        Linux*)  os="linux" ;;
        Darwin*) os="osx" ;;
        *)       os="linux" ;;  # default fallback
    esac
    case "$(uname -m)" in
        x86_64|amd64) arch="x64" ;;
        aarch64|arm64) arch="arm64" ;;
        armv7l)        arch="arm" ;;
        *)             arch="x64" ;;  # default fallback
    esac
    echo "${os}-${arch}"
}
