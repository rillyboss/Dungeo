# Claude Code Hooks - Workflow Enhancement Recommendations

**Purpose:** Automate common workflows and enforce quality standards
**Last Updated:** 2025-01-12

---

## Overview

Claude Code supports **hooks** - shell commands that execute automatically in response to events. This document recommends hooks to enhance your TestRPGGame development workflow.

---

## Recommended Hooks

### 1. Pre-Tool-Call Hook: Automatic Testing

**Purpose:** Run tests before potentially breaking changes
**Trigger:** Before executing tool calls that modify code

**Configuration:**
```json
{
  "hooks": {
    "preToolCall": {
      "command": "dotnet test --no-build --verbosity quiet",
      "description": "Run tests before code modifications",
      "enabled": true,
      "toolPatterns": ["Edit", "Write"],
      "continueOnFailure": false
    }
  }
}
```

**Benefits:**
- Catches regressions immediately
- Prevents committing broken code
- Provides instant feedback

**Considerations:**
- Adds ~2-3 seconds per edit
- May be too slow for rapid iterations
- Consider disabling for non-critical changes

### 2. Post-Tool-Call Hook: Format & Build

**Purpose:** Ensure code is formatted and builds after changes
**Trigger:** After file modifications

**Configuration:**
```json
{
  "hooks": {
    "postToolCall": {
      "command": "dotnet format && dotnet build --no-restore",
      "description": "Format and build after changes",
      "enabled": true,
      "toolPatterns": ["Edit", "Write", "NotebookEdit"]
    }
  }
}
```

**Benefits:**
- Consistent code formatting
- Immediate build verification
- Catches compilation errors early

**Alternative (faster):**
```json
{
  "hooks": {
    "postToolCall": {
      "command": "dotnet build --no-restore",
      "description": "Quick build check",
      "enabled": true
    }
  }
}
```

### 3. User-Prompt-Submit Hook: Pre-Commit Check

**Purpose:** Verify project state before user submits prompts
**Trigger:** When user is about to submit a prompt

**Configuration:**
```json
{
  "hooks": {
    "userPromptSubmit": {
      "command": "./scripts/pre-commit-check.sh",
      "description": "Verify tests and build before starting work",
      "enabled": true,
      "frequency": "once-per-session"
    }
  }
}
```

**Script:** `scripts/pre-commit-check.sh`
```bash
#!/bin/bash
set -e

echo "🔍 Running pre-commit checks..."

# Check if tests pass
if ! dotnet test --no-build --verbosity quiet > /dev/null 2>&1; then
    echo "❌ Tests failing! Fix before continuing."
    exit 1
fi

# Check if build succeeds
if ! dotnet build --no-restore > /dev/null 2>&1; then
    echo "❌ Build failing! Fix before continuing."
    exit 1
fi

echo "✅ All checks passed!"
exit 0
```

**Benefits:**
- Ensures clean starting state
- Catches lingering issues
- Prevents building on broken foundation

### 4. Post-Session Hook: Generate Summary

**Purpose:** Create session summary for documentation
**Trigger:** End of session

**Configuration:**
```json
{
  "hooks": {
    "postSession": {
      "command": "./scripts/session-summary.sh",
      "description": "Generate session summary",
      "enabled": true
    }
  }
}
```

**Script:** `scripts/session-summary.sh`
```bash
#!/bin/bash

echo "📊 Session Summary Generated:"
echo "- Tests: $(dotnet test --no-build --verbosity quiet 2>&1 | grep -oP '\d+(?= passed)')/375"
echo "- Files modified: $(git status --short | wc -l)"
echo "- Lines changed: $(git diff --stat | tail -1)"
```

---

## Workflow-Specific Hooks

### Hook 5: Data File Validation

**Purpose:** Validate JSON data files after edits
**Use Case:** Editing game content files

**Configuration:**
```json
{
  "hooks": {
    "postToolCall": {
      "command": "node scripts/validate-json.js",
      "description": "Validate JSON data files",
      "enabled": true,
      "filePatterns": ["*.json"],
      "continueOnFailure": true
    }
  }
}
```

