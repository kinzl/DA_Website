const dateOptions = {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
};

function drawMap(data, inputLastTimeStamp, distance) {
    if (data == null) return;
    console.log(data);
    const map = L.map('map').setView([51.5074, -0.1278], 13);

    // Use CartoDB Dark Matter tiles for a dark map style
    L.tileLayer('https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}.png', {
        attribution: 'Map data &copy; <a href="https://www.openstreetmap.org/">OpenStreetMap</a> contributors',
        maxZoom: 19
    }).addTo(map);

    const routeCoordinates = data.map(item => [item.posY, item.posX]);

    // const route = L.polyline(routeCoordinates, { color: 'blue' }).addTo(map);
    const route = L.polyline(routeCoordinates, {color: '#5FBD00'}).addTo(map);

    console.log(data);

    // const lblDistance = $('#totalDistance').html((distance / 1000).toFixed(3) + " km");

    // Add square markers for each coordinate
    data.forEach(item => {
        const markerCoordinates = [item.posY, item.posX];
        const marker = L.marker(markerCoordinates, {
            icon: L.divIcon({
                className: 'custom-square-marker-icon',
                iconSize: [12, 12], // Adjust the size as needed
                html: '<div class="square-marker"></div>'
            })
        }).addTo(map);

        const date = new Date(item.positionTime);
        marker.bindPopup(date.toLocaleDateString("de", dateOptions));
    });

    // Add some additional map enhancements
    L.control.scale().addTo(map);
    L.control.zoom({position: 'topright'}).addTo(map);

    const bounds = L.latLngBounds(routeCoordinates);
    map.fitBounds(bounds);
}

function drawAltitudeDiagram(coordinates) {
    const ctx = document.getElementById('altitudeChart');
    console.log(coordinates.map(coord => coord.posZ));

    new Chart(ctx, {
        type: 'line', // Use 'line' for altitude diagram
        data: {
            labels: coordinates.map(coord => new Date(coord.positionTime).toLocaleDateString("de", dateOptions)),
            datasets: [{
                label: 'Altitude',
                data: coordinates.map(coord => coord.posZ),
                borderColor: 'blue',
                borderWidth: 2,
                pointBackgroundColor: 'blue',
                fill: false
            }]
        },
        options: {
            scales: {
                y: {
                    beginAtZero: false // Adjust as needed based on your altitude data
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

// // Example usage:
// const altitudeCoordinates = [
//     { label: 'Point A', altitude: 100 },
//     { label: 'Point B', altitude: 150 },
//     { label: 'Point C', altitude: 120 },
//     // Add more coordinates as needed
// ];
//
// drawAltitudeDiagram(altitudeCoordinates);
