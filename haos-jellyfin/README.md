# Jellyfin Home Assistant Add-on

Jellyfin is a Free Software Media System that puts you in control of managing and streaming your media.

## Installation

1. Add this repository URL to your Home Assistant instance (Settings → Add-ons → Add-on Store → ⋮ → Repositories)
2. Search for "Jellyfin Media Server"
3. Click "Install" and then "Start"
4. Open the Web UI from the add-on page

## Hardware Acceleration

### Intel VA-API (default)
Intel GPUs are detected automatically via `/dev/dri`. VA-API is enabled by default.

### Additional options
Available in the add-on configuration:
- `log_level`: debug, info, notice, warning, error, fatal
- `host`: listen address (default: 0.0.0.0)
- `port`: listen port (default: 8096)

## Architecture
- amd64
- aarch64

## Support
[GitHub Issues](https://github.com/thcuba/haos-jellyfin/issues)
