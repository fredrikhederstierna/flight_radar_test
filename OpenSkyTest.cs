using System;
using System.IO;
using System.Net;
using System.Text;

// Custom string substring utility function
public static partial class StringExtensions
{
    public static string MySubstring(this string s, int start, int end)
    {
        // Java style substring arguments, argument end is index, not legth
        return s.Substring(start, end - start);
    }
}

namespace OpenSky
{
    // Switzerland
    //   https://opensky-network.org/api/states/all?lamin=45.8389&lomin=5.9962&lamax=47.8229&lomax=10.5226

    // New Jersey
    //   https://opensky-network.org/api/states/all?lamin=39.065456&lomin=-75.448057&lamax=41.386476&lomax=-73.657286

    // Bjärred Sweden: 55°43'0.01"N, 13°1'0.01"E
    //   https://opensky-network.org/api/states/all?lamin=54.000000&lomin=12.000000&lamax=56.000000&lomax=14.000000


    class OpenSkyTest
    {

        private string OPENSKY_BASE_URL = "https://opensky-network.org/api/states/all";

        public void setBaseURL(String url)
        {
            this.OPENSKY_BASE_URL = url;
        }

        public String getBaseURL()
        {
            return this.OPENSKY_BASE_URL;
        }

        private static bool isStringInt(String s)
        {
            try
            {
                int.Parse(s);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
                return false;
            }
        }

