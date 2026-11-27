package com.delhikitchen.deliverydriver

import android.Manifest
import android.content.Context
import android.content.Intent
import android.content.pm.PackageManager
import android.content.res.ColorStateList
import android.location.LocationManager
import android.net.Uri
import android.os.Build
import android.os.Bundle
import android.provider.Settings
import android.view.View
import android.widget.Toast
import androidx.activity.result.contract.ActivityResultContracts
import androidx.appcompat.app.AlertDialog
import androidx.appcompat.app.AppCompatActivity
import androidx.core.app.NotificationManagerCompat
import androidx.core.content.ContextCompat
import androidx.lifecycle.lifecycleScope
import com.delhikitchen.deliverydriver.databinding.ActivityMainBinding
import kotlinx.coroutines.flow.collectLatest
import kotlinx.coroutines.launch
import java.util.Locale

class MainActivity : AppCompatActivity() {

    private lateinit var binding: ActivityMainBinding

    private val defaultServerUrl = "http://192.168.1.94:5140/api/tracking/location"

    private val prefs by lazy {
        getSharedPreferences("driver_prefs", Context.MODE_PRIVATE)
    }

    private val permissionLauncher = registerForActivityResult(
        ActivityResultContracts.RequestMultiplePermissions()
    ) { permissions ->
        val fineLocationGranted = permissions[Manifest.permission.ACCESS_FINE_LOCATION] ?: false
        val coarseLocationGranted = permissions[Manifest.permission.ACCESS_COARSE_LOCATION] ?: false
        val notificationGranted = if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.TIRAMISU) {
            permissions[Manifest.permission.POST_NOTIFICATIONS] ?: true
        } else {
            true
        }

