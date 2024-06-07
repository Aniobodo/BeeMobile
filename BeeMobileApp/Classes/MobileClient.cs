using BeeAPPLIB;
using BeeMobileApp.Pages;
using CommunityToolkit.Maui.Views;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;


namespace BeeMobileApp.Classes
{
    public class MobileClient : MainPage        //This class is used to connect the mobile device with the server application of BeeAPP (Connect to BeeAPPServer)
    {
        private Socket _socket { get; set; }
        public bool Valid { get; set; } = false;
        public string ReturnData { get; set; }

        //static new async Task<string> DisplayActionSheet(string title, string cancel, string destruction, params string[] buttons) => await DisplayActionSheet(title, cancel, destruction, buttons);

        private const int BUFFER_SIZE = 10480000;

        public async Task ConnectClient()   //Tries to connect to the server. If not connected then shows a message
        {
            var isconnected = Init();
            while (!isconnected)
            {
                /*Gilt nicht ab 23.04.2024 App.IsWaiting.Close(null);*/
                string result = DisplayActionSheet("Cannot connect to the server", "Cancel", null, "Retry").ToString();
                if (result == "Retry")
                {
                    this.ShowPopup(new WaitPopup());
                    await Globals.Wait1MicSec();
                    isconnected = Init();
                }
                else break;
            }
            if (isconnected && Preferences.ContainsKey("ID") && !string.IsNullOrEmpty(Generic.GetID())) { ClientToServerConnection(); }
            /*Gilt nicht ab 23.04.2024 App.IsWaiting.Close(null);*/
        }
        public bool Init()      //_socket is initialysed using the BeeAPPServer credentials and tries connection to server is made
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.ConnectAsync(Constants.IPfromOutside, Constants.PortFromOutside);   //If someone wants to debug than they can use a local IP here.

            Stopwatch sw = Stopwatch.StartNew();
            while (true) { if (sw.ElapsedMilliseconds > 3000 || _socket.Connected) break; } //Wait to see if the client socket makes a connection.
            sw.Stop();

            if (!_socket.Connected) _socket.Close();
            return _socket.Connected;
        }

        public void ClientRefresh()     //Call this method everytime before requesting data from server
        {
            Valid = false; ReturnData = null;
        }

        public bool Register(string option, string email)   //register the user on app/ gets user info from the server/password reset/saves the user info/Change password
        {
            string SendData = option + ";" + email; SocketSend("1", SendData); return true;
        }

        public void UserSignIn(string email, string password) //Login user
        {
            string SendData = email + ";" + password; SocketSend("2", SendData);
        }

        public void ExportData(string proj)     //Exports the bee data to the server
        {
            string SendData = Constants.DataSep + proj + Constants.DataSep + Generic.GetID();
            SocketSend("3", SendData);
        }

        public void GetProjects(string gameOrsite)  //Get all the bee projects
        {
            string SendData = Constants.DataSep + Generic.GetID() + Constants.DataSep + gameOrsite; SocketSend("4", SendData);
        }

        public void LockGame(string proj, string email, string GID, string islock) //Saves or unsaves the game
        {
            string SendData = Constants.DataSep + proj + Constants.DataSep + email + Constants.DataSep + GID + Constants.DataSep + islock; SocketSend("5", SendData);
        }

        public void ProductionLogisticQR(string personal, string proj, string qr) //Production and Logistic update
        {
            string SendData = Constants.DataSep + personal + Constants.DataSep + proj + Constants.DataSep + qr; SocketSend("6", SendData);
        }

        public void RankingChart(string index)  //Gets the ranking data
        {
            string SendData = Constants.DataSep + Globals.GameOrSite + Constants.DataSep + index + Constants.DataSep + Globals.ProjName; SocketSend("7", SendData);
        }

        public void GetStrucMemInfo(string proj, string siteGame)     //Get all the stuctural members of a project
        {
            string SendData = Constants.DataSep + proj + Constants.DataSep + siteGame; SocketSend("8", SendData);
        }

        public void NaviOptions(string pname, string sname, string gamesite, string getset, string value = null, string gameHost = null, string GID = null) //Sets or gets the navigation value of rollout plan
        {
            string SendData = Constants.DataSep + pname + Constants.DataSep + sname + Constants.DataSep + gamesite + Constants.DataSep + getset + Constants.DataSep + value + Constants.DataSep + gameHost + Constants.DataSep + GID;
            SocketSend("9", SendData);
        }

