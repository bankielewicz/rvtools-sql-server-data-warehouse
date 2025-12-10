/**
 * RVTools Web - Chart.js Configuration Helpers
 *
 * Standard color palette and chart configuration utilities.
 */

// Standard color palette for consistent chart styling
const ChartColors = {
    primary: '#0d6efd',
    success: '#198754',
    warning: '#ffc107',
    danger: '#dc3545',
    info: '#0dcaf0',
    secondary: '#6c757d',
    dark: '#212529',
    light: '#f8f9fa',

    // Extended palette for multi-series charts
    palette: [
        '#0d6efd', // Blue
        '#198754', // Green
        '#ffc107', // Yellow
        '#dc3545', // Red
        '#6f42c1', // Purple
        '#fd7e14', // Orange
        '#20c997', // Teal
        '#d63384', // Pink
    ],

    // Transparent versions for area fills
    transparent: {
        primary: 'rgba(13, 110, 253, 0.1)',
        success: 'rgba(25, 135, 84, 0.1)',
        warning: 'rgba(255, 193, 7, 0.1)',
        danger: 'rgba(220, 53, 69, 0.1)',
        info: 'rgba(13, 202, 240, 0.1)',
    }
};

// Get color from palette by index (cycles if more series than colors)
function getChartColor(index) {
    return ChartColors.palette[index % ChartColors.palette.length];
}

// Get transparent color from palette by index
function getChartColorTransparent(index, opacity = 0.1) {
    const hex = ChartColors.palette[index % ChartColors.palette.length];
    const r = parseInt(hex.slice(1, 3), 16);
    const g = parseInt(hex.slice(3, 5), 16);
    const b = parseInt(hex.slice(5, 7), 16);
    return `rgba(${r}, ${g}, ${b}, ${opacity})`;
}

// Default chart options
const DefaultChartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
        legend: {
            display: true,
            position: 'top',
            labels: {
                usePointStyle: true,
                padding: 15
            }
        },
        tooltip: {
            mode: 'index',
            intersect: false,
            backgroundColor: 'rgba(0, 0, 0, 0.8)',
            titleFont: { weight: 'bold' },
            padding: 10,
            cornerRadius: 4
        }
    },
    interaction: {
        mode: 'nearest',
        axis: 'x',
        intersect: false
    },
    scales: {
        x: {
            grid: {
                display: false
            }
        },
        y: {
            beginAtZero: true,
            grid: {
                color: 'rgba(0, 0, 0, 0.05)'
            }
        }
    }
};

// Create a line chart configuration
function createLineChartConfig(labels, datasets, options = {}) {
    return {
        type: 'line',
        data: {
            labels: labels,
            datasets: datasets.map((ds, i) => ({
                label: ds.label,
                data: ds.data,
                borderColor: ds.borderColor || getChartColor(i),
                backgroundColor: ds.backgroundColor || getChartColorTransparent(i),
                fill: ds.fill || false,
                tension: ds.tension !== undefined ? ds.tension : 0.3,
                pointRadius: ds.pointRadius !== undefined ? ds.pointRadius : 3,
                pointHoverRadius: 5,
                borderWidth: ds.borderWidth || 2
            }))
        },
        options: Object.assign({}, DefaultChartOptions, options)
    };
}

// Create an area chart configuration (line with fill)
function createAreaChartConfig(labels, datasets, options = {}) {
    const config = createLineChartConfig(labels, datasets, options);
    config.data.datasets.forEach(ds => {
        ds.fill = true;
    });
    return config;
}

// Format number for display (with appropriate suffix)
function formatNumber(num) {
    if (num === null || num === undefined) return '-';
    if (num >= 1000000000) return (num / 1000000000).toFixed(1) + 'B';
    if (num >= 1000000) return (num / 1000000).toFixed(1) + 'M';
    if (num >= 1000) return (num / 1000).toFixed(1) + 'K';
    return num.toFixed(0);
}

// Format bytes for display
function formatBytes(bytes, decimals = 1) {
    if (bytes === null || bytes === undefined) return '-';
    if (bytes === 0) return '0 B';

    const k = 1024;
    const sizes = ['B', 'KiB', 'MiB', 'GiB', 'TiB', 'PiB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));

    return parseFloat((bytes / Math.pow(k, i)).toFixed(decimals)) + ' ' + sizes[i];
}

// Format percentage for display
function formatPercent(value, decimals = 1) {
    if (value === null || value === undefined) return '-';
    return value.toFixed(decimals) + '%';
}
