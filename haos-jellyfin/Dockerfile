# Jellyfin for HAOS with FFmpeg and Hardware Acceleration Support
# Base image: linuxserver/jellyfin (highly optimized)

FROM linuxserver/jellyfin:latest

# Install additional tools and hardware acceleration drivers
# mesa-vulkan-drivers: Support for Vulkan (AMD/Intel)
# libva-drm2 & vainfo: Support for VA-API (Intel/AMD hardware transcoding)
# libvulkan1: Vulkan runtime support
# vulkan-tools: Vulkan diagnostics
RUN apt-get update && apt-get install -y --no-install-recommends \
    jq \
    curl \
    gnupg2 \
    mesa-vulkan-drivers \
    libva-drm2 \
    libvulkan1 \
    vulkan-tools \
    vainfo \
    && rm -rf /var/lib/apt/lists/* /tmp/* /var/tmp/*

# Set environment variables for hardware acceleration
ENV LIBVA_DRIVER_NAME=radeonsi
ENV LIBVA_DRIVERS_PATH=/usr/lib/x86_64-linux-gnu/dri
ENV VK_ICD_FILENAMES=/usr/share/vulkan/icd.d/radeon_icd.x86_64.json:/usr/share/vulkan/icd.d/intel_icd.x86_64.json

# Create jellyfin user and ensure it has permissions for GPU access
RUN usermod -a -G video jellyfin && \
    usermod -a -G render jellyfin

# Expose Jellyfin port
EXPOSE 8096

# Health check to verify Jellyfin is responding
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8096/health || exit 1

# Run jellyfin with proper settings
CMD ["/init"]
