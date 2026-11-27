package com.delhikitchen.deliverydriver

import android.annotation.SuppressLint
import android.app.Notification
import android.app.NotificationChannel
import android.app.NotificationManager
import android.app.PendingIntent
import android.app.Service
import android.content.Context
import android.content.Intent
import android.content.pm.ServiceInfo
import android.location.Location
import android.location.LocationListener
import android.location.LocationManager
import android.os.Build
import android.os.Bundle
import android.os.IBinder
import android.os.Looper
import android.os.PowerManager
import android.util.Log
import androidx.core.app.NotificationCompat
import com.google.android.gms.location.FusedLocationProviderClient
import com.google.android.gms.location.LocationCallback
import com.google.android.gms.location.LocationRequest
import com.google.android.gms.location.LocationResult
import com.google.android.gms.location.LocationServices
import com.google.android.gms.location.Priority
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.SupervisorJob
import kotlinx.coroutines.cancel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import java.util.Locale

class LocationService : Service() {

    private val serviceScope = CoroutineScope(SupervisorJob() + Dispatchers.Main)
    private lateinit var fusedLocationClient: FusedLocationProviderClient
    private lateinit var notificationManager: NotificationManager
    private lateinit var systemLocationManager: LocationManager
    private var wakeLock: PowerManager.WakeLock? = null

    private var serverUrl: String = "http://192.168.1.94:5140/api/tracking/location"
    private var driverId: String = "Driver 1"
    private var deliveryType: String = "Food"
    private var serviceStartTime: Long = 0L

    private val fusedCallback = object : LocationCallback() {
        override fun onLocationResult(result: LocationResult) {
            val location = result.lastLocation ?: return
            handleLocationUpdate(location)
        }
    }

    private val fallbackListener = object : LocationListener {
        override fun onLocationChanged(location: Location) {
            handleLocationUpdate(location)
        }
        @Deprecated("Deprecated in Java")
        override fun onStatusChanged(provider: String?, status: Int, extras: Bundle?) {}
        override fun onProviderEnabled(provider: String) {}
        override fun onProviderDisabled(provider: String) {}
    }

    override fun onCreate() {
        super.onCreate()
        fusedLocationClient = LocationServices.getFusedLocationProviderClient(this)
        notificationManager = getSystemService(Context.NOTIFICATION_SERVICE) as NotificationManager
        systemLocationManager = getSystemService(Context.LOCATION_SERVICE) as LocationManager

        createNotificationChannel()

        // Acquire WakeLock so device doesn't pause GPS when user turns off screen or minimizes app
        try {
            val powerManager = getSystemService(Context.POWER_SERVICE) as PowerManager
            wakeLock = powerManager.newWakeLock(
                PowerManager.PARTIAL_WAKE_LOCK,
                "DelhiKitchen:LiveLocationWakeLock"
            ).apply {
                setReferenceCounted(false)
                acquire(12 * 60 * 60 * 1000L) // 12 hours timeout safety limit
            }
            Log.d(TAG, "WakeLock acquired for background GPS tracking.")
        } catch (e: Exception) {
            Log.w(TAG, "Could not acquire wakeLock: ${e.message}")
        }

        // Start foreground immediately in onCreate to satisfy Android 14 requirements
        startForegroundWithLiveNotification()
    }

    override fun onStartCommand(intent: Intent?, flags: Int, startId: Int): Int {
        when (intent?.action) {
            ACTION_STOP -> {
                Log.d(TAG, "Received ACTION_STOP -> stopping sharing immediately")
                stopLocationSharing()
                return START_NOT_STICKY
            }
            ACTION_START -> {
                serverUrl = intent.getStringExtra(EXTRA_SERVER_URL) ?: serverUrl
                driverId = intent.getStringExtra(EXTRA_DRIVER_ID) ?: driverId
                deliveryType = intent.getStringExtra(EXTRA_DELIVERY_TYPE) ?: deliveryType
                Log.d(TAG, "Starting location sharing for: $driverId ($deliveryType) -> $serverUrl")

                serviceStartTime = System.currentTimeMillis()
                _isRunning.value = true

                // Update notification with driver & category details
                val typeEmoji = if (deliveryType.equals("Grocery", true)) "🛒" else "🚗"
                val initialText = "Broadcasting $typeEmoji $deliveryType live GPS..."
                val notification = buildLiveNotification(initialText)
                notificationManager.notify(NOTIFICATION_ID, notification)

                startLocationUpdates()
            }
        }
        return START_STICKY
    }

