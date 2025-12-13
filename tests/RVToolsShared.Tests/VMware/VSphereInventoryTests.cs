using RVToolsShared.VMware.Models;

namespace RVToolsShared.Tests.VMware;

/// <summary>
/// Unit tests for VSphereInventory.
/// </summary>
public class VSphereInventoryTests
{
    [Fact]
    public void Duration_CalculatedCorrectly()
    {
        var inventory = new VSphereInventory
        {
            CollectionStartTime = new DateTime(2024, 1, 1, 10, 0, 0),
            CollectionEndTime = new DateTime(2024, 1, 1, 10, 5, 30)
        };

        Assert.Equal(TimeSpan.FromMinutes(5.5), inventory.Duration);
    }

    [Fact]
    public void TotalVMs_ReturnsCorrectCount()
    {
        var inventory = new VSphereInventory
        {
            VirtualMachines = new List<VmInfo>
            {
                new() { VmId = "vm-1", Name = "VM1" },
                new() { VmId = "vm-2", Name = "VM2" },
                new() { VmId = "vm-3", Name = "VM3" }
            }
        };

        Assert.Equal(3, inventory.TotalVMs);
    }

    [Fact]
    public void TotalHosts_ReturnsCorrectCount()
    {
        var inventory = new VSphereInventory
        {
            Hosts = new List<HostInfo>
            {
                new() { HostId = "host-1", Name = "Host1" },
                new() { HostId = "host-2", Name = "Host2" }
            }
        };

        Assert.Equal(2, inventory.TotalHosts);
    }

    [Fact]
    public void TotalDatastores_ReturnsCorrectCount()
    {
        var inventory = new VSphereInventory
        {
            Datastores = new List<DatastoreInfo>
            {
                new() { DatastoreId = "ds-1", Name = "Datastore1" }
            }
        };

        Assert.Equal(1, inventory.TotalDatastores);
    }

    [Fact]
    public void TotalClusters_ReturnsCorrectCount()
    {
        var inventory = new VSphereInventory
        {
            Clusters = new List<ClusterInfo>
            {
                new() { ClusterId = "cluster-1", Name = "Cluster1" },
                new() { ClusterId = "cluster-2", Name = "Cluster2" }
            }
        };

        Assert.Equal(2, inventory.TotalClusters);
    }

    [Fact]
    public void DefaultValues_AreEmpty()
    {
        var inventory = new VSphereInventory();

        Assert.Empty(inventory.Datacenters);
        Assert.Empty(inventory.Clusters);
        Assert.Empty(inventory.Hosts);
        Assert.Empty(inventory.Datastores);
        Assert.Empty(inventory.Networks);
        Assert.Empty(inventory.ResourcePools);
        Assert.Empty(inventory.Folders);
        Assert.Empty(inventory.VirtualMachines);
        Assert.Empty(inventory.VmDetails);
        Assert.Empty(inventory.VmDisks);
        Assert.Empty(inventory.VmNics);
        Assert.Empty(inventory.VmSnapshots);
        Assert.Empty(inventory.CollectionErrors);
        Assert.False(inventory.IsComplete);
    }

    [Fact]
    public void CollectionErrors_CanBeAdded()
    {
        var inventory = new VSphereInventory();
        inventory.CollectionErrors.Add("Error 1");
        inventory.CollectionErrors.Add("Error 2");

        Assert.Equal(2, inventory.CollectionErrors.Count);
        Assert.Contains("Error 1", inventory.CollectionErrors);
        Assert.Contains("Error 2", inventory.CollectionErrors);
    }
}
