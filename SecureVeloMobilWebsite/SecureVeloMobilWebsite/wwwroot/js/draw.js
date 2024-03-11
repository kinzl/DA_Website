const dateOptions = {
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
};

function drawMap(data) {
    if (data == null) return;
    console.log(data);
    const map = L.map('map').setView([51.5074, -0.1278], 13);

    // Use CartoDB Dark Matter tiles for a dark map style
    L.tileLayer('https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}.png', {
        attribution: 'Map data &copy; <a href="https://www.openstreetmap.org/">OpenStreetMap</a> contributors',
        maxZoom: 19
    }).addTo(map);

    const routeCoordinates = data.map(item => [item.posY, item.posX]);

    L.polyline(routeCoordinates, {color: '#5FBD00'}).addTo(map);

    // Add square markers for each coordinate
    data.forEach(item => {
        const markerCoordinates = [item.posY, item.posX];
        const marker = L.marker(markerCoordinates, {
            icon: L.divIcon({
                className: 'custom-square-marker-icon',
                // iconSize: [8, 8], // Adjust the size as needed
                // pointRadius: 0.2,
                html: '<div class="square-marker"></div>'
            })
        }).addTo(map);

        const date = new Date(item.positionTime);
        marker.bindPopup(date.toLocaleTimeString("de", dateOptions) + "<br>" + item.currentSpeed.toFixed(1) + " km/h");
    });

    // Add some additional map enhancements
    L.control.scale().addTo(map);
    // L.control.zoom({position: 'topright'}).addTo(map);

    const bounds = L.latLngBounds(routeCoordinates);
    map.fitBounds(bounds);
}

function drawAltitudeDiagram(coordinates) {
    const ctx = document.getElementById('altitudeChart');
    console.log(coordinates.map(coord => coord.posZ));
    const timestamps = coordinates.map(coord => new Date(coord.positionTime).toLocaleTimeString("de", dateOptions));

    new Chart(ctx, {
        type: 'line', // Use 'line' for altitude diagram
        data: {
            labels: timestamps,
            datasets: [{
                label: 'Höhenprofil',
                data: coordinates.map(coord => coord.posZ),
                borderColor: '#5FBD00',
                borderWidth: 2,
                pointRadius: 0.2,
                pointBackgroundColor: '#5FBD00',
                fill: false
            }]
        },
        options: {
            scales: {
                x: {
                    ticks: {
                        callback: (value, index, values) => {
                            const firstTimestamp = new Date(coordinates[0].positionTime).toLocaleTimeString("de", dateOptions);
                            const lastTimestamp = new Date(coordinates[coordinates.length - 1].positionTime).toLocaleTimeString("de", dateOptions);
                            return index === 0 ? firstTimestamp : (index === values.length - 1 ? lastTimestamp : '');
                        }
                    }
                },
                y: {
                    beginAtZero: false
                }
            },
            plugins: {
                legend: {
                    display: true,
                    position: 'top'
                }
            }
        }
    });
}