        if (fineLocationGranted || coarseLocationGranted) {
            if (!notificationGranted || !NotificationManagerCompat.from(this).areNotificationsEnabled()) {
                showNotificationPermissionDialog()
            }
            startSharingService()
        } else {
            Toast.makeText(
                this,
                "Precise location permission is required for live delivery tracking.",
                Toast.LENGTH_LONG
            ).show()
        }
    }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        binding = ActivityMainBinding.inflate(layoutInflater)
        setContentView(binding.root)

        loadSavedSettings()
        setupListeners()
        observeServiceState()
    }

    private fun loadSavedSettings() {
        val randomSuffix = (100..999).random()
        val defaultId = "Driver-$randomSuffix"
        val savedDriverId = prefs.getString("driver_id", defaultId) ?: defaultId
        val savedServerUrl = prefs.getString("server_url", defaultServerUrl) ?: defaultServerUrl
        val savedDeliveryType = prefs.getString("delivery_type", "Food") ?: "Food"

        binding.etDriverId.setText(savedDriverId)
        binding.etServerUrl.setText(savedServerUrl)

        if (savedDeliveryType.equals("Grocery", true)) {
            binding.rbGrocery.isChecked = true
        } else {
            binding.rbFood.isChecked = true
        }
    }

    private fun saveSettings() {
        val driverId = binding.etDriverId.text?.toString()?.trim() ?: "Driver 1"
        val serverUrl = binding.etServerUrl.text?.toString()?.trim() ?: defaultServerUrl
        val deliveryType = if (binding.rbGrocery.isChecked) "Grocery" else "Food"

        prefs.edit()
            .putString("driver_id", driverId)
            .putString("server_url", serverUrl)
            .putString("delivery_type", deliveryType)
            .apply()
    }

    private fun setupListeners() {
        binding.btnToggleSharing.setOnClickListener {
            saveSettings()
            if (LocationService.isRunning.value) {
                stopSharingService()
            } else {
                checkDeviceGpsAndPermissions()
            }
        }

        // Test connection button to instantly verify phone-to-PC network connectivity
        binding.btnTestConnection.setOnClickListener {
            saveSettings()
            val url = binding.etServerUrl.text?.toString()?.trim() ?: defaultServerUrl
            binding.btnTestConnection.isEnabled = false
            binding.btnTestConnection.text = "Testing..."

            lifecycleScope.launch {
                val (success, message) = NetworkClient.testConnection(url)
                binding.btnTestConnection.isEnabled = true
                binding.btnTestConnection.text = "Test Server Connection"

                AlertDialog.Builder(this@MainActivity)
                    .setTitle(if (success) "Connection Succeeded! ✅" else "Connection Failed ❌")
                    .setMessage(message)
                    .setPositiveButton("OK", null)
                    .show()
            }
        }
    }

    private fun checkDeviceGpsAndPermissions() {
        // 1. Check if device master location toggle (GPS) is turned ON
        val locationManager = getSystemService(Context.LOCATION_SERVICE) as LocationManager
        val isGpsEnabled = locationManager.isProviderEnabled(LocationManager.GPS_PROVIDER)
        val isNetworkEnabled = locationManager.isProviderEnabled(LocationManager.NETWORK_PROVIDER)

        if (!isGpsEnabled && !isNetworkEnabled) {
            AlertDialog.Builder(this)
                .setTitle("Turn On Device Location (GPS)")
                .setMessage("Your phone's Location / GPS toggle is currently turned OFF. Please turn on Location in Quick Settings or Android Settings to allow live GPS tracking.")
                .setPositiveButton("Open Location Settings") { _, _ ->
                    startActivity(Intent(Settings.ACTION_LOCATION_SOURCE_SETTINGS))
                }
                .setNegativeButton("Cancel", null)
                .show()
            return
        }

        // 2. Check runtime permissions
        checkPermissionsAndStart()
    }

    private fun checkPermissionsAndStart() {
        val permissionsToRequest = mutableListOf(
            Manifest.permission.ACCESS_FINE_LOCATION,
            Manifest.permission.ACCESS_COARSE_LOCATION
        )

        // Android 13+ (API 33) requires POST_NOTIFICATIONS
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.TIRAMISU) {
            if (ContextCompat.checkSelfPermission(this, Manifest.permission.POST_NOTIFICATIONS)
                != PackageManager.PERMISSION_GRANTED
            ) {
                permissionsToRequest.add(Manifest.permission.POST_NOTIFICATIONS)
            }
        }

        val missingPermissions = permissionsToRequest.filter {
            ContextCompat.checkSelfPermission(this, it) != PackageManager.PERMISSION_GRANTED
        }

        if (missingPermissions.isEmpty()) {
            if (!NotificationManagerCompat.from(this).areNotificationsEnabled()) {
                showNotificationPermissionDialog()
                return
            }
            startSharingService()
        } else {
            // Request permissions via system prompt
            permissionLauncher.launch(missingPermissions.toTypedArray())
        }
    }

    private fun showNotificationPermissionDialog() {
        AlertDialog.Builder(this)
            .setTitle("Enable Notifications")
            .setMessage("Notifications must be turned ON so the live location widget and Stop button can appear on your lock screen and Samsung Now Bar.")
            .setPositiveButton("Open Settings") { _, _ ->
                val intent = Intent().apply {
                    if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
                        action = Settings.ACTION_APP_NOTIFICATION_SETTINGS
                        putExtra(Settings.EXTRA_APP_PACKAGE, packageName)
                    } else {
                        action = Settings.ACTION_APPLICATION_DETAILS_SETTINGS
                        data = Uri.fromParts("package", packageName, null)
                    }
                }
                startActivity(intent)
            }
            .setNegativeButton("Continue Anyway") { _, _ ->
                startSharingService()
            }
            .show()
    }

    private fun startSharingService() {
        val driverId = binding.etDriverId.text?.toString()?.trim().takeUnless { it.isNullOrEmpty() } ?: "Driver 1"
        val serverUrl = binding.etServerUrl.text?.toString()?.trim().takeUnless { it.isNullOrEmpty() } ?: defaultServerUrl
        val deliveryType = if (binding.rbGrocery.isChecked) "Grocery" else "Food"

        val serviceIntent = Intent(this, LocationService::class.java).apply {
            action = LocationService.ACTION_START
            putExtra(LocationService.EXTRA_DRIVER_ID, driverId)
            putExtra(LocationService.EXTRA_DELIVERY_TYPE, deliveryType)
            putExtra(LocationService.EXTRA_SERVER_URL, serverUrl)
        }

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            ContextCompat.startForegroundService(this, serviceIntent)
        } else {
            startService(serviceIntent)
        }
    }

    private fun stopSharingService() {
        LocationService.forceStopLocally()
        updateUiForSharingState(false)

        val stopIntent = Intent(this, LocationService::class.java).apply {
            action = LocationService.ACTION_STOP
        }
        startService(stopIntent)
    }

    private fun observeServiceState() {
        // Observe sharing active/idle state
        lifecycleScope.launch {
            LocationService.isRunning.collectLatest { running ->
                updateUiForSharingState(running)
            }
        }

        // Observe live coordinate updates
        lifecycleScope.launch {
            LocationService.latestLocation.collectLatest { update ->
                if (update != null && LocationService.isRunning.value) {
                    binding.layoutTelemetry.visibility = View.VISIBLE
                    binding.tvLiveCoordinates.text = String.format(
                        Locale.US,
                        "GPS: %.5f, %.5f",
                        update.latitude,
                        update.longitude
                    )

                    val speedMph = (update.speed ?: 0f) * 2.23694f
                    val accuracyText = if (update.accuracy != null) "±${update.accuracy.toInt()}m" else "High"
                    binding.tvSpeedAccuracy.text = String.format(
                        Locale.US,
                        "Speed: %.1f mph • Accuracy: %s",
                        speedMph,
                        accuracyText
                    )
                } else if (!LocationService.isRunning.value) {
                    binding.layoutTelemetry.visibility = View.GONE
                }
            }
        }

        // Observe transmission success
        lifecycleScope.launch {
            LocationService.lastSendStatus.collectLatest { success ->
                if (success != null && LocationService.isRunning.value) {
                    if (success) {
                        binding.tvLastPingTime.text = "Streaming live • Sent"
                        binding.tvLastPingTime.setTextColor(ContextCompat.getColor(this@MainActivity, R.color.accent))
                    } else {
                        binding.tvLastPingTime.text = "Error reaching server"
                        binding.tvLastPingTime.setTextColor(ContextCompat.getColor(this@MainActivity, R.color.danger))
                    }
                }
            }
        }
    }

    private fun updateUiForSharingState(isSharing: Boolean) {
        if (isSharing) {
            val deliveryType = if (binding.rbGrocery.isChecked) "Grocery" else "Food"
            val typeEmoji = if (deliveryType == "Grocery") "🛒" else "🚗"

            // Button changes to STOP SHARING
            binding.btnToggleSharing.text = getString(R.string.stop_sharing)
            binding.btnToggleSharing.setIconResource(R.drawable.ic_stop)
            binding.btnToggleSharing.backgroundTintList = ColorStateList.valueOf(
                ContextCompat.getColor(this, R.color.danger)
            )

            // Status header
            binding.tvStatusHeader.text = "Broadcasting $typeEmoji $deliveryType"
            binding.tvStatusHeader.setTextColor(ContextCompat.getColor(this, R.color.accent))
            binding.viewStatusDot.backgroundTintList = ColorStateList.valueOf(
                ContextCompat.getColor(this, R.color.accent)
            )
            binding.tvStatusSub.text = "Live notification active in background and lock screen. Tap 'Stop Sharing' in app or on the notification to end."

            binding.tvLastPingTime.text = "Acquiring location..."
            binding.tvLastPingTime.setTextColor(ContextCompat.getColor(this, R.color.primary))

            // Disable editing endpoint during active broadcast
            binding.etDriverId.isEnabled = false
            binding.etServerUrl.isEnabled = false
            binding.btnTestConnection.isEnabled = false
            binding.rbFood.isEnabled = false
            binding.rbGrocery.isEnabled = false
        } else {
            // Button changes to START SHARING
            binding.btnToggleSharing.text = getString(R.string.start_sharing)
            binding.btnToggleSharing.setIconResource(R.drawable.ic_play)
            binding.btnToggleSharing.backgroundTintList = ColorStateList.valueOf(
                ContextCompat.getColor(this, R.color.accent)
            )

            // Status header
            binding.tvStatusHeader.text = "Idle"
            binding.tvStatusHeader.setTextColor(ContextCompat.getColor(this, R.color.text_secondary))
            binding.viewStatusDot.backgroundTintList = ColorStateList.valueOf(
                ContextCompat.getColor(this, R.color.text_secondary)
            )
            binding.tvStatusSub.text = "Press 'Start Sharing' below to broadcast your live GPS coordinates to the website."
            binding.tvLastPingTime.text = "Not sharing"
            binding.tvLastPingTime.setTextColor(ContextCompat.getColor(this, R.color.text_secondary))
            binding.layoutTelemetry.visibility = View.GONE

            // Re-enable editing endpoint
            binding.etDriverId.isEnabled = true
            binding.etServerUrl.isEnabled = true
            binding.btnTestConnection.isEnabled = true
            binding.rbFood.isEnabled = true
            binding.rbGrocery.isEnabled = true
        }
    }
}
