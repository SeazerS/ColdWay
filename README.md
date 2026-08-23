# ColdWay

Unity FPS Survival — a 3D survival game developed during a 4-person internship at NevGame (Feb–Jun 2026).

**Status:** ~80% complete. Development paused due to a missing final character asset — the project currently uses a placeholder/standard character, so it hasn't been packaged into a playable build.

**Systems I designed and coded:**

- **Companion AI** (`KopekAI.cs`) — the project's most complex system. A reactive AI for the dog companion: monitors the player's temperature and energy in real time, automatically navigates to the nearest safe point (fire or storm shelter) when either drops critically, reacts to warn the player, and includes stuck-detection recovery.
- **Temperature system** (`SicaklikSistemi.cs`) — the core survival pressure. Zone-based cooldown rates, modified by wind, wetness, time of day, and cave shelter; recovers near fire based on distance and fire intensity; feeds into movement speed and a checkpoint/death system.
- **Storm/weather event system** (`FirtinaSistemi.cs`) — a randomized weather event that coordinates fog density, lighting exposure, wind and snow intensity, fire extinguishing, companion AI behavior, audio, and UI warnings, all through smoothed transitions rather than instant switches.
- **Fire-lighting mechanic** (`Player_Mech.cs`, `AtesSistemi.cs`) — a multi-phase interaction: place a wood base, consume wood from the inventory, hold-to-fill a lighting progress bar, then ignite.
- **Energy system** (`EnerjiKontrol.cs`) — a second survival resource with its own zone-based drain and recovery items.
- **Day/night cycle** (`GeceGunduzSistemi.cs`) and **save/load system** (`SaveSistemi.cs`).
- Supporting systems: tree cutting (`AgacKesme.cs`), footprint/snow-tracking (`AyakIziSistemi.cs`), and checkpoint/respawn (`CheckpointSistemi.cs`).

**Not my work:** the `UI_Sc` and `Inventory` folders were built by another team member.
