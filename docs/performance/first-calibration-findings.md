# First PSO calibration findings

Machine:
- Intel Core i7-6900K
- 8 physical / 16 logical cores
- .NET 10.0.10
- Windows 11

The first benchmark showed:

| workload | sequential | parallel | parallel / sequential |
|---|---:|---:|---:|
| 32x8 | 171.4 us | 1058.6 us | 6.18 |
| 40x32 | 612.4 us | 1612.7 us | 2.63 |
| 64x64 | 1916.4 us | 4496.8 us | 2.35 |
| 128x64 | 4984.9 us | 5065.6 us | 1.02 |
| 256x64 | 19513.8 us | 16321.1 us | 0.84 |
| 256x256 | 40166.9 us | 20294.3 us | 0.51 |

Interpretation before v0.8.1:
- parallel execution becomes clearly useful between workloads 8192 and 16384;
- the existing threshold 8192 is therefore too aggressive for this machine;
- however the threshold must not yet be changed because structural overheads were identified;
- 128x64 -> 256x64 sequential scaling is ~3.91x rather than ~2x, indicating
  that materializing the fully connected graph contaminates the canonical fast path;
- the parallel implementation adds a roughly fixed managed-allocation overhead,
  consistent with repeated partitioner/scheduling setup.

v0.8.1 removes those two structural effects and fuses objective evaluation with
personal-best update.

The parallel crossover must be measured again after v0.8.1.