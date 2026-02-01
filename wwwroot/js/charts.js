// Chart.js helper functions for Blazor interop

window.chartInterop = {
    charts: {},

    createLineChart: function (canvasId, labels, datasets, options) {
        if (typeof Chart === 'undefined') {
            console.error('Chart.js is not loaded');
            return;
        }
        
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;

        // Destroy existing chart if any
        if (this.charts[canvasId]) {
            this.charts[canvasId].destroy();
        }

        this.charts[canvasId] = new Chart(ctx, {
            type: 'line',
            data: {
                labels: labels,
                datasets: datasets.map(ds => ({
                    label: ds.label,
                    data: ds.data,
                    borderColor: ds.borderColor || '#C41E3A',
                    backgroundColor: ds.backgroundColor || 'rgba(196, 30, 58, 0.1)',
                    tension: 0.3,
                    fill: ds.fill !== undefined ? ds.fill : true
                }))
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'top',
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            callback: function(value) {
                                return '$' + value.toLocaleString();
                            }
                        }
                    }
                },
                ...options
            }
        });
    },

    createPieChart: function (canvasId, labels, data, colors, options) {
        if (typeof Chart === 'undefined') {
            console.error('Chart.js is not loaded');
            return;
        }
        
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;

        if (this.charts[canvasId]) {
            this.charts[canvasId].destroy();
        }

        this.charts[canvasId] = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: labels,
                datasets: [{
                    data: data,
                    backgroundColor: colors || [
                        '#C41E3A', '#28a745', '#ffc107', '#17a2b8',
                        '#6c757d', '#343a40', '#e83e8c', '#fd7e14'
                    ],
                    borderWidth: 2,
                    borderColor: '#ffffff'
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'right',
                    },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                const value = context.parsed;
                                const total = context.dataset.data.reduce((a, b) => a + b, 0);
                                const percentage = ((value / total) * 100).toFixed(1);
                                return `${context.label}: $${value.toLocaleString()} (${percentage}%)`;
                            }
                        }
                    }
                },
                ...options
            }
        });
    },

    createBarChart: function (canvasId, labels, datasets, options) {
        if (typeof Chart === 'undefined') {
            console.error('Chart.js is not loaded');
            return;
        }
        
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;

        if (this.charts[canvasId]) {
            this.charts[canvasId].destroy();
        }

        this.charts[canvasId] = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: datasets.map(ds => ({
                    label: ds.label,
                    data: ds.data,
                    backgroundColor: ds.backgroundColor || '#C41E3A',
                    borderRadius: 4
                }))
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'top',
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            callback: function(value) {
                                return '$' + value.toLocaleString();
                            }
                        }
                    }
                },
                ...options
            }
        });
    },

    updateChart: function (canvasId, labels, datasets) {
        const chart = this.charts[canvasId];
        if (!chart) return;

        chart.data.labels = labels;
        datasets.forEach((ds, index) => {
            if (chart.data.datasets[index]) {
                chart.data.datasets[index].data = ds.data;
            }
        });
        chart.update();
    },

    destroyChart: function (canvasId) {
        if (this.charts[canvasId]) {
            this.charts[canvasId].destroy();
            delete this.charts[canvasId];
        }
    }
};

// File download helper
window.downloadFile = function (fileName, contentType, base64Content) {
    const link = document.createElement('a');
    link.download = fileName;
    link.href = `data:${contentType};base64,${base64Content}`;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};

// Theme management
window.themeManager = {
    setTheme: function (theme) {
        document.documentElement.setAttribute('data-theme', theme);
        localStorage.setItem('theme', theme);
    },
    
    getTheme: function () {
        return localStorage.getItem('theme') || 'light';
    },
    
    initTheme: function () {
        const savedTheme = this.getTheme();
        document.documentElement.setAttribute('data-theme', savedTheme);
        return savedTheme;
    }
};