**Script:** `scripts/validate-json.js`
```javascript
const fs = require('fs');
const path = process.argv[2];

try {
    JSON.parse(fs.readFileSync(path, 'utf8'));
    console.log(`✅ ${path} is valid JSON`);
} catch (e) {
    console.error(`❌ ${path} is invalid: ${e.message}`);
    process.exit(1);
}
```

### Hook 6: Test Coverage Report

**Purpose:** Track test coverage after test modifications
**Use Case:** Adding or modifying tests

**Configuration:**
```json
{
  "hooks": {
    "postToolCall": {
      "command": "dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura",
      "description": "Generate coverage report",
      "enabled": true,
      "filePatterns": ["*Tests.cs"]
    }
  }
}
```

### Hook 7: Architecture Compliance Check

**Purpose:** Ensure no Console I/O in game logic
**Use Case:** After modifying game logic files

**Configuration:**
```json
{
  "hooks": {
    "postToolCall": {
      "command": "./scripts/check-console-usage.sh",
      "description": "Verify no Console I/O in game logic",
      "enabled": true,
      "filePatterns": ["TestRPGGame/**/*.cs", "!TestRPGGame/Interfaces/**/*.cs", "!TestRPGGame/UI/**/*.cs"]
    }
  }
}
```

**Script:** `scripts/check-console-usage.sh`
```bash
#!/bin/bash

FILE=$1
if grep -q "Console\." "$FILE"; then
    echo "❌ Warning: Console I/O found in $FILE"
    echo "Game logic should use IGameInterface instead!"
    exit 1
fi

echo "✅ No Console I/O in $FILE"
exit 0
```

---

## Hook Combinations

### Aggressive Quality Mode (Development)
```json
{
  "hooks": {
    "preToolCall": {
      "command": "dotnet build --no-restore",
      "toolPatterns": ["Edit", "Write"]
    },
    "postToolCall": {
      "command": "dotnet test --no-build --verbosity quiet && ./scripts/check-console-usage.sh",
      "toolPatterns": ["Edit", "Write"]
    }
  }
}
```

**Use When:** Working on critical features, refactoring

### Balanced Mode (Recommended)
```json
{
  "hooks": {
    "preToolCall": {
      "command": "dotnet build --no-restore",
      "toolPatterns": ["Edit"]
    },
    "postToolCall": {
      "command": "./scripts/quick-validation.sh",
      "toolPatterns": ["Edit", "Write"]
    }
  }
}
```

### Fast Iteration Mode (Prototyping)
```json
{
  "hooks": {
    "postToolCall": {
      "command": "dotnet build --no-restore",
      "enabled": true
    }
  }
}
```

**Use When:** Rapid prototyping, content creation

---

## Git Integration Hooks

### Hook 8: Pre-Commit Validation

**Purpose:** Enforce quality before commits
**Configuration:** Add to `.git/hooks/pre-commit` (if using git hooks)

```bash
#!/bin/bash
set -e

echo "Running pre-commit checks..."

# Run tests
dotnet test --no-build --verbosity quiet

# Check for Console usage in game logic
for file in $(git diff --cached --name-only --diff-filter=ACM | grep "\.cs$"); do
    if [[ $file =~ ^TestRPGGame/(Systems|Combat|Entities)/ ]]; then
        if grep -q "Console\." "$file"; then
            echo "❌ Console I/O found in $file"
            exit 1
        fi
    fi
done

# Validate JSON files
for file in $(git diff --cached --name-only --diff-filter=ACM | grep "\.json$"); do
    if ! python -m json.tool "$file" > /dev/null 2>&1; then
        echo "❌ Invalid JSON in $file"
        exit 1
    fi
done

echo "✅ All pre-commit checks passed!"
```

---

## Performance Considerations

### Hook Execution Time

| Hook | Typical Duration | Impact |
|------|-----------------|--------|
| `dotnet build --no-restore` | 1-2s | Low |
| `dotnet test --no-build` | 2-3s | Medium |
| `dotnet format` | 1-2s | Low |
| `dotnet test /p:CollectCoverage=true` | 4-5s | High |
| JSON validation | <0.5s | Very Low |
| Console usage check | <0.5s | Very Low |

### Optimization Strategies

1. **Conditional Execution:** Use `filePatterns` to target specific files
2. **Incremental Testing:** Only test affected projects
3. **Parallel Execution:** Run independent checks concurrently
4. **Caching:** Leverage build caching (`--no-restore`, `--no-build`)

