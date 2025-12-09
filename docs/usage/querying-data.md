# Querying Data

> SQL query examples for common use cases.

**Navigation**: [Home](../../README.md) | [Importing Data](./importing-data.md) | [Reports](./reports.md)

---

## Current State Queries

### All VMs

```sql
SELECT
    VM,
    Powerstate,
    CPUs,
    Memory,
    Host,
    Cluster,
    Datacenter
FROM Current.vInfo
ORDER BY VM;
```

### Powered On VMs

```sql
SELECT VM, CPUs, Memory, Host
FROM Current.vInfo
WHERE Powerstate = 'poweredOn'
ORDER BY VM;
```

### VMs by Cluster

```sql
SELECT
    Cluster,
    COUNT(*) AS VMCount,
    SUM(CPUs) AS TotalvCPUs,
    SUM(Memory) AS TotalMemoryMiB
FROM Current.vInfo
WHERE Powerstate = 'poweredOn'
GROUP BY Cluster
ORDER BY VMCount DESC;
```

### Large VMs (>8 vCPU or >32GB RAM)

```sql
SELECT VM, CPUs, Memory, Host, Cluster
FROM Current.vInfo
WHERE CPUs > 8 OR Memory > 32768
ORDER BY CPUs DESC, Memory DESC;
```

## Historical Queries

### Point-in-Time Query

"What was the VM configuration on a specific date?"

```sql
SELECT
    VM,
    Powerstate,
    CPUs,
    Memory,
    ValidFrom,
    ValidTo
FROM History.vInfo
WHERE VM = 'MyVM'
  AND ValidFrom <= '2024-06-01'
  AND (ValidTo > '2024-06-01' OR ValidTo IS NULL);
```

### Configuration Changes

"When did this VM's memory change?"

```sql
SELECT
    VM,
    Memory,
    ValidFrom,
    ValidTo
FROM History.vInfo
WHERE VM = 'MyVM'
ORDER BY ValidFrom;
```

### VM Lifecycle

"When was this VM created/deleted?"

```sql
-- First appearance (creation)
SELECT VM, MIN(ValidFrom) AS FirstSeen
FROM History.vInfo
WHERE VM = 'MyVM'
GROUP BY VM;

-- Last appearance (if deleted)
SELECT VM, MAX(ValidTo) AS LastSeen
FROM History.vInfo
WHERE VM = 'MyVM' AND ValidTo IS NOT NULL
GROUP BY VM;
```

## Cross-Table Queries

### VM with Disk Details

```sql
SELECT
    i.VM,
    i.CPUs,
    i.Memory,
    d.Disk,
    d.CapacityMiB,
    d.Thin
FROM Current.vInfo i
JOIN Current.vDisk d ON i.VM = d.VM AND i.[VI SDK Server] = d.[VI SDK Server]
WHERE i.VM = 'MyVM';
```

### VM with Network Adapters

```sql
SELECT
    i.VM,
    n.Network,
    n.[Mac Address],
    n.[IPv4 Address]
FROM Current.vInfo i
JOIN Current.vNetwork n ON i.VM = n.VM AND i.[VI SDK Server] = n.[VI SDK Server]
ORDER BY i.VM, n.Network;
```

### Host Utilization Summary

```sql
SELECT
    h.Host,
    h.[# CPU] AS CPUs,
    h.[# Cores] AS Cores,
    h.[# Memory] AS MemoryMiB,
    h.[CPU usage %] AS CPUUsage,
    h.[Memory usage %] AS MemoryUsage,
    COUNT(i.VM) AS VMCount
FROM Current.vHost h
LEFT JOIN Current.vInfo i ON h.Host = i.Host AND h.[VI SDK Server] = i.[VI SDK Server]
    AND i.Powerstate = 'poweredOn'
GROUP BY h.Host, h.[# CPU], h.[# Cores], h.[# Memory],
         h.[CPU usage %], h.[Memory usage %]
ORDER BY h.Host;
```

