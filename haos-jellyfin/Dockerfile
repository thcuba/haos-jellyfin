# Jellyfin for HAOS with Hardware Acceleration Support
# Base image: official jellyfin/jellyfin 12.0-rc6
ARG BUILD_FROM=jellyfin/jellyfin:12.0-rc6
FROM ${BUILD_FROM}

# Install additional tools and hardware acceleration drivers
# mesa-vulkan-drivers: Support for Vulkan (AMD/Intel)
# libva-drm2: Support for VA-API (Intel/AMD hardware transcoding)
# libvulkan1: Vulkan runtime support
# Using latest available versions from base image repositories for compatibility
RUN apt-get update && apt-get install -y --no-install-recommends \
    jq \
    curl \
    mesa-vulkan-drivers \
    libva-drm2 \
    libvulkan1 \
    && rm -rf /var/lib/apt/lists/* /tmp/* /var/tmp/*

# VA-API driver is auto-detected by libva at runtime (iHD on Intel,
# radeonsi on AMD, none on ARM). Do NOT force a single driver so that
# amd64 (Intel/AMD) and aarch64 all work correctly.

# The official jellyfin/jellyfin base image hard-codes `--datadir /config`
# in its ENTRYPOINT, which would ignore our persistent /share paths.
# Override the ENTRYPOINT with no hard-coded paths so that the JELLYFIN_*_DIR
# environment variables (set in config.yaml to /share/jellyfin/...) are honored.
# This keeps config/data/cache/log persistent across restarts.
ENTRYPOINT ["/jellyfin/jellyfin"]

# Expose Jellyfin port
EXPOSE 8096

# Health check using Jellyfin's public API endpoint
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f --silent http://localhost:8096/System/Info/Public || exit 1