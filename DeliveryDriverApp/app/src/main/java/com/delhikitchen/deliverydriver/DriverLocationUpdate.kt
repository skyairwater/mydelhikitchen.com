package com.delhikitchen.deliverydriver

import java.text.SimpleDateFormat
import java.util.Date
import java.util.Locale
import java.util.TimeZone

/**
 * Data model matching the ASP.NET Core TrackingApiController JSON contract.
 */
data class DriverLocationUpdate(
    val driverId: String,
    val deliveryType: String = "Food",
    val latitude: Double,
    val longitude: Double,
    val speed: Float? = null,
    val bearing: Float? = null,
    val accuracy: Float? = null,
    val batteryLevel: Int? = null,
    val isSharing: Boolean = true,
    val timestamp: String = getCurrentIsoTimestamp()
) {
    fun toJson(): String {
        return buildString {
            append("{")
            append("\"driverId\":\"").append(escapeJson(driverId)).append("\",")
            append("\"deliveryType\":\"").append(escapeJson(deliveryType)).append("\",")
            append("\"latitude\":").append(latitude).append(",")
            append("\"longitude\":").append(longitude).append(",")
            if (speed != null) append("\"speed\":").append(speed).append(",")
            if (bearing != null) append("\"bearing\":").append(bearing).append(",")
            if (accuracy != null) append("\"accuracy\":").append(accuracy).append(",")
            if (batteryLevel != null) append("\"batteryLevel\":").append(batteryLevel).append(",")
            append("\"isSharing\":").append(isSharing).append(",")
            append("\"timestamp\":\"").append(timestamp).append("\"")
            append("}")
        }
    }

    companion object {
        fun getCurrentIsoTimestamp(): String {
            val sdf = SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSS'Z'", Locale.US)
            sdf.timeZone = TimeZone.getTimeZone("UTC")
            return sdf.format(Date())
        }

        private fun escapeJson(str: String): String {
            return str.replace("\\", "\\\\")
                .replace("\"", "\\\"")
                .replace("\n", "\\n")
                .replace("\r", "\\r")
        }
    }
}
