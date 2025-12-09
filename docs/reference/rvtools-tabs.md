# RVTools Tabs Reference

> All 27 RVTools tabs and their columns.

**Navigation**: [Home](../../README.md) | [Stored Procedures](./stored-procedures.md) | [Troubleshooting](./troubleshooting.md)

---

## Overview

RVTools exports 27 tabs containing approximately 850 columns of VMware inventory data.

| Category | Tabs | Description |
|----------|------|-------------|
| Virtual Machines | vInfo, vCPU, vMemory, vDisk, vPartition, vNetwork, vCD, vUSB, vSnapshot, vTools | VM configuration and state |
| Hosts | vHost, vHBA, vNIC | ESXi host details |
| Networking | vSwitch, vPort, dvSwitch, dvPort, vSC_VMK | Virtual networking |
| Storage | vDatastore, vMultiPath, vFileInfo | Storage infrastructure |
| Cluster | vCluster, vRP | Cluster and resource pools |
| Licensing | vLicense | License information |
| Other | vSource, vHealth, vMetaData | Source info and health |

---

## VM Tabs

### vInfo (98 columns)

Primary VM inventory - most comprehensive VM data.

| Column | Description |
|--------|-------------|
| VM | Virtual machine name |
| Powerstate | poweredOn, poweredOff, suspended |
| Template | Is this a template |
| CPUs | Number of vCPUs |
| Memory | Memory size (MiB) |
| NICs | Number of network adapters |
| Disks | Number of virtual disks |
| Primary IP Address | Primary IP address |
| Network #1-8 | Connected networks |
| Resource pool | Resource pool name |
| Folder | Folder name |
| Datacenter | Datacenter name |
| Cluster | Cluster name |
| Host | ESXi host name |
| OS according to the configuration file | Configured OS |
| OS according to the VMware Tools | Detected OS |
| HW version | Hardware version |
| CBT | Changed Block Tracking enabled |
| Annotation | VM annotation/notes |
| VM ID | VM managed object ID |
| VM UUID | VM UUID |
| VI SDK Server | vCenter server |

### vCPU (37 columns)

CPU allocation and usage per VM.

| Column | Description |
|--------|-------------|
| VM | Virtual machine name |
| CPUs | Number of vCPUs |
| Sockets | Number of sockets |
| Cores p/s | Cores per socket |
| Reservation | CPU reservation |
| Limit | CPU limit |
| Shares | CPU shares |
| Hot Add | CPU hot add enabled |

### vMemory (41 columns)

Memory allocation and usage per VM.

| Column | Description |
|--------|-------------|
| VM | Virtual machine name |
| Size MiB | Memory size |
| Consumed | Consumed memory |
| Active | Active memory |
| Ballooned | Ballooned memory |
| Swapped | Swapped memory |
| Reservation | Memory reservation |
| Limit | Memory limit |
| Hot Add | Memory hot add |

### vDisk (48 columns)

Virtual disk configuration per VM.

| Column | Description |
|--------|-------------|
| VM | Virtual machine name |
| Disk | Disk name |
| Capacity MiB | Disk capacity |
| Thin | Thin provisioned |
| Disk Mode | Disk mode |
| Controller | Disk controller |
| Disk Path | VMDK path |

### vPartition (30 columns)

Guest partition information.

| Column | Description |
|--------|-------------|
| VM | Virtual machine name |
| Disk | Partition path |
| Capacity MiB | Partition capacity |
| Consumed MiB | Used space |
| Free MiB | Free space |
| Free % | Free percentage |

### vNetwork (35 columns)

VM network adapter configuration.

| Column | Description |
|--------|-------------|
| VM | Virtual machine name |
| Network | Network name |
| Adapter | Adapter type |
| Mac Address | MAC address |
| IPv4 Address | IPv4 address |
| IPv6 Address | IPv6 address |
| Connected | Is connected |

### vCD (28 columns)

CD/DVD device configuration.

| Column | Description |
|--------|-------------|
| VM | Virtual machine name |
| Device Node | Device node |
| Connected | Is connected |
| Device Type | Device type |

### vUSB (33 columns)

USB device configuration.

| Column | Description |
|--------|-------------|
| VM | Virtual machine name |
| Device Node | Device node |
| Device Type | Device type |
| Connected | Is connected |

### vSnapshot (29 columns)

Snapshot details per VM.

| Column | Description |
|--------|-------------|
| VM | Virtual machine name |
| Name | Snapshot name |
| Description | Snapshot description |
| Date / time | Snapshot date/time |
| Size MiB (total) | Total snapshot size |
| Quiesced | Was quiesced |

### vTools (37 columns)

VMware Tools status per VM.

| Column | Description |
|--------|-------------|
| VM | Virtual machine name |
| Tools | Tools status |
| Tools Version | Tools version |
| Upgradeable | Is upgradeable |
| Upgrade Policy | Upgrade policy |

---

## Host Tabs

### vHost (71 columns)

ESXi host details.

