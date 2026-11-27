package com.delhikitchen.deliverydriver

import android.content.BroadcastReceiver
import android.content.Context
import android.content.Intent
import android.util.Log

class NotificationActionReceiver : BroadcastReceiver() {
    override fun onReceive(context: Context, intent: Intent) {
        if (intent.action == ACTION_STOP_SERVICE) {
            Log.d("NotificationActionReceiver", "Stop Sharing pressed from notification action!")
            LocationService.stopFromNotification(context)
        }
    }

    companion object {
        const val ACTION_STOP_SERVICE = "com.delhikitchen.deliverydriver.ACTION_STOP_SERVICE"
    }
}
