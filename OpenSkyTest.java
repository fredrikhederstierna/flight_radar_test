
import java.io.BufferedReader;
import java.io.File;
import java.io.FileWriter;
import java.io.InputStreamReader;
import java.io.IOException;

import java.net.MalformedURLException;
import java.net.URL;

import java.util.Date;

/**
 * Simple test of OpenSky API for aircraft info.
 *
 * @author Fredrik Hederstierna 2023
 */


// Switzerland
//   https://opensky-network.org/api/states/all?lamin=45.8389&lomin=5.9962&lamax=47.8229&lomax=10.5226

// New Jersey
//   https://opensky-network.org/api/states/all?lamin=39.065456&lomin=-75.448057&lamax=41.386476&lomax=-73.657286

// Bjärred Sweden: 55°43'0.01"N, 13°1'0.01"E
//   https://opensky-network.org/api/states/all?lamin=54.000000&lomin=12.000000&lamax=56.000000&lomax=14.000000


public class OpenSkyTest
{
  private String OPENSKY_BASE_URL = "https://opensky-network.org/api/states/all";

  public void setBaseURL(String url) {
    this.OPENSKY_BASE_URL = url;
  }

  public String getBaseURL() {
    return this.OPENSKY_BASE_URL;
  }

  private static boolean isStringInt(String s) {
    try {
      Integer.parseInt(s);
      return true;
    }
    catch (NumberFormatException nfe) {
      return false;
    }
  }

  /** arg = <key>:<value> */
  private static void parseReply(String arg) {

    Date date = new Date();

    int colon = arg.indexOf(':');
    String key = arg.substring(0,colon);
    String value = arg.substring(colon+1);

    char firstChar = key.charAt(0);
    char lastChar = key.charAt(key.length()-1);
    if (firstChar == '\"' && lastChar == '\"') {
      // unquote "key" into just key
      key = key.substring(1, key.length()-1);
      if (key.equals("time")) {
        // no further parsing, time is in standard Unix format
        date.setTime((long)Long.parseLong(value) * 1000);
        System.out.println("TIME = " + value + " (" + date + ")");
      }
      else if (key.equals("states")) {
        // parse [list]
        char firstListChar = value.charAt(0);
        char lastListChar = value.charAt(value.length()-1);
        if (firstListChar == '[' && lastListChar == ']') {

          value = value.substring(1, value.length()-1);
          int pos = 0;
          int len = value.length();
          int n = 0;
          while (true) {
            int begin = value.indexOf('[');
            int end = value.indexOf(']');
            if ((begin == -1) || (end == -1)) {
              break;
            }
            String param = value.substring(begin+1, end);
            String[] ss = param.split(",");

            n++;
            System.out.println("AIRPLANE[" + n + "]:");

            /* FROM: https://openskynetwork.github.io/opensky-api/python.html
               class opensky_api.StateVector(arr)
               Represents the state of a vehicle at a particular time.
               It has the following fields:
            */

            for (int i = 0; i < ss.length; i++) {
              System.out.print("    ");
              boolean printTime = false;
              String entry = ss[i];
              switch (i) {
                // icao24: str - ICAO24 address of the transmitter in hex string representation.
              case 0: System.out.print("icao24"); break;
                // callsign: str - callsign of the vehicle. Can be None if no callsign has been received.
              case 1: System.out.print("callsign"); break;
                // origin_country: str - inferred through the ICAO24 address.
              case 2: System.out.print("origin_country"); break;
                // time_position: int - seconds since epoch of last position report.
                //     Can be None if there was no position report received by OpenSky within 15s before.
              case 3: System.out.print("time_position");
                printTime = true;
                break;
                // last_contact: int - seconds since epoch of last received message from this transponder.
              case 4: System.out.print("last_contact");
                printTime = true;
                break;
                // longitude: float - in ellipsoidal coordinates (WGS-84) and degrees. Can be None.
              case 5: System.out.print("longitude"); break;
                // latitude: float - in ellipsoidal coordinates (WGS-84) and degrees. Can be None.
              case 6: System.out.print("latitude"); break;
                // geo_altitude: float - geometric altitude in meters. Can be None.
              case 7: System.out.print("geo_altitude"); break;
                // on_ground: bool - true if aircraft is on ground (sends ADS-B surface position reports).
              case 8: System.out.print("on_ground"); break;
                // velocity: float - over ground in m/s. Can be None if information not present.
              case 9: System.out.print("velocity"); break;
                // true_track: float - in decimal degrees (0 is north). Can be None if information not present.
              case 10: System.out.print("true_track"); break;
                // vertical_rate: float - in m/s, incline is positive, decline negative. Can be None if information not present.
              case 11: System.out.print("vertical_rate"); break;
                // sensors: list [int] - serial numbers of sensors which received messages from the vehicle within the validity period of this state vector.
                //    Can be None if no filtering for sensor has been requested.
                // TODO: parsing, arg can be in LIST [...] format ???
              case 12: System.out.print("sensors"); break;
                // baro_altitude: float - barometric altitude in meters. Can be None.
              case 13: System.out.print("baro_altitude"); break;
                // squawk: str - transponder code aka Squawk. Can be None.
              case 14: System.out.print("squawk"); break;
                // spi: bool - special purpose indicator.
              case 15: System.out.print("spi"); break;
                // position_source: int - origin of this state’s position:
                //    0 = ADS-B
                //    1 = ASTERIX
                //    2 = MLAT
                //    3 = FLARM
              case 16: System.out.print("position_source"); break;
                // category: int - aircraft category:
                //    0 = No information at all
                //    1 = No ADS-B Emitter Category Information
                //    2 = Light (< 15500 lbs)
                //    3 = Small (15500 to 75000 lbs)
                //    4 = Large (75000 to 300000 lbs)
                //    5 = High Vortex Large (aircraft such as B-757)
                //    6 = Heavy (> 300000 lbs)
                //    7 = High Performance (> 5g acceleration and 400 kts)
                //    8 = Rotorcraft
                //    9 = Glider / sailplane
                //   10 = Lighter-than-air
                //   11 = Parachutist / Skydiver
                //   12 = Ultralight / hang-glider / paraglider
                //   13 = Reserved
                //   14 = Unmanned Aerial Vehicle
                //   15 = Space / Trans-atmospheric vehicle
                //   16 = Surface Vehicle – Emergency Vehicle
                //   17 = Surface Vehicle – Service Vehicle
                //   18 = Point Obstacle (includes tethered balloons)
                //   19 = Cluster Obstacle
                //   20 = Line Obstacle.
              case 17: System.out.print("category"); break;
                
              default: System.out.println("state param UNDEFINED:" + i); break;
              }
              System.out.print(" = " + entry);
              if (printTime) {
                if (isStringInt(entry)) {
                  date.setTime((long)Long.parseLong(entry) * 1000);
                  System.out.print(" (" + date + ")");
                }
              }
              System.out.println();
            }

            // skip and move to next entry
            value = value.substring(end+1);            
          }
        }
      }
      else {
        System.out.println("?UNKNOWN KEY=" + key);
      }
    }
  }