| Column | Description |
|--------|-------------|
| Host | Host name |
| Datacenter | Datacenter |
| Cluster | Cluster |
| ESX Version | ESXi version |
| CPU Model | CPU model |
| # CPU | Number of CPUs |
| # Cores | Total cores |
| # Memory | Total memory (MiB) |
| CPU usage % | CPU utilization |
| Memory usage % | Memory utilization |
| # VMs | Number of VMs |
| Vendor | Hardware vendor |
| Model | Hardware model |
| Serial number | Serial number |

### vHBA (13 columns)

Host bus adapter information.

| Column | Description |
|--------|-------------|
| Host | Host name |
| Device | Device name |
| Type | HBA type |
| WWN | World Wide Name |

### vNIC (14 columns)

Host network adapter information.

| Column | Description |
|--------|-------------|
| Host | Host name |
| Network Device | Device name |
| Speed | Link speed |
| MAC | MAC address |

---

## Networking Tabs

### vSwitch (23 columns)

Virtual switch configuration.

| Column | Description |
|--------|-------------|
| Host | Host name |
| Switch | Switch name |
| # Ports | Number of ports |
| MTU | Maximum transmission unit |

### vPort (22 columns)

Port group configuration.

| Column | Description |
|--------|-------------|
| Host | Host name |
| Port Group | Port group name |
| Switch | Switch name |
| VLAN | VLAN ID |

### dvSwitch (30 columns)

Distributed virtual switch configuration.

| Column | Description |
|--------|-------------|
| Name | Switch name |
| Datacenter | Datacenter |
| Version | Version |
| Host members | Number of hosts |
| Max Ports | Maximum ports |

### dvPort (41 columns)

Distributed port group configuration.

| Column | Description |
|--------|-------------|
| Port | Port name |
| Switch | Switch name |
| VLAN | VLAN configuration |
| # Ports | Number of ports |

### vSC_VMK (15 columns)

Service console and VMkernel ports.

| Column | Description |
|--------|-------------|
| Host | Host name |
| Port Group | Port group |
| IP Address | IPv4 address |
| Subnet mask | Subnet mask |

---

## Storage Tabs

### vDatastore (31 columns)

Datastore details.

| Column | Description |
|--------|-------------|
| Name | Datastore name |
| Type | Datastore type (VMFS, NFS) |
| Capacity MiB | Total capacity |
| In Use MiB | Used space |
| Free MiB | Free space |
| Free % | Free percentage |
| # VMs | Number of VMs |
| # Hosts | Number of hosts |

### vMultiPath (35 columns)

Storage multipathing configuration.

| Column | Description |
|--------|-------------|
| Host | Host name |
| Datastore | Datastore name |
| Policy | Multipath policy |
| Path 1-8 | Path names |

### vFileInfo (8 columns)

Datastore file information.

| Column | Description |
|--------|-------------|
| File Name | File name |
| File Type | File type |
| File Size in bytes | File size |
| Path | Full path |

---

## Cluster Tabs

### vCluster (36 columns)

Cluster configuration.

| Column | Description |
|--------|-------------|
| Name | Cluster name |
| NumHosts | Number of hosts |
| TotalCpu | Total CPU (MHz) |
| TotalMemory | Total memory |
| HA enabled | HA enabled |
| DRS enabled | DRS enabled |

### vRP (49 columns)

Resource pool configuration.

| Column | Description |
|--------|-------------|
| Resource Pool name | Pool name |
| # VMs | Total VMs |
| CPU reservation | CPU reservation |
| Mem reservation | Memory reservation |

---

## Other Tabs

### vSource (14 columns)

vCenter/ESXi source information.

| Column | Description |
|--------|-------------|
| Name | Source name |
| API version | API version |
| Version | Version |
| Build | Build number |

### vLicense (10 columns)

License information.

| Column | Description |
|--------|-------------|
| Name | License name |
| Key | License key |
| Total | Total licenses |
| Used | Used licenses |
| Expiration Date | Expiration date |

### vHealth (5 columns)

Health check results.

| Column | Description |
|--------|-------------|
| Name | Check name |
| Message | Health message |
| Message type | Info, Warning, Error |

### vMetaData (4 columns)

Export metadata.

| Column | Description |
|--------|-------------|
| RVTools version | Full version |
| xlsx creation datetime | Export date/time |

---

## Natural Keys

Each tab uses these natural keys for MERGE operations:

| Tab | Natural Key |
|-----|-------------|
| vInfo | VM + VI SDK Server |
| vHost | Host + VI SDK Server |
| vDatastore | Name + VI SDK Server |
| vCluster | Name + VI SDK Server |
| vSnapshot | VM + Name + VI SDK Server |
| (other tables) | Primary entity + VI SDK Server |

---

## Next Steps

- [Database Schema](../architecture/database-schema.md) - Schema structure
- [Stored Procedures](./stored-procedures.md) - Import processing

## Need Help?

See [Troubleshooting](./troubleshooting.md) or [open an issue](https://github.com/bankielewicz/RVToolsDW/issues).
