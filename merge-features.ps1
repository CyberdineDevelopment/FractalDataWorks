# Merge Strategy Execution Script for FractalDataWorks Developer Kit (PowerShell)
# Merges feature branches: architectural-analysis, comprehensive-docs, complete-tests
# Generated: 2025-10-08 by Claude Code
#
# USAGE:
#   .\merge-features.ps1              # Run all merges
#   .\merge-features.ps1 -DryRun      # Preview without merging
#   .\merge-features.ps1 -Help        # Show help

param(
    [switch]$DryRun,
    [switch]$SkipBuild,
    [switch]$Help
)

$ErrorActionPreference = "Stop"

# ==========================================
# CONFIGURATION
# ==========================================

$RepoPath = "D:\Development\Developer-Kit"
$BaseBranch = "features/fractaldataworks"

# Branches to merge (in order)
$Branches = @(
    @{
        Name = "feature/architectural-analysis"
        Description = "Add architectural analysis documentation"
    },
    @{
        Name = "feature/comprehensive-docs"
        Description = "Add comprehensive project documentation"
    },
    @{
        Name = "feature/complete-tests"
        Description = "Improve test coverage to 18.09%"
    }
)

# ==========================================
# HELPER FUNCTIONS
# ==========================================

function Write-Header {
    param([string]$Message)
    Write-Host ""
    Write-Host "==========================================" -ForegroundColor Cyan
    Write-Host $Message -ForegroundColor Cyan
    Write-Host "==========================================" -ForegroundColor Cyan
    Write-Host ""
}

function Write-Success {
    param([string]$Message)
    Write-Host "✓ $Message" -ForegroundColor Green
}

function Write-Error {
    param([string]$Message)
    Write-Host "✗ $Message" -ForegroundColor Red
}

function Write-Warning {
    param([string]$Message)
    Write-Host "⚠ $Message" -ForegroundColor Yellow
}

function Write-Info {
    param([string]$Message)
    Write-Host "ℹ $Message" -ForegroundColor Cyan
}

function Show-Help {
    Write-Host @"
FractalDataWorks Developer Kit - Feature Branch Merge Script (PowerShell)

USAGE:
    .\merge-features.ps1 [OPTIONS]

OPTIONS:
    -DryRun       Preview merges without executing
    -Help         Show this help message
    -SkipBuild    Skip build verification steps (faster, less safe)

DESCRIPTION:
    Safely merges feature branches into features/fractaldataworks in optimal order:
    1. feature/architectural-analysis (documentation)
    2. feature/comprehensive-docs (documentation)
    3. feature/complete-tests (test improvements)

    The script performs pre-flight checks, executes merges with verification,
    and provides rollback options if issues occur.

EXAMPLES:
    # Preview what will happen
    .\merge-features.ps1 -DryRun

    # Execute merges with full verification
    .\merge-features.ps1

    # Execute merges without build checks (faster)
    .\merge-features.ps1 -SkipBuild

SAFETY:
    - Working directory must be clean
    - Build state documented before/after each merge
    - Automatic rollback on new build errors
    - All operations logged for review

See MERGE_STRATEGY.md for detailed documentation.
"@
}

# ==========================================
# ARGUMENT HANDLING
# ==========================================

if ($Help) {
    Show-Help
    exit 0
}

# ==========================================
# PRE-FLIGHT CHECKS
# ==========================================

Write-Header "FractalDataWorks Developer Kit - Feature Branch Merge"

if ($DryRun) {
    Write-Info "DRY RUN MODE - No changes will be made"
}

Write-Header "PRE-FLIGHT CHECKS"

# Change to repository
if (-not (Test-Path $RepoPath)) {
    Write-Error "Repository path not found: $RepoPath"
    exit 1
}
Set-Location $RepoPath
Write-Success "Repository path: $RepoPath"

# Check Git availability
try {
    $gitVersion = git --version
    Write-Success "Git available: $gitVersion"
} catch {
    Write-Error "Git command not found"
    exit 1
}

# Check current branch
$currentBranch = git branch --show-current
if ($currentBranch -ne $BaseBranch) {
    Write-Error "Must be on branch $BaseBranch"
    Write-Host "Current branch: $currentBranch"
    Write-Host ""
    Write-Host "To fix: git checkout $BaseBranch"
    exit 1
}
Write-Success "On correct branch: $BaseBranch"