        public void RemoveResetProj(string Pname, string gameSite, string GID = null)       //Remove the site project of resets the game project
        {
            string SendData = Constants.DataSep + Pname + Constants.DataSep + gameSite + Constants.DataSep + Generic.GetID() + Constants.DataSep + GID; SocketSend("10", SendData);
        }
        public void DataBars(string proj, string strucmem, string BarsID, string LayerOrCarpet, string gameSite)    //Get the info of the bee bars (carpet bars or manual bars)
        {
            string SendData = Constants.DataSep + proj + Constants.DataSep + strucmem + Constants.DataSep + BarsID + Constants.DataSep + LayerOrCarpet + Constants.DataSep + gameSite + Constants.DataSep + Generic.GetID();
            SocketSend("11", SendData);
        }
        public void GetProjectDetails(string proj, string gamesite, string id, string GID)      //Gets all the information of a project
        {
            string SendData = Constants.DataSep + proj + Constants.DataSep + gamesite + Constants.DataSep + id + Constants.DataSep + GID; SocketSend("12", SendData);
        }
        public void UsersAndPartners(string UandP, string proj, string id = null, string emails = null, string GID = null)      //Gets the app users / gets or sets the project partners
        {
            string SendData = Constants.DataSep + UandP + Constants.DataSep + proj + Constants.DataSep + id + Constants.DataSep + emails + Constants.DataSep + GID; SocketSend("13", SendData);
        }

        public void CheckStorage(string proj)       //Checks all the carpet storages for any missing carpet (carpet which is not scanned yet)
        {
            string SendData = Constants.DataSep + proj; SocketSend("14", SendData);
        }

        public void StorageInfo(string gameSite, string Project, string storagename, string QrText, string size, string Pos, EnumOrientation orient, string NaviVal) //Gets or sets the information of a storage
        {
            string SendData = Constants.DataSep + gameSite + Constants.DataSep + Project + Constants.DataSep + storagename + Constants.DataSep + QrText + Constants.DataSep + size + Constants.DataSep +
                    Pos + Constants.DataSep + orient + Constants.DataSep + NaviVal;
            SocketSend("15", SendData);
        }
        public void CheckCarpet(string proj, string strucmem, string gameSite, string Host, string GID)   //Check if the crane team has sent a new carpet
        {
            string SendData = Constants.DataSep + proj + Constants.DataSep + strucmem + Constants.DataSep + gameSite + Constants.DataSep + Host + Constants.DataSep + GID;
            SocketSend("16", SendData);
        }
        public void GetStorage(string proj, string storagename)     //Get position of the next place on the storage
        {
            string SendData = Constants.DataSep + proj + Constants.DataSep + storagename + Constants.DataSep + "Storage" + Constants.DataSep + Globals.GameOrSite;
            SocketSend("17", SendData);
        }

        public void Anchorage(string proj, string strucmem, string carpet, string gameSite)       //Get carpet anchor points
        {
            string SendData = Constants.DataSep + proj + Constants.DataSep + strucmem + Constants.DataSep + carpet + Constants.DataSep + gameSite; SocketSend("18", SendData);
        }
        public void GetRollOutPlan(string gameSite, string proj, string strucmem)       //Get the rollout info for all layers
        {
            string SendData = Constants.DataSep + gameSite + Constants.DataSep + proj + Constants.DataSep + strucmem; SocketSend("19", SendData);
        }
        public void PerformanceUpdate(string pr, string strucmem, string qr, string plan)      //Update the performance and qr data of the carpet on server.
        {
            string SendData = Constants.DataSep + pr + Constants.DataSep + strucmem + Constants.DataSep + qr + Constants.DataSep + plan + Constants.DataSep + Globals.GameOrSite + Constants.DataSep + Globals.GameHostID + Constants.DataSep + Globals.GameID;
            SocketSend("20", SendData);
        }
        public void GeomImport(string pr, string strucmem, string gameSite)       //Import the geometry of the structutal member
        {
            string SendData = Constants.DataSep + pr + Constants.DataSep + strucmem + Constants.DataSep + gameSite; SocketSend("21", SendData);
        }

