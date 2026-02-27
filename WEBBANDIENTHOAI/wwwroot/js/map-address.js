/**
 * map-address.js
 * Handle address autocomplete and interactive map integration using Leaflet & Nominatim.
 */

class MapAddressIntegration {
    constructor(options) {
        this.mapId = options.mapId || 'map-container';
        this.inputId = options.inputId || 'Address';
        this.latId = options.latId || 'Latitude';
        this.lngId = options.lngId || 'Longitude';
        this.suggestionsId = options.suggestionsId || 'address-suggestions';
        
        this.map = null;
        this.marker = null;
        this.debounceTimer = null;
        
        // DOM Elements
        this.inputEl = document.getElementById(this.inputId);
        this.latEl = document.getElementById(this.latId);
        this.lngEl = document.getElementById(this.lngId);
        this.mapContainer = document.getElementById(this.mapId);
        this.suggestionsContainer = document.getElementById(this.suggestionsId);
        
        if (!this.inputEl || !this.mapContainer) {
            console.warn("MapIntegration: Missing required DOM elements.");
            return;
        }

        this.initMap();
        this.initAutocomplete();
    }

    initMap() {
        // Default to Ho Chi Minh City if no coordinates are provided
        let initialLat = 10.762622;
        let initialLng = 106.660172;
        let zoomLevel = 13;

        // Check if we already have coordinates
        if (this.latEl && this.latEl.value && this.lngEl && this.lngEl.value) {
            const parsedLat = parseFloat(this.latEl.value);
            const parsedLng = parseFloat(this.lngEl.value);
            // Replace comma with dot depending on culture if it failed
            if (!isNaN(parsedLat) && !isNaN(parsedLng)) {
                initialLat = parsedLat;
                initialLng = parsedLng;
                zoomLevel = 16;
            }
        }

        // Initialize Map
        this.map = L.map(this.mapId).setView([initialLat, initialLng], zoomLevel);

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '&copy; OpenStreetMap contributors'
        }).addTo(this.map);

        // Initialize Marker
        this.marker = L.marker([initialLat, initialLng], {
            draggable: true
        }).addTo(this.map);

        // Handle Marker Drag
        this.marker.on('dragend', async (e) => {
            const position = this.marker.getLatLng();
            this.updateCoordinateInputs(position.lat, position.lng);
            await this.reverseGeocode(position.lat, position.lng);
        });

        // Handle Map Click
        this.map.on('click', async (e) => {
            const position = e.latlng;
            this.marker.setLatLng(position);
            this.updateCoordinateInputs(position.lat, position.lng);
            await this.reverseGeocode(position.lat, position.lng);
        });

        // Fix map rendering issue when container becomes visible
        setTimeout(() => {
            this.map.invalidateSize();
        }, 500);
    }

    initAutocomplete() {
        if (!this.suggestionsContainer) return;

        this.inputEl.addEventListener('input', (e) => {
            clearTimeout(this.debounceTimer);
            const query = e.target.value.trim();

            if (query.length < 3) {
                this.suggestionsContainer.classList.add('hidden');
                return;
            }

            this.debounceTimer = setTimeout(() => {
                this.fetchSuggestions(query);
            }, 500);
        });

        // Hide suggestions when clicking outside
        document.addEventListener('click', (e) => {
            if (!this.inputEl.contains(e.target) && !this.suggestionsContainer.contains(e.target)) {
                this.suggestionsContainer.classList.add('hidden');
            }
        });
    }

    async fetchSuggestions(query) {
        try {
            // Using our backend proxy MapController
            const response = await fetch(`/Map/Autocomplete?q=${encodeURIComponent(query)}`);
            if (!response.ok) return;

            const data = await response.json();
            this.renderSuggestions(data);
        } catch (error) {
            console.error('Error fetching address suggestions:', error);
        }
    }

    renderSuggestions(results) {
        this.suggestionsContainer.innerHTML = '';
        
        if (results.length === 0) {
            this.suggestionsContainer.classList.add('hidden');
            return;
        }

        results.forEach(place => {
            const li = document.createElement('li');
            li.className = 'px-4 py-2 hover:bg-gray-200 cursor-pointer text-sm font-semibold text-black border-b border-gray-200 last:border-0';
            li.textContent = place.display_name;
            
            li.addEventListener('click', () => {
                this.selectLocation(place);
            });
            
            this.suggestionsContainer.appendChild(li);
        });

        this.suggestionsContainer.classList.remove('hidden');
    }

    selectLocation(place) {
        const lat = parseFloat(place.lat);
        const lon = parseFloat(place.lon);

        this.inputEl.value = place.display_name;
        this.suggestionsContainer.classList.add('hidden');

        this.updateCoordinateInputs(lat, lon);
        
        // Update Map
        const pos = new L.LatLng(lat, lon);
        this.map.setView(pos, 16);
        this.marker.setLatLng(pos);
    }

    async reverseGeocode(lat, lon) {
        try {
            this.inputEl.value = 'Đang tải địa chỉ...';
            // Using our backend proxy MapController
            const response = await fetch(`/Map/Reverse?lat=${lat}&lon=${lon}`);
            if (!response.ok) {
                this.inputEl.value = 'Không thể lấy địa chỉ tự động.';
                return;
            }

            const data = await response.json();
            if (data && data.display_name) {
                this.inputEl.value = data.display_name;
            } else {
                this.inputEl.value = 'Vị trí không xác định';
            }
        } catch (error) {
            console.error('Error in reverse geocoding:', error);
            this.inputEl.value = 'Lỗi kết nối bản đồ';
        }
    }

    updateCoordinateInputs(lat, lng) {
        if (this.latEl) this.latEl.value = lat;
        if (this.lngEl) this.lngEl.value = lng;
    }
}

// Ensure it can be globally attached
window.MapAddressIntegration = MapAddressIntegration;