# Check for uncommitted changes
$gitStatus = git status --porcelain
if ($gitStatus) {
    Write-Error "Working directory has uncommitted changes"
    Write-Host ""
    Write-Host "Uncommitted changes:"
    git status --short
    Write-Host ""
    Write-Host "To fix:"
    Write-Host "  git add .                    # Stage all changes"
    Write-Host "  git commit -m `"message`"      # Commit changes"
    Write-Host "  # OR"
    Write-Host "  git stash                    # Stash changes temporarily"
    exit 1
}
Write-Success "Working directory clean"

# Verify all branches exist
Write-Info "Verifying branches exist..."
$allBranchesExist = $true
foreach ($branch in $Branches) {
    $branchName = $branch.Name
    try {
        git rev-parse --verify $branchName 2>&1 | Out-Null
        Write-Success "Branch exists: $branchName"
    } catch {
        Write-Error "Branch not found: $branchName"
        $allBranchesExist = $false
    }
}

if (-not $allBranchesExist) {
    Write-Host ""
    Write-Error "Not all branches exist. Cannot proceed."
    exit 1
}

# Check .NET availability for build verification
if (-not $SkipBuild) {
    try {
        $dotnetVersion = dotnet --version
        Write-Success ".NET available: $dotnetVersion"
    } catch {
        Write-Warning ".NET CLI not found - build verification will be skipped"
        $SkipBuild = $true
    }
}

# Document current build state
$preMergeErrors = 0
if (-not $SkipBuild -and -not $DryRun) {
    Write-Info "Documenting current build state..."
    dotnet restore > $null 2>&1
    if (dotnet build -v quiet > build-pre-merge.log 2>&1) {
        Write-Success "Current build: SUCCESS"
        $preMergeErrors = 0
    } else {
        $preMergeErrors = (Select-String -Pattern "error " -Path build-pre-merge.log).Count
        Write-Warning "Current build has $preMergeErrors errors (documented)"
    }
}

Write-Success "All pre-flight checks passed"

# ==========================================
# DRY RUN PREVIEW
# ==========================================

if ($DryRun) {
    Write-Header "DRY RUN - MERGE PREVIEW"

    foreach ($branch in $Branches) {
        $branchName = $branch.Name
        $description = $branch.Description

        Write-Host ""
        Write-Host "Branch: $branchName" -ForegroundColor White
        Write-Host "Description: $description"
        Write-Host ""
        Write-Host "Files to be merged:"
        git diff --name-status "$BaseBranch..$branchName" | Select-Object -First 20
        $fileCount = (git diff --name-only "$BaseBranch..$branchName" | Measure-Object).Count
        if ($fileCount -gt 20) {
            Write-Host "... and $($fileCount - 20) more files"
        }
        Write-Host ""
        Write-Host "Commits to be merged:"
        git log --oneline "$BaseBranch..$branchName" | Select-Object -First 5
        $commitCount = (git log --oneline "$BaseBranch..$branchName" | Measure-Object).Count
        if ($commitCount -gt 5) {
            Write-Host "... and $($commitCount - 5) more commits"
        }
    }

    Write-Host ""
    Write-Info "Dry run complete. No changes made."
    Write-Host ""
    Write-Host "To execute merges, run: .\merge-features.ps1"
    exit 0
}

# ==========================================
# MERGE FUNCTION
# ==========================================

function Merge-Branch {
    param(
        [string]$BranchName,
        [string]$Description,
        [int]$MergeNumber
    )

    Write-Header "MERGE $MergeNumber`: $BranchName"

    # Show what will be merged
    Write-Host "Description: $Description"
    Write-Host ""
    Write-Host "Files to be merged:"
    git diff --name-status "$BaseBranch..$BranchName"
    Write-Host ""

    $fileCount = (git diff --name-only "$BaseBranch..$BranchName" | Measure-Object).Count
    Write-Info "Total files: $fileCount"

    # Perform merge
    Write-Host ""
    Write-Info "Executing merge..."

    $filesList = (git diff --name-only "$BaseBranch..$BranchName") -join "`n- "
    $mergeMessage = @"
Merge $BranchName`: $Description

Merged from worktree analysis and documentation phase.

Changes included:
- $filesList

🤖 Generated with [Claude Code](https://claude.com/claude-code)

Co-Authored-By: Claude <noreply@anthropic.com>
"@

    try {
        git merge --no-ff $BranchName -m $mergeMessage
        Write-Success "Merge successful"
    } catch {
        Write-Error "MERGE CONFLICT DETECTED"
        Write-Host ""
        Write-Host "Conflicting files:"
        git diff --name-only --diff-filter=U
        Write-Host ""
        Write-Warning "MANUAL RESOLUTION REQUIRED:"
        Write-Host "  1. Resolve conflicts in the files listed above"
        Write-Host "  2. Stage resolved files: git add <file>"
        Write-Host "  3. Complete merge: git commit"
        Write-Host "  4. Re-run this script to continue with remaining merges"
        Write-Host ""
        Write-Info "Or abort merge: git merge --abort"
        exit 1
    }

    # Verify build after merge
    if (-not $SkipBuild) {
        Write-Host ""
        Write-Info "Verifying build..."

        if (dotnet build -v quiet > "build-after-$BranchName.log" 2>&1) {
            Write-Success "Build successful after merge"
        } else {
            $postErrors = (Select-String -Pattern "error " -Path "build-after-$BranchName.log").Count
            Write-Warning "Build has $postErrors errors"

            # Compare error counts
            if ($postErrors -gt $script:preMergeErrors) {
                Write-Error "NEW ERRORS INTRODUCED BY MERGE"
                Write-Host "Pre-merge errors: $script:preMergeErrors"
                Write-Host "Post-merge errors: $postErrors"
                Write-Host "New errors: $($postErrors - $script:preMergeErrors)"
                Write-Host ""
                Write-Warning "ROLLBACK OPTIONS:"
                Write-Host "  1. Automatic rollback (recommended): This will undo the merge"
                Write-Host "  2. Keep merge and fix errors manually"
                Write-Host ""

                $response = Read-Host "Rollback merge? [Y/n]"
                if ($response -notmatch "^[Nn]$") {
                    git reset --hard HEAD~1
                    Write-Info "Merge rolled back"
                    exit 1
                } else {
                    Write-Warning "Continuing with errors - you must fix them manually"
                }
            } else {
                Write-Success "No new errors introduced (pre-existing: $script:preMergeErrors)"
            }

            # Update error count for next iteration
            $script:preMergeErrors = $postErrors
        }
    }

    Write-Host ""
    Write-Success "MERGE COMPLETE: $BranchName"
    Start-Sleep -Seconds 1
}

# ==========================================
# EXECUTE MERGES
# ==========================================

Write-Header "EXECUTING MERGES"

$mergeCount = 0
foreach ($branch in $Branches) {
    $mergeCount++
    Merge-Branch -BranchName $branch.Name -Description $branch.Description -MergeNumber $mergeCount
}

# ==========================================
# POST-MERGE VERIFICATION
# ==========================================

Write-Header "POST-MERGE VERIFICATION"

Write-Host "Git Status:"
git status
Write-Host ""

Write-Host "Recent Commits:"
git log --oneline --graph -10
Write-Host ""

if (-not $SkipBuild) {
    Write-Host "Build Logs Generated:"
    Get-ChildItem -Path build-*.log -ErrorAction SilentlyContinue | Select-Object Name, Length
    Write-Host ""

    Write-Info "Running final build..."
    if (dotnet build -v quiet > build-final.log 2>&1) {
        Write-Success "Final build: SUCCESS"
    } else {
        $finalErrors = (Select-String -Pattern "error " -Path build-final.log).Count
        Write-Warning "Final build has $finalErrors errors"
        Write-Host "See build-final.log for details"
    }
}

# ==========================================
# SUCCESS SUMMARY
# ==========================================

Write-Header "ALL MERGES COMPLETE"

Write-Success "$mergeCount branches merged successfully"
Write-Host ""

Write-Host "Branches Merged:" -ForegroundColor White
foreach ($branch in $Branches) {
    Write-Host "  ✓ $($branch.Name)"
}
Write-Host ""

Write-Host "NEXT STEPS:" -ForegroundColor White
Write-Host "  1. Review changes: git log --oneline --graph -10"
if (-not $SkipBuild) {
    Write-Host "  2. Review build-final.log for any issues"
}
Write-Host "  3. Run tests: dotnet test"
Write-Host "  4. If satisfied, push: git push origin $BaseBranch"
Write-Host "  5. Create PR to master branch"
Write-Host ""

Write-Info "Branches NOT merged (by design):"
Write-Host "  - feature/data-containers-future (deferred work)"
Write-Host ""

Write-Host "See MERGE_STRATEGY.md for PR creation and next steps."
Write-Host ""
