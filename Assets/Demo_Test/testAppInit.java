package test;

import android.app.Activity;
import android.app.Application;
import android.os.Bundle;
import android.util.Log;

import com.api.unityactivityinterface.IOnAppCreate;

public class testAppInit implements IOnAppCreate {
    @Override
    public void onCreate(Application app) {
        Log.v("Unity","on A PPCreate"+app);
    }
}