        public void PlanOptions(string pr, string strucmem, string purpose, string CraneIns, string Info = null, string carpetsDone = null)
        {
            string SendData = Constants.DataSep + pr + Constants.DataSep + strucmem + Constants.DataSep + purpose + Constants.DataSep + CraneIns + Constants.DataSep + Info +
                    Constants.DataSep + carpetsDone + Constants.DataSep + Globals.GameOrSite + Constants.DataSep + Globals.GameHostID + Constants.DataSep + Generic.GetID() + Constants.DataSep + Globals.GameID;
            SocketSend("22", SendData);
        }

        public void TeamInfo(string proj, string strucmem, string parameter)  //Gets or sets the team information
        {
            string SendData = Constants.DataSep + proj + Constants.DataSep + strucmem + Constants.DataSep + parameter; SocketSend("23", SendData);
        }
        public void GetStorageInfo(string proj)     //Gets all the storage that have been created
        {
            string SendData = Constants.DataSep + proj; SocketSend("24", SendData);
        }
        public void DeleteStorage(string proj, string storagename)       //Delete a storage
        {
            string SendData = Constants.DataSep + proj + Constants.DataSep + storagename; SocketSend("25", SendData);
        }
        public void ClientToServerConnection()
        {
            string SendData = Constants.DataSep + Generic.GetID(); SocketSend("26", SendData);
        }
        public void SendProtcol(string proj, string email)      //Sends a pdf report of the project
        {
            string SendData = Constants.DataSep + proj + Constants.DataSep + email; SocketSend("27", SendData);
        }
        public void SaveComment(string proj, string strucmem, string comment, char isImg, string ImgFile = null)      //Saves any comment made in the app to the project log on server
        {
            string SendData = Constants.DataSep + proj + Constants.DataSep + isImg + Constants.DataSep + strucmem + Constants.DataSep + comment + Constants.DataSep + ImgFile; SocketSend("28", SendData);
        }
        public void StrucMemOverview(string gameSite, string proj, string strucmem, string email = null, string GID = null)     // Gets the overview of the structutal member
        {
            string SendData = Constants.DataSep + gameSite + Constants.DataSep + proj + Constants.DataSep + strucmem + Constants.DataSep + email + Constants.DataSep + GID; SocketSend("29", SendData);
        }

        private async void SocketSend(string CommandNr, string message)
        {
            try
            {
                if (CommandNr == "0") message = "Exit";     //If CommandNr = 0 then close the connection
                else message = CommandNr + ";" + message;
                var msg = Encoding.UTF8.GetBytes(Constants.BamAPPKey + message); //Add Constants.BeeAPPKey to the message to ensure that the message came from BeeAPP client only
                _socket.Send(msg);

                byte[] recieve_bytes = new byte[BUFFER_SIZE];
                var rec_count = 0;
                do
                {
                    var buf = new byte[BUFFER_SIZE];
                    var count = _socket.Receive(buf);
                    var temp = new byte[count]; Array.Copy(buf, temp, count);
                    temp.CopyTo(recieve_bytes, rec_count);
                    rec_count += count;
                    Stopwatch sw = Stopwatch.StartNew();
                    while (true) { if (sw.ElapsedMilliseconds > 50) break; }
                    sw.Stop(); //Wait for a short while to ensure that this socket client gets the next data packet to read. If any data packet is left to read then _socket.Available > 0
                } while (_socket.Available > 0);

                //int rec_count = socket.Receive(recieve_bytes);
                byte[] data = new byte[rec_count];
                Array.Copy(recieve_bytes, data, rec_count);
                var recieve_string = Encoding.UTF8.GetString(data);
                if (recieve_string.Contains(Constants.GID) && recieve_string.Contains(Constants.ZID)) //Check if the recieved data contains Constants.SID and Constants.FID. This will check if the recieved data is complete or not.
                {
                    recieve_string = recieve_string.Replace(Constants.GID, null); recieve_string = recieve_string.Replace(Constants.ZID, null);
                    var contentMessage = recieve_string.Split("<Valid>", StringSplitOptions.None);
                    Valid = contentMessage[0] == "1";
                    ReturnData = contentMessage[1];
                }
                else { if (CommandNr != "0") { DisConnect(); await ConnectClient(); } }
            }
            catch { if (CommandNr != "0") { DisConnect(); await ConnectClient(); } }
        }
        public void DisConnect() { try { SocketSend("0", null); _socket.Shutdown(SocketShutdown.Both); _socket.Close(); } catch { return; } }

    }
}
