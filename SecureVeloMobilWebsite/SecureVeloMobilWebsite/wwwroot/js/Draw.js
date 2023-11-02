const dateOptions = {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
};

function drawMap(data, inputLastTimeStamp) {
    const map = L.map('map').setView([51.5074, -0.1278], 13);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: 'Map data &copy; <a href="https://www.openstreetmap.org/">OpenStreetMap</a> contributors',
        maxZoom: 19
    }).addTo(map);

    const routeCoordinates = data.map(item => [item.posY, item.posX]);

    const route = L.polyline(routeCoordinates, {color: 'blue'}).addTo(map);

    // Calculate the total distance of the route
    let distance = 0;
    for (let i = 0; i < routeCoordinates.length - 1; i++) {
        let from = L.latLng(routeCoordinates[i]);
        let to = L.latLng(routeCoordinates[i + 1]);
        distance += from.distanceTo(to);
    }

    const lblDistance = $('#totalDistance');
    lblDistance.html(distance.toFixed(4) + " Meters");

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

        // marker.bindPopup('PosX: ' + item.posX);
        // marker.bindPopup('PosY: ' + item.posY);
        const date = new Date(item.positionTime);
        marker.bindPopup(date.toLocaleDateString("en-US", dateOptions));
    });

    // Add some additional map enhancements
    L.control.scale().addTo(map);
    L.control.zoom({position: 'topright'}).addTo(map);

    const bounds = L.latLngBounds(routeCoordinates);
    map.fitBounds(bounds);
}


function drawCo2Graphic() {
    const ctx = document.getElementById('co2Chart');

    new Chart(ctx, {
        type: 'bar', // Use 'bar' for vertical bars
        data: {
            labels: ['Red', 'Blue', 'Yellow', 'Green', 'Purple', 'Orange'],
            datasets: [{
                label: '# of Votes',
                data: [12, 19, 3, 5, 2, 3],
                borderWidth: 1
            }]
        },
        options: {
            indexAxis: 'y', // Use 'y' for vertical bars
            scales: {
                x: {
                    beginAtZero: true
                }
            }
        }
    });
}
