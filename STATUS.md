# Project status

## Current milestone

**Published: public source repository and GitHub macOS preview.** The isolated Unity project folder is `Mothlight-Last-Bloom`, and its product title is `Mothlight: Last Bloom`. The main `IndeGameLab` checkout was not changed. The playable bundle is `Builds/macOS/Mothlight.app`.

## Repository

- Public GitHub repository: https://github.com/Aniket-a14/mothlight-last-bloom
- Description: original 2D Unity arcade game for macOS about gathering fireflies, feeding a moonflower, and surviving a moonlit garden.
- Topics: `unity`, `unity-game`, `2d-game`, `indie-game`, `macos`, `arcade-game`, `game-development`, `procedural-art`.
- Unity Hub display name is `Mothlight: Last Bloom`; its source-control indicator is connected to this GitHub repository.
- The macOS app bundle is a local build artifact and is not tracked in Git.
- Public preview: https://github.com/Aniket-a14/mothlight-last-bloom/releases/tag/v0.1.0-preview
- Release archive: `Builds/releases/Mothlight-Last-Bloom-macOS-universal-preview.zip` (45 MB, universal arm64/x86_64); remote asset was downloaded and SHA-256 matched the local archive.
- Release blocker: the app is ad-hoc signed, has no Developer ID identity, and is not notarized. Gatekeeper assessment rejected it (`spctl -a -vv --type execute`). Do not call this a friction-free release.
- itch.io upload was not completed: the site presented an automated security verification page in the existing browser session. No bypass was attempted.

## Design and content

- `Mothlight: Last Bloom` is a three-minute top-down arcade game about gathering fireflies and feeding a moonflower while avoiding thorn spirits.
- Controls: WASD/arrow keys to move; hold Space or left mouse to gather; release to feed the nearby flower or repel spirits. Esc pauses; R restarts.
- Artwork is original and drawn procedurally at runtime. Ambient music and chimes are synthesized at runtime. No external asset packs, paid services, or network features are used. See `ASSET_PROVENANCE.md` and `RESEARCH_NOTES.md`.

## Verification evidence

- Unity 6000.3.26f1 Edit Mode suite: **12 passed, 0 failed**, from `test-results.xml`.
- Runtime Editor smoke from the preceding verification: gathering and delivery advanced (`carry=1 delivered=1`); pause/resume/restart/title/begin callbacks all fired in a fresh Play session.
- Standalone build: Unity macOS build succeeded with no reported build errors or warnings. The executable is a universal Mach-O containing both `arm64` and `x86_64` slices.
- Standalone launch: app process started and rendered the title/menu on the Apple M5 Metal device. Its Player.log contains initialization information and no error or exception entries.
- Standalone start and movement input were exercised through the computer-use bridge; movement toward the lower-right edge was visible. Repeated thorn hits ended the short run before I could confirm the corrected escape response at the exact corner.

## Corner movement fix (2026-11-01)

- Fixed the moth retaining velocity into a clamped wall: boundary contact now cancels only the outward component, so inward steering responds immediately from edges and corners. Direction acceleration increased from 20 to 32 world units/s² for more responsive short inputs.
- Added Edit Mode coverage for all four edges, blocked corner axes, and inward movement. Updated suite: **12 passed, 0 failed** (`test-results.xml`).
- Rebuilt the universal macOS app successfully with no build errors or warnings and relaunched it. The input bridge intermittently reports `noWindowsAvailable`, and rapid thorn hits ended the short run before direct visual confirmation of precise corner recovery.

## Mac health and performance snapshot

Read-only samples were taken on 2026-11-01 (Asia/Kolkata) while the standalone app was open at its idle title screen:

- Mothlight process: approximately **41 seconds elapsed**, **0.0% CPU** in the last `ps` sample, **364,704 KiB RSS**. This is an idle title-screen observation, not gameplay performance.
- System CPU: **87.27% idle** in the one-shot `top` sample; load averages were 7.06 / 6.38 / 5.38.
- macOS memory-pressure tool reported **62% free**. `top` separately reported 15 GiB used, 344 MiB unused, and 3,695 MiB compressed.
- Swap: **2,438 MiB used / 3,072 MiB total** at the sample time.
- `pmset -g therm`: no thermal warning and no performance warning recorded; no CPU power status recorded. This does not provide a measured temperature or sustained thermal test.
- No FPS, frame-time, power, or gameplay-load benchmark was captured. Avoid interpreting the idle CPU sample as an active-play performance result.

## Remaining review

- Play a full round on the target Mac to assess game feel, audio level, win/loss flow, and sustained thermals under active play.
- Install a Developer ID Application certificate in the local keychain and notarize the app before promoting it beyond the GitHub preview. This Mac currently has zero valid code-signing identities, and full Xcode is not installed.
- Publish the downloadable build on itch.io after signing/notarization and page assets are ready.
- Existing implementation is playable and locally built; the above human/runtime review is still outstanding.

## Change log

- 2026-11-01: Added this state/evidence record after producing and launching the universal macOS build; recorded the passive Mac health snapshot and the native input bridge limitation.
- 2026-11-01: Renamed the project folder and Unity product, published the source to a public GitHub repository with game-focused topics, linked it in Unity Hub, and rebuilt/launched the universal macOS app with the updated bundle identifier.
- 2026-11-01: Published GitHub prerelease `v0.1.0-preview` with the universal macOS archive and checksum; downloaded the public archive and verified the hash. Gatekeeper rejected the ad-hoc signed app, so itch.io and normal public distribution remain pending Developer ID signing and notarization.
