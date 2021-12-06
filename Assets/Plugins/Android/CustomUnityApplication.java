package com.api.unityactivityinterface;
import test.testAppInit;
import android.app.Application;
import java.util.ArrayList;
public class CustomUnityApplication extends Application{
     ArrayList<IOnAppCreate> _ionappcreate = new ArrayList<IOnAppCreate>();
     ArrayList<IOnAppTerminate> _ionappterminate = new ArrayList<IOnAppTerminate>();
     public CustomUnityApplication(){
          testAppInit _test_testappinit = new testAppInit();
          _ionappcreate.add(_test_testappinit);
     }
     @Override
     public void onCreate(){
          super.onCreate();
          for (IOnAppCreate item: _ionappcreate){
               item.onCreate(this);
          }
     }
     public void onTerminate(){
          super.onTerminate();
          for (IOnAppTerminate item: _ionappterminate){
               item.onTerminate(this);
          }
     }
}
