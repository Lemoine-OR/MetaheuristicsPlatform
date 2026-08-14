# PSO parallel crossover protocol

After v0.8.1:

- 1280 work units (40x32): parallel is slower;
- 4096 work units (64x64): parallel is already faster.

Focused crossover suite:

| Swarm | Dimension | Work units |
|---:|---:|---:|
| 48 | 32 | 1536 |
| 56 | 32 | 1792 |
| 64 | 32 | 2048 |
| 80 | 32 | 2560 |
| 96 | 32 | 3072 |
| 112 | 32 | 3584 |
| 128 | 32 | 4096 |

Run:

```powershell
.\benchmarks\run-pso-calibration.ps1 -Suite Crossover
```

Then test shape sensitivity at exactly 4096 work units:

```powershell
.\benchmarks\run-pso-calibration.ps1 -Suite Shape
```

Shapes:
- 32x128
- 64x64
- 128x32
- 256x16

If ratios differ materially, Auto should use particle count and dimension separately
rather than only their product.