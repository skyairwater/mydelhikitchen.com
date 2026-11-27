# Delivery Driver Companion App (Android - Kotlin)

An accompanying Android application for delivery drivers that streams high-accuracy GPS coordinates in real-time to the Delhi Kitchen website tracking dashboard.

---

## Key Features

1. **One-Tap "Start Sharing" / "Stop Sharing"**:
   - Single toggle button to start or cease location streaming.
   - Shows live GPS latitude/longitude, speed (mph), accuracy radius, and ping status right on screen.

2. **Works While App Is Minimized**:
   - Runs as an Android **Foreground Service** (`LocationService`) with `android:foregroundServiceType="location"`.
   - Android will not kill the process while driving or switching between Google Maps/Waze navigation.

3. **Samsung Now Bar & Lock Screen Support**:
   - Uses `NotificationCompat.CATEGORY_NAVIGATION` and `NotificationCompat.VISIBILITY_PUBLIC`.
   - On **Samsung Galaxy devices running One UI (6.1 / 7)**, this ongoing service automatically displays on the **Now Bar** dynamic lock screen widget and Always On Display.
   - Includes an inline **"Stop Sharing"** action button that works directly from the lock screen or Samsung Now Bar without needing to unlock the phone or open the app.
   - Includes a live chronometer timer (`setUsesChronometer(true)`) showing active duration.

4. **No Security / No Friction**:
   - Directly transmits JSON updates via HTTP POST to `/api/tracking/location`.

---

## How to Open & Run the App

### Option A: Open in Android Studio
1. Launch **Android Studio**.
2. Select **Open** and browse to this folder:  
   `c:\Users\Ganes\.gemini\antigravity\scratch\EcommerceStore\DeliveryDriverApp`
3. Let Gradle sync dependencies.
4. Plug in your Android device (or launch an emulator) and press **Run** (`Shift + F10`).

### Option B: Connecting Phone to Your Dev PC
- **Android Studio Emulator**: Leave the default URL `http://10.0.2.2:5000/api/tracking/location` (or your port).
- **Physical Phone on the Same Wi-Fi**:
  1. Find your computer's local Wi-Fi IP address (e.g. `192.168.1.150`).
  2. In the app's **Server Endpoint URL** field, enter:  
     `http://192.168.1.150:5000/api/tracking/location`
- **Public / Remote Testing**:
  - Use your public domain or ngrok tunnel (e.g. `https://xyz.ngrok-free.app/api/tracking/location`).

---

## JSON Payload Structure

Every 2.5 seconds while moving, the app sends:

```json
{
  "driverId": "Driver 1",
  "latitude": 35.227145,
  "longitude": -80.843128,
  "speed": 11.2,
  "bearing": 45.0,
  "accuracy": 3.8,
  "batteryLevel": 88,
  "isSharing": true,
  "timestamp": "2026-11-25T21:07:00.000Z"
}
```
