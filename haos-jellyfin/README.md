# Jellyfin Home Assistant Add-on

Jellyfin is a Free Software Media System that puts you in control of managing and streaming your media.

## Installation

1. Add this repository URL to your Home Assistant instance (Settings → Add-ons → Add-on Store → ⋮ → Repositories)
2. Search for "Jellyfin Media Server"
3. Click "Install" and then "Start"
4. Open the Web UI from the add-on page

## Version

This add-on ships **Jellyfin 12.0 RC6** (preview release), built from the official `jellyfin/jellyfin` image.
A release candidate is intended for testing before the final 12.0 public release.

## Hardware Acceleration

Jellyfin can use hardware transcoding via VA-API (Intel/AMD), NVENC/NVDEC (NVIDIA), and V4L2 (ARM/Raspberry Pi).

### VA-API (Intel and AMD)
The add-on bundles the userspace VA-API drivers:
- **Intel**: `intel-media-va-driver-non-free` (iHD driver, QSV/VA-API)
- **AMD**: `mesa-va-drivers` (radeonsi)

GPUs are auto-detected via `/dev/dri`. The appropriate driver is chosen automatically by libva — do not force a single driver.

### NVIDIA (NVENC/NVDEC)
NVIDIA in Docker works through the **host**, not the container:
1. Install the NVIDIA proprietary driver on the HAOS host (`/dev/nvidia*` devices appear).
2. Install `nvidia-container-toolkit` on the host and configure Docker to use the `nvidia` runtime.
The add-on maps `/dev/nvidia*` and sets `NVIDIA_VISIBLE_DEVICES=all` + `NVIDIA_DRIVER_CAPABILITIES=compute,video,utility`. No container-side driver is installed (NVIDIA kernel modules cannot be bundled in the image).

### ARM / aarch64
On aarch64 (e.g. Raspberry Pi) the add-on bundles `libva-driver-v4l2-extra` for VA-API via V4L2. Support depends on the CPU/SBC — hardware transcoding may be limited or unavailable on some boards.

### Persistent storage
Jellyfin config, data, cache, and logs are persisted on the HAOS `/share/jellyfin`
directory (mapped `share:rw`), so your library, users, and settings survive add-on
restarts, updates, and re-installs.

## Architecture
- amd64
- aarch64

## Support
[GitHub Issues](https://github.com/thcuba/haos-jellyfin/issues)