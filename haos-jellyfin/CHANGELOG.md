# Changelog

## 12.0.6
- Upgraded Jellyfin media server to upstream **12.0-rc6** (preview), switching the base image from `linuxserver/jellyfin` to the official `jellyfin/jellyfin`.
- Moved add-on versioning to the `12.0.x` scheme used for release candidates (RC6 = 12.0.6).
- Persist Jellyfin config/data/cache/log on HAOS `/share/jellyfin` so library, users, and settings survive restarts, updates, and re-installs. The Dockerfile now overrides the official image ENTRYPOINT so the `JELLYFIN_*_DIR` variables are honored.
- Auto-detect VA-API driver (iHD on Intel, radeonsi on AMD) instead of forcing iHD (fix #473).
- Fixed/removed dead add-on options and redundant image tags (fix #474, #476).
- Added userspace GPU drivers: Intel `intel-media-va-driver-non-free`, AMD `mesa-va-drivers`, ARM/RPi `libva-driver-v4l2-extra`. Added `vainfo` diagnostic tool. Set NVIDIA passthrough env vars (`NVIDIA_VISIBLE_DEVICES`, `NVIDIA_DRIVER_CAPABILITIES`) and mapped `/dev/nvidia*` devices.

## 10.11.11
- Aligned Jellyfin media server to upstream stable version 10.11.11.
- Enabled native GPU hardware acceleration support via the `video: true` declaration to ensure seamless transcoding under Home Assistant OS (HAOS).
- Fully validated, compatible, and optimized for HAOS system resources and architecture configurations (amd64, aarch64).

## 1.0.0
- Initial release of Jellyfin HAOS Add-on.
- Support for amd64 and aarch64 architectures.
- Hardware acceleration support (VA-API, Vulkan).