# KYOKA — Device Compatibility & Optimization Matrix

Hardware specifications, performance presets, rendering tiering, and compatibility benchmarks for Windows PC and Android devices.

---

## 1. Android Device Tiers & Specifications

| Tier | Minimum Hardware | Target Settings | Target FPS | Notes |
|---|---|---|---|---|
| **Tier 1 (Entry)** | 4GB RAM, Snapdragon 680 / Helio G88, Mali-G52 / Adreno 610 | **LOW**: Resolution 0.7x, Shadows OFF, Render Distance 150m, 30 FPS cap | 30 FPS | Optimized low memory footprint, simplified shaders |
| **Tier 2 (Mid-Range)** | 6GB-8GB RAM, Snapdragon 778G / Dimensity 7050, Mali-G68 / Adreno 642L | **MEDIUM**: Resolution 0.85x, Hard Shadows ON, Render Distance 250m, 60 FPS cap | 45-60 FPS | Balanced visuals and battery preservation |
| **Tier 3 (Flagship)** | 8GB-16GB RAM, Snapdragon 8 Gen 1/2/3, Dimensity 9200/9300, Adreno 740+ | **HIGH**: Resolution 1.0x, Soft Shadows, Full Post-processing, 60-120 FPS | 60-120 FPS | Cel-shaded effects, full particle density |

### Supported Android OS:
- **Minimum**: Android 7.0 (API Level 24).
- **Target**: Android 14 (API Level 34).
- **Architecture**: 64-bit ARM (`arm64-v8a`). Note: 32-bit `armeabi-v7a` is deprecated and not supported.

---

## 2. Windows PC Hardware Specifications

| Component | Minimum Requirement | Recommended Specification |
|---|---|---|
| **Operating System** | Windows 10 (64-bit) | Windows 11 (64-bit) |
| **Processor** | Intel Core i3-8100 / AMD Ryzen 3 1200 | Intel Core i5-11400 / AMD Ryzen 5 5600 |
| **Memory** | 8 GB RAM | 16 GB RAM |
| **Graphics** | NVIDIA GTX 960 / AMD RX 560 (2GB VRAM, DX11/Vulkan) | NVIDIA RTX 3060 / AMD RX 6600 (6GB+ VRAM, DX12) |
| **DirectX** | Version 11 | Version 12 |
| **Storage** | 5 GB available space | SSD with 10 GB available space |

---

## 3. Mobile Performance Optimization Principles
1. **Dynamic Resolution Scaling**: Low and Medium presets scale viewport resolution to maintain consistent 30/60 FPS without GPU thermal throttling.
2. **Object Pooling**: Projectiles, slash effects, and damage indicators utilize pooled game objects rather than constant runtime Instantiate/Destroy allocations.
3. **Occlusion Culling & Streaming**: The 6 major regions are spatially segregated (`regionRoots` dictionaries) so that only the active region consumes draw calls and physics cycles.
4. **Physics Optimization**: Fixed timestep set to 0.02s (50Hz), with ground vehicle rigidbodies configured with rotational constraints to eliminate unnecessary physics recalculations.

