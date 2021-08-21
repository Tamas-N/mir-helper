using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace MiR_Helper.rest
{
    /// <summary>
    /// Out robot class will contain all of the data conatined within a robot. 
    /// This includes connection parameters (such as IP), along with other abstract data such as position, etc. 
    /// </summary>
    public class robot
    {
        private static bool isFleet;
        private static IPAddress IP;
        private static AuthenticationManager Auth;

        /// <summary>
        /// Empty constructor, just initialize all the variables so we don't get null exceptions later on
        /// </summary>
        public robot()
        {
            isFleet = false;
            IP = new IPAddress(0x2414188f);
            /*Auth = new AuthenticationManager();*/
        }

        public void GetIP(string RawIPAddress)
        {
            try
            { 
                IP = IPAddress.Parse(RawIPAddress);
            }
            catch (ArgumentNullException e)
            {

            }
            catch (FormatException e)
            {

            }
            catch (Exception e)
            {

            }
        }
    }
}
