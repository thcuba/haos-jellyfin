# Jellyfin Home Assistant Add-on

Jellyfin is a Free Software Media System that puts you in control of managing and streaming your media.

## Installation

1. Add this repository URL to your Home Assistant instance (Settings → Add-ons → Add-on Store → ⋮ → Repositories)
2. Search for "Jellyfin Media Server"
3. Click "Install" and then "Start"
4. Open the Web UI from the add-on page

## Hardware Acceleration

### VA-API (Intel and AMD)
GPUs are auto-detected via `/dev/dri`. The VA-API driver is chosen automatically
by libva (Intel `iHD`, AMD `radeonsi`).

### ARM / aarch64
On aarch64 (e.g. Raspberry Pi) hardware transcoding depends on the CPU's support;
the add-on runs fine, but GPU transcoding may be unavailable on some SBCs.

### Persistent storage
Jellyfin config, data, cache, and logs are persisted on the HAOS `/share/jellyfin`
directory (mapped `share:rw`), so your library, users, and settings survive add-on
restarts, updates, and re-installs.

## Architecture
- amd64
- aarch64

## Support
[GitHub Issues](https://github.com/thcuba/haos-jellyfin/issues)
