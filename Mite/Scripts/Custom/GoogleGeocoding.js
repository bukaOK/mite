var GoogleGeocoding = {
    geocoder: null,

    init: function () {
        geocoder = new google.maps.Geocoder();
        navigator.geolocation.getCurrentPosition(function (pos) {
            var latlng = new google.maps.LatLng(pos.coords.latitude, pos.coords.longitude);
            return geocoder.geocode({
                'location': latlng,
            }, function (results, status) {
                if (status === 'OK') {
                    console.log(results);
                    results[0].address_components.forEach(function (component) {
                        if (component.types[0] === 'locality') {

                        }
                    });
                }
            });
        });

    },
    initCity: function (cityName) {
        return $.ajax({
            url: ''
        })
    }
}