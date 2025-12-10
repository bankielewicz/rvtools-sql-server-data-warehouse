// RVTools Web - Site JavaScript

(function () {
    'use strict';

    // Sidebar toggle functionality
    document.addEventListener('DOMContentLoaded', function () {
        var sidebarToggle = document.getElementById('sidebarToggle');
        var sidebarWrapper = document.getElementById('sidebar-wrapper');

        if (sidebarToggle && sidebarWrapper) {
            // Check for saved sidebar state
            var sidebarCollapsed = localStorage.getItem('sidebarCollapsed') === 'true';
            if (sidebarCollapsed) {
                sidebarWrapper.classList.add('collapsed');
            }

            sidebarToggle.addEventListener('click', function (e) {
                e.preventDefault();
                sidebarWrapper.classList.toggle('collapsed');
                // Save state to localStorage
                localStorage.setItem('sidebarCollapsed', sidebarWrapper.classList.contains('collapsed'));
            });

            // Double-click to reset if stuck
            sidebarToggle.addEventListener('dblclick', function (e) {
                e.preventDefault();
                localStorage.removeItem('sidebarCollapsed');
                sidebarWrapper.classList.remove('collapsed');
            });
        }

        // Initialize error toasts
        var errorToast = document.getElementById('errorToast');
        if (errorToast) {
            var toast = new bootstrap.Toast(errorToast, {
                autohide: false
            });
            toast.show();
        }

        // Initialize DataTables on tables with .data-table class
        if (typeof $.fn.DataTable !== 'undefined') {
            $('.data-table').each(function () {
                $(this).DataTable({
                    pageLength: 25,
                    order: [[0, 'asc']],
                    responsive: true,
                    language: {
                        search: 'Filter:',
                        lengthMenu: 'Show _MENU_ entries',
                        info: 'Showing _START_ to _END_ of _TOTAL_ entries',
                        infoEmpty: 'No entries available',
                        infoFiltered: '(filtered from _MAX_ total entries)',
                        emptyTable: 'No data available'
                    }
                });
            });
        }
    });

    // Cascading dropdown helper for filter forms
    window.RVTools = window.RVTools || {};

    RVTools.loadDropdown = function (url, targetSelector, params, addAllOption) {
        var $target = $(targetSelector);
        var currentValue = $target.val();

        $.get(url, params, function (data) {
            $target.empty();

            if (addAllOption !== false) {
                $target.append('<option value="">(All)</option>');
            }

            $.each(data, function (i, item) {
                var selected = item.value === currentValue ? ' selected' : '';
                $target.append('<option value="' + item.value + '"' + selected + '>' + item.label + '</option>');
            });
        }).fail(function () {
            console.error('Failed to load dropdown data from ' + url);
        });
    };

    // Setup cascading dropdowns for common filter patterns
    RVTools.setupCascadingFilters = function () {
        // vCenter Server -> Datacenter cascade
        $('#viSdkServerSelect').on('change', function () {
            var viServer = $(this).val();
            RVTools.loadDropdown('/api/FilterData/datacenters', '#datacenterSelect', { viSdkServer: viServer });
            RVTools.loadDropdown('/api/FilterData/clusters', '#clusterSelect', { viSdkServer: viServer });
        });

        // Datacenter -> Cluster cascade
        $('#datacenterSelect').on('change', function () {
            var datacenter = $(this).val();
            var viServer = $('#viSdkServerSelect').val();
            RVTools.loadDropdown('/api/FilterData/clusters', '#clusterSelect', {
                datacenter: datacenter,
                viSdkServer: viServer
            });
        });
    };

    // Format bytes to human readable
    RVTools.formatBytes = function (bytes, decimals) {
        if (bytes === 0 || bytes === null) return '0 B';
        decimals = decimals || 2;
        var k = 1024;
        var sizes = ['B', 'KB', 'MB', 'GB', 'TB', 'PB'];
        var i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(decimals)) + ' ' + sizes[i];
    };

    // Format MiB to GiB
    RVTools.formatMiBtoGiB = function (mib, decimals) {
        if (mib === null || mib === undefined) return '-';
        decimals = decimals || 2;
        return (mib / 1024).toFixed(decimals) + ' GiB';
    };

    // Format percentage
    RVTools.formatPercent = function (value, decimals) {
        if (value === null || value === undefined) return '-';
        decimals = decimals || 1;
        return parseFloat(value).toFixed(decimals) + '%';
    };

    // Get status badge class based on value
    RVTools.getStatusBadgeClass = function (status) {
        if (!status) return 'badge-status-unknown';
        var s = status.toLowerCase();
        if (s === 'critical' || s === 'expired' || s === 'error') return 'badge-status-critical';
        if (s === 'warning' || s === 'expiring soon' || s === 'expiring') return 'badge-status-warning';
        if (s === 'normal' || s === 'valid' || s === 'compliant' || s === 'ok') return 'badge-status-normal';
        return 'badge-status-unknown';
    };

    // Get power state badge class
    RVTools.getPowerStateBadgeClass = function (state) {
        if (!state) return 'badge-poweredoff';
        var s = state.toLowerCase();
        if (s === 'poweredon') return 'badge-poweredon';
        if (s === 'poweredoff') return 'badge-poweredoff';
        if (s === 'suspended') return 'badge-suspended';
        return 'badge-poweredoff';
    };

})();
