# Google Maps API Project

## Overview
This project utilizes the Google Maps API to integrate and visualize geographical data within a Maker project. It enables users to interact with maps, customize markers, and retrieve location-based information.

## Features
- Display interactive Google Maps
- Add custom markers to the map
- Retrieve location details (latitude, longitude, and address)
- Handle user inputs for specific locations
- Dynamic updates based on real-time data

## Prerequisites
Before starting, ensure you have the following:
- A Google Cloud Platform (GCP) account
- A project created in GCP with the Google Maps JavaScript API enabled
- An API key for accessing the Google Maps API
- Basic knowledge of HTML, CSS, and JavaScript

- ## Technologies Used
- **Google Maps JavaScript API**: For map rendering and interaction.
- **HTML5**: Structuring the webpage.
- **CSS3**: Styling the interface.
- **JavaScript (Vanilla)**: Implementing functionality and API integration.
- **Google Cloud Platform (GCP)**: Managing API keys and project settings.
- **C#**: Backend processing and data handling.
- **WPF (Windows Presentation Foundation)**: Desktop application interface.

## Installation
1. Clone the repository:
   ```bash
   git clone <repository-url>
   cd <repository-folder>
   ```

2. Install dependencies:
   This project uses vanilla JavaScript and doesn't require additional packages. If there are additional dependencies, include them in this section.

3. Set up your API key:
   - Replace `YOUR_API_KEY` in the code with your actual Google Maps API key.

## Usage
1. Open the `index.html` file in your browser to view the map interface.
2. Customize the code to fit your requirements:
   - **Markers:** Edit the marker data in `script.js`.
   - **Map Options:** Modify the map's zoom level, center, and other properties in `script.js`.

## File Structure
```
.
├── index.html          # Main HTML file
├── styles.css          # CSS file for styling
├── script.js           # JavaScript file for API interactions
└── README.md           # Documentation
```

## Configuration
1. Enable billing on your Google Cloud project.
2. Restrict the API key to specific domains for security.
3. Adjust any additional configurations, such as geolocation services or map styling.

## Example Code Snippet
Here’s a basic example of initializing the map:
```javascript
function initMap() {
    const mapOptions = {
        center: { lat: -34.397, lng: 150.644 },
        zoom: 8,
    };

    const map = new google.maps.Map(document.getElementById("map"), mapOptions);

    const marker = new google.maps.Marker({
        position: { lat: -34.397, lng: 150.644 },
        map: map,
        title: "Hello World!",
    });
}
```
Make sure to include the Google Maps API script in `index.html`:
```html
<script async defer src="https://maps.googleapis.com/maps/api/js?key=YOUR_API_KEY&callback=initMap"></script>
```

## Troubleshooting
- **Map not loading:** Check the browser console for errors and ensure your API key is correctly configured.
- **Markers not displaying:** Ensure the marker data is correctly formatted and the map object is properly initialized.
- **API quota exceeded:** Review your Google Cloud usage and consider increasing your quota.

## Resources
- [Google Maps JavaScript API Documentation](https://developers.google.com/maps/documentation/javascript/overview)
- [Google Cloud Platform Console](https://console.cloud.google.com/)


![image](https://github.com/user-attachments/assets/da9bfbf5-652f-487f-ab9e-df243d7283a8)
