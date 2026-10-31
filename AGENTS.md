# Agent instructions

## Read before working

- At the start of every task, read `AGENTS.md`, `README.md`, `docs/DEVELOPMENT_SETUP.md`, and `docs/PRD/V1.0.md`, plus other documentation relevant to the task.
- Keep changes aligned with the documented product scope, current milestone, and confirmed implementation status.

## Code and communication

- Design a readable, understandable codebase with clear names and small, focused types and methods.
- Follow the PRD's MVVM direction. Separate UI, state, and services as needed; avoid unnecessary abstractions and scaffolding.
- Always declare explicit access modifiers on C# types and members where the language permits them. Declare method return types explicitly, including `void` when applicable.
- Avoid unnecessary comments. Add a short comment only when it explains a non-obvious decision or constraint.
- Keep explanations and documentation direct and as short as clarity allows. Do not use emojis.

## Commits and branches

- Follow [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/): `<type>[optional scope]: <description>`, for example `feat(notes): add note search` or `fix(storage): preserve recovery backups`.
- Use `feat` for features, `fix` for bug fixes, and `docs`, `refactor`, `perf`, `test`, `build`, `ci`, `style`, `chore`, or `revert` as appropriate for other changes.
- Write concise, imperative descriptions without a trailing period. Use lowercase types and scopes, and keep each commit focused on one logical change.
- Mark breaking changes with `!` before the colon or a `BREAKING CHANGE:` footer, and explain the impact and migration steps.
- Name new branches `<type>/<short-kebab-case-description>`, using the appropriate commit type as the prefix. Examples: `feat/note-search`, `fix/window-position`, and `docs/setup-guide`.
- Keep each branch focused on one feature, fix, or maintenance task.

## Keep context current

- Update `docs/PRD/V1.0.md` when a confirmed breakthrough, implemented feature, or accepted product change is not reflected there. Keep scope, status, and acceptance criteria accurate; distinguish proposals from implemented behavior.
- Update `docs/DEVELOPMENT_SETUP.md` when setup steps change or a new development issue is confirmed. Record the evidence and resolution without presenting an unverified cause as fact.
- Keep `README.md` aligned with the product's actual capabilities and setup instructions.
- When a prompt or conversation introduces a new lasting rule, ask the user whether to add it to `AGENTS.md` for future conversations. Persist it after confirmation; an explicit request to update `AGENTS.md` already provides that confirmation.
