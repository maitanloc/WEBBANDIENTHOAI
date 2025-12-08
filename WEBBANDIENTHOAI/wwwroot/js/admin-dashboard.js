const createRevenueChart = (labels, data) => {
    const ctx = document.getElementById('revenueChart').getContext('2d');
    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: 'Doanh thu',
                data: data,
                backgroundColor: 'rgba(75, 192, 192, 0.2)',
                borderColor: 'rgba(75, 192, 192, 1)',
                borderWidth: 1
            }]
        },
        options: {
            scales: {
                y: {
                    beginAtZero: true
                }
            }
        }
    });
};

if (window.DASHBOARD && window.DASHBOARD.revenueByDayLabels && window.DASHBOARD.revenueByDay) {
    createRevenueChart(window.DASHBOARD.revenueByDayLabels, window.DASHBOARD.revenueByDay);
}