    private fun createNotificationChannel() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            val channel = NotificationChannel(
                CHANNEL_ID,
                "Delhi Kitchen Live Delivery Tracking",
                NotificationManager.IMPORTANCE_LOW // Low keeps it persistent and quiet without beeping every 2 seconds
            ).apply {
                description = "Shows real-time delivery GPS coordinates and persistent Stop button"
                lockscreenVisibility = Notification.VISIBILITY_PUBLIC
                setShowBadge(false)
                setSound(null, null)
                enableVibration(false)
            }
            notificationManager.createNotificationChannel(channel)
        }
    }

    private fun startForegroundWithLiveNotification() {
        val typeEmoji = if (deliveryType.equals("Grocery", true)) "🛒" else "🚗"
        val notification = buildLiveNotification("Broadcasting $typeEmoji $deliveryType live GPS...")
        try {
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.UPSIDE_DOWN_CAKE) {
                startForeground(
                    NOTIFICATION_ID,
                    notification,
                    ServiceInfo.FOREGROUND_SERVICE_TYPE_LOCATION
                )
            } else {
                startForeground(NOTIFICATION_ID, notification)
            }
        } catch (e: Exception) {
            Log.e(TAG, "startForeground error: ${e.message}")
            try {
                startForeground(NOTIFICATION_ID, notification)
            } catch (e2: Exception) {
                Log.e(TAG, "Fallback startForeground error: ${e2.message}")
            }
        }
    }

    private fun buildLiveNotification(subtitleText: String): Notification {
        // 1. Tapping notification body opens MainActivity
        val openAppIntent = Intent(this, MainActivity::class.java).apply {
            flags = Intent.FLAG_ACTIVITY_SINGLE_TOP or Intent.FLAG_ACTIVITY_CLEAR_TOP
        }
        val openAppPendingIntent = PendingIntent.getActivity(
            this,
            2001,
            openAppIntent,
            PendingIntent.FLAG_UPDATE_CURRENT or PendingIntent.FLAG_IMMUTABLE
        )

        // 2. Tapping "Stop Sharing" sends broadcast to NotificationActionReceiver
        // (BroadcastReceiver works 100% reliably in background and on lock screen, unlike PendingIntent.getService)
        val stopBroadcastIntent = Intent(this, NotificationActionReceiver::class.java).apply {
            action = NotificationActionReceiver.ACTION_STOP_SERVICE
        }
        val stopPendingIntent = PendingIntent.getBroadcast(
            this,
            2002,
            stopBroadcastIntent,
            PendingIntent.FLAG_UPDATE_CURRENT or PendingIntent.FLAG_IMMUTABLE
        )

        val typeBadge = if (deliveryType.equals("Grocery", true)) "🛒 Grocery" else "🚗 Food"

        val stopAction = NotificationCompat.Action.Builder(
            R.drawable.ic_stop,
            "Stop Sharing",
            stopPendingIntent
        ).build()

        val builder = NotificationCompat.Builder(this, CHANNEL_ID)
            .setSmallIcon(R.drawable.ic_notification)
            .setContentTitle("Delhi Kitchen • $typeBadge Delivery")
            .setContentText(subtitleText)
            .setStyle(NotificationCompat.BigTextStyle().bigText(subtitleText))
            .setContentIntent(openAppPendingIntent)
            .setOngoing(true)
            .setOnlyAlertOnce(true)
            .setCategory(NotificationCompat.CATEGORY_SERVICE)
            .setVisibility(NotificationCompat.VISIBILITY_PUBLIC)
            .setPriority(NotificationCompat.PRIORITY_MAX)
            .setShowWhen(true)
            .setUsesChronometer(true)
            .setWhen(if (serviceStartTime > 0) serviceStartTime else System.currentTimeMillis())
            .setForegroundServiceBehavior(NotificationCompat.FOREGROUND_SERVICE_IMMEDIATE)
            .addAction(stopAction)

        return builder.build()
    }

    @SuppressLint("MissingPermission")
    private fun startLocationUpdates() {
        try {
            // 1. Immediately query current location with high accuracy
            fusedLocationClient.getCurrentLocation(Priority.PRIORITY_HIGH_ACCURACY, null)
                .addOnSuccessListener { location ->
                    if (location != null && _isRunning.value) {
                        Log.d(TAG, "Immediate getCurrentLocation fix: ${location.latitude}, ${location.longitude}")
                        handleLocationUpdate(location)
                    }
                }

            // 2. Also check cached lastLocation as fallback
            fusedLocationClient.lastLocation.addOnSuccessListener { location ->
                if (location != null && _isRunning.value && _latestLocation.value == null) {
                    Log.d(TAG, "Cached lastLocation fix: ${location.latitude}, ${location.longitude}")
                    handleLocationUpdate(location)
                }
            }

            // 3. Request periodic high-accuracy fused location updates (0m distance delta so stationary phones still update)
            val locationRequest = LocationRequest.Builder(
                Priority.PRIORITY_HIGH_ACCURACY,
                UPDATE_INTERVAL_MS
            ).apply {
                setMinUpdateIntervalMillis(FASTEST_INTERVAL_MS)
                setMinUpdateDistanceMeters(0f)
                setWaitForAccurateLocation(false)
            }.build()

            fusedLocationClient.requestLocationUpdates(
                locationRequest,
                fusedCallback,
                Looper.getMainLooper()
            )

            // 4. Register system LocationManager GPS & Network providers as redundant fallback
            try {
                if (systemLocationManager.isProviderEnabled(LocationManager.GPS_PROVIDER)) {
                    systemLocationManager.requestLocationUpdates(
                        LocationManager.GPS_PROVIDER,
                        UPDATE_INTERVAL_MS,
                        0f,
                        fallbackListener,
                        Looper.getMainLooper()
                    )
                }
                if (systemLocationManager.isProviderEnabled(LocationManager.NETWORK_PROVIDER)) {
                    systemLocationManager.requestLocationUpdates(
                        LocationManager.NETWORK_PROVIDER,
                        UPDATE_INTERVAL_MS,
                        0f,
                        fallbackListener,
                        Looper.getMainLooper()
                    )
                }
            } catch (e: Exception) {
                Log.w(TAG, "LocationManager fallback registration: ${e.message}")
            }

            Log.d(TAG, "Location providers successfully registered.")
        } catch (e: SecurityException) {
            Log.e(TAG, "Location permission missing: ${e.message}")
            stopLocationSharing()
        }
    }

    private fun handleLocationUpdate(location: Location) {
        if (!_isRunning.value) return

        // Indoor/Accuracy Guard: allow up to 180m initially
        if (location.hasAccuracy() && location.accuracy > 180f) {
            Log.d(TAG, "Skipping coarse fix with accuracy ${location.accuracy}m")
            return
        }

        val speedMps = if (location.hasSpeed()) location.speed else 0f
        val speedMph = (speedMps * 2.23694f)
        val bearing = if (location.hasBearing()) location.bearing else null
        val accuracy = if (location.hasAccuracy()) location.accuracy else null
        val battery = NetworkClient.getBatteryLevel(this)

        val update = DriverLocationUpdate(
            driverId = driverId,
            deliveryType = deliveryType,
            latitude = location.latitude,
            longitude = location.longitude,
            speed = speedMps,
            bearing = bearing,
            accuracy = accuracy,
            batteryLevel = battery,
            isSharing = true
        )

        _latestLocation.value = update

        // Update live notification with latest speed & coordinates
        val coordText = String.format(Locale.US, "GPS: %.4f, %.4f • %.1f mph", location.latitude, location.longitude, speedMph)
        val notification = buildLiveNotification(coordText)
        notificationManager.notify(NOTIFICATION_ID, notification)

        // Stream to server
        serviceScope.launch {
            val success = NetworkClient.sendLocation(serverUrl, update)
            _lastSendStatus.value = success
        }
    }

    private fun stopLocationSharing() {
        Log.d(TAG, "Stopping location service immediately...")

        _isRunning.value = false

        // Release WakeLock
        try {
            wakeLock?.let {
                if (it.isHeld) {
                    it.release()
                    Log.d(TAG, "WakeLock released.")
                }
            }
        } catch (e: Exception) {
            Log.w(TAG, "Error releasing wakeLock: ${e.message}")
        }

        try {
            fusedLocationClient.removeLocationUpdates(fusedCallback)
        } catch (e: Exception) {
            Log.e(TAG, "Error removing location callbacks: ${e.message}")
        }

        try {
            systemLocationManager.removeUpdates(fallbackListener)
        } catch (e: Exception) {
            Log.e(TAG, "Error removing fallback listener: ${e.message}")
        }

        try {
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.N) {
                stopForeground(STOP_FOREGROUND_REMOVE)
            } else {
                @Suppress("DEPRECATION")
                stopForeground(true)
            }
            notificationManager.cancel(NOTIFICATION_ID)
            stopSelf()
        } catch (e: Exception) {
            Log.e(TAG, "Error stopping foreground: ${e.message}")
        }

        val lastLoc = _latestLocation.value
        val url = serverUrl
        val dId = driverId
        val dType = deliveryType
        serviceScope.launch(Dispatchers.IO) {
            try {
                val finalUpdate = DriverLocationUpdate(
                    driverId = dId,
                    deliveryType = dType,
                    latitude = lastLoc?.latitude ?: 0.0,
                    longitude = lastLoc?.longitude ?: 0.0,
                    isSharing = false
                )
                NetworkClient.sendLocation(url, finalUpdate)
            } catch (e: Exception) {
                Log.w(TAG, "Could not send stop status: ${e.message}")
            }
        }
    }

    override fun onDestroy() {
        super.onDestroy()
        serviceScope.cancel()
        try {
            fusedLocationClient.removeLocationUpdates(fusedCallback)
            systemLocationManager.removeUpdates(fallbackListener)
            wakeLock?.let { if (it.isHeld) it.release() }
        } catch (e: Exception) { }
        _isRunning.value = false
        Log.d(TAG, "LocationService onDestroy complete.")
    }

    override fun onBind(intent: Intent?): IBinder? = null

    companion object {
        private const val TAG = "LocationService"
        const val CHANNEL_ID = "delhi_kitchen_driver_live_v7"
        const val NOTIFICATION_ID = 9110

        const val ACTION_START = "com.delhikitchen.deliverydriver.action.START"
        const val ACTION_STOP = "com.delhikitchen.deliverydriver.action.STOP"

        const val EXTRA_SERVER_URL = "extra_server_url"
        const val EXTRA_DRIVER_ID = "extra_driver_id"
        const val EXTRA_DELIVERY_TYPE = "extra_delivery_type"

        private const val UPDATE_INTERVAL_MS = 2000L
        private const val FASTEST_INTERVAL_MS = 1000L

        private val _isRunning = MutableStateFlow(false)
        val isRunning: StateFlow<Boolean> = _isRunning.asStateFlow()

        private val _latestLocation = MutableStateFlow<DriverLocationUpdate?>(null)
        val latestLocation: StateFlow<DriverLocationUpdate?> = _latestLocation.asStateFlow()

        private val _lastSendStatus = MutableStateFlow<Boolean?>(null)
        val lastSendStatus: StateFlow<Boolean?> = _lastSendStatus.asStateFlow()

        fun forceStopLocally() {
            _isRunning.value = false
        }

        fun stopFromNotification(context: Context) {
            _isRunning.value = false
            val stopIntent = Intent(context, LocationService::class.java).apply {
                action = ACTION_STOP
            }
            context.startService(stopIntent)
        }
    }
}
