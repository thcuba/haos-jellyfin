# Changelog

## 10.11.12
- Persist Jellyfin config/data/cache/log on HAOS `/share/jellyfin` so library, users, and settings survive restarts, updates, and re-installs (#472).
- Auto-detect VA-API driver (iHD on Intel, radeonsi on AMD) instead of forcing iHD, unblocking transcoding on AMD and aarch64 (#473).
- Remove dead add-on options (`log_level`, `host`, `port`) that no script applied (#474).
- Disable auto-scheduled version bump (add-on version now independent from upstream) (#475).
- Make the CI smoke test genuinely blocking and clean up redundant image tags (#476).

## 10.11.11
- Aligned Jellyfin media server to upstream stable version 10.11.11.
- Enabled native GPU hardware acceleration support via the `video: true` declaration to ensure seamless transcoding under Home Assistant OS (HAOS).
- Fully validated, compatible, and optimized for HAOS system resources and architecture configurations (amd64, aarch64).

## 1.0.0
- Initial release of Jellyfin HAOS Add-on.
- Support for amd64 and aarch64 architectures.
- Hardware acceleration support (VA-API, Vulkan).
