using System;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.IO;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.Util;



namespace MiR_Helper.rest
{
    /// <summary>
    /// Out robot class will contain all of the data conatined within a robot. 
    /// This includes connection parameters (such as IP), along with other abstract data such as position, etc. 
    /// </summary>
    public class Robot
    {
        private static bool isFleet;
        private static IPAddress IP;
        private static AuthenticationManager Auth;
        public string Authorization { get; set; }
        public string IpAddress { get; set; }
        public MiRStatus Status { get; private set; }
        public bool StopTest { get; set; }

        public event EventHandler<MirTestEventArgs> UpdateTestStatus;

        /// <summary>
        /// Empty constructor, just initialize all the variables so we don't get null exceptions later on
        /// </summary>
        public Robot()
        {
            isFleet = false;
            IP = new IPAddress(0x2414188f);
            /*Auth = new AuthenticationManager();*/
        }

        public async Task UpdateStatus()
        {
            Status = await GetMiRStatus();
        }

        public async Task<List<MiRTestData>> StoppingDistanceTest()
        {
            var testData = new List<MiRTestData>();
            StopTest = false;


            await UpdateStatus();
            while(Status.state_id!= MiRStateId.Executing && Status.state_id!=MiRStateId.InManual)
            {
                if (StopTest)
                    break;
                OnUpdateTestStatus("Waiting for Executing or InManual State", new MiRTestData() { MiRStatus = Status, dataTime = TimeSpan.Zero });
                Thread.Sleep(500);
                await UpdateStatus();

            }

            var startime = DateTime.Now;
            while(!(Status.state_id==MiRStateId.InError && Status.velocity.linear==0))
            {
                if (StopTest)
                    break;
                MiRTestData data = new MiRTestData();
                await UpdateStatus();
                var testTime = DateTime.Now-startime;
                data.MiRStatus = Status;
                data.dataTime = testTime;
                testData.Add(data);
                if(Status.state_id!=MiRStateId.InError)
                    OnUpdateTestStatus("Test started, waiting for InError state", data);
                else
                    OnUpdateTestStatus("Robot in error, waiting for standstill state", data);
                 Thread.Sleep(100);
            }
            OnUpdateTestStatus("Test finished", testData.Last());
            return testData;
        }

        /// <summary>
        /// Save the measurement data into an excel file
        /// </summary>
        public void SaveDataToFile(List<MiRTestData> TestData)
        {

            try
            {
                if (TestData == null)
                    throw new Exception();

                IWorkbook workbook = new XSSFWorkbook();
                ISheet sheet1 = workbook.CreateSheet("MirTest");

                // Creating header style
                var boldFont = workbook.CreateFont();
                boldFont.IsBold = true;
                var headerStyle = workbook.CreateCellStyle();
                headerStyle.SetFont(boldFont);
                headerStyle.IsLocked = true;
                headerStyle.BorderBottom = BorderStyle.Medium;
                headerStyle.BorderLeft= BorderStyle.Medium;
                headerStyle.BorderRight= BorderStyle.Medium; 
                headerStyle.BorderTop= BorderStyle.Medium;
                headerStyle.FillForegroundColor = IndexedColors.Yellow.Index;
                headerStyle.FillPattern = FillPattern.SolidForeground;

                // Filling header
                IRow header = sheet1.CreateRow(0);
                header.CreateCell(0).SetCellValue("Status");
                header.CreateCell(1).SetCellValue("Position X");
                header.CreateCell(2).SetCellValue("Position Y");
                header.CreateCell(3).SetCellValue("Orientation");
                header.CreateCell(4).SetCellValue("Linear Velocity");
                header.CreateCell(5).SetCellValue("Angular Velocity");
                header.CreateCell(6).SetCellValue("Time");
                

                for(int j=0;j<=6;j++)
                {
                    header.GetCell(j).CellStyle = headerStyle;
                    sheet1.AutoSizeColumn(j);
                    
                    
                }

                // Creating formating for numeric values
                var numStyle = workbook.CreateCellStyle();
                numStyle.DataFormat = workbook.CreateDataFormat().GetFormat("0.00");


                // Setting cell values
                var i = 1;
                foreach (var data in TestData)
                {
                    var row = sheet1.CreateRow(i);
                    row.CreateCell(0).SetCellValue(data.MiRStatus.state_id.ToString());
                    row.CreateCell(1).SetCellValue(data.MiRStatus.position.x);
                    row.CreateCell(2).SetCellValue(data.MiRStatus.position.x);
                    row.CreateCell(3).SetCellValue(data.MiRStatus.position.orientation);
                    row.CreateCell(4).SetCellValue(data.MiRStatus.velocity.linear);
                    row.CreateCell(5).SetCellValue(data.MiRStatus.velocity.angular);
                    row.CreateCell(6).SetCellValue(data.dataTime.TotalSeconds);

                    row.GetCell(1).CellStyle = numStyle;
                    row.GetCell(2).CellStyle = numStyle;
                    row.GetCell(3).CellStyle = numStyle;
                    row.GetCell(4).CellStyle = numStyle;
                    row.GetCell(5).CellStyle = numStyle;
                    row.GetCell(6).CellStyle = numStyle;


                    i++;
                }

                sheet1.CreateFreezePane(0, 1, 0, 1);
                sheet1.AutoSizeColumn(0);

                // Creating time-speed linechart
                var numberOfRows = TestData.Count;
                var drawing = sheet1.CreateDrawingPatriarch();
                var anchor = drawing.CreateAnchor(0, 0, 0, 0, 8, 5, 18, 15);
                var chart = drawing.CreateChart(anchor);
                var legend = chart.GetOrCreateLegend();

                var dataFactory = chart.ChartDataFactory;
                var chartAxisFactory = chart.ChartAxisFactory;

                var lineChartData = dataFactory.CreateLineChartData<double, double>();
                var bottomAxis = chartAxisFactory.CreateCategoryAxis(NPOI.SS.UserModel.Charts.AxisPosition.Bottom);
                var leftAxis = chartAxisFactory.CreateValueAxis(NPOI.SS.UserModel.Charts.AxisPosition.Right);
                


                var xs = NPOI.SS.UserModel.Charts.DataSources.FromNumericCellRange(sheet1, new NPOI.SS.Util.CellRangeAddress(0, numberOfRows, 6, 6));
                var ys = NPOI.SS.UserModel.Charts.DataSources.FromNumericCellRange(sheet1, new NPOI.SS.Util.CellRangeAddress(0, numberOfRows, 4, 4));

                lineChartData.AddSeries(xs, ys);
                

                chart.Plot(lineChartData, bottomAxis, leftAxis);

                using (FileStream sw = File.Create(@"C:\Users\nemetht\MirTest\test.xlsx"))
                {
                    workbook.Write(sw);
                    sw.Close();
                }
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
           
            }
            
            
        }