## Capacity Planning Queries

### Cluster Capacity

```sql
SELECT
    c.Name AS Cluster,
    c.NumHosts,
    c.NumCpuCores,
    c.TotalMemory / 1024 AS MemoryGiB,
    c.EffectiveMemory / 1024 AS EffectiveMemoryGiB,
    COUNT(i.VM) AS VMCount
FROM Current.vCluster c
LEFT JOIN Current.vInfo i ON c.Name = i.Cluster
GROUP BY c.Name, c.NumHosts, c.NumCpuCores, c.TotalMemory, c.EffectiveMemory
ORDER BY c.Name;
```

### Datastore Usage

```sql
SELECT
    Name AS Datastore,
    Type,
    [Capacity MiB] / 1024 AS CapacityGiB,
    [In Use MiB] / 1024 AS UsedGiB,
    [Free MiB] / 1024 AS FreeGiB,
    [Free %] AS FreePercent
FROM Current.vDatastore
ORDER BY [Free %];
```

### Over-Provisioned Datastores

```sql
SELECT
    Name AS Datastore,
    [Capacity MiB] / 1024 AS CapacityGiB,
    [Provisioned MiB] / 1024 AS ProvisionedGiB,
    CAST([Provisioned MiB] AS FLOAT) / NULLIF([Capacity MiB], 0) AS OverprovisionRatio
FROM Current.vDatastore
WHERE [Provisioned MiB] > [Capacity MiB]
ORDER BY OverprovisionRatio DESC;
```

## Health Check Queries

### VMs with Snapshots

```sql
SELECT
    s.VM,
    COUNT(*) AS SnapshotCount,
    SUM(s.[Size MiB (total)]) AS TotalSizeMiB,
    MIN(s.[Date / time]) AS OldestSnapshot
FROM Current.vSnapshot s
GROUP BY s.VM
ORDER BY SnapshotCount DESC;
```

### Old Snapshots (>7 days)

```sql
SELECT
    VM,
    Name AS SnapshotName,
    [Date / time] AS SnapshotDate,
    DATEDIFF(DAY, [Date / time], GETDATE()) AS AgeDays,
    [Size MiB (total)] AS SizeMiB
FROM Current.vSnapshot
WHERE DATEDIFF(DAY, [Date / time], GETDATE()) > 7
ORDER BY AgeDays DESC;
```

### VMware Tools Issues

```sql
SELECT
    VM,
    Tools AS ToolsStatus,
    [Tools Version],
    Upgradeable
FROM Current.vTools
WHERE Tools <> 'toolsOk'
   OR Upgradeable = 'true'
ORDER BY Tools, VM;
```

## Trend Queries

### VM Count Growth

```sql
SELECT
    CAST(ValidFrom AS DATE) AS SnapshotDate,
    COUNT(DISTINCT VM) AS VMCount
FROM History.vInfo
WHERE ValidFrom >= DATEADD(MONTH, -6, GETDATE())
GROUP BY CAST(ValidFrom AS DATE)
ORDER BY SnapshotDate;
```

### Storage Growth

```sql
SELECT
    CAST(ValidFrom AS DATE) AS SnapshotDate,
    SUM([Capacity MiB]) / 1024 AS TotalCapacityGiB,
    SUM([In Use MiB]) / 1024 AS TotalUsedGiB
FROM History.vDatastore
WHERE ValidFrom >= DATEADD(MONTH, -6, GETDATE())
  AND IsCurrent = 0  -- Include only point-in-time snapshots
GROUP BY CAST(ValidFrom AS DATE)
ORDER BY SnapshotDate;
```

---

## Next Steps

- [Reports](./reports.md) - Pre-built views
- [Database Schema](../architecture/database-schema.md) - Full schema reference

## Need Help?

See [Troubleshooting](../reference/troubleshooting.md) or [open an issue](https://github.com/bankielewicz/RVToolsDW/issues).