  /**
   * Get ADSB data, and store optionally to file system
   */
  public void getADSB(double latMin, double latMax,
                      double longMin, double longMax,
                      FileWriter fileWriter) throws IOException {
    try {
      String requestString = String.format(OPENSKY_BASE_URL + "?lamin=%.6f&lomin=%.6f&lamax=%.6f&lomax=%.6f", latMin, longMin, latMax, longMax);
      URL url = new URL(requestString);
      BufferedReader in = new BufferedReader(new InputStreamReader(url.openStream()));
      StringBuilder lines = new StringBuilder();
      String line;
      do {
        line = in.readLine();
        if (line != null) {
          lines.append(line);
          if (fileWriter != null) {
            fileWriter.write(line);
            fileWriter.flush();
          }
        }
      } while (line != null);

      String s = lines.toString();
      char firstChar = s.charAt(0);
      char lastChar = s.charAt(s.length()-1);
      if (firstChar == '{' && lastChar == '}') {
        s = s.substring(1, s.length()-1);
        // recursive descent parsing
        int comma = s.indexOf(',');
        String left = s.substring(0,comma);
        String right = s.substring(comma+1);
        parseReply(left);
        parseReply(right);
      }
    } catch (MalformedURLException mue) {
      System.out.println("Malformed URL: " + mue.getMessage());
    }
  }

  /**
   * Main test function
   */
  public static void main(String[] args) {
    final String OPENSKY_DATA_FILENAME = "/home/fredrik/github/flight_radar_data.raw";
    try {
      OpenSkyTest opensky = new OpenSkyTest();
      File file = new File(OPENSKY_DATA_FILENAME);
      FileWriter fileWriter = new FileWriter(file);
      file.createNewFile();
      // long lat for Bjärred Sweden
      opensky.getADSB(54.0, 56.0, //lamin, lamax
                      12.0, 14.0, //lomin, lomax
                      fileWriter);
    }
    catch (IOException ioe) {
      System.out.println("IO Exception: " + ioe.getMessage());
      ioe.printStackTrace();
    }
  }
}
