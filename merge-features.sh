#!/usr/bin/env bash
# Merge Strategy Execution Script for FractalDataWorks Developer Kit
# Merges feature branches: architectural-analysis, comprehensive-docs, complete-tests
# Generated: 2025-10-08 by Claude Code
#
# USAGE:
#   bash merge-features.sh              # Run all merges
#   bash merge-features.sh --dry-run    # Preview without merging
#   bash merge-features.sh --help       # Show help

set -e  # Exit on error
set -u  # Exit on undefined variable

# ==========================================
# CONFIGURATION
# ==========================================

REPO_PATH="D:/Development/Developer-Kit"
BASE_BRANCH="features/fractaldataworks"
DRY_RUN=false

# Branches to merge (in order)
declare -a BRANCHES=(
    "feature/architectural-analysis:Add architectural analysis documentation"
    "feature/comprehensive-docs:Add comprehensive project documentation"
    "feature/complete-tests:Improve test coverage to 18.09%"
)

# Color codes
if [ -t 1 ]; then
    RED='\033[0;31m'
    GREEN='\033[0;32m'
    YELLOW='\033[1;33m'
    BLUE='\033[0;34m'
    BOLD='\033[1m'
    NC='\033[0m'
else
    RED=''
    GREEN=''
    YELLOW=''
    BLUE=''
    BOLD=''
    NC=''
fi

# ==========================================
# HELPER FUNCTIONS
# ==========================================

print_header() {
    echo ""
    echo -e "${BLUE}=========================================="
    echo -e "$1"
    echo -e "==========================================${NC}"
    echo ""
}

print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

print_error() {
    echo -e "${RED}✗ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠ $1${NC}"
}

print_info() {
    echo -e "${BLUE}ℹ $1${NC}"
}

show_help() {
    cat << EOF
FractalDataWorks Developer Kit - Feature Branch Merge Script

USAGE:
    $0 [OPTIONS]

OPTIONS:
    --dry-run       Preview merges without executing
    --help          Show this help message
    --skip-build    Skip build verification steps (faster, less safe)

DESCRIPTION:
    Safely merges feature branches into features/fractaldataworks in optimal order:
    1. feature/architectural-analysis (documentation)
    2. feature/comprehensive-docs (documentation)
    3. feature/complete-tests (test improvements)

    The script performs pre-flight checks, executes merges with verification,
    and provides rollback options if issues occur.

EXAMPLES:
    # Preview what will happen
    $0 --dry-run

    # Execute merges with full verification
    $0

    # Execute merges without build checks (faster)
    $0 --skip-build

SAFETY:
    - Working directory must be clean
    - Build state documented before/after each merge
    - Automatic rollback on new build errors
    - All operations logged for review

See MERGE_STRATEGY.md for detailed documentation.
EOF
}

# ==========================================
# ARGUMENT PARSING
# ==========================================

SKIP_BUILD=false

while [[ $# -gt 0 ]]; do
    case $1 in
        --dry-run)
            DRY_RUN=true
            shift
            ;;
        --skip-build)
            SKIP_BUILD=true
            shift
            ;;
        --help|-h)
            show_help
            exit 0
            ;;
        *)
            echo "Unknown option: $1"
            show_help
            exit 1
            ;;
    esac
done

# ==========================================
# PRE-FLIGHT CHECKS
# ==========================================

print_header "FractalDataWorks Developer Kit - Feature Branch Merge"

if [ "$DRY_RUN" = true ]; then
    print_info "DRY RUN MODE - No changes will be made"
fi

print_header "PRE-FLIGHT CHECKS"

# Change to repository
if [ ! -d "$REPO_PATH" ]; then
    print_error "Repository path not found: $REPO_PATH"
    exit 1
fi
cd "$REPO_PATH"
print_success "Repository path: $REPO_PATH"

# Check Git availability
if ! command -v git &> /dev/null; then
    print_error "Git command not found"
    exit 1
fi
print_success "Git available: $(git --version)"

# Check current branch
CURRENT_BRANCH=$(git branch --show-current)
if [ "$CURRENT_BRANCH" != "$BASE_BRANCH" ]; then
    print_error "Must be on branch $BASE_BRANCH"
    echo "Current branch: $CURRENT_BRANCH"
    echo ""
    echo "To fix: git checkout $BASE_BRANCH"
    exit 1
fi
print_success "On correct branch: $BASE_BRANCH"

# Check for uncommitted changes
if [ -n "$(git status --porcelain)" ]; then
    print_error "Working directory has uncommitted changes"
    echo ""
    echo "Uncommitted changes:"
    git status --short
    echo ""
    echo "To fix:"
    echo "  git add .                    # Stage all changes"
    echo "  git commit -m \"message\"      # Commit changes"
    echo "  # OR"
    echo "  git stash                    # Stash changes temporarily"
    exit 1
