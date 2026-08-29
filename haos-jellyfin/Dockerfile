# Jellyfin for HAOS with Hardware Acceleration Support
# Base image: official jellyfin/jellyfin 12.0-rc6
ARG BUILD_FROM=jellyfin/jellyfin:12.0-rc6
FROM ${BUILD_FROM}

# Build-time arch target (amd64 | arm64) used to install per-arch GPU drivers.
# HAOS exposes the device via /dev/dri and /dev/shm (mapped in config.yaml).
ARG TARGETARCH

# Install additional tools and hardware-acceleration drivers.
# The base image already ships the VA-API/Vulkan runtimes (libva-drm2,
# libvulkan1, mesa-vulkan-drivers, Intel OpenCL for tone-mapping).
# Here we add the vendor userspace drivers that jellyfin-ffmpeg loads via VA-API:
#   - amd64 (Intel)  : intel-media-va-driver-non-free  -> iHD driver (QSV/VA-API)
#   - amd64 (AMD)    : mesa-va-drivers                 -> radeonsi driver
#   - arm64 (ARM/RPi): none extra needed; VA-API goes through the V4L2/stateless
#                      kernel drivers, whose userspace support ships in the base
#                      image via mesa-va-drivers.
# The apt non-free component is enabled on-demand for the Intel driver.
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        jq \
        curl \
        vainfo \
        libva-drm2 \
        mesa-vulkan-drivers \
        libvulkan1 \
    && if [ "$TARGETARCH" = "amd64" ]; then \
        sed -i -E 's/^Components:(.*)/Components:\1 contrib non-free/' /etc/apt/sources.list.d/*.sources 2>/dev/null \
            && apt-get update \
            && apt-get install -y --no-install-recommends \
                intel-media-va-driver-non-free \
                mesa-va-drivers; \
    fi \
    && rm -rf /var/lib/apt/lists/* /tmp/* /var/tmp/*

# VA-API driver is auto-detected by libva at runtime (iHD on Intel,
# radeonsi on AMD, V4L2 on ARM). Do NOT force a single driver so that
# amd64 (Intel/AMD) and aarch64 all work correctly.

# NVIDIA support: the docker image cannot bundle NVIDIA kernel drivers
# (they live on the host via the NVIDIA proprietary driver +
# nvidia-container-toolkit). We only expose the standard passthrough env vars;
# the HAOS add-on simply maps the NVIDIA device. The base image already sets
# NVIDIA_VISIBLE_DEVICES/NVIDIA_DRIVER_CAPABILITIES; we assert them here.
ENV NVIDIA_VISIBLE_DEVICES=all \
    NVIDIA_DRIVER_CAPABILITIES=compute,video,utility

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