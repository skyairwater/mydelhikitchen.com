package com.delhikitchen.deliverydriver

import android.content.Context
import android.os.BatteryManager
import android.util.Log
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import okhttp3.MediaType.Companion.toMediaType
import okhttp3.OkHttpClient
import okhttp3.Request
import okhttp3.RequestBody.Companion.toRequestBody
import java.util.concurrent.TimeUnit

object NetworkClient {
    private const val TAG = "NetworkClient"

    private val JSON_MEDIA_TYPE = "application/json; charset=utf-8".toMediaType()

    private val httpClient = OkHttpClient.Builder()
        .connectTimeout(5, TimeUnit.SECONDS)
        .writeTimeout(5, TimeUnit.SECONDS)
        .readTimeout(5, TimeUnit.SECONDS)
        .retryOnConnectionFailure(true)
        .build()

    /**
     * Sends the driver location payload to the web server endpoint asynchronously.
     */
    suspend fun sendLocation(serverUrl: String, payload: DriverLocationUpdate): Boolean {
        return withContext(Dispatchers.IO) {
            try {
                val json = payload.toJson()
                val body = json.toRequestBody(JSON_MEDIA_TYPE)

                val request = Request.Builder()
                    .url(serverUrl)
                    .post(body)
                    .build()

                httpClient.newCall(request).execute().use { response ->
                    if (response.isSuccessful) {
                        Log.d(TAG, "Location sent successfully: ${response.code}")
                        true
                    } else {
                        Log.w(TAG, "Server error (${response.code}): ${response.message}")
                        false
                    }
                }
            } catch (e: Exception) {
                Log.e(TAG, "Failed to send location update: ${e.message}", e)
                false
            }
        }
    }

    /**
     * Test connection to the server endpoint with detailed status.
     */
    suspend fun testConnection(serverUrl: String): Pair<Boolean, String> {
        return withContext(Dispatchers.IO) {
            try {
                val testPayload = DriverLocationUpdate(
                    driverId = "ConnectionTest",
                    latitude = 0.0,
                    longitude = 0.0,
                    isSharing = false
                )
                val body = testPayload.toJson().toRequestBody(JSON_MEDIA_TYPE)
                val request = Request.Builder()
                    .url(serverUrl)
                    .post(body)
                    .build()

                httpClient.newCall(request).execute().use { response ->
                    if (response.isSuccessful) {
                        Pair(true, "Connected! Server returned HTTP ${response.code}")
                    } else {
                        Pair(false, "Server responded with HTTP ${response.code}: ${response.message}")
                    }
                }
            } catch (e: Exception) {
                Pair(false, "Connection failed: ${e.localizedMessage ?: e.message}")
            }
        }
    }

    /**
     * Helper to read device battery percentage.
     */
    fun getBatteryLevel(context: Context): Int? {
        return try {
            val batteryManager = context.getSystemService(Context.BATTERY_SERVICE) as? BatteryManager
            batteryManager?.getIntProperty(BatteryManager.BATTERY_PROPERTY_CAPACITY)
        } catch (e: Exception) {
            null
        }
    }
}
