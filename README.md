# Mothlight: Last Bloom

An original 2D Unity arcade game with macOS and Windows x64 preview builds.

A tiny moth has one last night to wake the moonflower. Gather drifting fireflies with a held hum, carry them back, and release a bright pulse to feed the bloom and scatter the thorn spirits.

## Play

- **WASD / arrow keys** — flutter around the garden
- **Hold Space or left mouse** — hum and pull nearby fireflies into orbit (up to 6)
- **Release** — send the light outward; near the moonflower it feeds the bloom, and anywhere it knocks back thorn spirits
- **Esc** — pause; **R** — restart

Wake all three petals before the 3-minute night ends. Three thorn hits end the run. The title screen explains the loop before play begins.

## Run

Open `Mothlight.app` in `Builds/macOS/`. On first launch, macOS may ask you to confirm opening a locally built app. Open this project folder with Unity 6000.3.26f1 to inspect or edit the source.

## Download preview

A macOS universal preview is available on the [GitHub Releases page](https://github.com/Aniket-a14/mothlight-last-bloom/releases/tag/v0.1.0-preview). This preview is ad-hoc signed and not notarized; Gatekeeper may block it when downloaded. A Developer ID signed and notarized release is still needed for normal public distribution.

A Windows x64 preview is also on the [GitHub Releases page](https://github.com/Aniket-a14/mothlight-last-bloom/releases/tag/v0.1.0-preview). Extract the ZIP and run `Mothlight.exe` from the extracted folder. This build was created on macOS and has not yet been launched on a Windows computer.

The macOS app bundle is a local build artifact and is not tracked in Git. The project uses original procedural artwork and synthesized audio; it has no network features, paid assets, or external asset packs. See [STATUS.md](STATUS.md) for current verification and known limits, and [ASSET_PROVENANCE.md](ASSET_PROVENANCE.md) for asset details.
