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

## Keep context current

- Update `docs/PRD/V1.0.md` when a confirmed breakthrough, implemented feature, or accepted product change is not reflected there. Keep scope, status, and acceptance criteria accurate; distinguish proposals from implemented behavior.
- Update `docs/DEVELOPMENT_SETUP.md` when setup steps change or a new development issue is confirmed. Record the evidence and resolution without presenting an unverified cause as fact.
- Keep `README.md` aligned with the product's actual capabilities and setup instructions.
- When a prompt or conversation introduces a new lasting rule, ask the user whether to add it to `AGENTS.md` for future conversations. Persist it after confirmation; an explicit request to update `AGENTS.md` already provides that confirmation.