fi
print_success "Working directory clean"

# Verify all branches exist
print_info "Verifying branches exist..."
ALL_BRANCHES_EXIST=true
for BRANCH_INFO in "${BRANCHES[@]}"; do
    BRANCH_NAME="${BRANCH_INFO%%:*}"
    if ! git rev-parse --verify "$BRANCH_NAME" > /dev/null 2>&1; then
        print_error "Branch not found: $BRANCH_NAME"
        ALL_BRANCHES_EXIST=false
    else
        print_success "Branch exists: $BRANCH_NAME"
    fi
done

if [ "$ALL_BRANCHES_EXIST" != true ]; then
    echo ""
    print_error "Not all branches exist. Cannot proceed."
    exit 1
fi

# Check .NET availability for build verification
if [ "$SKIP_BUILD" = false ]; then
    if ! command -v dotnet &> /dev/null; then
        print_warning ".NET CLI not found - build verification will be skipped"
        SKIP_BUILD=true
    else
        print_success ".NET available: $(dotnet --version)"
    fi
fi

# Document current build state
if [ "$SKIP_BUILD" = false ] && [ "$DRY_RUN" = false ]; then
    print_info "Documenting current build state..."
    dotnet restore > /dev/null 2>&1 || true
    if dotnet build -v quiet > build-pre-merge.log 2>&1; then
        print_success "Current build: SUCCESS"
        PRE_MERGE_ERRORS=0
    else
        PRE_MERGE_ERRORS=$(grep -c "error " build-pre-merge.log || echo "0")
        print_warning "Current build has $PRE_MERGE_ERRORS errors (documented)"
    fi
fi

print_success "All pre-flight checks passed"

# ==========================================
# DRY RUN PREVIEW
# ==========================================

if [ "$DRY_RUN" = true ]; then
    print_header "DRY RUN - MERGE PREVIEW"

    for BRANCH_INFO in "${BRANCHES[@]}"; do
        BRANCH_NAME="${BRANCH_INFO%%:*}"
        DESCRIPTION="${BRANCH_INFO#*:}"

        echo ""
        echo -e "${BOLD}Branch: $BRANCH_NAME${NC}"
        echo "Description: $DESCRIPTION"
        echo ""
        echo "Files to be merged:"
        git diff --name-status "$BASE_BRANCH..$BRANCH_NAME" | head -20
        FILE_COUNT=$(git diff --name-only "$BASE_BRANCH..$BRANCH_NAME" | wc -l)
        if [ "$FILE_COUNT" -gt 20 ]; then
            echo "... and $((FILE_COUNT - 20)) more files"
        fi
        echo ""
        echo "Commits to be merged:"
        git log --oneline "$BASE_BRANCH..$BRANCH_NAME" | head -5
        COMMIT_COUNT=$(git log --oneline "$BASE_BRANCH..$BRANCH_NAME" | wc -l)
        if [ "$COMMIT_COUNT" -gt 5 ]; then
            echo "... and $((COMMIT_COUNT - 5)) more commits"
        fi
    done

    echo ""
    print_info "Dry run complete. No changes made."
    echo ""
    echo "To execute merges, run: $0"
    exit 0
fi

# ==========================================
# MERGE FUNCTION
# ==========================================

