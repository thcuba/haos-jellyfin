# Jellyfin for HAOS - With Hardware Acceleration

Jellyfin is a free software media system that puts you in control of managing and streaming your media.

<<<<<<< HEAD
## Key Features
- Full hardware transcoding support (VA-API & Vulkan)
- H.264, H.265, VP9, AV1 decoding
- Multi-user support with parental controls
- DLNA, Chromecast, and mobile apps
- Direct integration with Home Assistant OS
=======
<p align="center">
<img alt="Logo Banner" src="https://raw.githubusercontent.com/jellyfin/jellyfin-ux/master/branding/SVG/banner-logo-solid.svg?sanitize=true"/>
<br/>
<br/>
<a href="https://github.com/jellyfin/jellyfin"><img alt="GPL 2.0 License" src="https://img.shields.io/github/license/jellyfin/jellyfin.svg"/></a>
<a href="https://github.com/jellyfin/jellyfin/releases"><img alt="Current Release" src="https://img.shields.io/github/release/jellyfin/jellyfin.svg"/></a>
<a href="https://translate.jellyfin.org/projects/jellyfin/jellyfin-core/?utm_source=widget"><img alt="Translation Status" src="https://translate.jellyfin.org/widgets/jellyfin/-/jellyfin-core/svg-badge.svg"/></a>
<a href="https://hub.docker.com/r/jellyfin/jellyfin"><img alt="Docker Pull Count" src="https://img.shields.io/docker/pulls/jellyfin/jellyfin.svg"/></a>
<br/>
<a href="https://opencollective.com/jellyfin"><img alt="Donate" src="https://img.shields.io/opencollective/all/jellyfin.svg?label=backers"/></a>
<a href="https://features.jellyfin.org"><img alt="Submit Feature Requests" src="https://img.shields.io/badge/fider-vote%20on%20features-success.svg"/></a>
<a href="https://matrix.to/#/#jellyfinorg:matrix.org"><img alt="Chat on Matrix" src="https://img.shields.io/matrix/jellyfinorg:matrix.org.svg?logo=matrix"/></a>
<a href="https://github.com/jellyfin/jellyfin/releases.atom"><img alt="Release RSS Feed" src="https://img.shields.io/badge/rss-releases-ffa500?logo=rss" /></a>
<a href="https://github.com/jellyfin/jellyfin/commits/master.atom"><img alt="Master Commits RSS Feed" src="https://img.shields.io/badge/rss-commits-ffa500?logo=rss" /></a>
</p>
>>>>>>> upstream/release-12.z

## Hardware Acceleration Setup

### For Intel/AMD GPUs (VA-API)
The add-on automatically uses VA-API when available. Ensure:
1. Your GPU is supported by the Mesa VA-API drivers
2. The `/dev/dri` device is accessible (handled by HAOS)

### For Modern GPUs (Vulkan)
Vulkan support is enabled by default for:
- AMD GPUs (RADV driver)
- Intel GPUs (Intel ANV driver)
- NVIDIA GPUs (via Nouveau - limited support)

### To Enable in Jellyfin UI
1. Go to Dashboard → Playback → Transcoding
2. Set Hardware Acceleration to:
   - **VA-API** (for Intel/AMD)
   - **Vulkan** (for AMD/Intel)
3. Click Save

### Verifying Hardware Acceleration
Check the Jellyfin dashboard or logs for:
```
Hardware acceleration: VA-API enabled
Hardware acceleration: Vulkan enabled
```

## Installation
1. Add this repository to Home Assistant OS Supervisor
2. Install the "Jellyfin Media Server" add-on
3. Start the add-on and configure your media folders
4. Enable hardware acceleration in Jellyfin Dashboard

## Support
For issues, please visit: https://github.com/jellyfin/jellyfin