        /** arg = <key>:<value> */
        private static void parseReply(string arg)
        {
            int colon = arg.IndexOf(':');
            string key = arg.MySubstring(0, colon);
            string value = arg.Substring(colon + 1);

            char firstChar = key[0]; // charAt
            char lastChar = key[key.Length - 1]; // charAt
            //Console.WriteLine("Key={0} Value={1} First={2} Last={3}", key, value, firstChar, lastChar);
            if (firstChar == '\"' && lastChar == '\"')
            {
                // unquote "key" into just key
                key = key.MySubstring(1, key.Length - 1);
                if (key.Equals("time"))
                {
                    // no further parsing, time is in standard Unix format
                    long unixTimestamp = (long)long.Parse(value);
                    // Unix timestamp is seconds past epoch
                    DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    dateTime = dateTime.AddSeconds((Int32)unixTimestamp).ToLocalTime();
                    Console.WriteLine("TIME = {0}", dateTime);
                }
                else if (key.Equals("states"))
                {
                    // parse [list]
                    char firstListChar = value[0]; // charAt(0)
                    char lastListChar = value[(value.Length - 1)]; // charAt
                    if (firstListChar == '[' && lastListChar == ']')
                    {
                        value = value.MySubstring(1, value.Length - 1);
                        //int pos = 0;
                        int len = value.Length;
                        int n = 0;
                        while (true)
                        {
                            int begin = value.IndexOf('[');
                            int end = value.IndexOf(']');
                            if ((begin == -1) || (end == -1))
                            {
                                break;
                            }
                            String param = value.Substring(begin + 1, end);
                            String[] ss = param.Split(",");

                            n++;
                            Console.WriteLine("AIRPLANE[" + n + "]:");

                            /* FROM: https://openskynetwork.github.io/opensky-api/python.html
                               class opensky_api.StateVector(arr)
                               Represents the state of a vehicle at a particular time.
                               It has the following fields:
                            */

                            for (int i = 0; i < ss.Length; i++)
                            {
                                Console.WriteLine("    ");
                                bool printTime = false;
                                string entry = ss[i];
                                switch (i)
                                {
                                    // icao24: str - ICAO24 address of the transmitter in hex string representation.
                                    case 0: Console.WriteLine("icao24"); break;
                                    // callsign: str - callsign of the vehicle. Can be None if no callsign has been received.
                                    case 1: Console.WriteLine("callsign"); break;
                                    // origin_country: str - inferred through the ICAO24 address.
                                    case 2: Console.WriteLine("origin_country"); break;
                                    // time_position: int - seconds since epoch of last position report.
                                    //     Can be None if there was no position report received by OpenSky within 15s before.
                                    case 3:
                                        Console.WriteLine("time_position");
                                        printTime = true;
                                        break;
                                    // last_contact: int - seconds since epoch of last received message from this transponder.
                                    case 4:
                                        Console.WriteLine("last_contact");
                                        printTime = true;
                                        break;
                                    // longitude: float - in ellipsoidal coordinates (WGS-84) and degrees. Can be None.
                                    case 5: Console.WriteLine("longitude"); break;
                                    // latitude: float - in ellipsoidal coordinates (WGS-84) and degrees. Can be None.
                                    case 6: Console.WriteLine("latitude"); break;
                                    // geo_altitude: float - geometric altitude in meters. Can be None.
                                    case 7: Console.WriteLine("geo_altitude"); break;
                                    // on_ground: bool - true if aircraft is on ground (sends ADS-B surface position reports).
                                    case 8: Console.WriteLine("on_ground"); break;
                                    // velocity: float - over ground in m/s. Can be None if information not present.
                                    case 9: Console.WriteLine("velocity"); break;
                                    // true_track: float - in decimal degrees (0 is north). Can be None if information not present.
                                    case 10: Console.WriteLine("true_track"); break;
                                    // vertical_rate: float - in m/s, incline is positive, decline negative. Can be None if information not present.
                                    case 11: Console.WriteLine("vertical_rate"); break;
                                    // sensors: list [int] - serial numbers of sensors which received messages from the vehicle within the validity period of this state vector.
                                    //    Can be None if no filtering for sensor has been requested.
                                    // TODO: parsing, arg can be in LIST [...] format ???
                                    case 12: Console.WriteLine("sensors"); break;
                                    // baro_altitude: float - barometric altitude in meters. Can be None.
                                    case 13: Console.WriteLine("baro_altitude"); break;
                                    // squawk: str - transponder code aka Squawk. Can be None.
                                    case 14: Console.WriteLine("squawk"); break;
                                    // spi: bool - special purpose indicator.
                                    case 15: Console.WriteLine("spi"); break;
                                    // position_source: int - origin of this state’s position:
                                    //    0 = ADS-B
                                    //    1 = ASTERIX
                                    //    2 = MLAT
                                    //    3 = FLARM
                                    case 16: Console.WriteLine("position_source"); break;
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
                                    case 17: Console.WriteLine("category"); break;

                                    default: Console.WriteLine("state param UNDEFINED:" + i); break;
                                }
                                Console.WriteLine(" = " + entry);
                                if (printTime)
                                {
                                    if (isStringInt(entry))
                                    {
                                        // no further parsing, time is in standard Unix format
                                        long unixTimestamp = (long)long.Parse(entry);
                                        // Unix timestamp is seconds past epoch
                                        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                                        dateTime = dateTime.AddSeconds((Int32)unixTimestamp).ToLocalTime();
                                        Console.WriteLine("({0})", dateTime);
                                    }
                                }
                                Console.WriteLine();
                            }

                            // skip and move to next entry
                            value = value.Substring(end + 1);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("?UNKNOWN KEY=" + key);
                }
            }
        }

        internal static async Task<String> GetResponse(string request)
        {
            HttpClient wClient = new HttpClient();
            byte[] responseData = await wClient.GetByteArrayAsync(request); // success!
            UTF8Encoding utf8 = new UTF8Encoding();
            String response = utf8.GetString(responseData, 0, responseData.Length);
            return response;
        }

        /**
         * Get ADSB data, and store optionally to file system
         */
        public async Task<string> GetADSB(double latMin, double latMax,
                                  double longMin, double longMax)
        {
            try
            {
                string requestString = string.Format(OPENSKY_BASE_URL + "?lamin={0:0.000000}&lomin={1:0.000000}&lamax={2:0.000000}&lomax={3:0.000000}", latMin, longMin, latMax, longMax);
                Console.WriteLine("HttpRequest = {0}", requestString);
                var respTask = GetResponse(requestString);
                //Console.WriteLine("Task = {0}", respTask);
                var download = await respTask;
                Console.WriteLine("HttpResponse = {0}", download);

                string s = download;
                char firstChar = s[0];
                char lastChar = s[s.Length - 1];
                if (firstChar == '{' && lastChar == '}')
                {
                    s = s.Substring(1, s.Length - 1);
                    // recursive descent parsing
                    int comma = s.IndexOf(',');
                    String left = s.Substring(0, comma);
                    String right = s.Substring(comma + 1);
                    parseReply(left);
                    parseReply(right);
                }
                return download;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
            }
            return "<ERROR>";
        }

        static async Task Main(string[] args)
        {
            Console.WriteLine("OpenSkyTest: Begin");
            OpenSkyTest ost = new OpenSkyTest();
            // long lat for Bjärred Sweden
            var resp = ost.GetADSB(54.0, 56.0, //lamin, lamax
                        12.0, 14.0  //lomin, lomax
                        );
            var result = await resp;
            Console.WriteLine("OpenSkyTest: {0} end", result);
        }
    }
}
