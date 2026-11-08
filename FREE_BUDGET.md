# KYOKA — Free-Budget Plan

## Current rule
Start at ₹0. Do not purchase assets, subscriptions, APIs, hosting, or services and do not enable paid features without the user's explicit permission.

## Current project dependencies
- Unity 6 Editor: use Unity Personal only if the user's eligibility and current Unity terms permit it.
- Universal Render Pipeline (URP): package version pinned in `Packages/manifest.json`.
- Input System and Unity Test Framework: package versions pinned in the manifest.
- Blender and Krita: optional free tools for original assets.
- Git and GitHub Free: source control and collaboration.

## Multiplayer cost warning
The current project has no online multiplayer implementation or hosting. Free-tier relay/backend services have eligibility, usage, region, and quota limits that can change. Verify current official pricing and terms before enabling any service. Large-scale authoritative multiplayer hosting, storage, bandwidth, moderation, and distribution may eventually require paid infrastructure.

## Secrets and billing
- Never commit private keys, service credentials, signing keys, or production tokens.
- Keep server secrets outside the Unity client.
- Do not activate a paid plan or incur charges without explicit approval.
- Prefer local development and mock adapters until a real provider is selected.
