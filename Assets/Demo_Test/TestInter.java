package test;

import android.os.Bundle;
import android.util.Log;

import com.api.unityactivityinterface.IOnCreate;

public class TestInter implements IOnCreate {
    @Override
    public void onCreate(Bundle savedInstanceState) {
        Log.v("Unity","OnCreate");
    }
}