merge_branch() {
    local BRANCH_NAME=$1
    local DESCRIPTION=$2
    local MERGE_NUMBER=$3

    print_header "MERGE $MERGE_NUMBER: $BRANCH_NAME"

    # Show what will be merged
    echo "Description: $DESCRIPTION"
    echo ""
    echo "Files to be merged:"
    git diff --name-status "$BASE_BRANCH..$BRANCH_NAME"
    echo ""

    local FILE_COUNT
    FILE_COUNT=$(git diff --name-only "$BASE_BRANCH..$BRANCH_NAME" | wc -l)
    print_info "Total files: $FILE_COUNT"

    # Perform merge
    echo ""
    print_info "Executing merge..."

    local MERGE_MESSAGE
    MERGE_MESSAGE="Merge $BRANCH_NAME: $DESCRIPTION

Merged from worktree analysis and documentation phase.

Changes included:
$(git diff --name-only "$BASE_BRANCH..$BRANCH_NAME" | sed 's/^/- /')

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>"

    if git merge --no-ff "$BRANCH_NAME" -m "$MERGE_MESSAGE"; then
        print_success "Merge successful"
    else
        print_error "MERGE CONFLICT DETECTED"
        echo ""
        echo "Conflicting files:"
        git diff --name-only --diff-filter=U
        echo ""
        print_warning "MANUAL RESOLUTION REQUIRED:"
        echo "  1. Resolve conflicts in the files listed above"
        echo "  2. Stage resolved files: git add <file>"
        echo "  3. Complete merge: git commit"
        echo "  4. Re-run this script to continue with remaining merges"
        echo ""
        print_info "Or abort merge: git merge --abort"
        exit 1
    fi

    # Verify build after merge
    if [ "$SKIP_BUILD" = false ]; then
        echo ""
        print_info "Verifying build..."

        if dotnet build -v quiet > "build-after-$BRANCH_NAME.log" 2>&1; then
            print_success "Build successful after merge"
        else
            local POST_ERRORS
            POST_ERRORS=$(grep -c "error " "build-after-$BRANCH_NAME.log" || echo "0")
            print_warning "Build has $POST_ERRORS errors"

            # Compare error counts
            if [ "$POST_ERRORS" -gt "$PRE_MERGE_ERRORS" ]; then
                print_error "NEW ERRORS INTRODUCED BY MERGE"
                echo "Pre-merge errors: $PRE_MERGE_ERRORS"
                echo "Post-merge errors: $POST_ERRORS"
                echo "New errors: $((POST_ERRORS - PRE_MERGE_ERRORS))"
                echo ""
                print_warning "ROLLBACK OPTIONS:"
                echo "  1. Automatic rollback (recommended): This will undo the merge"
                echo "  2. Keep merge and fix errors manually"
                echo ""

                read -p "Rollback merge? [Y/n]: " -n 1 -r
                echo
                if [[ ! $REPLY =~ ^[Nn]$ ]]; then
                    git reset --hard HEAD~1
                    print_info "Merge rolled back"
                    exit 1
                else
                    print_warning "Continuing with errors - you must fix them manually"
                fi
            else
                print_success "No new errors introduced (pre-existing: $PRE_MERGE_ERRORS)"
            fi

            # Update error count for next iteration
            PRE_MERGE_ERRORS=$POST_ERRORS
        fi
    fi

    echo ""
    print_success "MERGE COMPLETE: $BRANCH_NAME"
    sleep 1
}

# ==========================================
# EXECUTE MERGES
# ==========================================

print_header "EXECUTING MERGES"

MERGE_COUNT=0
for BRANCH_INFO in "${BRANCHES[@]}"; do
    MERGE_COUNT=$((MERGE_COUNT + 1))
    BRANCH_NAME="${BRANCH_INFO%%:*}"
    DESCRIPTION="${BRANCH_INFO#*:}"

    merge_branch "$BRANCH_NAME" "$DESCRIPTION" "$MERGE_COUNT"
done

# ==========================================
# POST-MERGE VERIFICATION
# ==========================================

print_header "POST-MERGE VERIFICATION"

echo "Git Status:"
git status
echo ""

echo "Recent Commits:"
git log --oneline --graph -10
echo ""

if [ "$SKIP_BUILD" = false ]; then
    echo "Build Logs Generated:"
    ls -lh build-*.log 2>/dev/null || echo "No build logs found"
    echo ""

    print_info "Running final build..."
    if dotnet build -v quiet > build-final.log 2>&1; then
        print_success "Final build: SUCCESS"
    else
        local FINAL_ERRORS
        FINAL_ERRORS=$(grep -c "error " build-final.log || echo "0")
        print_warning "Final build has $FINAL_ERRORS errors"
        echo "See build-final.log for details"
    fi
fi

# ==========================================
# SUCCESS SUMMARY
# ==========================================

print_header "ALL MERGES COMPLETE"

print_success "$MERGE_COUNT branches merged successfully"
echo ""

echo -e "${BOLD}Branches Merged:${NC}"
for BRANCH_INFO in "${BRANCHES[@]}"; do
    BRANCH_NAME="${BRANCH_INFO%%:*}"
    echo "  ✓ $BRANCH_NAME"
done
echo ""

echo -e "${BOLD}NEXT STEPS:${NC}"
echo "  1. Review changes: git log --oneline --graph -10"
if [ "$SKIP_BUILD" = false ]; then
    echo "  2. Review build-final.log for any issues"
fi
echo "  3. Run tests: dotnet test"
echo "  4. If satisfied, push: git push origin $BASE_BRANCH"
echo "  5. Create PR to master branch"
echo ""

print_info "Branches NOT merged (by design):"
echo "  - feature/data-containers-future (deferred work)"
echo ""

echo "See MERGE_STRATEGY.md for PR creation and next steps."
echo ""
