package com.legat.tinyserv;

import android.content.Entity;
import android.net.wifi.WifiManager;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.text.format.Formatter;
import android.widget.TextView;

import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Map;

import fi.iki.elonen.NanoHTTPD;

public class MainActivity extends AppCompatActivity {
    MyServer myServer;
    TextView textView;
    EntityHolder entityHolder;
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        entityHolder = new EntityHolder();
        setContentView(R.layout.activity_main);
        textView = (TextView)findViewById(R.id.textView);
        WifiManager wm = (WifiManager) getSystemService(WIFI_SERVICE);
        String ip = Formatter.formatIpAddress(wm.getConnectionInfo().getIpAddress());
        textView.setText("Your ip: "+ip);
        try {
            myServer = new MyServer();
        } catch (IOException e) {
            e.printStackTrace();
        }
    }


     class MyServer extends NanoHTTPD {

        private final static int PORT = 8080;

        public MyServer() throws IOException {
            super(PORT);
            start();
            //System.out.println( "\nRunning! Point your browers to http://localhost:8080/ \n" );
        }
        @Override
        public Response serve(String uri, Method method,
                              Map<String, String> header,
                              Map<String, String> parameters,
                              Map<String, String> files) {
            String result="";
            if (uri.contains("getall")){

                for (Entity en:entityHolder.getEntityArrayList()){
                    result+=entityHolder.entityInJson(en)+"\n";
                }
            }
            else if (uri.contains("getreq")){
                ArrayList<Entity> tempList = new ArrayList<>();
               // parameters.get("level").equals("all")
                for (Entity en:entityHolder.getEntityArrayList()){
                    if (en.getResult().equals(parameters.get("level"))&&Long.parseLong(parameters.get("time"))<en.getScan_time()){
                        tempList.add(en);
                    }
                }
                for (Entity en:tempList){
                    result+=entityHolder.entityInJson(en)+"\n";;
                }
            }
            else if (uri.contains("file")) {
                FileInputStream fis = null;
                String fileName = uri.substring(uri.indexOf('e')+1);
                int nFile = Integer.parseInt(fileName);
                String fullFileName = String.format("enspect/"+ nFile+".esp");
                try {
                    fis = new FileInputStream(fullFileName);
                } catch (FileNotFoundException e) {

                    e.printStackTrace();
                }
                return new NanoHTTPD.Response(Response.Status.OK, "APPLICATION_OCTET_STREAM", fis);
            }

            return new Response(result);
        }

    }

    class Entity {
        long _id;
        long scan_time;

        public Entity(long _id, long scan_time, String id_num, String plate_num, String mateial, String result) {
            this._id = _id;
            this.scan_time = scan_time;
            this.id_num = id_num;
            this.plate_num = plate_num;
            this.mateial = mateial;
            this.result = result;
        }

        String id_num;
        String plate_num;
        String mateial;
        String result;

        public long get_id() {
            return _id;
        }

        public void set_id(long _id) {
            this._id = _id;
        }

        public long getScan_time() {
            return scan_time;
        }

        public void setScan_time(long scan_time) {
            this.scan_time = scan_time;
        }

        public String getId_num() {
            return id_num;
        }

        public void setId_num(String id_num) {
            this.id_num = id_num;
        }

        public String getPlate_num() {
            return plate_num;
        }

        public void setPlate_num(String plate_num) {
            this.plate_num = plate_num;
        }

        public String getMateial() {
            return mateial;
        }

        public void setMateial(String mateial) {
            this.mateial = mateial;
        }

        public String getResult() {
            return result;
        }

        public void setResult(String result) {
            this.result = result;
        }
    }

    class EntityHolder {
        ArrayList<Entity> entityArrayList;

        public ArrayList<Entity> getEntityArrayList() {
            return entityArrayList;
        }

        public EntityHolder() {
            this.entityArrayList = new ArrayList<>();
            entityArrayList.add(new Entity(1, 1000, "test1","plate1", "Ammonium Perchlorate", "EXPLOSIVES" ));
            entityArrayList.add(new Entity(2, 2000, "test2","plate2", "NONE", "PASSIVE" ));
            entityArrayList.add(new Entity(3, 3000, "test3","plate3", "Ammonium Perchlorate", "SUSPECTED" ));
            entityArrayList.add(new Entity(4, 4000, "test4","plate4", "Ammonium Perchlorate", "PASSIVE" ));
            entityArrayList.add(new Entity(5, 5000, "test5","plate5", "Ammonium Perchlorate", "PASSIVE" ));
            entityArrayList.add(new Entity(6, 6000, "test6","plate6", "Ammonium Perchlorate", "SUSPECTED" ));
        }
         public String entityInJson(Entity entity){
            String result="{";
            result+= "\"_id+\":\""+entity.get_id()+",";
            result+= "\"time+\":\""+entity.getScan_time()+",";
            result+= "\"id_num+\":\""+entity.getId_num()+",";
            result+= "\"plate_num+\":\""+entity.getPlate_num()+",";
            result+= "\"material+\":\""+entity.getMateial()+",";
            result+= "\"result+\":\""+entity.getResult();

            return result+"}";
        }
        public Entity  getEntBuId (long id){
            Entity result=null;
            for (Entity en:entityArrayList){

                if (en.get_id()==id){
                result = en;
                }
            }
            return result;
        }
    }
}