        public async Task<MiRStatus> GetMiRStatus()
        {
            try
            {
                MiRStatus mirStatus = new MiRStatus();
                var jBody = new JObject();
                var urlBody = "status?whitelist=";



                foreach (var whitelist in MiRStatus.WhiteList)
                {
                    urlBody += whitelist;
                    if (MiRStatus.WhiteList.Last() != whitelist)
                        urlBody += ",";
                }

                var rawStatus = await SendApiMessage(Method.GET, jBody, urlBody);

                if (!rawStatus.IsSuccessful)
                    throw new Exception("");

                mirStatus = JsonConvert.DeserializeObject<MiRStatus>(rawStatus.Content);

                return mirStatus;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private async Task<IRestResponse> SendApiMessage(Method method, JObject jObjectBody, string URLBody)
        {

            try
            {
                var baseURL = "http://" + IpAddress + "/api/v2.0.0/" + URLBody;


                //restClient = new RestClient("http://192.168.1.216/api/v2.0.0/status?whitelist=state_id,position,velocity");
                var restClient = new RestClient(baseURL);

                restClient.Timeout = -1;

                var request = new RestRequest(method);
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", "Basic QWRtaW46OGM2OTc2ZTViNTQxMDQxNWJkZTkwOGJkNGRlZTE1ZGZiMTY3YTljODczZmM0YmI4YTgxZjZmMmFiNDQ4YTkxOA==");
                request.AddHeader("Accept-Language", "en_US");


                request.AddParameter("application/json", ParameterType.RequestBody);

                IRestResponse response = await restClient.ExecuteAsync(request);


                return response;
            }
            catch (Exception)
            {

                throw;
            }


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

        protected virtual void OnUpdateTestStatus(string testStatus, MiRTestData testData)
        {
            if (UpdateTestStatus != null)
                UpdateTestStatus(this, new MirTestEventArgs() { TestStatus = testStatus, TestData = testData });
        }
    }

    public enum MiRStateId
    {
        ShuttingDown = 2,
        Ready = 3,
        Pause = 4,
        Executing = 5,
        Aborted = 6,
        Completed = 7,
        Docked = 8,
        Docking = 9,
        Estopped = 10,
        InManual = 11,
        InError = 12
    }

    public class MiRVelocity
    {
        public double linear { get; set; }
        public double angular { get; set; }
    }

    public class MiRPosition
    {
        public double x { get; set; }
        public double y { get; set; }
        public double orientation { get; set; }

    }



    public class MiRStatus
    {
        public MiRStateId state_id { get; set; }
        public MiRVelocity velocity { get; set; }
        public MiRPosition position { get; set; }

        public static readonly string[] WhiteList = { "state_id", "velocity", "position" };
    }

    public class MiRTestData
    {
        public MiRStatus MiRStatus { get; set; }
        public TimeSpan dataTime { get; set; }
    }

    public class MirTestEventArgs : EventArgs
    {
        public string TestStatus { get; set; }
        public MiRTestData TestData { get; set; }
    }


}
