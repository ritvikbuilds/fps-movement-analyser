---
name: brainstorming
description: "You MUST use this before any creative work - creating features, building components, adding functionality, or modifying behavior. Explores user intent, requirements and design before implementation using OpenSpec format."
---

# Brainstorming Ideas Into OpenSpec Proposals

## Overview

Help turn ideas into fully formed OpenSpec change proposals through natural collaborative dialogue.

Start by understanding the current project context, then ask questions one at a time to refine the idea. Once you understand what you're building, create an OpenSpec change proposal with proper specs and scenarios.

## The Process

**Understanding the idea:**
- Run `openspec list` and `openspec list --specs` to understand current state
- Review `openspec/project.md` for conventions
- Ask questions one at a time to refine the idea
- Prefer multiple choice questions when possible, but open-ended is fine too
- Only one question per message - if a topic needs more exploration, break it into multiple questions
- Focus on understanding: purpose, constraints, success criteria

**Exploring approaches:**
- Propose 2-3 different approaches with trade-offs
- Present options conversationally with your recommendation and reasoning
- Lead with your recommended option and explain why

**Creating the OpenSpec proposal:**
Once you understand what you're building:

1. Choose a unique verb-led `change-id` (kebab-case: `add-`, `update-`, `remove-`, `refactor-`)
2. Create `openspec/changes/<change-id>/` directory
3. Write `proposal.md`:
   - Why: 1-2 sentences on problem/opportunity
   - What Changes: bullet list (mark breaking changes with **BREAKING**)
   - Impact: affected specs and code
4. Create spec deltas in `specs/<capability>/spec.md`:
   - Use `## ADDED|MODIFIED|REMOVED Requirements` sections
   - Every requirement MUST have at least one `#### Scenario:`
5. Write `tasks.md` with implementation checklist
6. Add `design.md` only if needed (cross-cutting, new dependencies, security/perf concerns)
7. Run `openspec validate <change-id> --strict` and fix any issues

## After the Proposal

**Validation:**
- Run `openspec validate <change-id> --strict`
- Present the proposal for user approval
- Do NOT start implementation until proposal is approved

**Implementation (if continuing):**
- Follow tasks.md sequentially
- Mark tasks complete as you go
- After deployment, archive with `openspec archive <change-id> --yes`

## Key Principles

- **One question at a time** - Don't overwhelm with multiple questions
- **Multiple choice preferred** - Easier to answer than open-ended when possible
- **YAGNI ruthlessly** - Remove unnecessary features from all designs
- **Explore alternatives** - Always propose 2-3 approaches before settling
- **Scenarios are mandatory** - Every requirement needs at least one `#### Scenario:`
- **Validate before sharing** - Always run `openspec validate --strict`