---

## Hook Management

### Enabling/Disabling Hooks

**During rapid iteration:**
```bash
# Disable all hooks temporarily
claude-code --no-hooks

# Or set in config
{
  "hooks": {
    "enabled": false
  }
}
```

**For specific scenarios:**
```json
{
  "hooks": {
    "preToolCall": {
      "enabled": false  // Disable just this hook
    }
  }
}
```

### Debugging Hooks

**Add verbose output:**
```json
{
  "hooks": {
    "postToolCall": {
      "command": "echo 'Running post-tool-call hook...' && dotnet build",
      "verbose": true
    }
  }
}
```

**Test hook manually:**
```bash
# Run the hook command directly
dotnet test --no-build --verbosity quiet
echo "Exit code: $?"
```

---

## Recommended Configuration for TestRPGGame

### `~/.claude-code/config.json` (or project-level)

```json
{
  "hooks": {
    "preToolCall": {
      "command": "dotnet build --no-restore --verbosity quiet",
      "description": "Quick build check before edits",
      "enabled": true,
      "toolPatterns": ["Edit", "Write"],
      "continueOnFailure": false
    },
    "postToolCall": {
      "command": "./scripts/post-edit-check.sh",
      "description": "Validate changes after editing",
      "enabled": true,
      "toolPatterns": ["Edit", "Write"],
      "continueOnFailure": true
    },
    "userPromptSubmit": {
      "command": "dotnet test --no-build --verbosity minimal",
      "description": "Verify tests before starting new work",
      "enabled": true,
      "frequency": "once-per-session"
    }
  }
}
```

### `scripts/post-edit-check.sh`

```bash
#!/bin/bash

FILE=$1

# Quick format check
if [[ $FILE == *.cs ]]; then
    dotnet format --verify-no-changes --include "$FILE" > /dev/null 2>&1
    if [ $? -ne 0 ]; then
        echo "ℹ️  File needs formatting (will auto-format)"
        dotnet format --include "$FILE" > /dev/null 2>&1
    fi
fi

# Console usage check for game logic
if [[ $FILE =~ ^TestRPGGame/(Systems|Combat|Entities|Equipment|Abilities)/ ]]; then
    if grep -q "Console\." "$FILE"; then
        echo "⚠️  Warning: Console I/O in game logic file: $FILE"
        exit 1
    fi
fi

# JSON validation
if [[ $FILE == *.json ]]; then
    if ! python -m json.tool "$FILE" > /dev/null 2>&1; then
        echo "❌ Invalid JSON: $FILE"
        exit 1
    fi
fi

echo "✅ Validation passed: $FILE"
exit 0
```

---

## Setup Instructions

### 1. Create Scripts Directory
```bash
mkdir -p scripts
chmod +x scripts/*.sh
```

### 2. Add Hook Configuration
Create or edit `~/.claude-code/config.json` with recommended hooks.

### 3. Test Hooks
```bash
# Test individually
dotnet build --no-restore
dotnet test --no-build --verbosity quiet
./scripts/post-edit-check.sh TestRPGGame/GameCore.cs
```

### 4. Enable Gradually
Start with minimal hooks, add more as workflow stabilizes.

---

## Troubleshooting

### Hook Fails Every Time
- Check exit codes: `echo $?` after running command
- Verify paths are correct
- Test command manually first

### Hook Too Slow
- Use `--no-build`, `--no-restore` flags
- Target specific files with `filePatterns`
- Disable coverage collection

### Hook Interferes with Workflow
- Set `continueOnFailure: true`
- Reduce frequency
- Temporarily disable with `enabled: false`

---

## Summary

**Recommended Starting Point:**
1. **Pre-tool-call:** Quick build check
2. **Post-tool-call:** Format + validation
3. **User-prompt-submit:** Test verification (once per session)

**Add Later:**
- Coverage tracking
- Architecture compliance checks
- Git pre-commit hooks

**Always Remember:**
- Start minimal, add incrementally
- Monitor performance impact
- Adjust based on workflow needs

---

**Next Steps:**
1. Create `scripts/` directory
2. Add recommended config to Claude Code settings
3. Test hooks manually before enabling
4. Iterate based on experience
